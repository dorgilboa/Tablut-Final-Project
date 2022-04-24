using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using TablutBackend.Models;
using static TablutBackend.Models.TTType;

namespace TablutBackend.DAL
{
    
    public class GameDAL
    {
        const int DEPTH = 3;
        const int BLACK_WIN = 101;
        const int WHITE_WIN = 102;
        const int HIGHEST_SCORE = 1000;
        /// <summary>
        /// init and creates instance on db.
        /// </summary>
        /// <returns></returns>
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
                    w.kingboard.Print();
                    //o_board &= ~w.kingboard;
                    w.board = (BitSet)o_board.Clone();
                    //check win
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
                    //check win
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
            //w.board.Print();
            //b.board.Print();
            string cmndString = string.Format("update games set wb='{0}', bb='{1}', kb='{2}' where id = {3}", w.board, b.board, w.kingboard, id);
            MySqlCommand cmnd = new MySqlCommand(cmndString, conn);
            cmnd.ExecuteNonQuery();
            conn.Close();
            return captures;
        }

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
            // message is built from -> from, to, 
            switch (color)
            {
                case "black":
                    best = AlphaBetaTT(w, b, DEPTH, -HIGHEST_SCORE-1, HIGHEST_SCORE+1, false);
                    b.Move(best.from, best.to);
                    o_board = w.board;
                    message.Add(best.from);
                    message.Add(best.to);
                    List<int> captures = b.SetCaptures(best.to, ref o_board, kingpos, false);
                    if (captures.Count == 1 && captures.Contains(kingpos))
                        w.kingboard &= ~w.kingboard.MaskOn(kingpos);
                    if (b.Won(w.kingboard))
                    {
                        message.Add(BLACK_WIN);
                        DeleteGame(conn, id);
                        return message;
                    }
                    foreach (int capt in captures)
                    {
                        message.Add(capt);
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
                    foreach (int capt in captures)
                    {
                        message.Add(capt);
                    }
                    b.board = (BitSet)o_board.Clone();
                    w.board &= ~w.kingboard; // turn off the king bit that we added.
                    break;
                default:
                    break;
            }

            string cmndString = string.Format("update games set wb='{0}', bb='{1}', kb='{2}' where id = {3}", w.board, b.board, w.kingboard, id);
            MySqlCommand cmnd = new MySqlCommand(cmndString, conn);
            cmnd.ExecuteNonQuery();
            conn.Close();
            return message;
        }


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
            bool won = ((wturn && wb.Won()) || (!wturn && bb.Won(wb.kingboard)));
            if ((depth == 0 || won))
            {
                if (won)
                    value = HIGHEST_SCORE-1-(5-depth);
                else
                    value = (wturn && !opponent) || (!wturn && opponent) ? wb.Hueristic(bb, depth) : bb.Hueristic(wb, depth);
                //move.score = !opponent ? value : -value;
                Move ret = new Move(0, 0, (wturn && !opponent) || (!wturn && opponent));
                ret.score = value;
                StoreMoveTT(key, ret, alpha, beta);
                return ret;
            }
            List<Move> moves = ExtractMoves(wb, bb, ((wturn && !opponent) || (!wturn && opponent)));
            BitSet o_board;
            Move best = new Move(0,0, (wturn && !opponent) || (!wturn && opponent));

            best.score = -HIGHEST_SCORE-1;
            for (int i = 0; i < moves.Count; i++)
            {
                Move move = moves[i];
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
                if (move.from == kingpos && (move.to % 9 == 0 || (move.to-1) % 9 == 0 || move.to <= 9 || move.to > 72))
                    Console.WriteLine("hi");
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


        private static string GetKey(params Board[] boards)
        {
            return boards[0].ToString() + boards[1].ToString();
        }


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


        private static List<Move> ExtractMoves(WhiteBoard wb, BlackBoard bb, bool wturn)
        {
            if (wturn)
                return wb.GetAllMoves(bb);
            return bb.GetAllMoves(wb);
        }
    }
}