using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TablutServer
{
    internal class BitSet
    {
        private ulong low;
        private ulong high;

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
        /// <returns>'And' result on bs1 and bs2.</returns>
        public static BitSet operator <<(BitSet bs, int count)
        {
            BitSet res = new BitSet(bs.low, bs.high);
            for (int i = 0; i < count; i++)
            {
                if ((bs.low & ((ulong)1 << 31)) != 0)
                    res.high |= 1;
                res.low = bs.low << count;
                res.high <<= 1;
            }
            return res;
        }

        /// <summary>
        /// Bitwise 'shift-left' (<<) implementation on two-part bitset 
        /// varriable, works first on the low part then on high.
        /// </summary>
        /// <param name="bs">The Bitset-type operand.</param>
        /// <param name="count">The amount of shifts that are executed.</param>
        /// <returns>'And' result on bs1 and bs2.</returns>
        public static BitSet operator >>(BitSet bs, int count)
        {
            BitSet res = new BitSet(bs.low, bs.high);
            for (int i = 0; i < count; i++)
            {
                if ((bs.low & ((ulong)1 << 31)) != 0)
                    res.high |= 1;
                res.low = bs.low << count;
                res.high <<= 1;
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
    }
}
