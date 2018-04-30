using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using UI_For_NetworkProg.UserData.GroupInfo;
using UI_For_NetworkProg.UserData.ServerUserData;
using UI_For_NetworkProg.UserData.StudentInfo;
using UI_For_NetworkProg.UserData.TeacherInfo;

namespace UI_For_NetworkProg
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BackgroundWorker _progressBarWorker = new BackgroundWorker();
        private readonly BackgroundWorker _sendMailAsync = new BackgroundWorker();
        private readonly BackgroundWorker _getTeachersByTeacherName = new BackgroundWorker();
        private readonly BackgroundWorker _getBigDataFromServer = new BackgroundWorker();
        private readonly BackgroundWorker _checkForConnectionToServerWorker = new BackgroundWorker();
        private readonly Random _rnd = new Random();
        private ServerData _server;
        private List<Thread> _threads;
        public MainWindow()
        {
            _threads = new List<Thread>()
            {
                new Thread(()=>{_progressBarWorker.RunWorkerAsync();})
                {
                    Name = "_progressBarThread",Priority= (ThreadPriority)_rnd.Next(0,5)
                },
                new Thread(()=>{_sendMailAsync.RunWorkerAsync();})
                {
                    Name = "_senMailThread",Priority= (ThreadPriority)_rnd.Next(0,5)
                },
                new Thread(()=>{_getTeachersByTeacherName.RunWorkerAsync();})
                {
                    Name = "_getTeachersByTeacherName",Priority= (ThreadPriority)_rnd.Next(0,5)
                }

            };

            InitializeComponent();

            //Initialize worker to check connection and other work to server
            _checkForConnectionToServerWorker.WorkerSupportsCancellation = true;
            _checkForConnectionToServerWorker.DoWork += _checkForConnectionToServerWorker_DoWork;
            _checkForConnectionToServerWorker.RunWorkerAsync();

            //Initialize progress bar work
            _progressBarWorker.DoWork += _progressBarWorker_DoWork;
            _progressBarWorker.WorkerSupportsCancellation = true;
            _progressBarWorker.RunWorkerCompleted += _progressBarWorker_RunWorkerCompleted;
            _threads.FirstOrDefault(f => f.Name == "_progressBarThread")?.Start();

            //Initialize sendmail worker
            _sendMailAsync.DoWork += _sendMailAsync_DoWork;
        }

        private void _checkForConnectionToServerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                //Connect to server
                _server = new ServerData(true);

                //InitializeRabbitMq
                _server.GetSms("StudentAdded");

                //Initialize GetTeacherWorker
                _getTeachersByTeacherName.DoWork += _getTeachersByTeacherName_DoWork;
                _getTeachersByTeacherName.WorkerSupportsCancellation = true;

                //initialize big data worker getter
                _getBigDataFromServer.DoWork += _getBigDataFromServer_DoWork;
                _getBigDataFromServer.WorkerSupportsCancellation = true;
                _getBigDataFromServer.RunWorkerCompleted += _getBigDataFromServer_RunWorkerCompleted;
                _getBigDataFromServer.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                ErrorOrSuccessTextBlock.Dispatcher.InvokeAsync(() => { ErrorOrSuccessTextBlock.Text += ex; });
                _progressBarWorker.CancelAsync();
            }
        }

        private void _progressBarWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ResOfWorkingProgressBar.Dispatcher.InvokeAsync(() => { ResOfWorkingProgressBar.Value = 3000; });
            PercentsTextBlock.Dispatcher.InvokeAsync(() => { PercentsTextBlock.Text = 100 + "%"; });
        }

        private void _getBigDataFromServer_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _threads.FirstOrDefault(f => f.Name == "_getTeachersByTeacherName")?.Start();
            _progressBarWorker.CancelAsync();
        }
        private void _getBigDataFromServer_DoWork(object sender, DoWorkEventArgs e)
        {
            _server.GetAllDataFromServer();
        }
        private void _getTeachersByTeacherName_DoWork(object sender, DoWorkEventArgs e)
        {

            AvaliableTeachersListView.Dispatcher.InvokeAsync(() =>
                {
                    AvaliableTeachersListView.ItemsSource = _server.GeTeachersByName(TeacherNameTextBox.Text);
                });
            Thread.Sleep(20);

        }

        private void _progressBarWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int maxProgressBarRand = 3000;
            ResOfWorkingProgressBar.Dispatcher.InvokeAsync(() => { ResOfWorkingProgressBar.Maximum = maxProgressBarRand; });
            for (int i = 1; i <= maxProgressBarRand; i++)
            {
                switch (_progressBarWorker.CancellationPending)
                {
                    case true:
                        e.Cancel = true;
                        maxProgressBarRand = 0;
                        break;
                    case false:
                        double i1 = i;
                        ResOfWorkingProgressBar.Dispatcher.InvokeAsync(() => { ResOfWorkingProgressBar.Value = i1; });
                        Thread.Sleep(_rnd.Next(5, 20));
                        PercentsTextBlock.Dispatcher.InvokeAsync(() => { PercentsTextBlock.Text = Math.Round((i1 / maxProgressBarRand) * 100, 2) + "%"; });
                        break;
                }
            }
        }

        private void DropFileTextBlock_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] path = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                DropFileLabel.Content = path?[0];
            }
        }

        private void ShowDialogButtonClick(object sender, RoutedEventArgs e)
        {

            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Filter = "Файлы типа(*.cs)|*.cs" + "|Все файлы (*.*)|*.*",
                Multiselect = false,
                CheckFileExists = true
            };
            if (fileDialog.ShowDialog() == true)
            {
                DropFileLabel.Content = fileDialog.FileName;
            }
        }

        private void SendFileToSolve(object sender, RoutedEventArgs e)
        {
            try
            {
                _server.UploadFileToServer(new FileInfo(DropFileLabel.Content.ToString()), ((Group)GroupListView.SelectedItem).GroupName, ((Teacher)AvaliableTeachersListView.SelectedItem).TeacherName);
                ErrorOrSuccessTextBlock.Dispatcher.InvokeAsync(() => { ErrorOrSuccessTextBlock.Text += "\n Файл был отправлен успешно!"; });
            }
            catch (Exception exception)
            {
                ErrorOrSuccessTextBlock.Dispatcher.InvokeAsync(() => { ErrorOrSuccessTextBlock.Text += exception; });
            }
           
        }

        private void ResendButton_OnClick(object sender, RoutedEventArgs e)
        {
            _threads.FirstOrDefault(f => f.Name == "_senMailThread")?.Start();
        }
        private void _sendMailAsync_DoWork(object sender, DoWorkEventArgs e)
        {
            string smtpHost = "smtp.gmail.com";
            int smtpPort = 587;
            string login = "csharp.sdp.162@gmail.com";
            string password = "sdp123456789";
            SmtpClient client = new SmtpClient(smtpHost, smtpPort) { Credentials = new NetworkCredential(login, password) };
            string from = login;
            client.EnableSsl = true;

            ErrorOrSuccessTextBlock.Dispatcher.InvokeAsync(() =>
            {
                string to = _server.GetTeacherByName(TeacherNameTextBox.Text).TeacherMail;
                string subj = "Восстановка мэйла";
                string body = $"Ваш Guid = {_server.GetTeacherByName(TeacherNameTextBox.Text).TeacherGuid}";
                MailMessage message = new MailMessage(from, to, subj, body);
                client.SendCompleted += Client_SendCompleted;
                try
                {
                    client.SendMailAsync(message);
                }
                catch (Exception ex)
                {
                    ErrorOrSuccessTextBlock.Text += ex;
                }
            });
            Thread.Sleep(100);
        }
        private void Client_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ErrorOrSuccessTextBlock.Dispatcher.InvokeAsync(() =>
            {
                ErrorOrSuccessTextBlock.Text += "\nMessage was send successfully";
            });
        }

        private void TeacherNameTextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (!_getTeachersByTeacherName.IsBusy)
            {
                _getTeachersByTeacherName.RunWorkerAsync();
            }
        }

        private void DropTaskFileLabel_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] path = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                DropTaskFileLabel.Content = path?[0];
            }
        }

        private void ThemeNameTextBox_OnKeyUp(object sender, KeyEventArgs e)
        {

        }

        private void AvaliableTeachersListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GroupListView.Dispatcher.InvokeAsync(() => { GroupListView.ItemsSource = ((Teacher)AvaliableTeachersListView.SelectedItem).Groups.OrderBy(o => o.GroupName); });
        }

        private void GroupListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StudentsListView.ItemsSource = ((Group)GroupListView.SelectedItem).Students.OrderBy(o => o.StudentName);
        }
    }
}
