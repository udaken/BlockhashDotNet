using System;
using System.Collections;
using static System.MathF;

#if !NETCOREAPP2_0 && !NETCOREAPP2_1 && !NETCOREAPP2_2 && !NETCOREAPP3_0
namespace System
{
    internal static class MathF
    {
        public static float Abs(float x)
        {
            return Math.Abs(x);
        }
        public static float Truncate(float x)
        {
            return (float)Math.Truncate(x);
        }
        public static float Ceiling(float x)
        {
            return (float)Math.Ceiling(x);
        }
        public static float Floor(float x)
        {
            return (float)Math.Floor(x);
        }
    }
}
#endif

namespace BlockhashDotNet
{
    public enum Method
    {
        Normal = 1,
        Quick = 2,
    }
    public static class Blockhash
    {
        public static string ToHashString(this BitArray bits)
        {
            var length = (bits.Length - 1) / 8 + 1;
            var result = new System.Text.StringBuilder(length);
            for (var i = 0; i < length; i++)
            {
                var index = i * 8;
                result.AppendFormat("{0:x2}", (byte)(
                    (bits[index + 0] ? 0b10000000 : 0b0) |
                    (bits[index + 1] ? 0b1000000 : 0b0) |
                    (bits[index + 2] ? 0b100000 : 0b0) |
                    (bits[index + 3] ? 0b10000 : 0b0) |
                    (bits[index + 4] ? 0b1000 : 0b0) |
                    (bits[index + 5] ? 0b100 : 0b0) |
                    (bits[index + 6] ? 0b10 : 0b0) |
                    (bits[index + 7] ? 0b1 : 0b0)));
            }
            return result.ToString();
        }
        public static byte[] ToHashBytes(this BitArray bits)
        {
            var length = (bits.Length - 1) / 8 + 1;
            byte[] result = new byte[length];
            for (var i = 0; i < result.Length; i++)
            {
                var index = i * 8;
                result[i] = (byte)(
                    (bits[index + 0] ? 0b10000000 : 0b0) |
                    (bits[index + 1] ? 0b1000000 : 0b0) |
                    (bits[index + 2] ? 0b100000 : 0b0) |
                    (bits[index + 3] ? 0b10000 : 0b0) |
                    (bits[index + 4] ? 0b1000 : 0b0) |
                    (bits[index + 5] ? 0b100 : 0b0) |
                    (bits[index + 6] ? 0b10 : 0b0) |
                    (bits[index + 7] ? 0b1 : 0b0));
            }
            return result;
        }
        public static int HammingDistance(ReadOnlySpan<byte> hash1, ReadOnlySpan<byte> hash2)
        {
            var d = 0;
            if (hash1.Length != hash2.Length)
            {
                throw new ArgumentException("Can't compare hashes with different length");
            }

            for (var i = 0; i < hash1.Length; i++)
            {
                d += popcnt((byte)(hash1[i] ^ hash2[i]));
            }
            return d;
        }
        public static int HammingDistance(BitArray hash1, BitArray hash2)
        {
            var d = 0;

            var bits = hash1.Xor(hash2);
            for (var i = 0; i < bits.Length; i++)
            {
                d += bits[i] ? 1 : 0;
            }

            return d;
        }
        static float modff(float x, out float intptr)
        {
            intptr = Truncate(x);
            return x % 1f;
        }
        static (T, T) Twice<T>(T value) => (value, value);

        internal static int popcnt(byte value)
        {
            uint bits = value;
            bits = (bits & 0x55u) + (bits >> 1 & 0x55u);
            bits = (bits & 0x33u) + (bits >> 2 & 0x33u);
            bits = (bits & 0x0fu) + (bits >> 4 & 0x0fu);
            return (int)bits;
        }

        internal static float median(ReadOnlySpan<float> data)
        {
            Span<float> sorted = data.Length <= 128 ? stackalloc float[data.Length] : new float[data.Length];
            data.CopyTo(sorted);
            sorted.Sort();

            return (data.Length % 2 == 0) ?
                (sorted[data.Length / 2 - 1] + sorted[data.Length / 2]) / 2 :
                sorted[data.Length / 2];
        }

        internal static int median(ReadOnlySpan<int> data)
        {
            Span<int> sorted = data.Length <= 128 ? stackalloc int[data.Length] : new int[data.Length];
            data.CopyTo(sorted);
            sorted.Sort();

            return (data.Length % 2 == 0) ?
                (sorted[data.Length / 2 - 1] + sorted[data.Length / 2]) / 2 :
                sorted[data.Length / 2];
        }

        static BitArray translate_blocks_to_bits(ReadOnlySpan<int> blocks, int nblocks, int pixels_per_block)
        {
            float half_block_value = pixels_per_block * 256 * 3 / 2;
            int bandsize = nblocks / 4;
            var result = new BitArray(blocks.Length);

            for (int i = 0; i < 4; i++)
            {
                int m = median(blocks.Slice(i * bandsize, bandsize));
                for (int j = i * bandsize; j < (i + 1) * bandsize; j++)
                {
                    int v = blocks[j];
                    result[j] = v > m || (Abs(v - m) < 1 && m > half_block_value);
                }
            }
            return result;
        }

