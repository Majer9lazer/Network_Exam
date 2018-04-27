using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace UI_For_NetworkProg.UserData.StudentInfo
{
    [Serializable]
    public class Student
    {
        //Static read only peremennye
        private static readonly DirectoryInfo PathToDataBase = new DirectoryInfo((AppDomain.CurrentDomain.BaseDirectory)).Parent?.Parent?.Parent;
        private static readonly FileInfo UserDataBaseFileInfo = new FileInfo(PathToDataBase.FullName + @"/UserData/StudentInfo/Students_Db.xml");
        public static readonly XDocument StudentDb = XDocument.Load(UserDataBaseFileInfo.FullName);

        public Student() { }
        public Student(string studentName) { StudentName = studentName; }

        public string StudentName { get; set; }

        public static dynamic GetStudents()
        {
            var students = StudentDb.Root?.Elements().Elements().Select(s => new
            {
                student = new Student() { StudentName = s.Value }
            }).ToList();
            return students;

        }

    }
}
