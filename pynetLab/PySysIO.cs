using System.Text;

namespace Python.Runtime
{
    public class PySysIO
    {
        public static void ToConsole()
        {
            using (Py.GIL())
            {
                PythonEngine.RunSimpleString(@"
import sys
from System import Console
class output(object):
    def write(self, msg):
        Console.Write(msg)
    def writelines(self, msgs):
        for msg in msgs:
            Console.Write(msg)
    def flush(self):
        pass
    def close(self):
        pass
sys.stdout = sys.stderr = output()
");
            }
        }

        private static SysIOStream SysIOStream = new SysIOStream();

        public static void ToBuffer()
        {
            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                sys.stdout = sys.stderr = SysIOStream;
            }
        }

        public static void Clear()
        {
            SysIOStream.clear();
        }

        public static string GetOutput()
        {
            return SysIOStream.getvalue();
        }
    }

    /// <summary>
    /// Implement the interface of the sys.stdout redirection
    /// </summary>
    public class SysIOStream
    {
        private StringBuilder buffer;

        public SysIOStream()
        {
            buffer = new StringBuilder();
        }

        public void write(string str)
        {
            buffer.Append(str);
        }

        public void flush()
        {
        }

        public void close()
        {
        }

        public void clear()
        {
            buffer.Clear();
        }

        public string getvalue()
        {
            return buffer.ToString();
        }
    }
}
