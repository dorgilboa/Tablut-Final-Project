using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TablutBackend.Models
{
    public class BitSet : ICloneable
    {
        public ulong low { get; set; }
        public ulong high { get; set; }

        public BitSet(ulong low, ulong high)
        {
            this.low = low;
            this.high = high;
        }

        public BitSet()
        {
            this.low = 0;
            this.high = 0;
        }

        public BitSet(string bit_expr)
        {
            BitSet bs = new BitSet(0, 0);
            for (int i = 0; i < bit_expr.Length; i++)
            {
                bs <<= 1;
                if (bit_expr[i] == '1')
                    bs |= MaskOn(1);
            }
            this.low = bs.low;
            this.high = bs.high;
        }

        /// <summary>
        /// Bitwise 'shift-left' (<<) implementation on two-part bitset 
        /// varriable, works first on the low part then on high.
        /// </summary>
        /// <param name="bs">The Bitset-type operand.</param>
        /// <param name="count">The amount of shifts that are executed.</param>
        /// <returns>'Shift left' result on bs1 and bs2.</returns>
        public static BitSet operator <<(BitSet bs, int count)
        {
            BitSet res = new BitSet(bs.low, bs.high);
            for (int i = 0; i < count; i++)
            {
                res.high <<= 1;
                if ((res.low & ((ulong)1 << 63)) != 0)
                    res.high |= 1;
                res.low <<= 1;
            }
            return res;
        }

        /// <summary>
        /// Bitwise 'shift-right' (>>) implementation on two-part bitset 
        /// varriable, works first on the low part then on high.
        /// </summary>
        /// <param name="bs">The Bitset-type operand.</param>
        /// <param name="count">The amount of shifts that are executed.</param>
        /// <returns>'Shift-right' result on bs1 and bs2.</returns>
        public static BitSet operator >>(BitSet bs, int count)
        {
            BitSet res = new BitSet(bs.low, bs.high);
            for (int i = 0; i < count; i++)
            {
                res.low >>= 1;
                if ((res.high & (ulong)1) != 0)
                    res.low |= ((ulong)1 << 63);
                res.high >>= 1;
            }
            return res;
        }


        /// <summary>
        /// Bitwise 'and' (&) implementation on two-part bitset varriable,
        /// works first on the low part then on high.
        /// </summary>
        /// <param name="bs1">The first Bitset-type operand.</param>
        /// <param name="bs2">The second Bitset-type operand.</param>
        /// <returns>'And' result on bs1 and bs2.</returns>
        public static BitSet operator &(BitSet bs1, BitSet bs2)
        {
            BitSet res = new BitSet(bs1.low, bs1.high);
            res.low &= bs2.low;
            res.high &= bs2.high;
            return res;
        }

        /// <summary>
        /// Bitwise 'or' (|) implementation on two-part bitset varriable,
        /// works first on the low part then on high.
        /// </summary>
        /// <param name="bs1">The first Bitset-type operand.</param>
        /// <param name="bs2">The second Bitset-type operand.</param>
        /// <returns>'Or' result on bs1 and bs2.</returns>
        public static BitSet operator |(BitSet bs1, BitSet bs2)
        {
            BitSet res = new BitSet(bs1.low, bs1.high);
            res.low |= bs2.low;
            res.high |= bs2.high;
            return res;
        }

        /// <summary>
        /// Bitwise 'xor' (Exclusive OR = ^) implementation on two-part 
        /// bitset varriable, works first on the low part then on high.
        /// </summary>
        /// <param name="bs1">The first Bitset-type operand.</param>
        /// <param name="bs2">The second Bitset-type operand.</param>
        /// <returns>'Xor' result on bs1 and bs2.</returns>
        public static BitSet operator ^(BitSet bs1, BitSet bs2)
        {
            BitSet res = new BitSet(bs1.low, bs1.high);
            res.low ^= bs2.low;
            res.high ^= bs2.high;
            return res;
        }

        /// <summary>
        /// Bitwise 'complement' (NOT = ~) implementation on two-part bitset
        /// varriable, works first on the low part then on high.
        /// </summary>
        /// <param name="bs1">The Bitset-type operand.</param>
        /// <returns>Complementation on bs varriable (Bitwise NOT).</returns>
        public static BitSet operator ~(BitSet bs)
        {
            BitSet res = new BitSet(bs.low, bs.high);
            res.low = ~res.low;
            res.high = ~res.high;
            return res;
        }

        /// <summary>
        /// Creates and returns a new bitset mask with only one bit set on,
        /// according to the function's parameter.
        /// </summary>
        /// <param name="pos">The position on bitset board to turn on the bit.</param>
        /// <returns>a new bitset mask with only one bit set on</returns>
        public BitSet MaskOn(int pos)
        {
            if (pos != 0)
                return new BitSet(1, 0) << (pos-1);
            return new BitSet(0, 0);
        }

        /// <summary>
        /// Sets mask with only one bit on by calling MaskOn(pos), and checks if
        /// there is a bit on in given position.
        /// </summary>
        /// <param name="pos">The position of a bit in bitset board to check if it's on.</param>
        /// <returns>true if the bit in the given position is on, otherwise false</returns>
        public bool IsOn(int pos)
        {
            BitSet mask = MaskOn(pos);
            return (mask.Equals(this & mask));
        }


        public bool Equals(BitSet other)
        {
            return this.low == other.low && this.high == other.high;
        }


        /// <summary>
        /// Gets the position of the first bit that sets on in the board.
        /// </summary>
        /// <returns>position of the first bit that sets on in the board.</returns>
        public int GetFirstOn()
        {
            BitSet cloned = (BitSet)this.Clone();
            int pos = 0;
            while (pos <= 81)
                if (!(cloned & MaskOn(++pos)).Equals(MaskOn(0)))
                    return pos;
            return 0;
        }


        public object Clone()
        {
            return new BitSet(low, high);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// shows the board in debug.output window.
        /// </summary>
        public void Print()
        {
            for (int i = 1; i < 82; i++)
            {
                if (IsOn(i))
                    System.Diagnostics.Debug.Write('1');
                else
                    System.Diagnostics.Debug.Write('0');
                if (i % 9 == 0)
                    System.Diagnostics.Debug.WriteLine("");
            }
            System.Diagnostics.Debug.WriteLine("");
        }
    }
}