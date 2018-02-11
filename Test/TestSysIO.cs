using Python.Runtime;
using System;
using System.IO;

namespace Test
{
    class TestSysIO
    {
        public void ToConsoleOut()
        {
            // redirect to Console.Out, I usually put this at the entrance of a program.
            PySysIO.ToConsoleOut();

            //test
            using (Py.GIL())
            {
                PythonEngine.RunSimpleString("print('hello ConsoleOut!')");
            }
        }

        public void ToNullTextWriter()
        {
            // redirect to a TextWriter
            PySysIO.ToTextWriter();

            //test
            using (Py.GIL())
            {
                PythonEngine.RunSimpleString("print('hello NullTextWriter!')");
            }
        }

        public void ToTextWriter()
        {
            // redirect to a TextWriter
            PySysIO.ToTextWriter(new StringWriter());

            //test
            using (Py.GIL())
            {
                PythonEngine.RunSimpleString("print('hello TextWriter!')");
            }

            var writer = PySysIO.TextWriter as StringWriter;
            var content = writer.GetStringBuilder().ToString();
            Console.WriteLine(content);
        }
    }
}
