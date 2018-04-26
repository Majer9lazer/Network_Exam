using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UI_For_NetworkProg.UserData.GroupInfo;
using UI_For_NetworkProg.UserData.StudentInfo;

namespace ChatFor_StudentsAndTeachers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BackgroundWorker _getGroupsByNameWorker = new BackgroundWorker();
        private  readonly BackgroundWorker _getStudentByGroupNameWorker= new BackgroundWorker();
        public MainWindow()
        {
            InitializeComponent();
            _getGroupsByNameWorker.DoWork += _getGroupsByNameWorker_DoWork;
            _getStudentByGroupNameWorker.DoWork += _getStudentByGroupNameWorker_DoWork;
        }

        private void _getStudentByGroupNameWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            StudentsInCurrentGroup.Dispatcher.InvokeAsync(() =>
            {   
                StudentsInCurrentGroup.ItemsSource = Group.GetListOfStudentsByGroupName(((Group)GroupInfoListView.SelectedItem).GroupName, StudenNameTextBox.Text);
            });
        }

        private void _getGroupsByNameWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            GroupInfoListView.Dispatcher.InvokeAsync(() =>
            {
                GroupInfoListView.ItemsSource = Group.GetCurrentGroupByName(GroupNameTextBox.Text);
            });
        }

        private void GroupNameTextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            _getGroupsByNameWorker.RunWorkerAsync();
        }

        private void StudenNameTextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            _getStudentByGroupNameWorker.RunWorkerAsync();
        }


        //private void GroupInfoListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    var selectedItem =(Group) GroupInfoListView.SelectedItem;
            
        //    var ss = selectedItem.ToString();
        //}
    }
}
