using System;
//using BitArray = System.Collections.BitArray;
using BitArray = BlockhashDotNet.BitSet;
using System.Drawing;
using System.Drawing.Imaging;

namespace BlockhashDotNet.Winforms
{
    public static class Blockhash
    {
        public static BitArray Calc(int bits, Bitmap bitmap, Method method = Method.Normal)
        {
            BitmapData data = default;
            try
            {
                data = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);

                var result = BlockhashDotNet.Blockhash.Calc(bits, data.Scan0.ToReadOnlySpan(bitmap.Width * bitmap.Height * 4), bitmap.Width, bitmap.Height, method);
                return result;
            }
            finally
            {
                if (data != null)
                    bitmap.UnlockBits(data);
            }

        }

    }
}
