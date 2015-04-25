using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ColorFilter.Controller
{
    public interface IController
    {
        bool isOpen
        {
            get;
            set;
        }
        void handleLoadBitmapFromFIle();
        void getBitmap();
        void handleconvertRGBtoCMY();
        void handleInvert();
        void handleInvertUnsafe();
        void handleGamma(double red, double green, double blue);
        void handleGammaUnsafe(double red, double green, double blue);
        void handleBrightness(int brightness);
        void returnCMY();
        void handleUndo();
        void handleRedo();
        void handleSmooth();
        void handleEdgeDetect();
        void handleChannels();
        IList<int> returnCyanChannel();

        void handleFrequencyPermission(int maxValue, Color flag);

        void handleCompress();

        ///Cetvrta
        void handleWather();
        void handleMusic();
        void handlePlayMusic();
        void handleLoadSound();
    }
}