        static BitArray translate_blocks_to_bits(ReadOnlySpan<float> blocks, int nblocks, float pixels_per_block)
        {
            float half_block_value = pixels_per_block * 256 * 3 / 2;
            int bandsize = nblocks / 4;
            var result = new BitArray(blocks.Length);

            for (int i = 0; i < 4; i++)
            {
                float m = median(blocks.Slice(i * bandsize, bandsize));
                for (int j = i * bandsize; j < (i + 1) * bandsize; j++)
                {
                    float v = blocks[j];
                    result[j] = v > m || (Abs(v - m) < 1 && m > half_block_value);
                }
            }
            return result;
        }

        public static BitArray CalcQuick(int bits, ReadOnlySpan<byte> data, int width, int height)
        {
            if (bits <= 0)
                throw new ArgumentOutOfRangeException(nameof(bits));

            if (data.IsEmpty)
                throw new ArgumentNullException(nameof(data));

            if (width <= 0)
                throw new ArgumentNullException(nameof(width));

            if (width <= 0)
                throw new ArgumentNullException(nameof(height));

            if (data.Length != width * height * 4)
                throw new ArgumentException();

            return blockhash_quick(bits, data, width, height);
        }
        internal static BitArray blockhash_quick(int bits, ReadOnlySpan<byte> data, int width, int height)
        {
            int block_width = width / bits;
            int block_height = height / bits;

            int[] blocks = new int[bits * bits];
            for (int y = 0; y < bits; y++)
            {
                for (int x = 0; x < bits; x++)
                {
                    int value = 0;

                    for (int iy = 0; iy < block_height; iy++)
                    {
                        for (int ix = 0; ix < block_width; ix++)
                        {
                            int ii = ((y * block_height + iy) * width + (x * block_width + ix)) * 4;

                            var alpha = data[ii + 3];
                            value += (alpha == 0) ? 255 * 3 : data[ii] + data[ii + 1] + data[ii + 2];
                        }
                    }

                    blocks[y * bits + x] = value;
                }
            }

            return translate_blocks_to_bits(blocks, bits * bits, block_width * block_height);
        }
        public static BitArray Calc(int bits, ReadOnlySpan<byte> data, int width, int height, Method method = Method.Normal)
        {
            if (bits <= 0)
                throw new ArgumentOutOfRangeException(nameof(bits));

            if (data.IsEmpty)
                throw new ArgumentNullException(nameof(data));

            if (width <= 0)
                throw new ArgumentNullException(nameof(width));

            if (width <= 0)
                throw new ArgumentNullException(nameof(height));

            // Must ARGB
            if (data.Length != width * height * 4)
                throw new ArgumentException();

            return blockhash(bits, data, width, height, method);
        }
        internal static BitArray blockhash(int bits, ReadOnlySpan<byte> data, int width, int height, Method method)
        {
            if (width % bits == 0 && height % bits == 0 || method == Method.Quick)
            {
                return blockhash_quick(bits, data, width, height);
            }

            float block_width = (float)width / (float)bits;
            float block_height = (float)height / (float)bits;

            float[] blocks = new float[bits * bits];

            for (int y = 0; y < height; y++)
            {
                float y_mod = (y + 1) % block_height;
                float y_frac = modff(y_mod, out var y_int);

                float weight_top = (1 - y_frac);
                float weight_bottom = y_frac;

                // y_int will be 0 on bottom/right borders and on block boundaries
                var (block_top, block_bottom) = (y_int > 0 || (y + 1) == height) ?
                    Twice((int)Floor(y / block_height)) :
                    ((int)Floor(y / block_height), (int)Ceiling(y / block_height));

                for (int x = 0; x < width; x++)
                {
                    float x_mod = (x + 1) % block_width;
                    float x_frac = modff(x_mod, out var x_int);

                    float weight_left = (1 - x_frac);
                    float weight_right = x_frac;

                    // x_int will be 0 on bottom/right borders and on block boundaries
                    var (block_left, block_right) = (x_int > 0 || (x + 1) == width) ?
                        Twice((int)Floor(x / block_width)) :
                        ((int)Floor(x / block_width), (int)Ceiling(x / block_width));

                    int ii = (y * width + x) * 4;

                    var alpha = data[ii + 3];
                    float value = (alpha == 0) ? 255 * 3 : data[ii] + data[ii + 1] + data[ii + 2];

                    // add weighted pixel value to relevant blocks
                    blocks[block_top * bits + block_left] += value * weight_top * weight_left;
                    blocks[block_top * bits + block_right] += value * weight_top * weight_right;
                    blocks[block_bottom * bits + block_left] += value * weight_bottom * weight_left;
                    blocks[block_bottom * bits + block_right] += value * weight_bottom * weight_right;
                }
            }

            return translate_blocks_to_bits(blocks, bits * bits, (block_width * block_height));
        }
    }
}
