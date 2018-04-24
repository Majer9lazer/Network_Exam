using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
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
        private readonly Random _rnd = new Random();
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
                }

            };
            InitializeComponent();
            //Initialize progress bar work
            _progressBarWorker.DoWork += _progressBarWorker_DoWork;
            _threads.FirstOrDefault(f => f.Name == "_progressBarThread")?.Start();
            //Initialize senmail worker
            _sendMailAsync.DoWork += _sendMailAsync_DoWork;
           

        }



        private void _progressBarWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int maxProgressBarRand = _rnd.Next(100, 1000);
            ResOfWorkingProgressBar.Dispatcher.InvokeAsync(() => { ResOfWorkingProgressBar.Maximum = maxProgressBarRand; });
            for (int i = 1; i <= maxProgressBarRand; i++)
            {
                double i1 = i;
                ResOfWorkingProgressBar.Dispatcher.InvokeAsync(() => { ResOfWorkingProgressBar.Value = i1; });
                Thread.Sleep(_rnd.Next(5, 20));
                PercentsTextBlock.Dispatcher.InvokeAsync(() => { PercentsTextBlock.Text = Math.Round((i1 / maxProgressBarRand) * 100, 2) + "%"; });

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

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Файлы типа(*.cs)|*.cs" + "|Все файлы (*.*)|*.*";
            fileDialog.Multiselect = false;
            fileDialog.CheckFileExists = true;
            if (fileDialog.ShowDialog() == true)
            {
                DropFileLabel.Content = fileDialog.FileName;
            }
        }

        private void SendFileToSolve(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Файл был отправлен успешно , наверное");
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


            SmtpClient client = new SmtpClient(smtpHost, smtpPort);
            client.Credentials = new NetworkCredential(login, password);
            string from = login;
            client.EnableSsl = true;
            
            ErrorOrSuccessTextBlock.Dispatcher.InvokeAsync(() =>
            {

                string to = Teacher.GetTeacherByName(TeacherNameTextBox.Text).TeacherMail;
                string subj = "Восстановка мэйла";
                string body = $"Ваш Guid = {Teacher.GetTeacherByName(TeacherNameTextBox.Text).TeacherGuid}";
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
    }
}
