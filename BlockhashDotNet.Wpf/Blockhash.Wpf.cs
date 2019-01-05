using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

using System.Windows.Media;
using System.Windows.Media.Imaging;

using BlockhashDotNet;

namespace Blockhash.Wpf
{
    public partial class Blockhash
    {
        public static System.Collections.BitArray Calc(int bits, BitmapSource bitmapSource)
        {
            if (bitmapSource is null)
                throw new ArgumentNullException(nameof(bitmapSource));
            var bitmap = new FormatConvertedBitmap(bitmapSource, PixelFormats.Pbgra32, null, 1);

            var buf = new byte[bitmap.PixelWidth * 4 * bitmap.PixelHeight];
            bitmap.CopyPixels(buf, bitmap.PixelWidth * 4, 0);

            var result = BlockhashDotNet.Blockhash.Calc(bits, buf, bitmap.PixelWidth, bitmap.PixelHeight);
            return result;
        }
        public static System.Collections.BitArray Calc(int bits, WriteableBitmap bitmap)
        {
            if (bitmap is null)
                throw new ArgumentNullException(nameof(bitmap));

            try
            {
                bitmap.Lock();
                var result = BlockhashDotNet.Blockhash.Calc(bits, bitmap.BackBuffer.ToReadOnlySpan(bitmap.BackBufferStride * bitmap.PixelHeight), bitmap.PixelWidth, bitmap.PixelHeight);
                return result;
            }
            finally
            {

                bitmap.Unlock();
            }
        }
    }
}
