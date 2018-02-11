using Python.Runtime;
using System.IO;

namespace Test
{
    class TestSysIO
    {
        public void ToConsoleOut()
        {
            // redirect to Console.Out
            PySysIO.ToConsoleOut();
            //test
            using (Py.GIL())
            {
                PythonEngine.RunSimpleString("print('hello pythonnet!')");
            }
        }

        public void ToTextWriter()
        {
            // redirect to a TextWriter
            var writer = new StringWriter();
            PySysIO.ToTextWriter(writer);
            using (Py.GIL())
            {
                PythonEngine.RunSimpleString("print('hello pythonnet!')");
            }
            var content = writer.GetStringBuilder().ToString();
        }
    }
}
