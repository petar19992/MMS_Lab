using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using ColorFilter.View;
using ColorFilter.Controller;
using ColorFilter.Model;



/**http://www.codeproject.com/Articles/33838/Image-Processing-using-C **/
namespace ColorFilter
{
    public partial class Form1 : Form,IView
    {
        IController m_controller;
        ColorFilter.Model.Model model;
        Controller.Controller controller2;
        public Form1()
        {
            InitializeComponent();
            model = new Model.Model();
            Controller.Controller controller = new Controller.Controller(model, this);
        }

        public void AddListener(IController controller)
        {
            m_controller = controller;

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_controller.handleLoadBitmapFromFIle();
        }

        private void getRGBChannelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void getXYZChannelsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void getCMYChannelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (controller2 == null || !controller2.isOpen)
            {
                ChannelsForm cf = new ChannelsForm();
                controller2 = new Controller.Controller(model, cf);
                cf.Show();
            }
            else
            {
                controller2.getBitmap();
                controller2.handleconvertRGBtoCMY();
                controller2.returnCMY();
                this.Invalidate();
            }
            
        }
        #region Properties
        public Bitmap bitmap
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
            get
            {
                return imageInBytes;
            }
            set
            {
                //imageInBytes=value;
            }
        }
        public Bitmap cyan
        {
            get;
            set;
        }
        public Bitmap magenta
        {
            get;
            set;
        }
        public Bitmap yellow
        {
            get;
            set;
        }
        public IList<int> cyanChannel
        {
            get { return cyanChannel; }
            set
            {
                
                //Histogram h = new Histogram(Color.Cyan);
                //h.Data = value;
                //h.Title = "Cyan";
                //h.Show();
                //controller2.handleChannels();
            }
        }
        public IList<int> magentaChannel
        {
            get { return magentaChannel; }
            set
            {
                Histogram h = new Histogram(Color.Magenta);
                h.Data = value;
                h.Title = "Magenta";
                h.Show();
            }
        }
        public IList<int> yellowChannel
        {
            get { return yellowChannel; }
            set
            {
                Histogram h = new Histogram(Color.Yellow);
                h.Data = value;
                h.Title = "Yellow";
                h.Show();
            }
        }



        #endregion

        private void invertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_controller.handleInvert();
            controller2.getBitmap();
            controller2.handleconvertRGBtoCMY();
            controller2.returnCMY();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            m_controller.handleGamma(1.0, 1.0, 1.0);
            this.Invalidate();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            m_controller.handleGamma(0.9,0.9,0.9);
            this.Invalidate();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            m_controller.handleGamma(0.5, 0.5, 0.5);
            this.Invalidate();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            m_controller.handleGamma(0.4, 0.4, 0.4);
            this.Invalidate();
        }

        private void brightnessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_controller.handleBrightness(10);
            this.Invalidate();
        }

        private void invertUnsafeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_controller.handleInvertUnsafe();
            controller2.getBitmap();
            controller2.handleconvertRGBtoCMY();
            controller2.returnCMY();
            this.Invalidate();
        }

        private void gammaUnsafeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_controller.handleGammaUnsafe(0.5,0.5,0.5);
            controller2.getBitmap();
            controller2.handleconvertRGBtoCMY();
            controller2.returnCMY();
            this.Invalidate();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_controller.handleUndo();
            try
            {
                controller2.getBitmap();
                controller2.handleconvertRGBtoCMY();
                controller2.returnCMY();
            }
            catch { }
            this.Invalidate();
        }

        private void smoothToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_controller.handleSmooth();
            controller2.getBitmap();
            controller2.handleconvertRGBtoCMY();
            controller2.returnCMY();
            Invalidate();
        }

        private void edgeDetectHomogenityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_controller.handleEdgeDetect();
            controller2.getBitmap();
            controller2.handleconvertRGBtoCMY();
            controller2.returnCMY();
            Invalidate();
        }

        private void rGBHistogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_controller.handleChannels();
            //m_controller.handleHistogram();
            //controller2.getBitmap();
            ////controller2.handleconvertRGBtoCMY();
            //controller2.returnCMY();
            //Invalidate();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_controller.handleRedo();
            try
            {
                controller2.getBitmap();
                controller2.handleconvertRGBtoCMY();
                controller2.returnCMY();
            }
            catch { }
            this.Invalidate();
        }

        private void compressAndSaveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_controller.handleCompress();
            try
            {
                controller2.getBitmap();
                //controller2.handleconvertRGBtoCMY();
                controller2.returnCMY();
            }
            catch { }
            this.Invalidate();
        }

        private void watherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_controller.handleWather();
            m_controller.getBitmap();
            controller2.getBitmap();
            controller2.handleconvertRGBtoCMY();
            controller2.returnCMY();
            this.Invalidate();
        }

        private void editSoundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_controller.handleMusic();

        }

        private void playSoundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_controller.handlePlayMusic();
        }



    }
}
