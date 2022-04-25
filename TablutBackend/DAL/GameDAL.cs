using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TablutBackend.Models;
using static TablutBackend.Models.TTType;

namespace TablutBackend.DAL
{
    
    public class GameDAL
    {
        const int DEPTH = 4;
        const int BLACK_WIN = 101;
        const int WHITE_WIN = 102;
        const int HIGHEST_SCORE = 1000;
        
        /// <summary>
        /// Creates an instance of a new game in the database.
        /// </summary>
        /// <returns>returns the id of the created game</returns>
        public static long CreateGame()
        {
            MySqlConnection conn = ConnectToDB();
            WhiteBoard w = new WhiteBoard();
            BlackBoard b = new BlackBoard();
            string cmndString = string.Format(@"insert into games (wb, bb, kb) values({0}{1}{0},{0}{2}{0},{0}{3}{0});", "'", w.board, b.board, w.kingboard);//wb, bb, kb
            MySqlCommand cmnd = new MySqlCommand(cmndString, conn);
            cmnd.ExecuteNonQuery();
            long lastID = cmnd.LastInsertedId;
            conn.Close();
            return lastID;
        }

        /// <summary>
        /// Turn managment after client's turn request (PUT). This function calls for
        /// game's boards from database by given id, making the requested move by the
        /// client (from, to), checking for captures, win conditions and sending back
        /// a list of integers that resemble a message, then updates the boards back
        /// to the database:
        /// 101 / 102 for a finished game, other values for captured pieces' indieces.
        /// </summary>
        /// <param name="id"> The game's id the function gets from the client. </param>
        /// <param name="from"> The client's piece that he wishes to move. </param>
        /// <param name="to"> The place on board the client wishes to move to. </param>
        /// <param name="color"> The client's side-color. </param>
        /// <returns> A list of integers that resemble a message:
        /// 101 / 102 for a finished game, other values for captured pieces' indieces. </returns>
        public static List<int> SetTurn(long id, int from, int to, string color)
        {
            WhiteBoard w = new WhiteBoard();
            BlackBoard b = new BlackBoard();

            MySqlConnection conn = ConnectToDB();

            //retrieve boards from db...
            DataTable dTable = RetrieveBoards(conn, id);

            w.board = JsonConvert.DeserializeObject<BitSet>((string)dTable.Rows[0]["wb"]);
            b.board = JsonConvert.DeserializeObject<BitSet>((string)dTable.Rows[0]["bb"]);
            w.kingboard = JsonConvert.DeserializeObject<BitSet>((string)dTable.Rows[0]["kb"]);

            List<int> captures = null;
            List<int> endgame = new List<int>(1);
            BitSet o_board;
            int kingpos = w.kingboard.GetFirstOn();
            switch (color)
            {
                case "black":
                    b.Move(from, to);
                    o_board = w.board;
                    captures = b.SetCaptures(to, ref o_board, kingpos, false);
                    if (captures.Contains(kingpos))
                        w.kingboard &= ~w.kingboard.MaskOn(kingpos);
                    w.board = (BitSet)o_board.Clone();
                    //check for win
                    if (b.Won(w.kingboard))
                    {
                        endgame.Add(BLACK_WIN);
                        DeleteGame(conn, id);
                        return endgame;
                    }
                    break;
                case "white":
                    if (w.IsKingMoved(from))
                        w.KingMove(from, to);
                    else
                        w.Move(from, to);
                    
                    o_board = b.board;
                    //check for win
                    if (w.Won())
                    {
                        endgame.Add(WHITE_WIN);
                        DeleteGame(conn, id);
                        return endgame;
                    }

                    w.board |= w.kingboard; // merge to check if king was involved in capturing.
                    captures = w.SetCaptures(to, ref o_board, kingpos, true);
                    b.board = (BitSet)o_board.Clone();
                    w.board &= ~w.kingboard; // turn off the king bit that we added.
                    break;
                default:
                    break;
            }
            //update to db...
            string cmndString = string.Format("update games set wb='{0}', bb='{1}', kb='{2}' where id = {3}", w.board, b.board, w.kingboard, id);
            MySqlCommand cmnd = new MySqlCommand(cmndString, conn);
            cmnd.ExecuteNonQuery();
            conn.Close();
            return captures;
        }

