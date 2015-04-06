using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace ColorFilter.Model
{
    public class UndoAndRedo
    {
        public List<Node> nodesForUndo;
        public List<Node> nodesForRedo;
        public IList<Bitmap> bitmapsForUndo;
        public IList<Bitmap> bitmapsForRedo;
        public UndoAndRedo()
        {
            nodesForUndo = new List<Node>();
            nodesForRedo = new List<Node>();
            bitmapsForUndo = new List<Bitmap>();
            bitmapsForRedo = new List<Bitmap>();
        }
        public bool pushToUndo(Node node)
        {
            try 
            {
                nodesForUndo.Add(node);
                return true;
            }
            catch 
            {
                return false;
            }
        }
        public Node popFromUndo()
        {
            Node tmp=nodesForUndo[nodesForUndo.Count-1];
            nodesForUndo.RemoveAt(nodesForUndo.Count-1);
            return tmp;
        }

        public bool pushToRedo(Node node)
        {
            try
            {
                nodesForRedo.Add(node);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public Node popFromRedo()
        {
            Node tmp = nodesForRedo[nodesForRedo.Count - 1];
            nodesForRedo.RemoveAt(nodesForRedo.Count - 1);
            return tmp;
        }
        public void pushBitmaptoUndo(Bitmap bmp)
        {
            bitmapsForUndo.Add((Bitmap)bmp.Clone());
        }

        public void pushBitmaptoRedo(Bitmap bitmap)
        {
            bitmapsForRedo.Add((Bitmap)bitmap.Clone());
        }
        public void clearRedo()
        {
            nodesForRedo.Clear();
            bitmapsForRedo.Clear();
        }
    }
}
