using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Drawing;

using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private readonly VideoCapture capture;

        private BackgroundWorker worker_camera;

        public MainWindow()
        {
            InitializeComponent();
            capture = new VideoCapture();

            worker_camera = new BackgroundWorker();
            worker_camera.WorkerSupportsCancellation = true; //작업 취소 가능 여부 true 로 설정

            worker_camera.DoWork += new DoWorkEventHandler(worker_camera_DoWork); //해야할 작업을 실행할 메서드 정의
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            capture.Open(0, VideoCaptureAPIs.ANY);
            if (!capture.IsOpened())
            {
                return;
            }
            worker_camera.RunWorkerAsync();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            worker_camera.CancelAsync();
            capture.Dispose();
        }

        private void worker_camera_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (worker_camera.CancellationPending == true)
                {
                    e.Cancel = true;
                    return; //about work, if it's cancelled;
                }
                try
                {
                    using (var frameMat = capture.RetrieveMat())
                    {
                        var frameBitmap = BitmapConverter.ToBitmap(frameMat);                        
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate { CamImage.Source = ConvertBmpToImageSource(frameBitmap); }));
                    }
                }
                catch
                {
                }

                Thread.Sleep(100);
            }
        }

        public BitmapImage ConvertBmpToImageSource(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
    }
}
