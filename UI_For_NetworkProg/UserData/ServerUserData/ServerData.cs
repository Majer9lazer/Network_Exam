using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RestSharp;
using UI_For_NetworkProg.UserData.GroupInfo;
using UI_For_NetworkProg.UserData.StudentInfo;
using UI_For_NetworkProg.UserData.TeacherInfo;

namespace UI_For_NetworkProg.UserData.ServerUserData
{
    public class ServerData
    {
        public static string UrlToServer = @"http://localhost:50786/";
        public static RestClient Client = new RestClient(@"http://localhost:50786/");
        public List<Student> Students = new List<Student>();
        public List<Group> Groups = new List<Group>();
        public List<Teacher> Teachers = new List<Teacher>();
        public void GetAllDataFromServer()
        {
            //Students = GetStudentsFromServer();
            Groups = GetGroupsFromServer();
            Teachers = GetTeachersFromServer();
        }
        private List<Student> GetStudentsFromServer() => JsonConvert.DeserializeObject<List<Student>>(Client.Execute(new RestRequest("GetStudents", Method.GET)).Content);

        private List<Group> GetGroupsFromServer() => JsonConvert.DeserializeObject<List<Group>>(Client.Execute(new RestRequest("GetGroups", Method.GET)).Content);

        public List<Teacher> GetTeachersFromServer() => JsonConvert.DeserializeObject<List<Teacher>>(Client.Execute(new RestRequest("GetTeachers", Method.GET)).Content);

        public List<Teacher> GetTeachersByName(string teacherName) => (string.IsNullOrEmpty(teacherName)
            ? Teachers
            : Teachers.Where(w => w.TeacherName.Contains(teacherName))).ToList();

        public List<Teacher> GetTeacherInGroup(string groupName)=>
            Teachers.Select(s => new Teacher
            {
                TeacherName = s.TeacherName,
                Groups = s.Groups.Where(w => w.GroupName == groupName).ToList()
            }).ToList();
            
        public Teacher GetTeacherByName(string teacherName) => string.IsNullOrEmpty(teacherName)
            ? null
            : Teachers.FirstOrDefault(f => f.TeacherName == teacherName);
        public List<Student> GetStudentsByName(string studentName)
        {
            return string.IsNullOrEmpty(studentName)
                ? null
                : Students?.Where(w => w.StudentName.Contains(studentName)).ToList();
        }

        public List<Student> GetStudentsByTeacherName(string teacherName)
        {
            var query = Teachers.FirstOrDefault(f => f.TeacherName == teacherName).Groups;
            return null;
        }
        public List<Group> GetGroupsByName(string groupName) =>
             string.IsNullOrEmpty(groupName)
                ? Groups
                : Groups.Where(w => w.GroupName.ToLower().Contains(groupName.ToLower())).ToList();
        public void Add_Students(Student st) => Students.Add(st);
        public void Add_Groups(Group g) => Groups.Add(g);
        public void Add_Teachers(Teacher t) => Teachers.Add(t);

        public string UploadFileToServer(FileInfo fileInfo, string groupName, string teacherName)
        {
            string s = string.IsNullOrEmpty(fileInfo.FullName)
                    || string.IsNullOrEmpty(groupName)
                    || string.IsNullOrEmpty(teacherName)
                ? throw new ArgumentNullException() :
                "";
            if (string.IsNullOrEmpty(s))
                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        byte[] responce = webClient.UploadFile(UrlToServer + $"UploadFile/{fileInfo.Extension}/{groupName}/{teacherName}", fileInfo.FullName);
                        return Encoding.ASCII.GetString(responce);
                    }

                }
                catch (Exception e)
                {
                    return e.Message;
                }
            return s;
        }
        public ServerData()
        {

        }

        public ServerData(bool initializeRabbitMq)
        {
            //Using rabbit mq
            if (initializeRabbitMq)
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
            }

        }
        public string GetPasswordFromRabbitMq()
        {
            RestRequest request = new RestRequest("GetPasswordFromRabbitMq");
            IRestResponse response = Client.Execute(request);
            return response.Content;

        }

        private readonly IConnectionFactory _connection;
        public void GetSms(string queueName)
        {
            var connection = _connection.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new EventingBasicConsumer(channel);


            channel.BasicConsume(queue: queueName,
                autoAck: true,
                consumer: consumer);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                switch (queueName)
                {
                    case "StudentAdded":
                        Students.Add(JsonConvert.DeserializeObject<Student>(message));
                        break;
                }
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

        }
    }
}
