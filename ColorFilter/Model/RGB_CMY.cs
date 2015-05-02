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
    class RGB_CMY
    {
        public Bitmap cyan;
        public Bitmap magenta;
        public Bitmap yellow;
        public IList<int>[] m_channelHistograms = new IList<int>[3];

        public RGB_CMY()
        {
 
        }

        public IList<int>[] returnHistogramsChannels()
        {
            return m_channelHistograms;
        }
        public void convertRGBtoCMYRegular(Bitmap bmp)
        {
            cyan = new Bitmap(bmp.Width, bmp.Height);
            magenta = new Bitmap(bmp.Width, bmp.Height);
            yellow = new Bitmap(bmp.Width, bmp.Height);
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    Color c = bmp.GetPixel(i, j);
                    cyan.SetPixel(i, j, Color.FromArgb(0, c.G, c.B));
                    magenta.SetPixel(i, j, Color.FromArgb(c.R, 0, c.B));
                    yellow.SetPixel(i, j, Color.FromArgb(c.R, c.G, 0));
                }
            }
        }

        public void convertRGBtoCMYUnsafe(Bitmap bmp)
        {
            InitializeChannelHistograms(m_channelHistograms);
            String a = bmp.PixelFormat.ToString();
            if (a == "Format24bppRgb")
            {
                cyan = new Bitmap(bmp.Width, bmp.Height, bmp.PixelFormat);
                magenta = new Bitmap(bmp.Width, bmp.Height, bmp.PixelFormat);
                yellow = new Bitmap(bmp.Width, bmp.Height, bmp.PixelFormat);
                BitmapData bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
                BitmapData cyanData = cyan.LockBits(new Rectangle(0, 0, cyan.Width, cyan.Height), ImageLockMode.ReadWrite, cyan.PixelFormat);
                BitmapData magentaData = magenta.LockBits(new Rectangle(0, 0, magenta.Width, magenta.Height), ImageLockMode.ReadWrite, magenta.PixelFormat);
                BitmapData yellowData = yellow.LockBits(new Rectangle(0, 0, yellow.Width, yellow.Height), ImageLockMode.ReadWrite, yellow.PixelFormat);
                int stride = bmData.Stride;
                int stridecyan = cyanData.Stride;
                int stridemagenta = magentaData.Stride;
                int strideyellow = yellowData.Stride;
                System.IntPtr Scan0 = bmData.Scan0;

                System.IntPtr Scan01 = cyanData.Scan0;
                System.IntPtr Scan02 = magentaData.Scan0;
                System.IntPtr Scan03 = yellowData.Scan0;

                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;

                    byte* c = (byte*)(void*)Scan01;
                    byte* m = (byte*)(void*)Scan02;
                    byte* yell = (byte*)(void*)Scan03;

                    int nOffset = stride - bmp.Width * 3;

                    int cOffset = stridecyan - cyan.Width * 3;
                    int mOffset = stridemagenta - magenta.Width * 3;
                    int yellOffset = strideyellow - yellow.Width * 3;
                    int nWidth = bmp.Width * 3;

                    for (int y = 0; y < bmp.Height; y++)
                    {
                        for (int x = 0; x < bmp.Width; x++)
                        {
                            byte Blue = p[0];
                            c[0] = (byte)0;
                            m[0] = p[0];
                            yell[0] = p[0];
                            ++p;
                            ++c;
                            ++m;
                            ++yell;


                            byte Green = p[0];
                            c[0] = p[0];
                            m[0] = (byte)0;
                            yell[0] = p[0];
                            ++p;
                            ++c;
                            ++m;
                            ++yell;

                            byte Red = p[0];
                            c[0] = p[0];
                            m[0] = p[0];
                            yell[0] = (byte)0;
                            ++p;
                            ++c;
                            ++m;
                            ++yell;

                            m_channelHistograms[0][Convert.ToByte(Red)]++;
                            m_channelHistograms[1][Convert.ToByte(Green)]++;
                            m_channelHistograms[2][Convert.ToByte(Blue)]++;
                        }
                        p += nOffset;
                        c += cOffset;
                        m += mOffset;
                        yell += yellOffset;
                    }
                }

                bmp.UnlockBits(bmData);
                cyan.UnlockBits(cyanData);
                magenta.UnlockBits(magentaData);
                yellow.UnlockBits(yellowData);
            }
        }
        public void InitializeChannelHistograms(IList<int>[] a)
        {
            a[0] = new List<int>(256);
            a[1] = new List<int>(256);
            a[2] = new List<int>(256);

            for (int i = 0; i < 256; i++)
            {
                a[0].Add(0);
                a[1].Add(0);
                a[2].Add(0);
            }
        }


        public void compress(Bitmap bmp)
        {
            Bitmap bmptmp = (Bitmap)bmp.Clone();
            BitmapData bmData = bmptmp.LockBits(new Rectangle(0, 0, bmptmp.Width, bmptmp.Height), ImageLockMode.ReadWrite, bmptmp.PixelFormat);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;

            byte[] RedByte, BlueByte, GreenByte;
            RedByte = new byte[bmp.Height * bmp.Width]; // /3 zbog 3Bajta u pikselu
            BlueByte = new byte[bmp.Height * bmp.Width / 4]; // /6zbog 3 bajta i plus je dva puta manja
            GreenByte = new byte[bmp.Height * bmp.Width / 4];
            int counter1 = 0;
            int counter2 = 0;
            int counter3 = 0;
            int tmpH;
            int counterZero = 0;
            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                int nOffset = stride - bmptmp.Width * 3;

                int nWidth = bmptmp.Width;
                int nHeight = bmptmp.Height;
                if (nHeight % 2 == 1)
                    nHeight--;

                int counterRow = 0;
                int counterColumn = 0;

                byte[] tmpGreen = new byte[bmp.Width / 2];
                byte[] tmpBlue = new byte[bmp.Width / 2];
                int counterTmp = 0; //Indicira poziciju u pomocnom nizu Zelene Boje

                for (int y = 0; y < bmptmp.Height; y++)
                {
                    tmpH = y;
                    for (int x = 0; x < nWidth; x++)
                    {
                        byte Blue = p[0];
                        ++p;
                        byte Green = p[0];
                        ++p;
                        byte Red = p[0];
                        ++p;
                        RedByte[counter1++] = Red;
                        if (counterColumn % 2 == 0)
                        {
                            if (counterRow % 2 == 0)
                            {
                                tmpGreen[counterTmp] = Green;
                                tmpBlue[counterTmp] = Blue;
                                counterTmp++;
                                //Treba da prepise neke vrednosti
                            }
                            else
                            {
                                //Nista ne dira
                                BlueByte[counter2++] = Convert.ToByte((Blue + tmpBlue[counterTmp]) / 2);
                                GreenByte[counter3++] = Convert.ToByte((Green + tmpGreen[counterTmp]) / 2);
                                counterTmp++;
                            }
                        }
                        else
                        {
                            //nule bi trebalo da budu
                            counterZero++;
                        }
                        counterColumn++;
                    }
                    p += nOffset;
                    counterRow++;
                    counterTmp = 0;
                }
            }
            bmptmp.UnlockBits(bmData);
            byte[] finalArray = new byte[4 + RedByte.Length + BlueByte.Length + GreenByte.Length];
            finalArray[0] = Convert.ToByte(bmp.Width / 255);
            finalArray[1] = Convert.ToByte(bmp.Width % 255);

            finalArray[2] = Convert.ToByte(bmp.Height / 255);
            finalArray[3] = Convert.ToByte(bmp.Height % 255);

            RedByte.CopyTo(finalArray, 4);
            BlueByte.CopyTo(finalArray, 4 + RedByte.Length);
            GreenByte.CopyTo(finalArray, 4 + RedByte.Length + BlueByte.Length);

            File.WriteAllBytes("picture.petar", finalArray);
        }

        public Bitmap FrequencyPermission(Bitmap bmp,int MaxValue, Color flag)
        {
            IList<int>[] m_channelHistogramsTmp = new IList<int>[3];
            InitializeChannelHistograms(m_channelHistogramsTmp); //inicijalizovati tmp, ne klonirati vrednosti

            Bitmap cyantmp = (Bitmap)cyan.Clone();
            Bitmap magentatmp = (Bitmap)magenta.Clone();
            Bitmap yellowtmp = (Bitmap)yellow.Clone();
            Bitmap bmptmp = (Bitmap)bmp.Clone();
            BitmapData bmData = bmptmp.LockBits(new Rectangle(0, 0, bmptmp.Width, bmptmp.Height), ImageLockMode.ReadWrite, bmptmp.PixelFormat);
            BitmapData cyanData = cyantmp.LockBits(new Rectangle(0, 0, cyantmp.Width, cyantmp.Height), ImageLockMode.ReadWrite, cyantmp.PixelFormat);
            BitmapData magentaData = magentatmp.LockBits(new Rectangle(0, 0, magentatmp.Width, magentatmp.Height), ImageLockMode.ReadWrite, magentatmp.PixelFormat);
            BitmapData yellowData = yellowtmp.LockBits(new Rectangle(0, 0, yellowtmp.Width, yellowtmp.Height), ImageLockMode.ReadWrite, yellowtmp.PixelFormat);
            int stride = bmData.Stride;
            int stridecyan = cyanData.Stride;
            int stridemagenta = magentaData.Stride;
            int strideyellow = yellowData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;

            System.IntPtr Scan01 = cyanData.Scan0;
            System.IntPtr Scan02 = magentaData.Scan0;
            System.IntPtr Scan03 = yellowData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                byte* c = (byte*)(void*)Scan01;
                byte* m = (byte*)(void*)Scan02;
                byte* yell = (byte*)(void*)Scan03;

                int nOffset = stride - bmptmp.Width * 3;

                int cOffset = stridecyan - cyantmp.Width * 3;
                int mOffset = stridemagenta - magentatmp.Width * 3;
                int yellOffset = strideyellow - yellowtmp.Width * 3;
                int nWidth = bmptmp.Width * 3;

                for (int y = 0; y < bmptmp.Height; ++y)
                {
                    for (int x = 0; x < bmptmp.Width; ++x)
                    {
                        byte Blue = p[0];

                        if (flag == Color.Blue) //PLAVA
                        {
                            if (m_channelHistogramsTmp[2][Blue] >= MaxValue)
                            {
                                int counter = 1;
                                while (m_channelHistogramsTmp[2][(Blue + counter) % 256] >= MaxValue)
                                {
                                    counter++;
                                    if (counter >= 255)
                                        return bmp;
                                }
                                p[0] = Convert.ToByte((Blue + counter) % 256);
                                m_channelHistogramsTmp[2][(Blue + counter) % 256]++;
                            }
                            else
                            {
                                m_channelHistogramsTmp[2][Blue]++;
                            }
                        }

                        c[0] = (byte)0;
                        m[0] = p[0];
                        yell[0] = p[0];
                        ++p;
                        ++c;
                        ++m;
                        ++yell;


                        byte Green = p[0];

                        if (flag == Color.Green) //ZELENA
                        {
                            if (m_channelHistogramsTmp[1][Green] >= MaxValue)
                            {
                                int counter = 1;
                                while (m_channelHistogramsTmp[1][(Green + counter) % 256] >= MaxValue)
                                {
                                    counter++;
                                    if (counter >= 255)
                                        return bmp;
                                }
                                p[0] = Convert.ToByte((Green + counter) % 256);
                                m_channelHistogramsTmp[1][(Green + counter) % 256]++;
                            }
                            else
                            {
                                m_channelHistogramsTmp[1][Green]++;
                            }
                        }

                        c[0] = p[0];
                        m[0] = (byte)0;
                        yell[0] = p[0];
                        ++p;
                        ++c;
                        ++m;
                        ++yell;

                        byte Red = p[0];
                        if (flag == Color.Red) ///CRVENA
                        {
                            if (m_channelHistogramsTmp[0][Red] >= MaxValue)
                            {
                                int counter = 1;
                                while (m_channelHistogramsTmp[0][(Red + counter) % 256] >= MaxValue)
                                {
                                    counter++;
                                    if (counter >= 255)
                                        return bmp;
                                }
                                p[0] = Convert.ToByte((Red + counter) % 256);
                                m_channelHistogramsTmp[0][(Red + counter) % 256]++;
                            }
                            else
                            {
                                m_channelHistogramsTmp[0][Red]++;
                            }
                            m_channelHistogramsTmp[1][Convert.ToByte(Green)]++;
                            m_channelHistogramsTmp[2][Convert.ToByte(Blue)]++;
                        }
                        c[0] = p[0];
                        m[0] = p[0];
                        yell[0] = (byte)0;
                        ++p;
                        ++c;
                        ++m;
                        ++yell;
                        if (flag == Color.Green)
                        {
                            m_channelHistogramsTmp[0][Convert.ToByte(Red)]++;
                            m_channelHistogramsTmp[2][Convert.ToByte(Blue)]++;
                        }
                        if (flag == Color.Blue)
                        {
                            m_channelHistogramsTmp[1][Convert.ToByte(Green)]++;
                            m_channelHistogramsTmp[0][Convert.ToByte(Red)]++;
                        }
                    }
                    p += nOffset;
                    c += cOffset;
                    m += mOffset;
                    yell += yellOffset;
                }
            }

            m_channelHistograms = (IList<int>[])m_channelHistogramsTmp.Clone();
            bmptmp.UnlockBits(bmData);
            if (!bmptmp.Equals(bmp))
                bmp = (Bitmap)bmptmp.Clone();
            cyantmp.UnlockBits(cyanData);
            cyan = (Bitmap)cyantmp.Clone();
            magentatmp.UnlockBits(magentaData);
            magenta = (Bitmap)magentatmp.Clone();
            yellowtmp.UnlockBits(yellowData);
            yellow = (Bitmap)yellowtmp.Clone();
            return bmptmp;
        }
    }
}
