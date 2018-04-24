using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml.Linq;
using System.Xml.Serialization;
using UI_For_NetworkProg.UserData.StudentInfo;

namespace UI_For_NetworkProg.UserData.GroupInfo
{
    [Serializable]
    public class Group
    {
        private static readonly DirectoryInfo PathToDataBase = new DirectoryInfo((AppDomain.CurrentDomain.BaseDirectory)).Parent?.Parent;
        private static readonly FileInfo F1 = new FileInfo(PathToDataBase.FullName + @"/UserData/GroupInfo/Group_Db.xml");
        private readonly XDocument _groupDb = XDocument.Load(F1.FullName);


        public string GroupName { get; set; }
        public int? CountOfPupils { get; set; }
        public List<Student> Students = new List<Student>();
        public bool AddGroupWithSudents()
        {
            if (Students.Count <= 0 || _groupDb.Root == null) return false;
            if (!IsGroupIsExists(GroupName))
            {
                _groupDb.Root.Add(new XElement("group", new XElement(nameof(GroupName), GroupName)));
                _groupDb.Save(F1.FullName);
                _groupDb.Root?.Elements().Elements().FirstOrDefault(w => w.Value == GroupName)?.Parent?.Add(new XElement("students"));
                _groupDb.Save(F1.FullName);
                XElement studentElement = _groupDb.Root?.Elements().Elements().FirstOrDefault(f => f.Name == "students");

                foreach (Student s in Students)
                    studentElement?.Add(new XElement(nameof(s.StudentName), s.StudentName));

                _groupDb.Save(F1.FullName);
            }
            else
            {
                List<string> group = _groupDb.Root?.Elements().Elements().FirstOrDefault(f => f.Name == nameof(GroupName) && f.Value == GroupName)?.Parent?.Element("students")?.Elements().Select(s => s.Value).ToList();

                if (@group != null)
                {
                    List<string> both = Students.Select(s => s.StudentName).Intersect(@group).ToList();

                    if (both.Count == 0)
                    {
                        XElement studentElement = _groupDb.Root?.Elements().Elements().FirstOrDefault(f => f.Name == "students");
                        foreach (Student s in Students)
                            studentElement?.Add(new XElement(nameof(s.StudentName), s.StudentName));

                        _groupDb.Save(F1.FullName);
                    }
                    else
                    {
                        foreach (string s in both)
                            if (Students.Exists(e => e.StudentName == s))
                                Students.Remove(Students.ElementAt(Students.IndexOf(Students.FirstOrDefault(f => f.StudentName == s))));

                        return AddGroupWithSudents();
                    }
                }
            }
            return true;
        }
        public bool IsGroupIsExists(string groupName)
        {
            XElement group = _groupDb.Root?.Elements().Elements().FirstOrDefault(f => f.Name == nameof(GroupName) && f.Value == GroupName);
            if (group == null)
                return false;
            return true;
        }

    }
}
