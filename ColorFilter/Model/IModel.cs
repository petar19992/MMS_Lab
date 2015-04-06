using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace ColorFilter.Model
{
    public interface IModel
    {
        IList<int> cyanChannel
        { get; set; }
        IList<int> magentaChannel
        { get; set; }
        IList<int> yellowChannel
        { get; set; }
        void loadBitmapFromFile();

        void loadMyExtension(OpenFileDialog dlg);
        Bitmap returnBitmap();
        Byte[] returnImageInBytes();
        Bitmap returnCyanChannel();
        Bitmap returnMagentaChannel();
        Bitmap returnYellowChannel();
        void convertRGBtoCMYRegular();
        void convertRGBtoCMYUnsafe();
        void invert();
        void invertUnsafe();
        void saveInvertToUndo();
        void gammaRegular(double red, double green, double blue);
        void gammaUnsafe(double red, double green, double blue);
        void BrightnessRegular(int brightness);
        byte[] CreateGammaArray(double color);
        void Undo();
        void Redo();
        void returnBitmapFromUndoOrRedo(bool undo);
        void saveGamaToUndo();
        void smoothFIlter();
        void edgeDetectHomogenity();
        void InitializeChannelHistograms(IList<int>[] a);
        void clearRedo();
        void checkUndo();
        void FrequencyPermissionRed(int MaxValue, int flag);
        void compress();
    }
}
