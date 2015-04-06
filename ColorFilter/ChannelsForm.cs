using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ColorFilter.View;
using ColorFilter.Controller;
using ColorFilter.Model;

namespace ColorFilter
{
    public partial class ChannelsForm : Form,IView
    {
        IController m_controller;
        public ChannelsForm()
        {
            InitializeComponent();
        }
        public void AddListener(IController controller)
        {
            m_controller = controller;
            
        }
        #region Properties

        public Color penColor;

        private Bitmap histoBmp = new Bitmap(256, 256);
        private Bitmap histoBmp2 = new Bitmap(256, 256);
        private Bitmap histoBmp3 = new Bitmap(256, 256);
        public int MinValueCyan { get; set; }
        public int MaxValueCyan { get; set; }
        private IList<int> data;
        private IList<int> data2;
        private IList<int> data3;


        private int borderRed; //Ukazuju mi gde je kliknuto na histogramu
        private int borderGreen;
        private int borderBlue;
        public System.Drawing.Bitmap bitmap
        {
            get
            {
                return (Bitmap)pictureBox1.Image; 
            }
                
            set
            {
                this.pictureBox1.Image = value;
            }
        }
        public byte[] imageInBytes
        {
            get;
            set;
        }
        public IList<int> cyanChannel
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
                MaxValueCyan = data.Count - 1;

                Graphics g = Graphics.FromImage(histoBmp);

                g.Clear(Color.FromKnownColor(KnownColor.Control));

                Pen pen = new Pen(Color.Red); //Yellow

                if (data.Count > 0)
                {
                    int maxCount = data.Max();
                    float scaleFactor = 255.0f / maxCount;

                    for (int index = 0; index < MaxValueCyan; index++)
                    {
                        g.DrawLine(pen, new Point(index, 255), new Point(index, 255 - (int)(data[index] * scaleFactor)));
                    }
                }

