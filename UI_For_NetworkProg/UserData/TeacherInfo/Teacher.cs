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
        #region Properties
        //private static readonly DirectoryInfo PathToDataBase = new DirectoryInfo((AppDomain.CurrentDomain.BaseDirectory)).Parent?.Parent?.Parent;
        //private static readonly FileInfo F1 = new FileInfo(PathToDataBase.FullName + @"/UserData/TeacherInfo/Teachers_Db.xml");
        //public static readonly XDocument TeacherDb = XDocument.Load(F1.FullName);
        #endregion
        public string TeacherName { get; set; }
        public string TeacherLastName { get; set; }
        public string TeacherGuid { get; set; }
        public string TeacherMail { get; set; }
        public List<Group> Groups { get; set; }
        #region GetTeacherByName
        /*
        public static Teacher GetTeacherByName(string teacherName)
        {
            Teacher teacherToReturn = new Teacher();
            var teacher = TeacherDb.Root?.Elements().Elements()
                .FirstOrDefault(f => f.Name == nameof(TeacherName) && f.Value == teacherName)?.Parent;
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
        */
        #endregion
        #region GeTeachersByName
        /*
        public static List<Teacher> GeTeachersByName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var teachers = TeacherDb.Root?.Elements().Elements()
                    .Where(w => w.Name == nameof(TeacherName) && w.Value.Contains(name));

                List<Teacher> teachersList = new List<Teacher>();
                foreach (XElement elem in teachers)
                {
                    Teacher t = new Teacher();
                    foreach (XElement xelem in elem.Parent.Elements())
                    {
                        switch (xelem.Name.LocalName)
                        {
                            case "TeacherName":
                                {
                                    t.TeacherName = xelem.Value;
                                    break;
                                }
                            case "TeacherLastName":
                                {
                                    t.TeacherLastName = xelem.Value;
                                    break;
                                }
                        }
                    }
                    teachersList.Add(t);
                }
                return teachersList;
            }
            else
            {
                var teachers = TeacherDb.Root?.Elements();
                List<Teacher> teachersList = new List<Teacher>();
                foreach (XElement elem in teachers)
                {
                    Teacher t = new Teacher();
                    foreach (XElement el in elem.Elements())
                    {
                        switch (el.Name.LocalName)
                        {
                            case "TeacherName":
                                {
                                    t.TeacherName = el.Value;
                                    break;
                                }
                            case "TeacherLastName":
                                {
                                    t.TeacherLastName = el.Value;
                                    break;
                                }
                        }
                    }
                    teachersList.Add(t);
                }
                return teachersList;
            }
        }
        */
        #endregion
    }
}
