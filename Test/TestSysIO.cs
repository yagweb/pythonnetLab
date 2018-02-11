using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    class TestSysIO
    {
        public void Test()
        {
            PySysIO.ToConsole();

            using (Py.GIL())
            {
                PythonEngine.RunSimpleString("print('hello pythonnet!')");
            }

            PySysIO.ToBuffer();
            using (Py.GIL())
            {
                PythonEngine.RunSimpleString("print('hello pythonnet!')");
            }
            var content = PySysIO.GetOutput();
        }
    }
}
