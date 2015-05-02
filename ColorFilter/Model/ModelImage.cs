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
    public class ModelImage:IModel2
    {
        public Bitmap bmp;
        public Bitmap cyan;
        public Bitmap magenta;
        public Bitmap yellow;
        public UndoAndRedo uAr;
        public IList<int>[] m_channelHistograms = new IList<int>[3];


        public bool Load() 
        {
            OpenFileDialog dlg = new OpenFileDialog();
            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".bmp";
            dlg.Filter = "JPG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png|JPEG Files (*.jpeg)|*.jpeg|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif|petar Files (*.petar)|*.petar";
            dlg.InitialDirectory = @"C:\Users\Petar\Pictures";
            byte[] a = new byte[1];
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (Path.GetExtension(dlg.FileName).Equals(".petar")) { loadMyExtension(dlg); return true; }

                FileStream fsr = System.IO.File.OpenRead(dlg.FileName);
                a = new byte[fsr.Length];
                fsr.Read(a, 0, Convert.ToInt32(fsr.Length));
                bmp = (Bitmap)Bitmap.FromFile(dlg.FileName);
                String format = bmp.PixelFormat.ToString();
                if (format == "Format32bppArgb")
                {
                    MessageBox.Show("Format slike je 32-bitni. Slika ce biti konvertovana u 24-bitni format");
                    bmp = ConvertTo24bpp(bmp);
                }
                objectValue = bmp;
                return true;
            }
            return false;
        }
        public static Bitmap ConvertTo24bpp(Image img)
        {
            var bmp = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var gr = Graphics.FromImage(bmp))
                gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
            return bmp;
        }
        public void loadMyExtension(OpenFileDialog dlg)
        {
            byte[] bytes = File.ReadAllBytes(dlg.FileName);
            int width = bytes[0] * 255 + bytes[1];
            int height = bytes[2] * 255 + bytes[3];

            byte[] RedByte, BlueByte, GreenByte;
            RedByte = new byte[height * width]; // /3 zbog 3Bajta u pikselu
            BlueByte = new byte[height * width / 4]; // /6zbog 3 bajta i plus je dva puta manja
            GreenByte = new byte[height * width / 4];
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
                                Green = tmpGreen[counterTmp] = GreenByte[counterGreen++];
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
                            Green = GreenByte[counterGreen - 1];
                            Blue = BlueByte[counterBlue - 1];

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
        public bool Save() 
        {
            return true;
        }
        public bool Apply(Node o)
        {
            String wish = o.nameOfFunctuon;
            FiltersSimple filter = new FiltersSimple();
            RGB_CMY rgbTOcmy = new RGB_CMY();
            FiltersConvolutions convolutionFilter = new FiltersConvolutions();
            switch(wish)
            {
                case "invert": 
                    objectValue=(Bitmap)filter.invertUnsafe((Bitmap)objectValue);
                    break;
                case "gamma":
                    objectValue = (Bitmap)filter.gammaUnsafe((Bitmap)objectValue,Convert.ToDouble(o.listOfArguments[0]), Convert.ToDouble(o.listOfArguments[1]), Convert.ToDouble(o.listOfArguments[1]));
                    break;
                case "brightness":
                    objectValue = (Bitmap)filter.BrightnessRegular((Bitmap)objectValue, Convert.ToInt32(o.listOfArguments[0]));
                    break;
                case "smooth":
                    objectValue = (Bitmap)convolutionFilter.smoothFIlter((Bitmap)objectValue);
                    break;
                case "edge":
                    objectValue = (Bitmap)convolutionFilter.edgeDetectHomogenity((Bitmap)objectValue);
                    break;
                case "RGBtoCMY":
                    rgbTOcmy.convertRGBtoCMYUnsafe((Bitmap)objectValue);
                    cyan = rgbTOcmy.cyan;
                    magenta = rgbTOcmy.magenta;
                    yellow = rgbTOcmy.yellow;
                    break;
                case "returnHistogram":
                    m_channelHistograms = rgbTOcmy.returnHistogramsChannels();
                    break;
                case "FrequencyPermission":
                    objectValue = (Bitmap)rgbTOcmy.FrequencyPermission((Bitmap)objectValue, Convert.ToInt32(o.listOfArguments[0]), (Color)o.listOfArguments[1]);
                    m_channelHistograms = rgbTOcmy.returnHistogramsChannels();
                    break;
                    
            }
            return true;
        }

        public Object objectValue
        {
            get 
            {
                return bmp;
            }
            set
            {
                bmp = (Bitmap)value;
            }
        }

        public Object[] display()
        {
            return new Object[] { objectValue, cyan, magenta, yellow, m_channelHistograms };
        }
    }
}
