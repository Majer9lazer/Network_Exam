using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RestSharp;
using UI_For_NetworkProg.UserData.GroupInfo;
using UI_For_NetworkProg.UserData.ServerUserData;
using UI_For_NetworkProg.UserData.StudentInfo;
using UI_For_NetworkProg.UserData.TeacherInfo;

namespace ChatFor_StudentsAndTeachers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BackgroundWorker _getGroupsByNameWorker = new BackgroundWorker();
        private readonly BackgroundWorker _getBigDataFromServerBackgroundWorker = new BackgroundWorker();
        private readonly RestClient _client = new RestClient("http://localhost:52312/");
        private readonly ServerData _server = new ServerData();
        private readonly ConnectionFactory _connection;
        public MainWindow()
        {

            string Ip = "localhost";
            string Login = "best";
            string Password = "liza1999";
            _connection = new ConnectionFactory
            {
                HostName = Ip,
                UserName = Login,
                Password = Password,
                VirtualHost = "/"
            };
            InitializeComponent();
            Closed += MainWindow_Closed;
            //Initialize To Get data from server
            _getBigDataFromServerBackgroundWorker.DoWork += _getBigDataFromServerBackgroundWorker_DoWork;
            _getBigDataFromServerBackgroundWorker.WorkerSupportsCancellation = true;
            _getBigDataFromServerBackgroundWorker.RunWorkerCompleted += _getBigDataFromServerBackgroundWorker_RunWorkerCompleted;
            _getBigDataFromServerBackgroundWorker.RunWorkerAsync();

            //Initialize worker to get groups by name
            _getGroupsByNameWorker.DoWork += _getGroupsByNameWorker_DoWork;
            _getGroupsByNameWorker.WorkerSupportsCancellation = true;
            _getGroupsByNameWorker.RunWorkerCompleted += _getGroupsByNameWorker_RunWorkerCompleted;



        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _getBigDataFromServerBackgroundWorker.CancelAsync();
            _getBigDataFromServerBackgroundWorker.Dispose();
            _getGroupsByNameWorker.CancelAsync();
            _getGroupsByNameWorker.Dispose();



            Application.Current.Shutdown();

        }

        private void _getGroupsByNameWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {


        }

        private void _getBigDataFromServerBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }
        private void _getBigDataFromServerBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            switch (_getGroupsByNameWorker.CancellationPending)
            {
                case true:
                    e.Cancel = true;
                    break;
                case false:
                    {
                        try
                        {
                            _server.GetAllDataFromServer();
                        }
                        catch (Exception ex)
                        {
                            ErrorOrSuccesTextBlock.Dispatcher.InvokeAsync(() => { ErrorOrSuccesTextBlock.Text += ex; });
                        }
                        break;
                    }
            }

        }



        private void _getGroupsByNameWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            switch (_getGroupsByNameWorker.CancellationPending)
            {
                case true:
                    e.Cancel = true;
                    break;
                case false:
                    {
                        try
                        {
                            GroupListView.Dispatcher.InvokeAsync(() =>
                            {
                                GroupListView.ItemsSource = _server.GetGroupsByName(GroupNameTextBox.Text);
                            });
                        }
                        catch (Exception ex) { ErrorOrSuccesTextBlock.Dispatcher.InvokeAsync(() => { ErrorOrSuccesTextBlock.Text += ex; }); }
                        break;
                    }
            }

        }

        private void GroupNameTextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            _getGroupsByNameWorker.RunWorkerAsync();
        }
        private void SendMessage_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Message m = new Message()
                {
                    DateOfSending = DateTime.Now,
                    From = TeacherOrStudentListView.SelectedItem.GetType() == typeof(Teacher)
                        ? ((Teacher)TeacherOrStudentListView.SelectedItem).TeacherName
                        : ((Student)TeacherOrStudentListView.SelectedItem).StudentName,
                    GroupName = ((Group)GroupListView.SelectedItem).GroupName,
                    Type = TeacherOrStudentListView.SelectedItem.GetType() == typeof(Teacher)
                        ? "Teacher"
                        : "Student",
                    Msg = InputMessageTextBox.Text
                };
                if (UseRabbitMqRadioButton.IsChecked != null && (bool)UseRabbitMqRadioButton.IsChecked)
                {
                    RestRequest request = new RestRequest("AddMessage/{GroupName}/{Message}", Method.POST);
                    request.AddUrlSegment("GroupName", ((Group)GroupListView.SelectedItem).GroupName);
                    request.AddUrlSegment("Message", InputMessageTextBox.Text);
                    string content;
                    _client.ExecuteAsync(request, response => { content = response.Content; });
                    PublishMessage(m, m.GroupName);
                }
                else
                {
                    Thread receiveThread = new Thread(Receiver);
                    receiveThread.Start();
                    Chat.Send(m);
                    //MessageTextBlock.Dispatcher.InvokeAsync(() =>
                    //{
                    //    MessageTextBlock.Text += $"\n From : {m.Type} , Name : {m.From} , Date : {m.DateOfSending} \n\tMessage : {m.Msg}";
                    //});
                }



                MessagesScrollViewer.ScrollToEnd();
                InputMessageTextBox.Text = null;
            }
            catch (Exception exception)
            {
                ErrorOrSuccesTextBlock.Dispatcher.InvokeAsync(() => { ErrorOrSuccesTextBlock.Text += exception; });
            }
        }
        public void Receiver()
        {
            // Создаем UdpClient для чтения входящих данных
            UdpClient receivingUdpClient = new UdpClient(Chat.localPort);
            receivingUdpClient.JoinMulticastGroup(Chat.remoteIPAddress, 20);
            IPEndPoint RemoteIpEndPoint = null;

            try
            {

                MessageTextBlock.Dispatcher.InvokeAsync(() =>
                    {
                        MessageTextBlock.Text += "\n************Общий чат************";
                    });
                while (true)
                {
                    // Ожидание дейтаграммы
                    byte[] receiveBytes = receivingUdpClient.Receive(
                        ref RemoteIpEndPoint);

                    // Преобразуем и отображаем данные
                    Message m = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(receiveBytes));
                    MessageTextBlock.Dispatcher.InvokeAsync(() =>
                    {
                        MessageTextBlock.Text += $"\n From : {m.Type} , Name : {m.From} , Date : {m.DateOfSending} \n\tMessage : {m.Msg}";
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
        }
        private void OpenChatButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (((Group)GroupListView.SelectedItem).GroupName != null && !string.Equals(TeacherOrStudentListView.SelectedItem.ToString(), null, StringComparison.Ordinal))
                {
                    ChatGrid.Visibility = Visibility.Visible;
                    MessagesScrollViewer.Visibility = Visibility.Visible;
                    GetSmsFromRabbtMq(((Group)GroupListView.SelectedItem).GroupName);
                    ErrorOrSuccesTextBlock.Dispatcher.InvokeAsync(() =>
                        {
                            ErrorOrSuccesTextBlock.Text += "\nChat was opened successfully";
                        });
                }
            }
            catch (Exception exception)
            {
                ErrorOrSuccesTextBlock.Dispatcher.InvokeAsync(() =>
                {
                    ErrorOrSuccesTextBlock.Text += exception;
                });
            }
        }

        private void GetSmsFromRabbtMq(string groupName)
        {

            IConnection connection = _connection.CreateConnection();
            IModel channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: "logs", type: "fanout");

            channel.QueueBind(queue: groupName,
                exchange: "logs",
                routingKey: "");


            EventingBasicConsumer consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                byte[] body = ea.Body;
                Message message = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(body));
                MessageTextBlock.Dispatcher.InvokeAsync(() =>
                {
                    MessageTextBlock.Text += $"\n From : {message.Type} , Name : {message.From} , Date : {message.DateOfSending} \n\tMessage : {message.Msg}";
                });

            };
            channel.BasicConsume(queue: groupName,
                autoAck: true,
                consumer: consumer);
        }
        public void PublishMessage<T>(T message, string queueName) where T : class
        {
            using (var connection = _connection.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: "logs", type: "fanout");

                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));


                    channel.BasicPublish(exchange: "logs",
                        routingKey: queueName,
                        basicProperties: null,
                        body: body);
                    ErrorOrSuccesTextBlock.Dispatcher.InvokeAsync(() =>
                        {
                            ErrorOrSuccesTextBlock.Text += "\nMessage was sent successfully";
                        });
                }
            }


        }
        private void StudentRadioButton_OnChecked(object sender, RoutedEventArgs e)
        {
            TeacherOrStudentListViewGridView.Dispatcher.InvokeAsync(() =>
            {
                TeacherOrStudentListViewGridView.Columns.FirstOrDefault().DisplayMemberBinding = new Binding("StudentName");
                TeacherOrStudentListViewGridView.Columns.FirstOrDefault().Header = "StudentName";
                TeacherOrStudentListView.ItemsSource = ((Group)(GroupListView.SelectedItem)).Students;
            });
            StudentBorder.Dispatcher.InvokeAsync(() =>
                {
                    StudentBorder.Background = (Brush)new BrushConverter().ConvertFrom("#D46A00");

                });
            TeacherBorder.Dispatcher.InvokeAsync(() =>
            {
                TeacherBorder.Background = (Brush)new BrushConverter().ConvertFrom("#FFFFCA82");

            });
        }

        private void TeacherRadioButton_OnChecked(object sender, RoutedEventArgs e)
        {
            TeacherOrStudentListViewGridView.Dispatcher.InvokeAsync(() =>
            {

                TeacherOrStudentListViewGridView.Columns.FirstOrDefault().DisplayMemberBinding =
                    new Binding("TeacherName");
                TeacherOrStudentListViewGridView.Columns.FirstOrDefault().Header = "TeacherName";

                TeacherOrStudentListView.ItemsSource =
                    _server.GetTeacherInGroup(((Group)(GroupListView.SelectedItem)).GroupName).Where(w => w.Groups.Count > 0).ToList();
            });
            TeacherBorder.Dispatcher.InvokeAsync(() =>
            {
                TeacherBorder.Background = (Brush)new BrushConverter().ConvertFrom("#D46A00");

            });
            StudentBorder.Dispatcher.InvokeAsync(() =>
            {
                StudentBorder.Background = (Brush)new BrushConverter().ConvertFrom("#FFFFCA82");

            });
        }

    }
}
