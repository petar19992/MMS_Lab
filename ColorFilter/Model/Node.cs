using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorFilter.Model
{
    public class Node
    {
        public String nameOfFunctuon;
        public Object[] listOfArguments;

        public Node(String function, Object[] arguments)
        {
            nameOfFunctuon = function;
            listOfArguments = arguments;
        }
    }
}
