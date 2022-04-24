using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TablutBackend.Models
{
    public class WhiteBoard : Board
    {
        private readonly string KING_TEMPLATE = "000000000000000000000000000000000000000010000000000000000000000000000000000000000";
        private readonly int[] WHITE_WEIGHTS = { 2,2,2,0,0,0,2,2,2,
                                                  2,1,1,4,0,4,1,1,2,
                                                  2,1,2,2,2,2,2,1,2,
                                                  0,2,4,1,3,1,4,2,0,
                                                  0,0,2,3,0,3,2,0,0,
                                                  0,2,4,1,3,1,4,2,0,
                                                  2,1,2,2,2,2,2,1,2,
                                                  2,1,1,4,0,4,1,1,2,
                                                  2,2,2,0,0,0,2,2,2};
        private readonly int[] KING_WEIGHTS = { 5,1,3,3,3,3,3,1,5,
                                                  1,1,1,2,0,2,1,1,1,
                                                  3,1,2,1,1,1,2,1,3,
                                                  3,2,1,1,3,1,1,2,3,
                                                  3,0,1,3,3,3,1,0,3,
                                                  3,2,1,1,3,1,1,2,3,
                                                  3,1,2,1,1,1,2,1,3,
                                                  1,1,1,2,0,2,1,1,1,
                                                  5,1,3,3,3,3,3,1,5};
        public BitSet kingboard { get; set; }
        public WhiteBoard() : base(true)
        {
            kingboard = new BitSet(KING_TEMPLATE);
        }

        public bool IsKingMoved(int pos)
        {
            return kingboard.MaskOn(pos).Equals(kingboard & kingboard.MaskOn(pos));
        }

        public void KingMove(int from, int to)
        {
            kingboard &= ~kingboard.MaskOn(from);
            kingboard |= kingboard.MaskOn(to);
        }


        public override bool Won(BitSet other = null)
        {
            BitSet check = ~(new BitSet(KING_TEMPLATE)) & restrictions;
            if (!(kingboard & check).Equals(kingboard.MaskOn(0)))
                return true;
            return false;
        }

        public override List<Move> GetAllMoves(Board other, bool wturn = false, BitSet merged = null)
        {
            return base.GetAllMoves(other, true, kingboard | board | other.board);
        }

        public override List<int> GetAllPieces()
        {
            List<int> pieces = base.GetAllPieces();
            pieces.Add(kingboard.GetFirstOn());
            return pieces;
        }

        public override int Hueristic(Board other, int depth)
        {
            int kingpos = kingboard.GetFirstOn();
            int totalscore = 0;
            List<int> wpieces = base.GetAllPieces();
            List<int> bpieces = other.GetAllPieces();
            //amount of white players on board * 4.
            totalscore += wpieces.Count * 4;

            //amount of captured black players * 1.
            totalscore += (BLACK_PIECES - bpieces.Count);

            //add calc by areas...
            totalscore += CalcTotalWeight(WHITE_WEIGHTS, wpieces.ToArray()) / wpieces.Count;
            totalscore += CalcTotalWeight(KING_WEIGHTS, kingpos);

            //amount of ways for a king to the corners * 16.
            totalscore += CalcWaysToCorner(board | other.board, kingpos) * 16;

            //danger on king - minus amount of blacks & res. around him * 8.
            totalscore -= KingDanger(bpieces, kingpos) * 8;

            //guards around king - amount of whites * 1.
            totalscore += KingSafe(wpieces, kingpos);

            totalscore -= (5 - depth);

            return totalscore;
        }

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