using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TablutServer.BL
{
    internal class Board
    {
        private readonly ulong[] WHITEMASK = { 0, 0x040207C080400000 }; // approved
        private readonly ulong[] BLACKMASK = { 0x1C04, 0x20383808002038 }; // approved
        private readonly ulong[] NORMALMASK = { 0x10100, 0x0000010000000101 }; // approved
        private readonly ulong[] KINGMASK;

        private readonly int EDGE_SIZE = 9;

        private BitSet board;
        private BitSet restrictions;

        public List<int> SetCaptures(int pos, BitSet other)
        {
            BitSet cloned_other = (BitSet)other.Clone();
            List<int> posCaptured = new List<int>();
            //from up: not in first two lines.
            if (pos / EDGE_SIZE > 1)
            {
                int up = pos - EDGE_SIZE;
                if (other.IsOn(up))
                {
                    up -= EDGE_SIZE;
                    if (board.IsOn(up) || restrictions.IsOn(up))
                    {
                        cloned_other &= board.MaskOn(pos - EDGE_SIZE);
                        posCaptured.Add(pos - EDGE_SIZE);
                    }
                }
            }
            //from down: not in last two lines.
            if (pos / EDGE_SIZE < 8)
            {
                int down = pos + EDGE_SIZE;
                if (other.IsOn(down))
                {
                    down -= EDGE_SIZE;
                    if (board.IsOn(down) || restrictions.IsOn(down))
                    {
                        cloned_other &= board.MaskOn(pos + EDGE_SIZE);
                        posCaptured.Add(pos + EDGE_SIZE);
                    }
                }
            }
            //from left: not in first two cols.
            if (pos % EDGE_SIZE > 1)
            {
                int left = pos - 1;
                if (other.IsOn(left))
                {
                    left--;
                    if (board.IsOn(left) || restrictions.IsOn(left))
                    {
                        cloned_other &= board.MaskOn(pos - 1);
                        posCaptured.Add(pos - 1);
                    }
                }
            }
            //from right: not in last two cols.
            if (pos % EDGE_SIZE < 8)
            {
                int right = pos + 1;
                if (other.IsOn(right))
                {
                    right++;
                    if (board.IsOn(right) || restrictions.IsOn(right))
                    {
                        cloned_other &= board.MaskOn(pos + 1);
                        posCaptured.Add(pos + 1);
                    }
                }
            }
            other = cloned_other;
            return posCaptured;
        }
    }
}
