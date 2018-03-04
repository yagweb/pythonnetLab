using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    class TestPyQt
    {
        public static void PyQt4PurePython()
        {
            using (Py.GIL())
            {
                PythonEngine.Exec(
                    "import sys \n" +
                    "from PyQt4.QtGui import QApplication, QWidget \n" +
                    "app = QApplication(sys.argv) \n" +
                    "w = QWidget() \n" +
                    "w.resize(400, 300) \n" +
                    "w.move(400, 400) \n" +
                    "w.setWindowTitle('basic') \n" +
                    "w.show() \n" +
                    "app.exec_() \n" +
                    "# sys.exit(app.exec_()) \n"
                );
            }
        }

        public static void PyQt5PurePython()
        {
            using (Py.GIL())
            {
                PythonEngine.Exec(
                    "import sys \n" +
                    "from PyQt5.QtWidgets import QApplication, QWidget \n" +
                    "app = QApplication(sys.argv) \n" +
                    "w = QWidget() \n" +
                    "w.resize(400, 300) \n" +
                    "w.move(400, 400) \n" +
                    "w.setWindowTitle('hello pyqt5 from CLR') \n" +
                    "w.show() \n" +
                    "app.exec_() \n"
                );
            }
        }

        public static void PyQt5()
        {
            using (Py.GIL())
            {
                var scope = Py.CreateScope();
                var sys = scope.Import("sys");
                var QApplication = scope.Import("PyQt5.QtWidgets.QApplication", "QApplication");
                var QWidget = scope.Import("PyQt5.QtWidgets.QApplication", "QWidget");

                var app = QApplication(sys.argv);
                var w = QWidget();
                w.resize(400, 300);
                w.move(400, 400);
                w.setWindowTitle("hello pyqt5 from CLR");
                w.show();
                app.exec_();
            }
        }
    }
}
