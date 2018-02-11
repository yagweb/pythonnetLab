using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    class Program
    {
        static int Main(string[] args)
        {
            //new TestSysIO().Test();
            //TestMatplotlib.Plot();
            //TestMatplotlib.PlotInWindow();
            //new TestPyConverter().TestList();
            //new TestPyConverter().TestDict();
            //new TestPyConverter().TestObject();
            new TestPyConverter().TestHybird();
            return 0;
        }
    }
}
