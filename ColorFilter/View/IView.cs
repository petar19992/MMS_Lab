using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorFilter.Controller;
using System.Drawing;

namespace ColorFilter.View
{
    public interface IView
    {
        void AddListener(IController controller);

        System.Drawing.Bitmap bitmap
        {
            get;
            set;
        }
        byte[] imageInBytes
        {
            get;
            set;
        }
        Bitmap cyan
        {
            get;
            set;
        }
        Bitmap magenta
        {
            get;
            set;
        }
        Bitmap yellow
        {
            get;
            set;
        }

        IList<int> cyanChannel
        { get; set; }
        IList<int> magentaChannel
        { get; set; }
        IList<int> yellowChannel
        { get; set; }

    }
}
