using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorFilter.Model
{
    public class Node
    {
        public String type;
        public Object element;

        public Node(String t, Object el)
        {
            type = t;
            element = el;
        }
        public Node Clone()
        {
            return new Node(type, element);
        }
    }
}
