using System;
using System.Collections;

namespace BlockhashDotNet
{
    public struct BitSet
    {
        private const int bitsOfElem = 1 * 8;
        private readonly byte[] bits;
        public BitSet(int capacity)
        {
            bits = (capacity % bitsOfElem != 0 || capacity < 0) ? throw new ArgumentOutOfRangeException(nameof(capacity)) : new byte[capacity / bitsOfElem];
        }
        public BitSet(params byte[] bits)
        {
            this.bits = bits;
        }
        public int Length => bits.Length * bitsOfElem;
        public bool this[int i]
        {
            get => (bits[i / bitsOfElem] >> i % bitsOfElem) != 0;
            set
            {
                var bitpos = (bitsOfElem - 1) - (i % bitsOfElem);
                var offset = i / bitsOfElem;
                bits[offset] = (byte)((bits[offset] & ~(1 << bitpos)) | ((value ? 1 : 0) << bitpos));
            }
        }

        public ReadOnlySpan<byte> Bits => bits;
        public override string ToString() => BitConverter.ToString(bits);
        public BitSet Xor(BitSet other)
        {
            var a = bits.Clone() as byte[];
            for (var i = 0; i < a.Length; i++)
            {
                a[i] ^= other.bits[i];
            }
            return new BitSet(a);
        }
        public string ToHashString()
        {
            var result = new System.Text.StringBuilder(bits.Length * 2);
            for (var i = 0; i < bits.Length; i++)
            {
                result.AppendFormat("{0:x2}", bits[i]);
            }

            return result.ToString();
        }
        public byte[] ToHashBytes()
        {
            return bits.Clone() as byte[];
        }
    }
}