                pen.Dispose();
                g.Dispose();
                cyan = histoBmp;
                //histogramBox.Refresh();

                
            }
        }
        public IList<int> magentaChannel
        {
            get
            {
                return data2;
            }
            set
            {
                data2 = value;
                MaxValueCyan = data2.Count - 1;

                Graphics g = Graphics.FromImage(histoBmp2);

                g.Clear(Color.FromKnownColor(KnownColor.Control));

                Pen pen = new Pen(Color.Green); //Magenta

                if (data2.Count > 0)
                {
                    int maxCount = data2.Max();
                    float scaleFactor = 255.0f / maxCount;

                    for (int index = 0; index < MaxValueCyan; index++)
                    {
                        g.DrawLine(pen, new Point(index, 255), new Point(index, 255 - (int)(data2[index] * scaleFactor)));
                    }
                }
                pen.Dispose();
                g.Dispose();

                magenta = histoBmp2;
                //histogramBox.Refresh();

                
            }
        }
        public IList<int> yellowChannel
        {
            get
            {
                return data3;
            }
            set
            {
                data3 = value;
                MaxValueCyan = data3.Count - 1;

                Graphics g = Graphics.FromImage(histoBmp3);

                g.Clear(Color.FromKnownColor(KnownColor.Control));

                Pen pen = new Pen(Color.Blue); //Cyan

                if (data3.Count > 0)
                {
                    int maxCount = data3.Max();
                    float scaleFactor = 255.0f / maxCount;

                    for (int index = 0; index < MaxValueCyan; index++)
                    {
                        g.DrawLine(pen, new Point(index, 255), new Point(index, 255 - (int)(data3[index] * scaleFactor)));
                    }
                }
                pen.Dispose();
                g.Dispose();
                yellow = histoBmp3;
                //histogramBox.Refresh();

                
            }
        }
        public Bitmap cyan
        {
            get
            {
                return (Bitmap)pictureBox2.Image;
            }

            set
            {
                this.pictureBox2.Image = value;
            }
        }
        public Bitmap magenta
        {
            get
            {
                return (Bitmap)pictureBox3.Image;
            }

            set
            {
                this.pictureBox3.Image = value;
            }
        }
        public Bitmap yellow
        {
            get
            {
                return (Bitmap)pictureBox4.Image;
            }

            set
            {
                this.pictureBox4.Image = value;
            }
        }
        #endregion

        private void ChannelsForm_Shown(object sender, EventArgs e)
        {
            m_controller.getBitmap();
            m_controller.handleconvertRGBtoCMY();
            m_controller.returnCMY();
            m_controller.isOpen = true;


            penColor = Color.Black;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            UpdateStyles();

            MinValueCyan = 0;
            MaxValueCyan = 255;

            IList<int> someData = new List<int>();
            cyanChannel = new List<int>();
            magentaChannel = new List<int>();
            yellowChannel = new List<int>();
            //for (int i = 0; i < 256; i++)
            //{
            //    cyanChannel.Add(i); ;
            //    magentaChannel.Add(i); ;
            //    yellowChannel.Add(i); ;
            //    someData.Add(i);
            //}

            //cyanChannel = someData;
            //magentaChannel = someData;
            //yellowChannel = someData;

            m_controller.getBitmap();
            m_controller.handleconvertRGBtoCMY();
            m_controller.returnCMY();
            m_controller.isOpen = true;
            //cyanChannel = DataCyan;

        }

        private void ChannelsForm_FormClosed(object sender, FormClosedEventArgs e)
        {

            m_controller.isOpen = false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                m_controller.handleChannels();
            }
            else
            {
                m_controller.getBitmap();
                m_controller.handleconvertRGBtoCMY();
                m_controller.returnCMY();
                m_controller.isOpen = true;
            }
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if (checkBox1.Checked)
            {
                Bitmap histoBmpTmp = (Bitmap)histoBmp.Clone();
                Graphics g = Graphics.FromImage(histoBmpTmp);

                //g.Clear(Color.FromKnownColor(KnownColor.Control));

                Pen pen = new Pen(Color.Black);

                g.DrawLine(pen, new Point(0, e.Y), new Point(pictureBox2.Width + pictureBox2.Location.X, e.Y));
                borderRed = e.Y;

                pen.Dispose();
                g.Dispose();
                cyan = histoBmpTmp;
            }
        }

        private void pictureBox3_MouseMove(object sender, MouseEventArgs e)
        {
             if (checkBox1.Checked)
            {
                Bitmap histoBmpTmp = (Bitmap)histoBmp2.Clone();
                Graphics g = Graphics.FromImage(histoBmpTmp);

                //g.Clear(Color.FromKnownColor(KnownColor.Control));

                Pen pen = new Pen(Color.Black);

                g.DrawLine(pen, new Point(0, e.Y), new Point(pictureBox2.Width + pictureBox2.Location.X, e.Y));
                borderGreen = e.Y;

                pen.Dispose();
                g.Dispose();
                magenta = histoBmpTmp;
             }
        }

        private void pictureBox4_MouseMove(object sender, MouseEventArgs e)
        {
            if (checkBox1.Checked)
            {
                Bitmap histoBmpTmp = (Bitmap)histoBmp3.Clone();
                Graphics g = Graphics.FromImage(histoBmpTmp);

                //g.Clear(Color.FromKnownColor(KnownColor.Control));

                Pen pen = new Pen(Color.Black);

                g.DrawLine(pen, new Point(0, e.Y), new Point(pictureBox2.Width + pictureBox2.Location.X, e.Y));
                borderBlue = e.Y;

                pen.Dispose();
                g.Dispose();
                yellow = histoBmpTmp;
            }
        }

        private void pictureBox2_MouseLeave(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                cyan = histoBmp;
            }
        }

        private void pictureBox3_MouseLeave(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                magenta = histoBmp2;
            }
        }

        private void pictureBox4_MouseLeave(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                yellow = histoBmp3;
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                int maxCount = data.Max();
                int maxValue = ((255 - borderRed) * maxCount) / 255;
                m_controller.handleFrequencyPermission(maxValue,0); //0 za crvenu !!!
                m_controller.getBitmap();
                m_controller.returnCMY();
                m_controller.handleChannels();
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                int maxCount = data2.Max();
                int maxValue = ((255 - borderGreen) * maxCount) / 255;
                m_controller.handleFrequencyPermission(maxValue, 1); //1 za ZELENU
                m_controller.getBitmap();
                m_controller.returnCMY();
                m_controller.handleChannels();
            }
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                int maxCount = data3.Max();
                int maxValue = ((255 - borderBlue) * maxCount) / 255;
                m_controller.handleFrequencyPermission(maxValue, 2); //2 za plavu
                m_controller.getBitmap();
                m_controller.returnCMY();
                m_controller.handleChannels();
            }
        }



    }
}
