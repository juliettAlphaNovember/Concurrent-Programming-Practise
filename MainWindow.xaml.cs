using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Shapes;

namespace lab_5
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private bool ParseTextInputKN(out int K, out int N)
        {
            string K_string = K_TextBox.Text;
            string N_string = N_TextBox.Text;

            if (string.IsNullOrEmpty(K_string) || string.IsNullOrEmpty(N_string))
            {
                K = -1;
                N = -1;
                return false;
            }

            if (!Int32.TryParse(K_string, out K) || !Int32.TryParse(N_string, out N))
            {
                K = -1;
                N = -1;
                return false;
            }

            if (N < K)
            {
                return false;
            }
            return true;
        }

        private void Tasks_Click(object sender, RoutedEventArgs e)
        {
            int K;
            int N;

            if (!ParseTextInputKN(out K, out N))
            {
                return;
            }

            Task.Factory.StartNew(() =>
            {
                Task<long> taskCalculateDenominator = Task<long>.Factory.StartNew(() =>
                {
                    long resultDenominator = 1;
                    for (int i = 1; i <= K; i++)
                    {
                        resultDenominator *= i;
                    }
                    return resultDenominator;
                });

                // Calculate numerator
                long resultNumerator = 1;
                for (int i = 1; i <= K; i++)
                {
                    resultNumerator *= N - i + 1;
                }

                taskCalculateDenominator.Wait();
                string text = (resultNumerator / taskCalculateDenominator.Result).ToString();
                Tasks_TextBox.Dispatcher.Invoke(new Action(() => Tasks_TextBox.Text = text));
            });
        }

        public static long CalculateDemoninator(int K)
        {
            long denominator = 1;
            for (int i = 1; i <= K; i++)
            {
                denominator *= i;
            }
            return denominator;
        }

        private void Delegates_Click(object sender, RoutedEventArgs e)
        {
            int K;
            int N;

            if (!ParseTextInputKN(out K, out N))
            {
                return;
            }

            Func<int, long> op = CalculateDemoninator;
            IAsyncResult result = op.BeginInvoke(K, null, null);

            // Calculate numerator
            long resultNumerator = 1;
            for (int i = 1; i <= K; i++)
            {
                resultNumerator *= N - i + 1;
            }

            long resultDenominator = op.EndInvoke(result);
            string text = (resultNumerator / resultDenominator).ToString();
            Delegates_TextBox.Text = text;
        }

        public Task<long> CalculateDenominatorAsync(int K)
        {
            Task<long> taskCalculateDenominator = Task<long>.Factory.StartNew(() =>
            {
                long resultDenominator = 1;
                for (int i = 1; i <= K; i++)
                {
                    resultDenominator *= i;
                }
                return resultDenominator;
            });
            return taskCalculateDenominator;
        }

        private async void Async_Click(object sender, RoutedEventArgs e)
        {
            int K;
            int N;

            if (!ParseTextInputKN(out K, out N))
            {
                return;
            }

            long resultDenominator = await CalculateDenominatorAsync(K);

            // Calculate numerator
            long resultNumerator = 1;
            for (int i = 1; i <= K; i++)
            {
                resultNumerator *= N - i + 1;
            }

            string text = (resultNumerator / resultDenominator).ToString();
            Async_TextBox.Text = text;
        }

        private void Kompresja_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderBrowserDialog()
            {
                Description = "Select directory to open"
            };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }

            var directory = new DirectoryInfo(dlg.SelectedPath);
            var directoryCompressed = directory.CreateSubdirectory("Compressed");

            var tasks = new List<Task>();
            foreach (var item in directory.EnumerateFiles())
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    using (Stream fileToCompress = item.OpenRead())
                    using (Stream compressedFile = File.Create(System.IO.Path.Combine(directoryCompressed.FullName, item.Name) + ".gz"))
                    using (GZipStream gzip = new GZipStream(compressedFile, CompressionMode.Compress))
                    {
                        fileToCompress.CopyTo(gzip);
                    }
                }));
            }

            foreach (var item in tasks)
            {
                item.Wait();
            }
            System.Windows.Forms.MessageBox.Show("Compression finished successfully", "Lab 5", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Dekompresja_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderBrowserDialog()
            {
                Description = "Select directory to open"
            };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }

            var directory = new DirectoryInfo(dlg.SelectedPath);
            var directoryDeompressed = directory.Parent.CreateSubdirectory("Decompressed");
            

            var tasks = new List<Task>();
            foreach (var item in directory.EnumerateFiles())
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    var name = item.Name.Substring(0, item.Name.Length - 3);
                    using (Stream fileToDecompress = item.OpenRead())
                    using (GZipStream gzip = new GZipStream(fileToDecompress, CompressionMode.Decompress))
                    using (Stream decompressedFile = File.Create(System.IO.Path.Combine(directoryDeompressed.FullName, item.Name.Substring(0, item.Name.Length - 3))))
                    {
                        gzip.CopyTo(decompressedFile);
                    }
                }));
            }

            foreach (var item in tasks)
            {
                item.Wait();
            }
            System.Windows.Forms.MessageBox.Show("Decompression finished successfully", "Lab 5", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static readonly Random getrandom = new Random();

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            lock (getrandom) // synchronize
            {
                Check_TextBox.Text = getrandom.Next().ToString();
            }
        }

        private void Fibonacci_DoWork(object sender, DoWorkEventArgs args)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            int n = (int)args.Argument;
            long fib_0 = 0;
            long fib_1 = 1;

            if (n == 0 || n == 1)
            {
                args.Result = n;
                worker.ReportProgress(100);
                return;
            }

            worker.ReportProgress(0);
            long result = 0;
            float percent = 100.0F / n;
            float progress = percent;
            for (int i = 2; i <= n; i++)
            {
                result = fib_0 + fib_1;
                fib_0 = fib_1;
                fib_1 = result;

                System.Threading.Thread.Sleep(20);
                progress += percent;
                worker.ReportProgress((int)(progress));
            }
            args.Result = result;
        }

        private void Fibonacci_ProgressChanged(object sender, ProgressChangedEventArgs args)
        {
            Fibonacci_ProgressBar.Value = args.ProgressPercentage;
        }

        private void Fibonacci_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            Get_TextBox.Text = ((long)args.Result).ToString();
        }

        private void Get_Click(object sender, RoutedEventArgs e)
        {
            string I_string = Fibonacci_TextBox.Text;
            int I;

            if (string.IsNullOrEmpty(I_string))
            {
                return;
            }

            if (!Int32.TryParse(I_string, out I))
            {
                return;
            }

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += Fibonacci_DoWork;
            bw.ProgressChanged += Fibonacci_ProgressChanged;
            bw.RunWorkerCompleted += Fibonacci_RunWorkerCompleted;
            bw.WorkerReportsProgress = true;
            bw.RunWorkerAsync(I);
        }

        private void Resolve_Click(object sender, RoutedEventArgs e)
        {
            string[] hostNames = { "www.microsoft.com", "www.apple.com",
                "www.google.com", "www.ibm.com", "cisco.netacad.net",
                "www.oracle.com", "www.nokia.com", "www.hp.com", "www.dell.com",
                "www.samsung.com", "www.toshiba.com", "www.siemens.com",
                "www.amazon.com", "www.sony.com", "www.canon.com",
                "www.alcatel-lucent.com", "www.acer.com", "www.motorola.com" };

            DNS_TextBox.Text = "";
            hostNames
                .AsParallel()
                .ForAll(name =>
                {
                    string result = name + " => " + Dns.GetHostAddresses(name).Last().ToString() + "\n";
                    DNS_TextBox.Dispatcher.BeginInvoke(new Action(() => DNS_TextBox.AppendText(result)));
                });
        }

        private static readonly Regex regex = new Regex("[0-9]+");

        private static bool IsTextAllowed(string text)
        {
            return regex.IsMatch(text);
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
        }
    }
}