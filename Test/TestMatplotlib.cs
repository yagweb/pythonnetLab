using Python.Runtime;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Test
{
    public class TestMatplotlib
    {
        public static void Plot()
        {
            using (Py.GIL())
            {
                Numpy.Initialize();
                Matplotlib.Initialize();

                var scope = Py.CreateScope();
                var np = scope.Import("numpy", "np");
                var plt = scope.Import("matplotlib.pylab", "plt");

                var x = Numpy.NewArray(new double[]{ 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 });
                var y = np.sin(x);
                scope.Set("x", x);
                scope.Set("y", y);

                //directly plot
                plt.plot(x, y);
                plt.show();

                //use python slice grammer
                scope.Exec(
                    "fig = plt.figure() \n" +
                    "plt.plot(x[1:], y[1:]) \n" +
                    "plt.show()"
                );
            }
        }

        static AutoResetEvent mEvent = new AutoResetEvent(false);
        public static void PlotInWindow()
        {
            byte[] plotdata;
            using (Py.GIL())
            {
                Numpy.Initialize();
                Matplotlib.Initialize();

                var scope = Py.CreateScope();
                var np = scope.Import("numpy", "np");
                var plt = scope.Import("matplotlib.pylab", "plt");

                var x = Numpy.NewArray(new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 });
                var y = np.sin(x);
                scope.Set("x", x);
                scope.Set("y", y);
                
                scope.Exec(
                    "fig = plt.figure() \n" +
                    "plt.plot(x[1:], y[1:]) \n"
                );
                var fig = scope.Get("fig");
                plotdata = Matplotlib.SaveFigureToArray(fig, 200, "png");
            }

            Stream stream = new MemoryStream(plotdata);
            Thread th = new Thread(
                () => {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                    var img1 = new System.Windows.Controls.Image();
                    img1.Source = bitmapImage;
                    Window window = new Window();
                    window.Content = img1;
                    window.ShowDialog();
                    mEvent.Set();
                }
            );
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
            mEvent.WaitOne();
        }
    }
}
