using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorFilter.Model
{
    public class UndoRedo
    {
        IList<Node> undo;
        IList<Node> redo;
        public static UndoRedo instance;

        public static UndoRedo getInstance()
        {
            if (instance == null)
                instance = new UndoRedo();
            return instance;
        }

        private UndoRedo()
        {
            undo = new List<Node>();
            redo = new List<Node>();
        }
        public Node doSomething(Node n)
        {
            undo.Add(n.Clone());
            redo.Clear();
            return n;
        }

        public Node doUndo()
        {
            Node tmp;
            try
            {
                tmp= undo.Last();
                redo.Add(tmp.Clone());
                undo.RemoveAt(undo.Count - 1);
                return undo.Last();
            }
            catch { return null; }
            finally 
            { 
            } 
        }

        public Node doRedo()
        {
            Node tmp;
            try
            {
                tmp = redo.Last();
                undo.Add(tmp.Clone());
                redo.RemoveAt(redo.Count - 1);
                return tmp;
            }
            catch { return undo.Last(); }
            finally
            {
            } 
        }

    }
}
