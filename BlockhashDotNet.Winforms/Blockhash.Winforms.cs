using System;
using System.Drawing;
using System.Drawing.Imaging;
using BlockhashDotNet;

namespace BlockhashDotNet.Winforms
{
    public static class Blockhash
    {
        public static System.Collections.BitArray Calc(int bits, Bitmap bitmap, Method method = Method.Normal)
        {
            var data = default(BitmapData);
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
