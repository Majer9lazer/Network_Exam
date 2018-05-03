using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace UI_For_NetworkProg.UserData.StudentInfo
{
    [Serializable]
    public class Student
    {
        #region Properties
        //Static read only peremennye
        /*
        private static readonly DirectoryInfo PathToDataBase = new DirectoryInfo((AppDomain.CurrentDomain.BaseDirectory)).Parent?.Parent?.Parent;
        private static readonly FileInfo UserDataBaseFileInfo = new FileInfo(PathToDataBase.FullName + @"/UserData/StudentInfo/Students_Db.xml");
        public static readonly XDocument StudentDb = XDocument.Load(UserDataBaseFileInfo.FullName);
        */
        #endregion
        public Student() { }
        public Student(string studentName) { StudentName = studentName; }

        public string StudentName { get; set; }
        #region GetStudents
        /*
                public static List<Student> GetStudents()
                {
                    var students = StudentDb.Root?.Elements().Elements().Select(s => new  Student
                    {
                        StudentName = s.Value
                    }).ToList();
                    return students;

                }
                */
        #endregion
    }
}
