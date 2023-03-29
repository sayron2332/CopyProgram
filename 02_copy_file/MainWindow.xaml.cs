using IOExtensions;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Security;
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
using Path = System.IO.Path;

namespace _02_copy_file
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewModel model;
      
        public MainWindow()
        {
            InitializeComponent();
            model = new ViewModel()
            {
                Progress = 0
            };
            this.DataContext = model;
        }
        private async void CopyButtonClick(object sender, RoutedEventArgs e)
        {
            
            string filename = Path.GetFileName(model.Source);
            string destFilePath = Path.Combine(model.Destination , filename);
            CopyProcessInfo info = new CopyProcessInfo(filename);
            model.AddProcesses(info);
            await CopyFileAsync(model.Source, destFilePath, info);    
            //add item to list 

            MessageBox.Show("Complited!!!");
        }
        private Task CopyFileAsync(string src, string dest, CopyProcessInfo info)
        {
            #region Type 1 Type 2
            // type 1 - using File class
            //File.Copy(Source, destFilePath, true);

            //return Task.Run(() =>
            //{
            //    // type 2- using FileStream class
            //    using FileStream streamSource = new FileStream(src, FileMode.Open, FileAccess.Read);
            //    using FileStream streamDest = new FileStream(dest, FileMode.Create, FileAccess.Write);

            //    byte[] buffer = new byte[1024 * 8];//8KB   //12
            //    int bytes = 0;
            //    do
            //    {
            //        bytes = streamSource.Read(buffer, 0, buffer.Length);//0.5

            //        streamDest.Write(buffer, 0, bytes);//8
            //        //% = total  received 
            //        float procent = streamDest.Length / (streamSource.Length / 100);
            //        model.Progress = procent;

            //    } while (bytes > 0);

            //});
            #endregion
            return FileTransferManager.CopyWithProgressAsync(src, dest, (progress) =>
            {
                model.Progress = progress.Percentage;
                info.Percentage = progress.Percentage;
                info.BytesPerSecond = progress.BytesPerSecond;/// 1024 / 1024;
            }, false);
        }
        //void ProgressHandler(TransferProgress pr)
        //{
        //    model.Progress = pr.Percentage;
        //}
        private void OpenSourceClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if(dialog.ShowDialog() == true)
            {
                model.Source = dialog.FileName;
            }
        }
        private void OpenDestClick(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog  dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if(dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                model.Destination = dialog.FileName;
            }
        }
    }
    [AddINotifyPropertyChangedInterface]
    class ViewModel
    {
        private ObservableCollection<CopyProcessInfo> processes;
        public ViewModel()
        {
            processes = new ObservableCollection<CopyProcessInfo>();
        }
        public IEnumerable<CopyProcessInfo> Processes => processes;
        public string Source { get; set; }
        public string Destination { get; set; }
        public double Progress { get; set; }
        public bool IsWaiting => Progress == 0;
        public void AddProcesses(CopyProcessInfo info)
        {
            processes.Add(info);
        }
    }
    [AddINotifyPropertyChangedInterface]
    class CopyProcessInfo
    {
        public CopyProcessInfo(string filename)
        {
            FileName = filename;
        }
        public string FileName { get; set; }
        public double Percentage { get; set; }
        public int PercentageInt => (int)Percentage;
        public double BytesPerSecond { get; set; }
        public double MegaBytesPerSecond => Math.Round(BytesPerSecond / 1024 / 1024, 2);
    }
}
