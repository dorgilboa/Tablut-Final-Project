using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TablutServer
{
    internal class BitSet : ICloneable
    {
        public ulong low { get; set; }
        public ulong high { get; set; }

        public BitSet(ulong low, ulong high)
        {
            this.low = low;
            this.high = high;
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

        public BitSet MaskOn(int pos)
        {
            return (pos > 63) ? new BitSet(0, (ulong)(1 << (pos - 63))) : new BitSet((ulong)(1 << pos), 0);
        }

        public bool IsOn(int pos)
        {
            BitSet mask = MaskOn(pos);
            return (mask == (this & mask));
        }


        public object Clone()
        {
            return new BitSet(low, high);
        }
    }
}
