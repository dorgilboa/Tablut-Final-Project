using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TablutBackend.Models
{
    public class WhiteBoard : Board
    {
        private readonly string KING_TEMPLATE = "000000000000000000000000000000000000000010000000000000000000000000000000000000000";
        public BitSet kingboard { get; set; }
        public WhiteBoard() : base(true)
        {
            kingboard = new BitSet(KING_TEMPLATE);
        }

        /// <summary>
        /// Checks if the king has been selected in last move.
        /// </summary>
        /// <param name="pos"> Position (index on board) to check on if the king is also there. </param>
        /// <returns> True / False if king was moved or not. </returns>
        public bool IsKingMoved(int pos)
        {
            return kingboard.MaskOn(pos).Equals(kingboard & kingboard.MaskOn(pos));
        }

        /// <summary>
        /// Responsible for the movement of the king on his own board. (Because
        /// he is seperated from the regular white (bit)board.
        /// </summary>
        /// <param name="from"> Source position index the king moves from. </param>
        /// <param name="to"> Dest. position index the king moves into. </param>
        public void KingMove(int from, int to)
        {
            kingboard &= ~kingboard.MaskOn(from);
            kingboard |= kingboard.MaskOn(to);
        }

        /// <summary>
        /// Check if the white player has won. gets no parameters yet other is
        /// neccessary for black player.
        /// </summary>
        /// <returns> True / False if king is on one of the corner positions. </returns>
        public override bool Won(BitSet other = null)
        {
            BitSet check = ~(new BitSet(KING_TEMPLATE)) & restrictions;
            if (!(kingboard & check).Equals(kingboard.MaskOn(0)))
                return true;
            return false;
        }

        /// <summary>
        /// Extract all moves can possibly be for white player by calling base method with wturn = true.
        /// </summary>
        public override List<Move> GetAllMoves(Board other, bool wturn = false, BitSet merged = null)
        {
            return base.GetAllMoves(other, true, kingboard | board | other.board);
        }

        /// <summary>
        /// Extract all white pieces' indieces on board by calling base method, and adding the king's
        /// index to the list.
        /// </summary>
        /// <returns> A list of integer that resembles all indexes of white pieces on board. </returns>
        public override List<int> GetAllPieces()
        {
            List<int> pieces = base.GetAllPieces();
            pieces.Add(kingboard.GetFirstOn());
            return pieces;
        }

        /// <summary>
        /// pct. stands for percentage.
        /// The main function for calculating the board's situation for a white player.
        /// This function rely on few factors:
        /// pct. of alive white players * 18 
        /// pct. of captured black players * 14
        /// Avg diff between white players' weights + king's weight on board up to 10 score, minus blacks avg score. times 2.
        /// Amount of ways for a king to the corners * 20 - highly weighted score.
        /// pct. of dangerous pieces around king - minus 32.
        /// pct. of white pieces around him * 8.
        /// depth's impact on score - We'd want to get same score in less depth into the tree.
        /// </summary>
        /// <param name="other"> Black Board. </param>
        /// <param name="depth"> Depth of the board's situation in game-tree. </param>
        /// <returns> The total score that has been affected from all these factors above. </returns>
        public override int Hueristic(Board other, int depth)
        {
            int kingpos = kingboard.GetFirstOn();
            int totalscore = 0;
            List<int> wpieces = base.GetAllPieces();
            List<int> bpieces = other.GetAllPieces();
            //pct. of white players on board * 18 weight.
            double whitepct = wpieces.Count / WHITE_PIECES;
            totalscore += (int)(whitepct * 18);

            //pct. of captured black players * 14 weight.
            double blackcappct = (BLACK_PIECES - bpieces.Count) / BLACK_PIECES;
            totalscore += (int)(blackcappct * 14);

            //evaluate difference between average pieces weights per side * 2.
            double weightscore = (CalcTotalWeight(WHITE_WEIGHTS, wpieces.ToArray()) / wpieces.Count) * 1.3;
            weightscore += CalcTotalWeight(KING_WEIGHTS, kingpos) * 0.7;
            weightscore -= (CalcTotalWeight(BLACK_WEIGHTS, bpieces.ToArray()) / bpieces.Count) * 2;
            totalscore += (int)(weightscore * 2);

            //amount of ways for a king to the corners * 20.
            totalscore += CalcWaysToCorner(board | other.board, kingpos) * 20;

            //danger on king - minus percentage of danger to king * 32 weight.
            double kingdangerpct = KingDanger(bpieces, kingpos) / PiecesToCaptureKing(kingpos);
            totalscore -= (int)(kingdangerpct * 32);

            //guards around king - pct. of whites of all around it * 8 weight.
            double kingsafepct = KingSafe(wpieces, kingpos) / PIECES_AROUND_KING;
            totalscore += (int)kingsafepct;

            totalscore -= (5 - depth);

            return totalscore;
        }

        /// <summary>
        /// Count the amount of ways the king has to the corners of the board.
        /// </summary>
        /// <param name="merged"> merged bit board of white and black pieces that might block the king in the way. </param>
        /// <param name="kingpos"> The position of the king. </param>
        /// <returns> amount of ways the king has to the corners of the board. </returns>
        public int CalcWaysToCorner(BitSet merged, int kingpos)
        {
            int curr = kingpos, ways = 0;
            bool found;
            if (kingpos % EDGE_SIZE== 0 || (kingpos - 1) % EDGE_SIZE== 0)
            {
                found = true;
                // up and down.
                while (curr < 82 && found)
                {
                    curr += EDGE_SIZE;
                    if (merged.IsOn(curr))
                        found = false;
                }
                ways += found ? 1 : 0;
                curr = kingpos;
                found = true;
                while (curr > 0 && found)
                {
                    curr -= EDGE_SIZE;
                    if (merged.IsOn(curr))
                        found = false;
                }
                ways += found ? 1 : 0;
            }
            else if (kingpos < EDGE_SIZE|| kingpos > 72)
            {
                found = true;
                // left and right.
                while (curr % EDGE_SIZE != 0 && found)
                {
                    curr++;
                    if (merged.IsOn(curr))
                        found = false;
                }
                ways += found ? 1 : 0;
                found = true;
                curr = kingpos;
                while ((curr-1) % EDGE_SIZE > 0 && found)
                {
                    curr --;
                    if (merged.IsOn(curr))
                        found = false;
                }
                ways += found ? 1 : 0;
            }
            return ways;
        }

        /// <summary>
        /// Count the amount of Black / restriction pieces around king.
        /// </summary>
        /// <param name="danger_pieces"> The black pieces' indieces list. </param>
        public int KingDanger(List<int> danger_pieces, int kingpos)
        {
            danger_pieces.Concat(RES_PLACES);
            int total = 0;
            if (danger_pieces.Contains(kingpos - 1))
                total++;
            if (danger_pieces.Contains(kingpos + 1))
                total++;
            if (danger_pieces.Contains(kingpos - EDGE_SIZE))
                total++;
            if (danger_pieces.Contains(kingpos + EDGE_SIZE))
                total++;
            return total;
        }

        /// <summary>
        /// Count the amount of white pieces around king.
        /// </summary>
        /// <param name="wpieces"> The white pieces' indieces list. </param>
        public int KingSafe(List<int> wpieces, int kingpos)
        {
            int total = 0;
            if (wpieces.Contains(kingpos - 1))
                total++;
            if (wpieces.Contains(kingpos + 1))
                total++;
            if (wpieces.Contains(kingpos - EDGE_SIZE))
                total++;
            if (wpieces.Contains(kingpos + EDGE_SIZE))
                total++;
            return total;
        }


        public override object Clone()
        {
            WhiteBoard copy = new WhiteBoard();
            copy.kingboard = (BitSet)kingboard.Clone();
            copy.board = (BitSet)board.Clone();
            return copy;
        }

        public override string ToString()
        {
            return base.ToString() + kingboard.ToString();
        }
    }
}