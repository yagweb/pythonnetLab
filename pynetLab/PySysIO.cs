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

        public static TextWriter TextWriter
        {
            get
            {
                return SysIOStream.TextWriter;
            }
        }

        public static TextWriter ToTextWriter(TextWriter writer = null)
        {
            using (Py.GIL())
            {
                SysIOStream.TextWriter = writer;
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
        private TextWriter _TextWriter;
        public TextWriter TextWriter
        {
            get
            {
                return _TextWriter == null ? Console.Out : _TextWriter;
            }
            set
            {
                _TextWriter = value;
            }
        }

        public SysIOWriter(TextWriter writer = null)
        {
            this._TextWriter = writer;
        }

        public void write(String str)
        {
            this.TextWriter.Write(str);
        }

        public void writelines(String[] str)
        {
            foreach (String line in str)
            {
                this.write(line);
            }
        }

        public void flush()
        {
            this.TextWriter.Flush();
        }

        public void close()
        {
            if (this._TextWriter != null)
            {
                this._TextWriter.Close();
            }
        }
    }
}
