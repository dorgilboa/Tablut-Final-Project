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
        // Restrictions indexes positions on board:
        protected readonly int[] RES_PLACES = { 1, 9, 73, 81, CENTER };
        const int CENTER = 41;
        protected const int PIECES_AROUND_KING = 4;
        // Weights for white pieces on board.
        protected readonly int[] WHITE_WEIGHTS = { 2,2,2,0,0,0,2,2,2,
                                                  2,1,1,4,0,4,1,1,2,
                                                  2,1,2,2,2,2,2,1,2,
                                                  0,2,4,1,3,1,4,2,0,
                                                  0,0,2,3,0,3,2,0,0,
                                                  0,2,4,1,3,1,4,2,0,
                                                  2,1,2,2,2,2,2,1,2,
                                                  2,1,1,4,0,4,1,1,2,
                                                  2,2,2,0,0,0,2,2,2};
        // Weights for king piece's positions on board.
        protected readonly int[] KING_WEIGHTS = { 5,1,3,3,3,3,3,1,5,
                                                  1,1,1,2,0,2,1,1,1,
                                                  3,1,2,1,1,1,2,1,3,
                                                  3,2,1,1,3,1,1,2,3,
                                                  3,0,1,3,3,3,1,0,3,
                                                  3,2,1,1,3,1,1,2,3,
                                                  3,1,2,1,1,1,2,1,3,
                                                  1,1,1,2,0,2,1,1,1,
                                                  5,1,3,3,3,3,3,1,5};
        // Weights for black pieces on board.
        protected readonly int[] BLACK_WEIGHTS = {    0,3,2,3,3,3,2,3,0,
                                                    3,2,5,1,3,1,5,2,3,
                                                    2,5,1,5,2,5,1,5,2,
                                                    3,1,5,1,2,1,5,1,3,
                                                    3,3,2,2,1,2,2,3,3,
                                                    3,1,5,1,2,1,5,1,3,
                                                    2,5,1,5,2,5,1,5,2,
                                                    3,2,5,1,3,1,5,2,3,
                                                    0,3,2,3,3,3,2,3,0 };

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

        /// <summary>
        /// Makes a required move from source piece to given desination.
        /// Changes the board bit-board according to the move.
        /// </summary>
        public void Move(int from, int to)
        {
            board &= ~board.MaskOn(from);
            board |= board.MaskOn(to);
        }

        /// <summary>
        /// Takes care of changing opponent's board according to the captures that were caused
        /// by moving a player's piece to 'pos' - a given index (position) on board. If black
        /// moved -> check for king capturing and then normal check, else -> check for normal capturing.
        /// search for captures from all 4 sides of the position the player moved into.
        /// </summary>
        /// <param name="pos"> position the player moved into. </param>
        /// <param name="other"> The other bitset that resmbles the opponent's board. </param>
        /// <param name="kingpos"> The position of the king. </param>
        /// <param name="wturn"> If it's a white / black turn. </param>
        /// <returns> A list of all the positions on board that captures were occured in.
        /// also updates in accordance the other bitset. </returns>
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

        /// <summary>
        /// if given king position is on or near center of the board - 4 pieces are needed
        /// to capture him. else -> only 2.
        /// </summary>
        public int PiecesToCaptureKing(int kingpos)
        {
            return kingpos == CENTER || kingpos == CENTER - 1 || kingpos == CENTER + 1 ||
                kingpos == CENTER - EDGE_SIZE || kingpos == CENTER + EDGE_SIZE ? 4 : 2;
        }

        /// <summary>
        /// Extraction of all moves a player can have. The function adds for every piece of the same
        /// color all of the possible moves it can do. for every move it appends a new move object to
        /// a moves list with no score or block-type for the TT (transposition tbl.).
        /// </summary>
        /// <param name="other"> The opponent's board object. </param>
        /// <param name="wturn"> If it's a white / black turn. </param>
        /// <param name="merged"> merged bit board of white and black pieces that might block each checked
        /// piece on his paths. </param>
        /// <returns> A list of all the possible move objects can be extracted from the board's situation
        /// for a given player. </returns>
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

        /// <summary>
        /// Adds all the on-setted bits inside the board bitset object, to a int-list that contains
        /// the indexes of all the pieces of unspecified 'team'.
        /// </summary>
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

        /// <summary>
        /// for every index of piece on board it gets its weight according to his position
        /// and sums all of them up.
        /// </summary>
        /// <param name="weight_arr"> The array that has weights for every position on board 
        /// for a single type of player. </param>
        /// <param name="pieces"> The list of indieces of pieces from the same team on board. </param>
        /// <returns> The sum of all the pieces' weights from the same team. </returns>
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