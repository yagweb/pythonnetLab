using Python.Runtime;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Test
{
    class REPLinWPF
    {
        static AutoResetEvent mEvent = new AutoResetEvent(false);
        public static void Main()
        {            
            Thread th = new Thread(
                () => {
                    Window window = new Window();
                    window.Content = new REPLTextBox();
                    window.ShowDialog();
                    mEvent.Set();
                }
            );
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
            mEvent.WaitOne();
        }
    }

    public class REPLTextBox : UserControl
    {
        public REPLTextBox()
        {
            this.TB = new TextBox()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            this.Content = this.TB;

            this.Prompt = ">>> ";
            this.HistoryCommand = new StringBuilder();
            this.TB.Text = "";
            this.TextBasePosition = 0;
            using (Py.GIL())
            {
                this.WelcomeMsg = $"Python{PythonEngine.Version}\n";
            }
            this.Clear();
            this.TB.KeyDown += new KeyEventHandler(this._OnKeyDown);
            this.TB.PreviewKeyDown += new KeyEventHandler(this._OnPreviewKeyDown);
            this.TB.PreviewTextInput += new TextCompositionEventHandler(this._OnPreviewTextInput);
            this.PreviewKeyUp += new KeyEventHandler(this._OnPreviewKeyUp);
            this.TB.KeyUp += new KeyEventHandler(this._OnKeyUp);

            using (Py.GIL())
            {
                PyScope = Py.CreateScope();
            }
            Console.SetOut(new TBTextWriter(this));
        }

        private TextBox TB;

        private PyScope PyScope;

        // It's not enough when block input is needed.
        private int TextBasePosition;

        private string WelcomeMsg;

        private string HistoryText;

        public string Prompt
        {
            get;
            private set;
        }

        //StringBuilder will be replace with a custom class
        // it will be used in _OnKeyUp
        public StringBuilder HistoryCommand
        {
            get;
            private set;
        }

        //text behind Base Postion cannot be deleted with backspace
        private void UpdateTextBasePosition()
        {
            this.TextBasePosition = this.TB.Text.Length;
            this.HistoryText = this.TB.Text;
        }
        
        private void Clear()
        {
            this.TB.Text = this.WelcomeMsg;
            this.TB.Text += this.Prompt;
            this.UpdateTextBasePosition();
            this.TB.ScrollToEnd();
            
            this.HistoryCommand.Clear();
        }

        private void WriteLine(string value)
        {
            this.TB.Text += value;
            this.TB.Text += Environment.NewLine;
        }

        public void Write(string value)
        {
            this.TB.Text += value;
        }
        
        //It will triggered first
        //But, it's only triggered after cursor moved by user        
        private void _OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((int)e.Key < 34)
            {
                return;
            }
            
            if (this.TB.SelectionStart < this.TextBasePosition)
            {
                this.TB.SelectionStart = this.TextBasePosition;
                this.TB.SelectionLength = 0;
                e.Handled = true;
            }
        }
        
        private void _OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return | e.Key == Key.Enter) // Catch a carrage return
            {
                this.SendCommand();
            }
        }

        /// <summary>
        /// send the command to python
        /// </summary>
        private void SendCommand()
        {
            string rawCommand = this.TB.Text.Remove(0, this.TextBasePosition).Trim();
            if (rawCommand.EndsWith(":"))
            {
                // Not implement
                this.TB.Text += Environment.NewLine;
                this.TB.Text += this.Prompt;
                this.Focus();
                this.TB.Select(this.TB.Text.Length, 0); //move cursor to the end
                return;
            }

            if (!string.IsNullOrEmpty(rawCommand))
            {
                this.TB.Text += Environment.NewLine;
                using (Py.GIL())
                {
                    try
                    {
                        PyObject script = PythonEngine.Compile(rawCommand, "", RunFlagType.Single);
                        var res = this.PyScope.Execute(script);
                        if(res != null)
                        {
                            this.WriteLine(res.ToString());
                        }
                    }
                    catch(Exception ex)
                    {
                        this.WriteLine(ex.Message);
                    }
                }
            }
            
            this.TB.Text += this.Prompt;
            this.TB.SelectionStart = this.TB.Text.Length;
            this.TB.ScrollToEnd();
            this.HistoryCommand.Append(rawCommand);
            this.UpdateTextBasePosition();
        }

        private void _OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (this.TB.SelectionStart < this.TextBasePosition)
            {
                this.UpdateTextBasePosition();
                e.Handled = true;
            }
        }

        /// <summary>
        /// 防止用户删除提示符
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            int index = this.TB.SelectionStart;
            
            if (e.Key == Key.Left && index < this.TextBasePosition)
            {
                this.TB.SelectionStart = this.TextBasePosition;
                this.TB.SelectionLength = 0;
                e.Handled = true;
            }
            
            if (e.Key == Key.Back && index < this.TextBasePosition)
            {
                //recover
                this.TB.SelectionStart = 0;
                this.TB.SelectionLength = this.TextBasePosition - 1;
                this.TB.SelectedText = this.HistoryText;

                //set the cursor
                this.TB.SelectionStart = this.TextBasePosition;
                this.TB.SelectionLength = 0;
                e.Handled = true;
            }
        }

        private void _OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up: // Command history next
                    break;
                case Key.Down: // Command history previous
                    break;
                case Key.Escape: // Command history exit
                    break;
                default:
                    break;
            }
        }
    }

    public class TBTextWriter : TextWriter
    {
        public TBTextWriter(REPLTextBox tb)
        {
            this.TB = tb;
        }

        private REPLTextBox TB;

        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }

        public override void Write(string value)
        {
            this.TB.Write(value);
        }
    }
}
