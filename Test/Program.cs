using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    class Program
    {
        // Config the embedded CPython
        static void PythonInitialize()
        {
            //  Set the PATH env variable for OS pythonXX.dll searching
            //  or put the pythonXX.dll into the current working directory or a directory in PATH
            //  Attension, PythonHome cannot be used for dll search!
            //string python = @"C:\Python\WinPython-64bit-2.7.10.3\python-2.7.10.amd64";
            //string python = @"C:\Python\WinPython-64bit-3.5.2.2Qt5\python-3.5.2.amd64";
            //string python = "C:\\Anaconda3";
            string python = "C:\\Anaconda3.5";
            Environment.SetEnvironmentVariable("PATH", python, EnvironmentVariableTarget.Process);
            
            // it will be used to set the PythonPath with defaut values
            PythonEngine.PythonHome = python;
            Console.WriteLine(PythonEngine.PythonPath);
            // Sometimes, we don't want to embed a completed CPython, 
            // and we only want to distrute our app with miminum files
            // then, we can set the Python Module search Path by hand
            //PythonEngine.PythonPath = String.Join(";", new List<string>() {
            //    $"{python}\\DLLs",
            //    $"{python}\\Lib",
            //    $"{python}\\Lib\\site_packages",
            //});

            //PythonEngine.ProgramName = "RuePyNet";
            PythonEngine.Initialize();

            PythonEngine.BeginAllowThreads(); //need be removed into the future
            //redirect python print to .NET Console
            PySysIO.ToConsoleOut();
        }

        //[MTAThread]
        //[STAThread]
        static int Main(string[] args)
        {
            PythonInitialize();

            //new TestSysIO().ToConsoleOut();
            //new TestSysIO().ToNullTextWriter();
            //new TestSysIO().ToTextWriter();

            TestPyQt.PyQt4PurePython(); //passed
            //TestPyQt.PyQt5PurePython(); //failed

            // Py3.6 + Qt5Agg, crashed down!
            // Py3.6 + TkAgg backend, passed
            // Py3.5 + TkAgg backend, passed
            // Py3.5 + Qt4Agg backend, passed
            //TestMatplotlib.SetUp("TkAgg");
            //TestMatplotlib.SetUp("Qt4Agg");
            //TestMatplotlib.SetUp("Qt5Agg");  //program crashed!
            //TestMatplotlib.Plot();
            //TestMatplotlib.PlotInWindow();

            // API is not stable.
            //new TestPyConverter().TestList();
            //new TestPyConverter().TestDict();
            //new TestPyConverter().TestObject();
            //new TestPyConverter().TestHybird();

            return 0;
        }
    }
}
