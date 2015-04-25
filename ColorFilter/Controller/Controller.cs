using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using ColorFilter.Model;
using ColorFilter.View;

namespace ColorFilter.Controller
{
    public class Controller:IController
    {
        IModel2 m_model_image;
        IModel2 m_model_sound;
        //IModel m_model;
        IView m_view;
        public bool isOpen
        {
            get;
            set;
        }

        //Ovo takodje menjao
        public Controller(/*IModel model,*/IModel2 modelImage, IModel2 modelSound, IView view)
        {
            //m_model = model;
            m_model_image = modelImage;
            m_model_sound = modelSound;
            m_view = view;
            m_view.AddListener(this);
        }
        //I ovo moram da izmenim
        public void handleLoadBitmapFromFIle()
        {
            //m_model.loadBitmapFromFile();
            //m_view.bitmap = m_model.returnBitmap();
            //m_view.imageInBytes = m_model.returnImageInBytes();
            m_model_image.Load();
            m_view.bitmap = (Bitmap)UndoRedo.getInstance().doSomething(new Node("Bitmap",(Bitmap)m_model_image.display()[0])).element;
        }
        public void handleGamma(double red, double green, double blue)
        {
            //m_model.gammaRegular(red, green, blue);
            //m_model.saveGamaToUndo();
            //m_model.clearRedo();
            //m_view.bitmap = m_model.returnBitmap();
        }
        public void handleGammaUnsafe(double red, double green, double blue)
        {
            //m_model.gammaUnsafe(red, green, blue);
            //m_model.saveGamaToUndo();
            //m_model.clearRedo();
            //m_view.bitmap = m_model.returnBitmap();

            m_model_image.Apply(new Pipeline("gamma", new Object[] { red, green,blue }));
            m_view.bitmap = (Bitmap)UndoRedo.getInstance().doSomething(new Node("Bitmap", (Bitmap)m_model_image.display()[0])).element;

        }
        public void handleInvert()
        {
            //m_model.invert();
            //m_model.saveInvertToUndo();
            //m_model.clearRedo();
            //m_view.bitmap = m_model.returnBitmap();
        }

        //Ovo cu da menjam
        public void handleInvertUnsafe()
        {
            //m_model.invertUnsafe();
            //m_model.saveInvertToUndo();
            //m_model.clearRedo();
            //m_view.bitmap = m_model.returnBitmap();

            m_model_image.Apply(new Pipeline("invert", null));
            m_view.bitmap = (Bitmap)UndoRedo.getInstance().doSomething(new Node("Bitmap", (Bitmap)m_model_image.display()[0])).element;

        }

        public void handleBrightness(int brightness)
        {
            //m_model.BrightnessRegular(brightness);
            //m_view.bitmap = m_model.returnBitmap();
            //m_model.clearRedo();
            m_model_image.Apply(new Pipeline("brightness", new Object[] { brightness }));
            m_view.bitmap = (Bitmap)UndoRedo.getInstance().doSomething(new Node("Bitmap", (Bitmap)m_model_image.display()[0])).element;

        }
        public void handleSmooth()
        {
            //m_model.smoothFIlter();
            //m_model.saveGamaToUndo();
            //m_model.clearRedo();
            //m_view.bitmap = m_model.returnBitmap();
            m_model_image.Apply(new Pipeline("smooth", null));
            m_view.bitmap = (Bitmap)UndoRedo.getInstance().doSomething(new Node("Bitmap", (Bitmap)m_model_image.display()[0])).element;

        }
        public void handleEdgeDetect()
        {
            //m_model.edgeDetectHomogenity();
            //m_model.saveGamaToUndo();
            //m_model.clearRedo();
            //m_view.bitmap = m_model.returnBitmap();
            m_model_image.Apply(new Pipeline("edge", null));
            m_view.bitmap = (Bitmap)UndoRedo.getInstance().doSomething(new Node("Bitmap", (Bitmap)m_model_image.display()[0])).element;

        }
        public void getBitmap()
        {
            //m_model.convertRGBtoCMYUnsafe();
            m_view.bitmap = (Bitmap)m_model_image.display()[0];
        }
        public void handleconvertRGBtoCMY()
        {
            //m_model.convertRGBtoCMYUnsafe();
            //m_model.convertRGBtoCMYUnsafe();

            m_model_image.Apply(new Pipeline("RGBtoCMY", null));
            //m_view.bitmap = (Bitmap)m_model_image.display();
        }
        public void returnCMY()
        {
            m_view.cyan = (Bitmap)m_model_image.display()[1];
            m_view.magenta = (Bitmap)m_model_image.display()[2];
            m_view.yellow = (Bitmap)m_model_image.display()[3];
        }

        //Ovo treba da sredim
        public void handleUndo()
        {
            try
            {
                Node tmp = UndoRedo.getInstance().doUndo();
                if (tmp.type.Equals("Bitmap"))
                {
                    m_view.bitmap = (Bitmap)tmp.element;
                }
            }
            catch { m_view.bitmap = null; }
            
            //m_model.Undo();
            //m_view.bitmap = m_model.returnBitmap();
        }
        public void handleRedo()
        {
            Node tmp = UndoRedo.getInstance().doRedo();
            if (tmp.type.Equals("Bitmap"))
            {
                m_view.bitmap = (Bitmap)tmp.element;
            }
            //m_model.Redo();
            //m_view.bitmap = m_model.returnBitmap();
        }
        public void handleChannels()
        {
            m_model_image.Apply(new Pipeline("returnHistogram", null));
            IList<int>[] tmp=(IList<int>[])m_model_image.display()[4];
            m_view.cyanChannel = tmp[0];
            m_view.magentaChannel = tmp[1];
            m_view.yellowChannel = tmp[2];
        }
        public IList<int> returnCyanChannel()
        {
            //return m_model.cyanChannel;
            IList<int>[] tmp = (IList<int>[])m_model_image.display()[4];
            return tmp[0];
        }
        public void handleFrequencyPermission(int maxValue, int flag)
        {
            //m_model.FrequencyPermissionRed(maxValue,flag);
            m_model_image.Apply(new Pipeline("FrequencyPermission", new Object[] { maxValue, flag }));
            
        }
        public void handleCompress()
        {
            //m_model.compress();
        }

        ////////////Cetvrta
        public void handleWather()
        {
            //m_model.Water(15, true);

            //m_model.saveGamaToUndo();
            //m_model.clearRedo();
        }
        public void handleMusic()
        {
            m_model_sound.Apply(new Pipeline("edit", null));
            //m_model.PlaySound();
        }
        public void handlePlayMusic()
        {
            m_model_sound.Apply(new Pipeline("play", null));
            //m_model.PlayMusic();
        }
        public void handleLoadSound()
        {
            m_model_sound.Load();
        }
    }
}