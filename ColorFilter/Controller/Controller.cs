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
        IModel m_model;
        IView m_view;
        public bool isOpen
        {
            get;
            set;
        }

        public Controller(IModel model, IView view)
        {
            m_model = model;
            m_view = view;
            m_view.AddListener(this);
        }
        public void handleLoadBitmapFromFIle()
        {
            m_model.loadBitmapFromFile();
            m_view.bitmap = m_model.returnBitmap();
            m_view.imageInBytes = m_model.returnImageInBytes();
        }
        public void handleGamma(double red, double green, double blue)
        {
            m_model.gammaRegular(red, green, blue);
            m_model.saveGamaToUndo();
            m_model.clearRedo();
            m_view.bitmap = m_model.returnBitmap();
        }
        public void handleGammaUnsafe(double red, double green, double blue)
        {
            m_model.gammaUnsafe(red, green, blue);
            m_model.saveGamaToUndo();
            m_model.clearRedo();
            m_view.bitmap = m_model.returnBitmap();
        }
        public void handleInvert()
        {
            m_model.invert();
            m_model.saveInvertToUndo();
            m_model.clearRedo();
            m_view.bitmap = m_model.returnBitmap();
        }
        public void handleInvertUnsafe()
        {
            m_model.invertUnsafe();
            m_model.saveInvertToUndo();
            m_model.clearRedo();
            m_view.bitmap = m_model.returnBitmap();
        }

        public void handleBrightness(int brightness)
        {
            m_model.BrightnessRegular(brightness);
            m_view.bitmap = m_model.returnBitmap();
            m_model.clearRedo();
        }
        public void handleSmooth()
        {
            m_model.smoothFIlter();
            m_model.saveGamaToUndo();
            m_model.clearRedo();
            m_view.bitmap = m_model.returnBitmap();
        }
        public void handleEdgeDetect()
        {
            m_model.edgeDetectHomogenity();
            m_model.saveGamaToUndo();
            m_model.clearRedo();
            m_view.bitmap = m_model.returnBitmap();
        }
        public void getBitmap()
        {
            //m_model.convertRGBtoCMYUnsafe();
            m_view.bitmap = m_model.returnBitmap();
        }
        public void handleconvertRGBtoCMY()
        {
            m_model.convertRGBtoCMYUnsafe();
            //m_model.convertRGBtoCMYUnsafe();
        }
        public void returnCMY()
        {
            m_view.cyan = m_model.returnCyanChannel();
            m_view.magenta = m_model.returnMagentaChannel();
            m_view.yellow = m_model.returnYellowChannel();
        }

        public void handleUndo()
        {
            m_model.Undo();
            m_view.bitmap = m_model.returnBitmap();
        }
        public void handleRedo()
        {
            m_model.Redo();
            m_view.bitmap = m_model.returnBitmap();
        }
        public void handleChannels()
        {
            m_view.cyanChannel = m_model.cyanChannel;
            m_view.magentaChannel = m_model.magentaChannel;
            m_view.yellowChannel = m_model.yellowChannel;
        }
        public IList<int> returnCyanChannel()
        {
            return m_model.cyanChannel;
        }
        public void handleFrequencyPermission(int maxValue, int flag)
        {
            m_model.FrequencyPermissionRed(maxValue,flag);
        }
        public void handleCompress()
        {
            m_model.compress();
        }
    }
}