using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace TablutBackend.Models
{
    public abstract class Board : ICloneable
    {
        private readonly string BLACK_TEMPLATE = "000111000000010000000000000100000001110000011100000001000000000000010000000111000";
        private readonly string WHITE_TEMPLATE = "000000000000000000000010000000010000001101100000010000000010000000000000000000000";
        private readonly string RESTRICTIONS =   "100000001000000000000000000000000000000010000000000000000000000000000000100000001";
        protected readonly int EDGE_SIZE = 9;
        protected readonly int WHITE_PIECES = 8;
        protected readonly int BLACK_PIECES = 16;
        protected readonly int[] RES_PLACES = { 1, 9, 73, 81, CENTER };
        const int CENTER = 41;

        public BitSet board { get; set; }
        public BitSet restrictions { get; set; }

        public Board(bool isWhite)
        {
            restrictions = new BitSet(RESTRICTIONS);
            if (isWhite)
                board = new BitSet(WHITE_TEMPLATE);
            else
                board = new BitSet(BLACK_TEMPLATE);
        }


        public abstract bool Won(BitSet other = null);

        public void Move(int from, int to)
        {
            board &= ~board.MaskOn(from);
            board |= board.MaskOn(to);
        }


        public List<int> SetCaptures(int pos, ref BitSet other, int kingpos, bool wturn)
        {
            BitSet cloned_other = (BitSet)other.Clone();
            List<int> posCaptured = new List<int>();
            //check for king capture if black turn...
            // if king remained in center or near center - must be eaten from all 4 sides...
            if (!wturn)
            {
                BlackBoard bb = this as BlackBoard;
                if (kingpos != CENTER && kingpos != CENTER + 1 && kingpos != CENTER - 1 && kingpos != CENTER + EDGE_SIZE && kingpos != CENTER - EDGE_SIZE)
                    cloned_other |= cloned_other.MaskOn(kingpos);
                if (bb.KingEaten(kingpos, pos))
                {
                    cloned_other &= ~cloned_other.MaskOn(kingpos);
                    posCaptured.Add(kingpos);
                    return posCaptured;
                }
            }
                    
            if (cloned_other.IsOn(CENTER) || kingpos == CENTER)
                restrictions &= ~other.MaskOn(CENTER);
            //from up: not in first two lines.
            if (pos / EDGE_SIZE > 1)
            {
                int up = pos - EDGE_SIZE;
                if (cloned_other.IsOn(up))
                {
                    up -= EDGE_SIZE;
                    if (board.IsOn(up) || restrictions.IsOn(up))
                    {
                        cloned_other &= ~board.MaskOn(pos - EDGE_SIZE);
                        posCaptured.Add(pos - EDGE_SIZE);
                    }
                }
            }
            //from down: not in last two lines.
            if (pos / EDGE_SIZE < 8)
            {
                int down = pos + EDGE_SIZE;
                if (cloned_other.IsOn(down))
                {
                    down += EDGE_SIZE;
                    if (board.IsOn(down) || restrictions.IsOn(down))
                    {
                        cloned_other &= ~board.MaskOn(pos + EDGE_SIZE);
                        posCaptured.Add(pos + EDGE_SIZE);
                    }
                }
            }
            //from left: not in first two cols.
            if (pos % EDGE_SIZE > 1)
            {
                int left = pos - 1;
                if (cloned_other.IsOn(left))
                {
                    left--;
                    if (board.IsOn(left) || restrictions.IsOn(left))
                    {
                        cloned_other &= ~board.MaskOn(pos - 1);
                        posCaptured.Add(pos - 1);
                    }
                }
            }
            //from right: not in last two cols.
            if (pos % EDGE_SIZE < 8)
            {
                int right = pos + 1;
                if (cloned_other.IsOn(right))
                {
                    right++;
                    if (board.IsOn(right) || restrictions.IsOn(right))
                    {
                        cloned_other &= ~board.MaskOn(pos + 1);
                        posCaptured.Add(pos + 1);
                    }
                }
            }
            if (cloned_other.IsOn(CENTER))
                restrictions &= cloned_other.MaskOn(CENTER);
            if (!wturn)
                cloned_other &= ~cloned_other.MaskOn(kingpos);

            other = cloned_other;
            return posCaptured;
        }

        public abstract int Hueristic(Board other, int depth);

        public virtual List<Move> GetAllMoves(Board other, bool wturn = false, BitSet merged = null)
        {
            List<Move> moves = new List<Move>();
            List<int> pieces = GetAllPieces();
            foreach (int piece in pieces)
            {
                int curr = piece;
                //check up
                curr -= EDGE_SIZE;
                while (curr > 0 && !merged.IsOn(curr))
                {
                    moves.Add(new Move(piece, curr, wturn));
                    curr -= EDGE_SIZE;
                }
                curr = piece;
                //check down
                curr += EDGE_SIZE;
                while (curr < 82 && !merged.IsOn(curr))
                {
                    moves.Add(new Move(piece, curr, wturn));
                    curr += EDGE_SIZE;
                }
                curr = piece;
                //check left
                curr--;
                while (curr %EDGE_SIZE!= 0 && !merged.IsOn(curr))
                {
                    moves.Add(new Move(piece, curr, wturn));
                    curr --;
                }
                curr = piece;
                //check right
                curr++;
                while ((curr - 1) %EDGE_SIZE!= 0 && !merged.IsOn(curr))
                {
                    moves.Add(new Move(piece, curr, wturn));
                    curr++;
                }
            }
            return moves;
        }

        public virtual List<int> GetAllPieces()
        {
            List<int> pieces = new List<int>();
            for (int i = 1; i <= EDGE_SIZE*EDGE_SIZE; i++)
            {
                if (board.IsOn(i))
                    pieces.Add(i);
            }
            return pieces;
        }

        public int CalcTotalWeight(int[] weight_arr, params int[] pieces)
        {
            int total = 0;
            foreach (int piece in pieces)
                total += weight_arr[piece - 1];
            return total;
        }

        public override string ToString()
        {
            return board.ToString();
        }
        
        public abstract object Clone();
    }
}