using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using System.Media;

namespace ColorFilter.Model
{
    class FiltersConvolutions
    {

        public static Bitmap fillBitmapWith(Bitmap b, byte n)
        {
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, b.PixelFormat);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - b.Width * 3;
                int nWidth = b.Width * 3;

                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < b.Width; ++x)
                    {
                        p[0] = n;
                        p[1] = n;
                        p[2] = n;
                        p += 3;
                    }
                    p += nOffset;
                }
            }

            b.UnlockBits(bmData);
            return b;
        }

        public Bitmap edgeDetectHomogenity(Bitmap bmp)
        {
            //uAr.pushBitmaptoUndo(bmp);
            // This one works by working out the greatest difference between a pixel and it's eight neighbours.
            // The threshold allows softer edges to be forced down to black, use 0 to negate it's effect.
            Bitmap b2 = (Bitmap)bmp.Clone();

            // GDI+ still lies to us - the return format is BGR, NOT RGB.
            BitmapData bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData bmData2 = b2.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            System.IntPtr Scan02 = bmData2.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                byte* p2 = (byte*)(void*)Scan02;

                int nOffset = stride - bmp.Width * 3;
                int nWidth = bmp.Width * 3;

                int nPixel = 0, nPixelMax = 0;

                p += stride;
                p2 += stride;

                for (int y = 1; y < bmp.Height - 1; ++y)
                {
                    p += 3;
                    p2 += 3;

                    for (int x = 3; x < nWidth - 3; ++x)
                    {
                        nPixelMax = Math.Abs(p2[0] - (p2 + stride - 3)[0]);
                        nPixel = Math.Abs(p2[0] - (p2 + stride)[0]);
                        if (nPixel > nPixelMax) nPixelMax = nPixel;

                        nPixel = Math.Abs(p2[0] - (p2 + stride + 3)[0]);
                        if (nPixel > nPixelMax) nPixelMax = nPixel;

                        nPixel = Math.Abs(p2[0] - (p2 - stride)[0]);
                        if (nPixel > nPixelMax) nPixelMax = nPixel;

                        nPixel = Math.Abs(p2[0] - (p2 + stride)[0]);
                        if (nPixel > nPixelMax) nPixelMax = nPixel;

                        nPixel = Math.Abs(p2[0] - (p2 - stride - 3)[0]);
                        if (nPixel > nPixelMax) nPixelMax = nPixel;

                        nPixel = Math.Abs(p2[0] - (p2 - stride)[0]);
                        if (nPixel > nPixelMax) nPixelMax = nPixel;

                        nPixel = Math.Abs(p2[0] - (p2 - stride + 3)[0]);
                        if (nPixel > nPixelMax) nPixelMax = nPixel;

                        if (nPixelMax < 5/**ovo promeni kasnije*/) nPixelMax = 0;

                        p[0] = (byte)nPixelMax;

                        ++p;
                        ++p2;
                    }

                    p += 3 + nOffset;
                    p2 += 3 + nOffset;
                }
            }
            bmp.UnlockBits(bmData);
            b2.UnlockBits(bmData2);
            return bmp;
        }


        public Bitmap smoothFIlter(Bitmap bmp)
        {
            //uAr.pushBitmaptoUndo(bmp);
            ConvMatrix m = new ConvMatrix();
            m.SetAll(1);
            m.Pixel = 1;
            m.Factor = 1 + 8;
            //Conv3x3(bmp, m);
            //Conv5x5(bmp, Matrix.Smooth5x5, 25, 0);
            universalConv(bmp, Matrix.Smooth5x5, 25, 0, 127);
            return bmp;

        }
        public bool universalConv(Bitmap b, double[,] m, int factor, int offset, byte fill)
        {
            int convMatrixWidth = m.GetLength(0);
            int convMatrixHeight = m.GetLength(1);
            // Avoid divide by zero errors
            if (0 == factor)
                return false;

            Bitmap bSrc = new Bitmap(b.Width + convMatrixWidth - 1, b.Height + convMatrixHeight - 1, b.PixelFormat);
            bSrc = fillBitmapWith(bSrc, fill);
            Graphics g = Graphics.FromImage(bSrc);
            g.DrawImage(b, 0, 0, b.Width, b.Height);

            g.Dispose();

            // GDI+ still lies to us - the return format is BGR, NOT RGB.
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData bmSrc = bSrc.LockBits(new Rectangle(0, 0, bSrc.Width, bSrc.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;

            int stridetmp = bmSrc.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            System.IntPtr SrcScan0 = bmSrc.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                byte* pSrc = (byte*)(void*)SrcScan0;

                int nOffset = stride - b.Width * 3;
                int nOffsettmp = stridetmp - bSrc.Width * 3;
                int nWidth = b.Width;
                int nHeight = b.Height - convMatrixWidth / 2;

                int nPixel, nPixel2, nPixel3;

                for (int y = 0; y < nHeight; y++)
                {
                    for (int x = 0; x < nWidth; x++)
                    {
                        nPixel = nPixel2 = nPixel3 = 0;
                        for (int i = 0; i < convMatrixHeight; i++)
                        {

                            for (int j = 0; j < convMatrixWidth; j++)
                            {
                                nPixel += Convert.ToInt32(pSrc[2 + (j * 3) + i * stridetmp] * m[i, j]);
                                nPixel2 += Convert.ToInt32(pSrc[1 + (j * 3) + i * stridetmp] * m[i, j]);
                                nPixel3 += Convert.ToInt32(pSrc[0 + (j * 3) + i * stridetmp] * m[i, j]);
                            }
                        }

                        nPixel = nPixel / factor + offset;
                        nPixel2 = nPixel2 / factor + offset;
                        nPixel3 = nPixel3 / factor + offset;

                        if (nPixel < 0) nPixel = 0;
                        if (nPixel > 255) nPixel = 255;

                        p[2 + stride * (convMatrixWidth / 2)] = (byte)nPixel;

                        if (nPixel2 < 0) nPixel2 = 0;
                        if (nPixel2 > 255) nPixel2 = 255;

                        p[1 + stride * (convMatrixWidth / 2)] = (byte)nPixel2;

                        if (nPixel3 < 0) nPixel3 = 0;
                        if (nPixel3 > 255) nPixel3 = 255;

                        p[0 + stride * (convMatrixWidth / 2)] = (byte)nPixel3;

                        p += 3;
                        pSrc += 3;
                    }
                    p += nOffset;
                    pSrc += (nOffsettmp + (convMatrixWidth - 1) * 3);
                }
            }

            b.UnlockBits(bmData);
            bSrc.UnlockBits(bmSrc);

            return true;
        }
    }
}
