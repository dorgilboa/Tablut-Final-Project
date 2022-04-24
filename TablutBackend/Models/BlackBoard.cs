using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TablutBackend.Models
{
    public class BlackBoard : Board
    {
        private readonly int[] BLACK_WEIGHTS = {    1,3,2,3,3,3,2,3,1,
                                                    3,2,5,1,3,1,5,2,3,
                                                    2,5,1,5,2,5,1,5,2,
                                                    3,1,5,1,2,1,5,1,3,
                                                    3,3,2,2,1,2,2,3,3,
                                                    3,1,5,1,2,1,5,1,3,
                                                    2,5,1,5,2,5,1,5,2,
                                                    3,2,5,1,3,1,5,2,3,
                                                    1,3,2,3,3,3,2,3,1 };
        public BlackBoard() : base(false)
        {}

        public bool KingEaten(int kingpos, int lastMovePos)
        {
            // if king remained in center or near center - must be eaten from all 4 sides...
            if (kingpos == 41 || kingpos == 42 || kingpos == 40 || kingpos == 32 || kingpos == 50)
                if (lastMovePos == kingpos - 1 || lastMovePos == kingpos + 1 || lastMovePos == kingpos - EDGE_SIZE || lastMovePos == kingpos + EDGE_SIZE)
                    return ((board.IsOn(kingpos-1) || restrictions.IsOn(kingpos-1)) && (board.IsOn(kingpos+1)
                    || restrictions.IsOn(kingpos + 1)) && (board.IsOn(kingpos-EDGE_SIZE) || 
                    restrictions.IsOn(kingpos - EDGE_SIZE)) && (board.IsOn(kingpos+EDGE_SIZE) || restrictions.IsOn(kingpos +EDGE_SIZE)));
            return false;
        }

        public override bool Won(BitSet other = null)
        {
            if (other.Equals(new BitSet(0,0)))
                return true;
            return false;
        }

        public override List<Move> GetAllMoves(Board other, bool wturn = false, BitSet merged = null)
        {
            WhiteBoard white = other as WhiteBoard;
            if (white == null)
                return null;
            return base.GetAllMoves(other, false, board | white.board | white.kingboard);
        }

        public override int Hueristic(Board other, int depth)
        {
            WhiteBoard wb = other as WhiteBoard;
            int kingpos = wb.kingboard.GetFirstOn();
            int totalscore = 0;
            List<int> bpieces = base.GetAllPieces();
            List<int> wpieces = other.GetAllPieces();
            //amount of captured white players on board * 2.
            totalscore += (WHITE_PIECES - wpieces.Count) * 2;

            //amount of black players on board * 2.
            totalscore += bpieces.Count * 2;

            //add calc by areas...
            totalscore += CalcTotalWeight(BLACK_WEIGHTS, bpieces.ToArray()) / bpieces.Count * 2;

            //minus amount of ways for a king to the corners * 16.
            totalscore -= wb.CalcWaysToCorner(board | other.board, kingpos) * 16;

            //danger on king - amount of blacks & res. around him * 8.
            totalscore += wb.KingDanger(bpieces, kingpos) * 8;

            //guards around king - amount of whites * 1.
            totalscore -= wb.KingSafe(wpieces, kingpos);

            totalscore -= (5 - depth);

            return totalscore;
        }

        public override object Clone()
        {
            BlackBoard copy = new BlackBoard();
            copy.board = (BitSet)board.Clone();
            return copy;
        }
    }
}