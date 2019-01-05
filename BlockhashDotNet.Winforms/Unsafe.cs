using System;

namespace BlockhashDotNet
{
    internal static unsafe class Unsafe
    {
        public static Span<byte> ToSpan(this IntPtr ptr, int length)
        {
            return new Span<byte>(ptr.ToPointer(), length);
        }
        public static ReadOnlySpan<byte> ToReadOnlySpan(this IntPtr ptr, int length)
        {
            return new ReadOnlySpan<byte>(ptr.ToPointer(), length);
        }
    }
}