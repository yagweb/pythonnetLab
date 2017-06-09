using System;

namespace Python.Runtime
{
    public class Matplotlib
    {
        public static void Initialize()
        {
            np = Py.Import("numpy");
            plt = Py.Import("matplotlib.pylab");
            PltFigureType = plt.GetAttr("Figure").Handle;
            var io = Py.Import("io");
            BytesIO = io.GetAttr("BytesIO");
        }

        internal static PyObject np;

        internal static PyObject plt;

        internal static IntPtr PltFigureType;

        internal static PyObject BytesIO;

        public static byte[] SaveFigureToArray(PyObject fig, int dpi = 200, string format = "png")
        {
            if (fig.GetPythonType().Handle != PltFigureType)
            {
                throw new Exception("object is not a matplotlib Figure");
            }

            dynamic _np = np;
            //buf = io.BytesIO()
            dynamic buf = BytesIO.Invoke();
            //fig.savefig(buf, dpi=__dpi__, format='png')
            fig.InvokeMethod("savefig", new PyTuple(new PyObject[] { buf }), Py.kw("dpi", dpi, "format", format));
            var buf_out = _np.array(buf.getbuffer(), Py.kw("dtype", Numpy.GetNumpyDataType(typeof(byte))));
            var arr = Numpy.ToArray(buf_out);
            return (byte[])arr;
        }
    }
}
