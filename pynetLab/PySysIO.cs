using System;
using System.IO;

namespace Python.Runtime
{
    public class PySysIO
    {
        public static void ToConsoleOut()
        {
            using (Py.GIL())
            {
                PythonEngine.RunSimpleString(@"
import sys
from System import Console
class output(object):
    def write(self, msg):
        Console.Out.Write(msg)
    def writelines(self, msgs):
        for msg in msgs:
            Console.Out.Write(msg)
    def flush(self):
        pass
    def close(self):
        pass
sys.stdout = sys.stderr = output()
");
            }
        }

        private static SysIOWriter SysIOStream = new SysIOWriter();

        public static TextWriter ToTextWriter(TextWriter writer = null)
        {
            using (Py.GIL())
            {
                if(writer != null)
                {
                    SysIOStream.TextWriter = writer;
                }
                dynamic sys = Py.Import("sys");
                sys.stdout = sys.stderr = SysIOStream;
            }
            return SysIOStream.TextWriter;
        }

        public static void Flush()
        {
            SysIOStream.flush();
        }
    }

    /// <summary>
    /// Implement the interface of the sys.stdout redirection
    /// </summary>
    public class SysIOWriter
    {
        public TextWriter TextWriter
        {
            get;
            internal set;
        }

        public SysIOWriter(TextWriter writer = null)
        {
            TextWriter = writer;
        }

        public void write(String str)
        {
            str = str.Replace("\n", Environment.NewLine);
            if (TextWriter != null)
            {
                TextWriter.Write(str);
            }
            else
            {
                Console.Out.Write(str);
            }
        }

        public void writelines(String[] str)
        {
            foreach (String line in str)
            {
                if (TextWriter != null)
                {
                    TextWriter.Write(str);
                }
                else
                {
                    Console.Out.Write(str);
                }
            }
        }

        public void flush()
        {
            if (TextWriter != null)
            {
                TextWriter.Flush();
            }
            else
            {
                Console.Out.Flush();
            }
        }

        public void close()
        {
            if (TextWriter != null)
            {
                TextWriter.Close();
            }
        }
    }
}
