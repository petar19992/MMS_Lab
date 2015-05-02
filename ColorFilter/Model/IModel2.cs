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
    public interface IModel2
    {
        bool Load();
        bool Save();
        bool Apply(Node o);

        Object[] display();

        Object objectValue
        {
            get;
            set;
        }
    }
}
