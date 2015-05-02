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
    public class ModelSound: IModel2
    {
        byte[] sound;
        public bool Load()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".wav";
            dlg.Filter = "WAV Files (*.wav)|*.wav";
            dlg.InitialDirectory = @"";
            byte[] a = new byte[1];
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                byte[] input = File.ReadAllBytes(dlg.FileName);
                return true;
            }
            return false;
        }
        public bool Save() { return true; }
        public bool Apply(Node o)
        {
            byte[] input = sound;
            short noChannels = BitConverter.ToInt16(input, 22);
            short bps = BitConverter.ToInt16(input, 34);
            int BPS = bps / 8;

            int data = input.Length - 44;
            int offset = data / noChannels; //br bajtova po kanalu
            int samples = offset / BPS; //broj sempla po kanalu

            byte[] m2 = new byte[input.Length - 44]; //Pokupimo samo zvuk
            Buffer.BlockCopy(input, 44, m2, 0, m2.Length); //Prebacimo ga iz inputa u m2

            byte[] skok = new byte[noChannels];
            Random rand = new Random();

            for (int i = 0; i < noChannels; i++)
            {
                skok[i] = Convert.ToByte(100 + rand.Next(0, 155)); //Izlupam neke vrednosti
            }

            for (int j = 0; j < noChannels; j++)
                for (int k = 0; k < samples; k++)
                    for (int i = 0; i < BPS; i++) // kad prodjem ovu for petlju, idem na sl kanal.... 
                    {
                        m2[j * BPS + k * BPS * noChannels + i] = (byte)((m2[j * BPS + k * BPS * noChannels + i] > skok[j])
                                                                    ? skok[j] : m2[j * BPS + k * BPS * noChannels + i]);
                    }

            Buffer.BlockCopy(m2, 0, input, 44, m2.Length);
            sound = input;
            File.WriteAllBytes("editSound.wav", input);
            //MessageBox.Show("File je sacuvan kao editSound.Wav");
            DialogResult dialogResult = MessageBox.Show("File je sacuvan, da li zelite da ga pustite ? ", "OK", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                if (sound != null)
                {
                    using (MemoryStream ms = new MemoryStream(sound))
                    {
                        SoundPlayer sp = new SoundPlayer(ms);
                        sp.Play();
                    }
                    return true;
                }
            }
            else if (dialogResult == DialogResult.No)
            {
                return true;
            }
            return false;
 
        }

        public Object objectValue
        {
            get
            {
                return sound;
            }
            set
            {
                sound = (byte[])value;
            }
        }

        public Object[] display()
        {
            if (sound != null)
            {
                using (MemoryStream ms = new MemoryStream(sound))
                {
                    SoundPlayer sp = new SoundPlayer(ms);
                    sp.Play();
                }
            }
            return new Object[] {objectValue};
        }
    }
}
