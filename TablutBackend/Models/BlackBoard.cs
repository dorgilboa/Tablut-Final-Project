using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TablutBackend.Models
{
    public class BlackBoard : Board
    {
        public BlackBoard() : base(false)
        {}

        /// <summary>
        /// Checks if the king is surrounded by 4 non-white pieces that aim to capture him.
        /// Works only if one of the pieces around him IS a part of the last move the black player
        /// executed.
        /// </summary>
        /// <param name="kingpos"> The index of the king on board. </param>
        /// <param name="lastMovePos"> The last position the black player moved one of his pieces
        /// into. </param>
        /// <returns></returns>
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

        /// <summary>
        /// Check if the black player has won. gets the king bitset to check if there is no bit on.
        /// </summary>
        /// <returns> True / False if king was captured or not. </returns>
        public override bool Won(BitSet other = null)
        {
            if (other.Equals(new BitSet(0,0)))
                return true;
            return false;
        }

        // merges all boards in order to call base function with correct merged boards that affect
        // the black pieces' moves.
        public override List<Move> GetAllMoves(Board other, bool wturn = false, BitSet merged = null)
        {
            WhiteBoard white = other as WhiteBoard;
            if (white == null)
                return null;
            return base.GetAllMoves(other, false, board | white.board | white.kingboard);
        }

        /// <summary>
        /// The main function for calculating the board's situation for a black player.
        /// This function rely on few factors:
        /// pct. of alive black players * 20 
        /// pct. of captured black players * 16
        /// Avg diff between blacks avg weights * 2 up to 10 score, to white players' weights + king's weight on board. times 3.2.
        /// Amount of ways for a king to the corners * -24 = unwanted result.
        /// pct. of dangerous pieces around king * 32.
        /// pct. of white pieces around him * -8.
        /// depth's impact on score - We'd want to get same score in less depth into the tree.
        /// </summary>
        /// <param name="other"> White Board. </param>
        /// <param name="depth"> Depth of the board's situation in game-tree. </param>
        /// <returns> The total score that has been affected from all these factors above. </returns>
        public override int Hueristic(Board other, int depth)
        {
            WhiteBoard wb = other as WhiteBoard;
            int kingpos = wb.kingboard.GetFirstOn();
            int totalscore = 0;
            List<int> bpieces = base.GetAllPieces();
            List<int> wpieces = other.GetAllPieces();
            //pct. of black players on board * 20 weight.
            double blackpct = bpieces.Count / BLACK_PIECES;
            totalscore += (int)(blackpct * 20);

            //pct. of captured white players * 16 weight.
            double blackcappct = (BLACK_PIECES - bpieces.Count) / BLACK_PIECES;
            totalscore += (int)(blackcappct * 16);

            //evaluate difference between average pieces weights per side * 3.2 weight.
            double weightscore = (CalcTotalWeight(BLACK_WEIGHTS, bpieces.ToArray()) / bpieces.Count) * 2;
            weightscore -= (CalcTotalWeight(WHITE_WEIGHTS, wpieces.ToArray()) / wpieces.Count) * 1.3;
            weightscore -= wb.CalcTotalWeight(KING_WEIGHTS, kingpos) * 0.7;
            totalscore += (int)(weightscore * 3.2);

            //amount of ways for a king to the corners * -24 weight.
            totalscore -= wb.CalcWaysToCorner(board | other.board, kingpos) * 24;

            //danger on king - percentage of danger to king * 32 weight.
            double kingdangerpct = wb.KingDanger(bpieces, kingpos) / PiecesToCaptureKing(kingpos);
            totalscore += (int)(kingdangerpct * 32);

            //guards around king - pct. of whites of all around it * -8 weight.
            double kingsafepct = wb.KingSafe(wpieces, kingpos) / PIECES_AROUND_KING;
            totalscore -= (int)kingsafepct;

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