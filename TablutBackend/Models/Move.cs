using System;
using System.Collections.Generic;

namespace TablutBackend.Models
{
    public enum TTType { UP, LOW, EXACT }
    public class Move
    {
        public int from { get; set; }
        public int to { get; set; }
        //public Board w_board { get; set; }
        //public Board b_board { get; set; }
        public int score { get; set; }
        public bool wturn { get; set; }

        public TTType type { get; set; }

        //public List<Move> moves { get; set; }

        public Move(int from, int to, bool wturn, TTType type = TTType.EXACT)
        {
            this.from = from;
            this.to = to;
            this.wturn = wturn;
            this.type = type;
            //moves = new List<Move>();
        }
    }
}