        /// <summary>
        /// Turn managment for the computer-side after the client made a turn (POST request).
        /// This function calls for game's boards from database by given id, searching for the
        /// best move by AlphaBetaTT algo and making the best move, checking for captures, win
        /// conditions and sending back a list of integers that resemble a message, then 
        /// updates the boards back to the database:
        /// from piece, to position, 101 / 102 for a finished game, other values for captured 
        /// pieces' indieces.
        /// </summary>
        /// <param name="id"> The game's id the function gets from the client. </param
        /// <param name="color"> The client's side-color. </param>
        /// <returns> From piece, to position, 101 / 102 for a finished game, other values for captured 
        /// pieces' indieces. </returns>
        public static List<int> SetAITurn(long id, string color)
        {
            WhiteBoard w = new WhiteBoard();
            BlackBoard b = new BlackBoard();

            MySqlConnection conn = ConnectToDB();

            //retrieve boards from db...
            DataTable dTable = RetrieveBoards(conn, id);

            w.board = JsonConvert.DeserializeObject<BitSet>((string)dTable.Rows[0]["wb"]);
            b.board = JsonConvert.DeserializeObject<BitSet>((string)dTable.Rows[0]["bb"]);
            w.kingboard = JsonConvert.DeserializeObject<BitSet>((string)dTable.Rows[0]["kb"]);

            Move best = null;
            List<int> message = new List<int>(3);
            BitSet o_board;
            int kingpos = w.kingboard.GetFirstOn();
            List<int> captures = null;
            // message is built from -> from, to, 
            switch (color)
            {
                case "black":
                    best = AlphaBetaTT(w, b, DEPTH, -HIGHEST_SCORE-1, HIGHEST_SCORE+1, false);
                    b.Move(best.from, best.to);
                    o_board = w.board;
                    message.Add(best.from);
                    message.Add(best.to);
                    captures = b.SetCaptures(best.to, ref o_board, kingpos, false);
                    if (captures.Count == 1 && captures.Contains(kingpos))
                        w.kingboard &= ~w.kingboard.MaskOn(kingpos);
                    if (b.Won(w.kingboard))
                    {
                        message.Add(BLACK_WIN);
                        DeleteGame(conn, id);
                        return message;
                    }
                    w.board = (BitSet)o_board.Clone();
                    break;
                case "white":
                    best = AlphaBetaTT(w, b, DEPTH, -HIGHEST_SCORE-1, HIGHEST_SCORE+1, true);
                    if (w.IsKingMoved(best.from))
                        w.KingMove(best.from, best.to);
                    else
                        w.Move(best.from, best.to);

                    o_board = b.board;
                    message.Add(best.from);
                    message.Add(best.to);
                    if (w.Won())
                    {
                        message.Add(WHITE_WIN);
                        DeleteGame(conn, id);
                        return message;
                    }
                    w.board |= w.kingboard; // merge to check if king was involved in capturing.
                    captures = w.SetCaptures(best.to, ref o_board, kingpos, true);
                    message.AddRange(captures);
                    b.board = (BitSet)o_board.Clone();
                    w.board &= ~w.kingboard; // turn off the king bit that we added.
                    break;
                default:
                    break;
            }
            message.AddRange(captures);

            string cmndString = string.Format("update games set wb='{0}', bb='{1}', kb='{2}' where id = {3}", w.board, b.board, w.kingboard, id);
            MySqlCommand cmnd = new MySqlCommand(cmndString, conn);
            cmnd.ExecuteNonQuery();
            conn.Close();
            return message;
        }


