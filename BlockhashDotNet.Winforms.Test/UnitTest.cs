using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using BlockhashDotNet;
using BlockhashDotNet.Winforms;

namespace BlockhashDotNet.Winforms.Test
{
    [TestClass]
    public class UnitTest
    {
        static void TestImpl(int bits, Method method, string expect, string filename)
        {
            var filenames = new[] { "puffy_white.png", "00133601.jpg", "clipper_ship.jpg", "00136101.jpg", "32499201.jpg", "00094701.jpg", "stoplights.jpg", "00011601.jpg", "01109801.jpg", "00002701.jpg", "Babylonian.png", "06855701.jpg", "00106901.jpg", "24442301.jpg", "00631801.jpg", "00631701.jpg", "emptyBasket.png" };
            //var filenames = new[] { "Babylonian.png",};

            var datadir = "testdata/";
            using (var bitmap = (Bitmap)Image.FromFile(datadir + filename))
            {
                var result = Blockhash.Calc(bits, bitmap, method);
                //var hash = BitConverter.ToString(result.ToHashBytes());
                var hash = result.ToHashString();

                Console.WriteLine($"{hash}  {filename}");
                Assert.AreEqual(expect, hash);
            }
        }
        [TestMethod]
        public void Test16BitNormal()
        {
            TestImpl(16, Method.Normal, "00000000fffffffff803f807f807f80ff90ff90fb90c9980ffffffff00000000", "puffy_white.png");
            TestImpl(16, Method.Normal, "00000fe07ff8fff81ff8399831983bcc35ac303c384c3ffc0ef08660c003ffff", "00133601.jpg");
            TestImpl(16, Method.Normal, "00007ff07ff07fe07fe67ff07560600077fe701e7f5e000079fd40410001ffff", "clipper_ship.jpg");
            TestImpl(16, Method.Normal, "000083f887fe8fff0fe00fe00ff80ff807fc07f807f803f803e003e007e0ffff", "00136101.jpg");
            TestImpl(16, Method.Normal, "0002001f1fff1fff7ff71ff301f300007f9278f0f8700fc0ff98cc88c1cc03fc", "32499201.jpg");
            TestImpl(16, Method.Normal, "03ff007d02da33f325e366690581b79fd103f307fc0474f93ff31e117f550700", "00094701.jpg");
            TestImpl(16, Method.Normal, "0fe01fe007e027c037c077c00fc00fe007e047c001c003c007e007e007e003c0", "stoplights.jpg");
            TestImpl(16, Method.Normal, "3fff1ca004e80fe81fe00fe01fe81de8fffb0ffb00610031003b03ff80df01ff", "00011601.jpg");
            TestImpl(16, Method.Normal, "f80ff80ff007f007f007f183f4c3c5c3c7c3c7e3c1e3c007c007c007e007ffff", "01109801.jpg");
            TestImpl(16, Method.Normal, "f81bf99ffb803400e07f8c7d049f058706013e233fe33fe11f600e638ea30def", "00002701.jpg");
            TestImpl(16, Method.Normal, "fe7ffe7ffe7ffc7ffc7ffc7ffc000000e107e187f89fe007c00ff81ff81ff83f", "Babylonian.png");
            TestImpl(16, Method.Normal, "fefd0fff000f0003037f03bc01bc11ff01fe01be01bd03ff007f007f0ffd0bf0", "06855701.jpg");
            TestImpl(16, Method.Normal, "ff80fe80fc01fe01ff00ff00ff00ff00fec0fd20f0e0fac0ecf0e7f0e7e0e040", "00106901.jpg");
            TestImpl(16, Method.Normal, "ffff8101bf0192059005860f9d8fbfa99f818f818f999e198fc182e18001ffff", "24442301.jpg");
            TestImpl(16, Method.Normal, "ffffc001bfa182018aa186e1877786e78623827385399fcf0001800fc0ffffff", "00631801.jpg");
            TestImpl(16, Method.Normal, "fffff803bd018001886382e78f7382afceef8e71813187070001800fc0ffffff", "00631701.jpg");
            //TestImpl(16, Method.Normal, "fffffe07e00380009f899f818f0b8c4b9f9fd00fcc07cc03c003e00fe07ff1ff", "emptyBasket.png");
        }
        [TestMethod]
        public void Test16BitQuick()
        {
            TestImpl(16, Method.Quick, "00000000fffffffff803f807f807f80ff90ff90fb90c9980ffffffff00000000", "puffy_white.png");
            TestImpl(16, Method.Quick, "00000fe07ff8fff81ff839d831883bcc39dc31ac300c3ffc9ef88e708041f1bf", "00133601.jpg");
            TestImpl(16, Method.Quick, "00007ff07ff07fe07fe67ff07560600077fe701e7f5e000079fd40410001ffff", "clipper_ship.jpg");
            TestImpl(16, Method.Quick, "000081f887ff8fff0fe00fe00ff80ff807fc07fc03f803f803e001e007f0ffff", "00136101.jpg");
            TestImpl(16, Method.Quick, "0002001f1fff1fff7ff31ff303f300006f9178f0f8701ee0ffc8ce88c1cc01fc", "32499201.jpg");
            TestImpl(16, Method.Quick, "03ff007d02da33f325e366f90501379fd103c12ffd04fc787ff31e511f111702", "00094701.jpg");
            TestImpl(16, Method.Quick, "0fe01fe007e027c037c077c00fe00fe007e047c001c003c007e007e007e003e0", "stoplights.jpg");
            TestImpl(16, Method.Quick, "3fff1ca004e80fe81fe00fe01fe81de8fffb8ffb00e3000000f303ff83ff4185", "00011601.jpg");
            TestImpl(16, Method.Quick, "f80ff80ff007f007f007f183f4c3c5c3c7c3c7e3c1e3c007c003c007e087ffff", "01109801.jpg");
            TestImpl(16, Method.Quick, "f81bf99ffb803400e07f8c5d849f049707033a033fe33fe1bfe00e618ee30ca7", "00002701.jpg");
            TestImpl(16, Method.Quick, "fe7ffe7ffe7ffc7ffc7ffc7ffc7f0000c103e187f18ff08f8003fc3ff81ff81f", "Babylonian.png");
            TestImpl(16, Method.Quick, "fefd0fff000f0003037f03bc00bd11ff01ff01fe01bc01ff01ff007f03fd03f8", "06855701.jpg");
            TestImpl(16, Method.Quick, "ff80fe80fe00fe01ff00ff00ff00ff00fec0ff00f820fae0fcc0e7f0e7e0e0c0", "00106901.jpg");
            TestImpl(16, Method.Quick, "ffff8001bfb182018e8186e58dd587e58641807187df9be780018007e07fffff", "00631801.jpg");
            TestImpl(16, Method.Quick, "ffff8101bf0192059005860f9d8fbf8d9f818f818f998e8b8f8982e18001ffff", "24442301.jpg");
            TestImpl(16, Method.Quick, "fffffc01bd0180018c61806fcf7181efae6d8e71893983878001800fc0ffefff", "00631701.jpg");
            //TestImpl(16, Method.Quick, "fffffe07e00380009b899f818f8f8c439f9fdc0fc807c803c003c00fe03ff1ff", "emptyBasket.png");
        }
    }
}
