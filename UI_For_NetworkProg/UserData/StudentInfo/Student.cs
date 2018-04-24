using System;

namespace UI_For_NetworkProg.UserData.StudentInfo
{
    [Serializable]
    public class Student
    {
        public Student()
        {

        }
        public Student(string studentName)
        {
            StudentName = studentName;
        }
        public string StudentName { get; set; }
        //public int? StudentAge { get; set; }

    }
}
