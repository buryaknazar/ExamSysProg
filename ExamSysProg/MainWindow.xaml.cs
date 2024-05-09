using ExamSysProg.Models;
using ExamSysProg.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExamSysProg
{
    public partial class MainWindow : Window
    {
        private string _startPath;
        private string _targetExtension = ".txt";
        private string _banWordsFilePath;
        private string _copiedFilesPath;

        public MainWindow()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void pnlControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void pnlControl_MouseEnter(object sender, MouseEventArgs e)
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        //Main Logic

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if(!String.IsNullOrEmpty(_startPath) && !String.IsNullOrEmpty(_banWordsFilePath))
            {
                List<string> words = await Task.Run(()=> FileService.ReadFileBanWordsAsync(_banWordsFilePath));
                List<ProgramResults> results = new List<ProgramResults>();

                await Task.Run(()=> SearchAndCopyFilesAsync(_startPath, _targetExtension, words, _copiedFilesPath, results, this.MainProgressBar));

                this.ResultItems.ItemsSource = results;
            }
            else
            {
                MessageBox.Show("Start Folder was not chosen or Ban words file was no loaded!", "Error");
            }
        }

        private void StartFolderButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.Description = "Виберіть папку";
            System.Windows.Forms.DialogResult result = folderBrowserDialog.ShowDialog();

            if(result == System.Windows.Forms.DialogResult.OK)
            {
                _startPath = folderBrowserDialog.SelectedPath;
            }
        }

        private void BanWordsButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            System.Windows.Forms.DialogResult dialogResult = ofd.ShowDialog();
            _banWordsFilePath = ofd.FileName;
        }

        private void ResultsFolderButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.Description = "Виберіть папку";
            System.Windows.Forms.DialogResult result = folderBrowserDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                _copiedFilesPath = folderBrowserDialog.SelectedPath;
            }
        }

        static async Task<List<string>> SearchAndCopyFilesAsync(string directoryPath, string targetExtension, List<string> searchWords, string destinationPath, List<ProgramResults> results, ProgressBar progressBar)
        {
            List<string> resultFiles = new List<string>();

            try
            {
                string[] files = Directory.GetFiles(directoryPath, $"*{targetExtension}");
                int totalFiles = files.Length;
                int processedFiles = 0;

                foreach (string filePath in files)
                {
                    ProgramResults tmpResults = await FileService.FileContainsWord(filePath, searchWords);

                    if (tmpResults.countOfMatches > 0)
                    {
                        resultFiles.Add(filePath);
                        results.Add(tmpResults);

                        string destinationFilePath = System.IO.Path.Combine(destinationPath, System.IO.Path.GetFileName(filePath));
                        await FileService.CopyAndReplaceWordsAsync(filePath, destinationFilePath, searchWords, "****");
                    }

                    processedFiles++;
                    UpdateProgressBar(progressBar, processedFiles, totalFiles);
                }

                string[] subdirectories = Directory.GetDirectories(directoryPath);

                List<Task<List<string>>> searchTasks = new List<Task<List<string>>>();

                foreach (string subdirectory in subdirectories)
                {
                    searchTasks.Add(SearchAndCopyFilesAsync(subdirectory, targetExtension, searchWords, destinationPath, results, progressBar));
                }

                await Task.WhenAll(searchTasks);

                foreach (var task in searchTasks)
                {
                    resultFiles.AddRange(await task);
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Error!", "Error");
            }

            return resultFiles;
        }

        static private void UpdateProgressBar(ProgressBar progressBar, int processedFiles, int totalFiles)
        {
            double progress = (double)processedFiles / totalFiles * 100;
            progressBar.Dispatcher.Invoke(() => progressBar.Value = progress, System.Windows.Threading.DispatcherPriority.Background);
        }
    }
}
