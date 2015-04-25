using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorFilter.Model
{
    public class Pipeline
    {
        public String nameOfFunctuon;
        public Object[] listOfArguments;

        public Pipeline(String nOf, Object[] lOa)
        {
            nameOfFunctuon=nOf;
            listOfArguments = lOa;
        }
    }
}