        /// <summary>
        /// Gets an open connection with the mysql database, and required id, and gives
        /// back the board's situation for the requested game.
        /// </summary>
        /// <param name="conn"> Open connection with the Mysql database to run queries on. </param>
        /// <param name="id"> The game's id the function gets from the client. </param>
        /// <returns></returns>
        public static DataTable RetrieveBoards(MySqlConnection conn, long id)
        {
            string cmndString = string.Format("select * from games where id = {0}", id);
            MySqlCommand cmnd = new MySqlCommand(cmndString, conn);
            MySqlDataAdapter MyAdapter = new MySqlDataAdapter();
            MyAdapter.SelectCommand = cmnd;
            DataTable dTable = new DataTable();
            MyAdapter.Fill(dTable);
            return dTable;
        }

        /// <summary>
        /// Gets an open connection with the mysql database, and required id, and deletes
        /// the board's situation for the requested game from the games table in the database.
        /// </summary>
        /// <param name="conn"> Open connection with the Mysql database to run queries on. </param>
        /// <param name="id"> The finished game's id the function gets from the client. </param>
        public static void DeleteGame(MySqlConnection conn, long id)
        {
            try
            {
                string Query = string.Format("delete from games where id = {0}", id);
                MySqlCommand Command = new MySqlCommand(Query, conn);
                MySqlDataReader Reader;
                Reader = Command.ExecuteReader();
                while (Reader.Read())
                {
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Opens a new mysql connection to the main mysql database server the
        /// project rely on. Returns it for future queries.
        /// </summary>
        /// <returns> Open connection with the Mysql database to run queries on. </returns>
        public static MySqlConnection ConnectToDB()
        {
            MySqlConnection conn;
            string myConnectionString;

            myConnectionString = "server=localhost;uid=dor;" +
                "pwd=g035341396;database=tablutdb";

            try
            {
                conn = new MySqlConnection();
                conn.ConnectionString = myConnectionString;
                conn.Open();
                return conn;
            }
            catch (MySqlException msex)
            {
                Console.WriteLine(msex.Message);
                return null;
            }
        }


        /// <summary>
        /// (TT stands for Transposition Table)
        /// The main algorithem for searching after the best move the computer can do. This function
        /// rely on the NEGAMAX algo, which is a derivative of the Minimax algorithem, that searches
        /// in the deepest, most optimal depth of a game tree for a future move after simulating few
        /// moves into the game in order to make the best move even after the opponent making his own
        /// best moves. In order to cut-down a lot of unneccessary moves, I used the Alpha Beta blocks
        /// to check for future moves that are already known as worse moves. The use in TRANSPOSITION
        /// TABLE is helpful for running few games on the server, or even for the same game to run onto
        /// same board-situations, and avoid unneccessary re-calculations that were already made. I
        /// saved the past moves in the cache that functionats as a Dictionary.
        /// Eventually the best move will be chosen and the ai will be able to reach victory.
        /// </summary>
        /// <param name="wb"> WhiteBoard object. Needed for the evaluation of the board's score and 
        /// running simulated moves during the recursives. </param>
        /// <param name="bb"> BlackBoard object. Needed for the evaluation of the board's score and 
        /// running simulated moves during the recursives. </param>
        /// <param name="depth"> The required Game-Tree depth I want the function to dive into.
        /// Will be initialized as 4 usually. </param>
        /// <param name="alpha"> The lower block. Responsible for the current ai player's move selection. </param>
        /// <param name="beta"> The higher block. responsible for the opposnent's futuristic move selection. </param>
        /// <param name="wturn"> Boolean that resembles whether the current ai player is the white side or not. </param>
        /// <param name="opponent"> On every recursive step it switches. An important key for function's calculations. </param>
        /// <returns> Move object that resembles the optimal move the computer can do. (eventually AND in 
        /// every recursive step). </returns>
        public static Move AlphaBetaTT(WhiteBoard wb, BlackBoard bb, int depth, int alpha, int beta, bool wturn, bool opponent = false)
        {              
            int value;
            WhiteBoard copy_wb; BlackBoard copy_bb;
            string key = GetKey(bb, wb);
            Move ttmove = CacheTT.Get(key);

            if (ttmove != null && ttmove.wturn == wturn)
            { 
                if (ttmove.type == LOW && ttmove.score > Math.Abs(alpha))
                    alpha = ttmove.score;
                if (ttmove.type == UP && ttmove.score < Math.Abs(beta))
                    beta = ttmove.score;
                if (ttmove.type == EXACT || alpha >= beta)
                    return ttmove;
            }
            bool won = (wb.Won() || bb.Won(wb.kingboard));
            if ((depth == 0 || won))
            {
                if (won)
                    value = HIGHEST_SCORE-1-(5-depth);
                else
                    value = wturn ? wb.Hueristic(bb, depth) : bb.Hueristic(wb, depth);
                Move ret = new Move(0, 0, (wturn && !opponent) || (!wturn && opponent));
                ret.score = value;
                StoreMoveTT(key, ret, alpha, beta);
                return ret;
            }
            List<Move> moves = ExtractMoves(wb, bb, ((wturn && !opponent) || (!wturn && opponent)));
            BitSet o_board;
            Move best = new Move(0,0, (wturn && !opponent) || (!wturn && opponent));

            best.score = -HIGHEST_SCORE-1;
            foreach (Move move in moves)
            {
                copy_bb = (BlackBoard)bb.Clone();
                copy_wb = (WhiteBoard)wb.Clone();
                Move bestnextmove;
                int kingpos = copy_wb.kingboard.GetFirstOn();
                if ((wturn && !opponent) || (!wturn && opponent))
                {
                    o_board = copy_bb.board;
                    copy_wb.Move(move.from, move.to);
                    copy_wb.board |= copy_wb.kingboard;
                    copy_wb.SetCaptures(move.to, ref o_board, kingpos, true);
                    copy_wb.board &= ~copy_wb.kingboard;
                    copy_bb.board = (BitSet)o_board.Clone();
                }
                else
                {
                    o_board = copy_wb.board;
                    copy_bb.Move(move.from, move.to);
                    List<int> captures = copy_bb.SetCaptures(move.to, ref o_board, kingpos, false);
                    if (captures.Count == 1 && captures.Contains(kingpos))
                        copy_wb.kingboard &= ~copy_wb.kingboard.MaskOn(kingpos);
                    copy_wb.board = (BitSet)o_board.Clone();
                }
                bestnextmove = AlphaBetaTT(copy_wb, copy_bb, depth - 1, -beta, -alpha, wturn, !opponent);
                if (-bestnextmove.score > best.score)
                {
                    best = move;
                    best.score = -bestnextmove.score;
                }
                if (best.score > alpha)
                    alpha = best.score;
                if (best.score >= beta)
                    break;
            }
            StoreMoveTT(key, best, alpha, beta);
            return best;
        }

        /// <summary>
        /// Sets a new key for a given board situation for cache storage usage.
        /// </summary>
        /// <param name="boards"> Two boards objects that sets the board's situation. </param>
        /// <returns> A string that resembles the key. </returns>
        private static string GetKey(params Board[] boards)
        {
            return boards[0].ToString() + boards[1].ToString();
        }

        /// <summary>
        /// This function stores a given move for a given board situation in the cache,
        /// as either the lowest block move can be for this situation, highest block or
        /// the most optimal move for this board situation.
        /// </summary>
        /// <param name="key"> string that resembles the board's situation. </param>
        /// <param name="move"> given move with score. </param>
        /// <param name="alpha"> The lower block. Responsible for the current ai player's move selection. </param>
        /// <param name="beta"> The higher block. responsible for the opposnent's futuristic move selection. </param>
        private static void StoreMoveTT(string key, Move move, int alpha, int beta)
        {
            if (move.score <= alpha)
                move.type = LOW;
            else if (move.score >= beta)
                move.type = UP;
            else
                move.type = EXACT;
            CacheTT.Add(key, move);
        }

        /// <summary>
        /// calls for right moves exctraction method according to the given turn side.
        /// </summary>
        /// <param name="wb"> White Board </param>
        /// <param name="bb"> Black Board </param>
        /// <param name="wturn"> Boolean -> if white turn or not. </param>
        /// <returns></returns>
        private static List<Move> ExtractMoves(WhiteBoard wb, BlackBoard bb, bool wturn)
        {
            return wturn ? wb.GetAllMoves(bb) : bb.GetAllMoves(wb);
        }
    }
}