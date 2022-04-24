using System;
using System.Collections.Generic;

namespace TablutBackend.Models
{
    public enum TTType { UP, LOW, EXACT }
    public class Move
    {
        public int from { get; set; }
        public int to { get; set; }
        public int score { get; set; }

        // wturn => is white turn.
        public bool wturn { get; set; }

        // TT = Transposition Table.
        public TTType type { get; set; }

        public Move(int from, int to, bool wturn, TTType type = TTType.EXACT)
        {
            this.from = from;
            this.to = to;
            this.wturn = wturn;
            this.type = type;
        }
    }
}