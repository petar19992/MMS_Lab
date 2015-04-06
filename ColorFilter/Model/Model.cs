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

namespace ColorFilter.Model
{
    public class Model:IModel
    {
        public Bitmap bmp;
        Bitmap cyan;
        Bitmap magenta;
        Bitmap yellow;
        public byte[] imageInBytes;
        public UndoAndRedo uAr;

        public IList<int>[] m_channelHistograms = new IList<int>[3];

        public Model()
        {
            uAr = new UndoAndRedo();
        }
        
        public void loadBitmapFromFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".bmp";
            dlg.Filter = "JPG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png|JPEG Files (*.jpeg)|*.jpeg|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif|petar Files (*.petar)|*.petar";
            dlg.InitialDirectory = @"C:\Users\Petar\Pictures";
            byte[] a = new byte[1];
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (Path.GetExtension(dlg.FileName).Equals(".petar")) { loadMyExtension(dlg); return; }
                   
                FileStream fsr = System.IO.File.OpenRead(dlg.FileName);
                a = new byte[fsr.Length];
                fsr.Read(a, 0, Convert.ToInt32(fsr.Length));
                imageInBytes = a;
                bmp = (Bitmap)Bitmap.FromFile(dlg.FileName);
                String format = bmp.PixelFormat.ToString();
                if (format == "Format32bppArgb")
                {
                    MessageBox.Show("Format slike je 32-bitni. Slika ce biti konvertovana u 24-bitni format");
                    bmp = ConvertTo24bpp(bmp);
                }
            }  
        }
        public void loadMyExtension(OpenFileDialog dlg) 
        {
            byte[] bytes = File.ReadAllBytes(dlg.FileName);
            int width = bytes[0] * 255 + bytes[1];
            int height = bytes[2] * 255 + bytes[3];

            byte[] RedByte, BlueByte, GreenByte;
            RedByte = new byte[height * width ]; // /3 zbog 3Bajta u pikselu
            BlueByte = new byte[height * width/4 ]; // /6zbog 3 bajta i plus je dva puta manja
            GreenByte = new byte[height * width/4 ];
            Array.Copy(bytes, 4, RedByte, 0, RedByte.Length);
            //bytes.CopyTo(cyanByte, 4);
            Array.Copy(bytes, 4 + RedByte.Length, BlueByte, 0, BlueByte.Length);
            //bytes.CopyTo(magentaByte, 4+cyanByte.Length);
            Array.Copy(bytes, 4 + RedByte.Length + BlueByte.Length, GreenByte, 0, GreenByte.Length);
            //bytes.CopyTo(yellowByte, 4 + cyanByte.Length+magentaByte.Length);

            Bitmap bmptmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmData = bmptmp.LockBits(new Rectangle(0, 0, bmptmp.Width, bmptmp.Height), ImageLockMode.ReadWrite, bmptmp.PixelFormat);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int counterRed = 0;
                int counterGreen = 0;
                int counterBlue = 0;
                //int nOffset = stride - bmp.Width * 3;

                int counterRow = 0;
                int counterColumn = 0;
                byte[] tmpGreen = new byte[width / 2];
                byte[] tmpBlue = new byte[width / 2];
                int counterTmp = 0; //Indicira poziciju u pomocnom nizu Zelene Boje

                for (int y = 0; y < bmptmp.Height; ++y)
                {
                    for (int x = 0; x < bmptmp.Width; ++x)
                    {
                        byte Red = RedByte[counterRed++];
                        byte Green;
                        byte Blue;


                        if (counterColumn % 2 == 0)
                        {
                            if (counterRow % 2 == 0)
                            {
                                //Treba da prepise neke vrednosti
                                Green = tmpGreen[counterTmp]=GreenByte[counterGreen++];
                                Blue = tmpBlue[counterTmp] = BlueByte[counterBlue++];
                                counterTmp++;
                            }
                            else
                            {

                                Green = tmpGreen[counterTmp];
                                Blue = tmpBlue[counterTmp];
                                counterTmp++;
                            }
                        }
                        else
                        {
                            Green = GreenByte[counterGreen-1];
                            Blue = BlueByte[counterBlue-1];

                        }

                        p[0] = Blue;
                        p++;
                        p[0] = Green;
                        p++;
                        p[0] = Red;
                        p++;
                        
                        counterColumn++;
                    }
                    counterRow++;
                    counterTmp = 0;
                    //p += nOffset;
                }

            }

            bmptmp.UnlockBits(bmData);
            bmp = (Bitmap)bmptmp.Clone();
        }

        public static Bitmap ConvertTo24bpp(Image img)
        {
            var bmp = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var gr = Graphics.FromImage(bmp))
                gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
            return bmp;
        }
        public void convertRGBtoCMYRegular()
        {
            cyan = new Bitmap(bmp.Width, bmp.Height);
            magenta = new Bitmap(bmp.Width, bmp.Height);
            yellow = new Bitmap(bmp.Width, bmp.Height);
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    Color c = bmp.GetPixel(i, j);
                    cyan.SetPixel(i, j, Color.FromArgb( 0, c.G, c.B));
                    magenta.SetPixel(i, j, Color.FromArgb(c.R, 0, c.B));
                    yellow.SetPixel(i, j, Color.FromArgb(c.R, c.G, 0));
                }
            }
        }
        public void convertRGBtoCMYUnsafe()
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


                             //**Sve je ovo lepo, ali nista bez RGB !!!**///
                            //byte Black = Convert.ToByte(Math.Min(255 - Red, 255 - Green));
                            //Black = Convert.ToByte(Math.Min(Black, 255 - Blue));
                            //double Red2, Green2, Blue2;
                            //Red2 = Red;
                            //Red2/= 255;
                            //Green2 = Green;
                            //Green2/= 255;
                            //Blue2 = Blue;
                            //Blue2/= 255;
                            //Double Black2 = Black;
                            //Black2 /= 255;
                            //double Cyan2, Magenta2, Yellow2;
                            //if (Black == 255)
                            //{
                            //    Cyan2 = Magenta2 = Yellow2 = 1;
                            //}
                            //else
                            //{
                            //    Cyan2 = ((1 - Red2 - Black2) / (1 - Black2));
                            //    Magenta2 = ((1 - Green2 - Black2) / (1 - Black2));
                            //    Yellow2 = ((1 - Blue2 - Black2) / (1 - Black2));
                            //}
                            //m_channelHistograms[0][Convert.ToByte(Cyan2 * 255)]++;
                            //m_channelHistograms[1][Convert.ToByte(Magenta2 * 255)]++;
                            //m_channelHistograms[2][Convert.ToByte(Yellow2 * 255)]++;
                            

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

        public void compress()
        {
            Bitmap bmptmp = (Bitmap)bmp.Clone();
            BitmapData bmData = bmptmp.LockBits(new Rectangle(0, 0, bmptmp.Width, bmptmp.Height), ImageLockMode.ReadWrite, bmptmp.PixelFormat);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;

            byte [] RedByte,BlueByte,GreenByte;
            RedByte = new byte[bmp.Height * bmp.Width]; // /3 zbog 3Bajta u pikselu
            BlueByte = new byte[bmp.Height * bmp.Width/4]; // /6zbog 3 bajta i plus je dva puta manja
            GreenByte = new byte[bmp.Height * bmp.Width/4];
            int counter1 = 0;
            int counter2 = 0;
            int counter3 = 0;
            int tmpH;
            int counterZero = 0;
            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                int nOffset = stride - bmptmp.Width * 3;
       
                int nWidth = bmptmp.Width ;
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
                                BlueByte[counter2++] = Convert.ToByte((Blue+tmpBlue[counterTmp])/2);
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
            byte[] finalArray = new byte[4+RedByte.Length + BlueByte.Length + GreenByte.Length];
            finalArray[0] = Convert.ToByte(bmp.Width / 255);
            finalArray[1] = Convert.ToByte(bmp.Width % 255);

            finalArray[2] = Convert.ToByte(bmp.Height / 255);
            finalArray[3] = Convert.ToByte(bmp.Height % 255);

            RedByte.CopyTo(finalArray, 4);
            BlueByte.CopyTo(finalArray, 4 + RedByte.Length);
            GreenByte.CopyTo(finalArray, 4 + RedByte.Length + BlueByte.Length);

            File.WriteAllBytes("picture.petar", finalArray);
            File.WriteAllBytes("Red.txt", RedByte);
            File.WriteAllBytes("Green.txt", GreenByte);
            File.WriteAllBytes("Blue.txt", BlueByte);
        }

        public void FrequencyPermissionRed(int MaxValue, int flag)
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

                        if (flag == 2) //PLAVA
                        {
                            if (m_channelHistogramsTmp[2][Blue] >= MaxValue)
                            {
                                int counter = 1;
                                while (m_channelHistogramsTmp[2][(Blue + counter) % 256] >= MaxValue)
                                {
                                    counter++;
                                    if (counter >= 255)
                                        return;
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

                        if (flag == 1) //ZELENA
                        {
                            if (m_channelHistogramsTmp[1][Green] >= MaxValue)
                            {
                                int counter = 1;
                                while (m_channelHistogramsTmp[1][(Green + counter) % 256] >= MaxValue)
                                {
                                    counter++;
                                    if (counter >= 255)
                                        return;
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
                        if(flag==0) ///CRVENA
                        {
                            if (m_channelHistogramsTmp[0][Red] >= MaxValue)
                            {
                                int counter = 1;
                                while (m_channelHistogramsTmp[0][(Red + counter) % 256] >= MaxValue)
                                {
                                    counter++;
                                    if (counter >= 255)
                                        return;
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
                        if (flag == 1)
                        {
                            m_channelHistogramsTmp[0][Convert.ToByte(Red)]++;
                            m_channelHistogramsTmp[2][Convert.ToByte(Blue)]++;
                        }
                        if (flag == 2)
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
            if(!bmptmp.Equals(bmp))
                bmp = (Bitmap)bmptmp.Clone();
            cyantmp.UnlockBits(cyanData);
            cyan = (Bitmap)cyantmp.Clone();
            magentatmp.UnlockBits(magentaData);
            magenta = (Bitmap)magentatmp.Clone();
            yellowtmp.UnlockBits(yellowData);
            yellow = (Bitmap)yellowtmp.Clone();
        }

        public static Bitmap fillBitmapWith(Bitmap b,byte n)
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
        public byte[] CreateGammaArray(double color)
        {
            byte[] gammaArray = new byte[256];
            for (int i = 0; i < 256; ++i)
            {
                gammaArray[i] = (byte)Math.Min(255,
        (int)((255.0 * Math.Pow(i / 255.0, 1.0 / color)) + 0.5));
            }
            return gammaArray;
        }

        #region properties
        public Bitmap returnCyanChannel()
        { return cyan; }
        public Bitmap returnMagentaChannel()
        { return magenta; }
        public Bitmap returnYellowChannel()
        { return yellow; }
        public Bitmap returnBitmap()
        {
            return bmp;
        }
        public Byte[] returnImageInBytes()
        {
            return imageInBytes;
        }
        public IList<int> cyanChannel
        {
            get
            {
                return m_channelHistograms[0];
            }
            set
            {
                m_channelHistograms[0] = value;
            }
        }
        public IList<int> magentaChannel
        {
            get
            {
                return m_channelHistograms[1];
            }
            set
            {
                m_channelHistograms[1] = value;
            }
        }
        public IList<int> yellowChannel
        {
            get
            {
                return m_channelHistograms[2];
            }
            set
            {
                m_channelHistograms[2] = value;
            }
        }
        #endregion

        #region unsafe
        public void invertUnsafe()
        {
            BitmapData bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat); // PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                int nOffset = stride - bmp.Width * 3;
                int nWidth = bmp.Width * 3;

                for (int y = 0; y < bmp.Height; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        p[0] = (byte)(255 - p[0]);
                        ++p;
                    }
                    p += nOffset;
                }
            }
            bmp.UnlockBits(bmData);
        }
        public void gammaUnsafe(double red, double green, double blue)
        {
            uAr.pushBitmaptoUndo(bmp);
            if (red < .2 || red > 5) return;
            if (green < .2 || green > 5) return;
            if (blue < .2 || blue > 5) return;

            byte[] redGamma = new byte[256];
            byte[] greenGamma = new byte[256];
            byte[] blueGamma = new byte[256];

            for (int i = 0; i < 256; ++i)
            {
                redGamma[i] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / red)) + 0.5));
                greenGamma[i] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / green)) + 0.5));
                blueGamma[i] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / blue)) + 0.5));
            }

            // GDI+ still lies to us - the return format is BGR, NOT RGB.
            BitmapData bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                int nOffset = stride - bmp.Width * 3;

                for (int y = 0; y < bmp.Height; ++y)
                {
                    for (int x = 0; x < bmp.Width; ++x)
                    {
                        p[2] = redGamma[p[2]];
                        p[1] = greenGamma[p[1]];
                        p[0] = blueGamma[p[0]];

                        p += 3;
                    }
                    p += nOffset;
                }
            }

            bmp.UnlockBits(bmData);

            return;
        }
        public void edgeDetectHomogenity()
        {
            uAr.pushBitmaptoUndo(bmp);
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
        }
        #endregion


        #region undoAndRedo
        public void saveInvertToUndo()
        {
            Node tmp = new Node("invertUnsafe", null);
            uAr.pushToUndo(tmp);
        }
        public void saveGamaToUndo()
        {
            Object[] l = new Object[1];
            l[0] = true;
            Node tmp = new Node("returnBitmapFromUndoOrRedo", l);
            uAr.pushToUndo(tmp);
        }
        public void brightnessToUndo(Object[] listOfArguments)
        {
            Node tmp = new Node("BrightnessRegular", listOfArguments);
            uAr.pushToUndo(tmp);
        }
        public void Undo()
        {
            try
            {
                Node tmp = uAr.popFromUndo();
                uAr.pushToRedo(tmp);
                String imeFunkcije = tmp.nameOfFunctuon;
                Type thisType = this.GetType();
                MethodInfo theMethod = thisType.GetMethod(imeFunkcije);
                Object[] listaArgumenata = tmp.listOfArguments;
                theMethod.Invoke(this, listaArgumenata);
            }
            catch { MessageBox.Show("Stack underflow"); }
        }

        public void Redo()
        {
            try
            {
                Node tmp = uAr.popFromRedo();
                if (tmp.nameOfFunctuon.Equals("returnBitmapFromUndoOrRedo"))
                {
                    tmp.listOfArguments[0] = !(bool)tmp.listOfArguments[0];
                    uAr.pushToUndo(tmp);
                }
                else
                    uAr.pushToUndo(tmp);

                String imeFunkcije = tmp.nameOfFunctuon;
                Type thisType = this.GetType();
                MethodInfo theMethod = thisType.GetMethod(imeFunkcije);
                Object[] listaArgumenata = tmp.listOfArguments;
                theMethod.Invoke(this, listaArgumenata);
            }
            catch { MessageBox.Show("Stack underflow"); }
        }
        public void returnBitmapFromUndoOrRedo(bool undo) //if undo==true than return from undo, else from redo
        {
            if (undo)
            {
                if ((uAr.bitmapsForUndo.Count - 1) < 0) return;
                bmp = (Bitmap)uAr.bitmapsForUndo[uAr.bitmapsForUndo.Count - 1].Clone();
                uAr.bitmapsForUndo.RemoveAt(uAr.bitmapsForUndo.Count - 1);
                uAr.pushBitmaptoRedo((Bitmap)bmp.Clone());
            }
            else
            {
                if ((uAr.bitmapsForRedo.Count - 1) < 0) return;
                bmp = (Bitmap)uAr.bitmapsForRedo[uAr.bitmapsForRedo.Count - 1].Clone();
                uAr.bitmapsForRedo.RemoveAt(uAr.bitmapsForRedo.Count - 1);
                uAr.pushBitmaptoUndo((Bitmap)bmp.Clone());
            }

        }
        #endregion


        public void smoothFIlter()
        {
            uAr.pushBitmaptoUndo(bmp);
            ConvMatrix m = new ConvMatrix();
            m.SetAll(1);
            m.Pixel = 1;
            m.Factor = 1 + 8;
            //Conv3x3(bmp, m);
            //Conv5x5(bmp, Matrix.Smooth5x5, 25, 0);
            universalConv(bmp, Matrix.Smooth5x5, 25, 0, 127);
        }
        public bool universalConv(Bitmap b, double[,] m, int factor, int offset, byte fill)
        {
            int convMatrixWidth = m.GetLength(0);
            int convMatrixHeight = m.GetLength(1);
            // Avoid divide by zero errors
            if (0 == factor)
                return false;

            Bitmap bSrc = new Bitmap(b.Width + convMatrixWidth-1, b.Height + convMatrixHeight-1, b.PixelFormat);
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
                int nHeight = b.Height- convMatrixWidth/2;

                int nPixel,nPixel2,nPixel3;

                for (int y = 0; y < nHeight; y++)
                {
                    for (int x = 0; x < nWidth; x++)
                    {
                        nPixel = nPixel2=nPixel3=0;
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

                        p[2 + stride*(convMatrixWidth/2)] = (byte)nPixel;

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
                    pSrc += (nOffsettmp + (convMatrixWidth-1)*3);
                }
            }

            b.UnlockBits(bmData);
            bSrc.UnlockBits(bmSrc);

            return true;
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
        public void clearRedo()
        {
            uAr.clearRedo();
        }
        public void checkUndo()
        {
            long capacity = 0;
            foreach (Bitmap b in uAr.bitmapsForUndo)
            {
                int tmp=b.Width*b.Height;
                capacity += Convert.ToInt64(54 + 3 * tmp);
            }
            long inMB = capacity / 1024 / 1024;
            if (inMB > 20)
            {
                uAr.bitmapsForUndo.RemoveAt(0);
                foreach (Node n in uAr.nodesForUndo)
                {
                    if (n.nameOfFunctuon.Equals("returnBitmapFromUndoOrRedo"))
                    {
                        uAr.nodesForUndo.Remove(n);
                    }
                }
            }
                
        }

        #region RegularFilters
        public void invert()
        {
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    Color c = bmp.GetPixel(i, j);
                    bmp.SetPixel(i, j, Color.FromArgb(255 - c.R, 255 - c.G, 255 - c.B));
                }
            }
        }

        public void gammaRegular(double red, double green, double blue)
        {
            uAr.pushBitmaptoUndo(bmp);
            Bitmap temp = bmp;
            Bitmap bmap = (Bitmap)temp.Clone();
            Color c;
            byte[] redGamma = CreateGammaArray(red);
            byte[] greenGamma = CreateGammaArray(green);
            byte[] blueGamma = CreateGammaArray(blue);
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    bmap.SetPixel(i, j, Color.FromArgb(redGamma[c.R],
                       greenGamma[c.G], blueGamma[c.B]));
                }
            }
            bmp = (Bitmap)bmap.Clone();
        }

        public void BrightnessRegular(int brightness)
        {
            Object[] listOfArguments = new Object[1];
            listOfArguments[0] = -brightness;
            brightnessToUndo(listOfArguments);
            Bitmap temp = (Bitmap)bmp;
            Bitmap bmap = (Bitmap)temp.Clone();
            if (brightness < -255) brightness = -255;
            if (brightness > 255) brightness = 255;
            Color c;
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    int cR = c.R + brightness;
                    int cG = c.G + brightness;
                    int cB = c.B + brightness;

                    if (cR < 0) cR = 1;
                    if (cR > 255) cR = 255;

                    if (cG < 0) cG = 1;
                    if (cG > 255) cG = 255;

                    if (cB < 0) cB = 1;
                    if (cB > 255) cB = 255;

                    bmap.SetPixel(i, j,
        Color.FromArgb((byte)cR, (byte)cG, (byte)cB));
                }
            }
            bmp = (Bitmap)bmap.Clone();
        }
        #endregion
     }
 }
        
