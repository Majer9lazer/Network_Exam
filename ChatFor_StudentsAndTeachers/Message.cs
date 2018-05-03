using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatFor_StudentsAndTeachers
{
    public class Message
    {
        public string From { get; set; }
        public string GroupName { get; set; }
        public DateTime DateOfSending { get; set; }
        public string Type { get; set; }
        public string Msg { get; set; }
    }
}
