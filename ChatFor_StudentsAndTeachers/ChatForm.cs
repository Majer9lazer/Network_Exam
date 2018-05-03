using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace ChatFor_StudentsAndTeachers
{
    public partial class ChatForm : Form
    {
        private delegate void Printer(string data);
        private delegate void Cleaner();

        readonly Printer _printer;
        readonly Cleaner _cleaner;
        private Socket _serverSocket;
        private const string ServerHost = "localhost";
        private const int ServerPort = 9933;
        public ChatForm()
        {
            InitializeComponent();
            _printer = Print;
            _cleaner = ClearChat;
            Connect();
            Thread clientThread = new Thread(Listner) { IsBackground = true };
            clientThread.Start();
        }

        private void InitializeComponent()
        {
        }

        private void Listner()
        {
            while (_serverSocket.Connected)
            {
                byte[] buffer = new byte[8196];
                int bytesRec = _serverSocket.Receive(buffer);
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRec);
                if (data.Contains("#updatechat"))
                {
                    UpdateChat(data);
                }
            }
        }
        private void Connect()
        {
            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry(ServerHost);
                IPAddress ipAddress = ipHost.AddressList[0];
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, ServerPort);
                _serverSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.Connect(ipEndPoint);
            }
            catch { Print("Сервер недоступен!"); }
        }
        private void ClearChat()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(_cleaner);
                return;
            }
            chatBox.Clear();
        }
        private void UpdateChat(string data)
        {
            //#updatechat&userName~data|username~data
            ClearChat();
            string[] messages = data.Split('&')[1].Split('|');
            int countMessages = messages.Length;
            if (countMessages <= 0) return;
            for (int i = 0; i < countMessages; i++)
            {
                try
                {
                    if (string.IsNullOrEmpty(messages[i])) continue;
                    Print(String.Format("[{0}]:{1}.", messages[i].Split('~')[0], messages[i].Split('~')[1]));
                }
                catch { continue; }
            }
        }
        private void Send(string data)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                int bytesSent = _serverSocket.Send(buffer);
            }
            catch { Print("Связь с сервером прервалась..."); }
        }
        private void Print(string msg)
        {
            if (InvokeRequired)
            {
                Invoke(_printer, msg);
                return;
            }
            if (chatBox.Text.Length == 0)
                chatBox.AppendText(msg);
            else
                chatBox.AppendText(Environment.NewLine + msg);
        }

        private void enterChat_Click(object sender, EventArgs e)
        {
            string name = userName.Text;
            if (string.IsNullOrEmpty(name)) return;
            Send("#setname&" + name);
            chatBox.Enabled = true;
            chat_msg.Enabled = true;
            chat_send.Enabled = true;
            userName.Enabled = false;
            enterChat.Enabled = false;
        }

        private void chat_send_Click(object sender, EventArgs e)
        {
            SendMessage();
        }
        private void SendMessage()
        {
            try
            {
                string data = chat_msg.Text;
                if (string.IsNullOrEmpty(data)) return;
                Send("#newmsg&" + data);
                chat_msg.Text = string.Empty;
            }
            catch { MessageBox.Show(@"Ошибка при отправке сообщения!"); }
        }
        private void chatBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
                SendMessage();
        }
        private void chat_msg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
                SendMessage();
        }
    }
}
