using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using UI_For_NetworkProg.UserData.GroupInfo;

namespace UI_For_NetworkProg.UserData.TeacherInfo
{
    [Serializable]
    public class Teacher
    {
        private static readonly DirectoryInfo PathToDataBase = new DirectoryInfo((AppDomain.CurrentDomain.BaseDirectory)).Parent?.Parent;
        private static readonly FileInfo F1 = new FileInfo(PathToDataBase.FullName + @"/UserData/TeacherInfo/Teachers_Db.xml");
        public static readonly XDocument TeacherDb = XDocument.Load(F1.FullName);

        public string TeacherName { get; set; }
        public string TeacherLastName { get; set; }
        public string TeacherGuid { get; set; }
        public string TeacherMail { get; set; }
        public List<Group> Groups { get; set; }

        public static Teacher GetTeacherByName(string teacherName)
        {
            var teacher = TeacherDb.Root?.Elements().Elements()
                .FirstOrDefault(f => f.Name == nameof(TeacherName) && f.Value == teacherName)?.Parent;

            Teacher teacherToReturn = new Teacher();
            foreach (XElement e in teacher.Elements())
            {
                switch (e.Name.LocalName)
                {
                    case "TeacherName": { teacherToReturn.TeacherName = e.Value; break; }
                    case "TeacherLastName": { teacherToReturn.TeacherLastName = e.Value; break; }
                    case "TeacherGuid": { teacherToReturn.TeacherGuid = e.Value; break; }
                    case "TeacherMail": { teacherToReturn.TeacherMail = e.Value; break; }
                }
            }

            return teacherToReturn;
        }

    }
}
