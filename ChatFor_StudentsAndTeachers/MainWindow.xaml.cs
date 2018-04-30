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
using UI_For_NetworkProg.UserData.ServerUserData;
using UI_For_NetworkProg.UserData.StudentInfo;

namespace ChatFor_StudentsAndTeachers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BackgroundWorker _getGroupsByNameWorker = new BackgroundWorker();
        private readonly BackgroundWorker _getStudentByGroupNameWorker = new BackgroundWorker();
        public MainWindow()
        {
            InitializeComponent();
           

            _getGroupsByNameWorker.DoWork += _getGroupsByNameWorker_DoWork;
            _getStudentByGroupNameWorker.DoWork += _getStudentByGroupNameWorker_DoWork;

        }

        private void _getStudentByGroupNameWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                StudentsInCurrentGroup.Dispatcher.InvokeAsync(() =>
                {
                    // StudentsInCurrentGroup.ItemsSource = Group.GetListOfStudentsByGroupName(((Group)GroupInfoListView.SelectedItem).GroupName, StudenNameTextBox.Text);
                });
            }
            catch (Exception ex)
            {
                ErrorOrSuccesTextBlock.Dispatcher.InvokeAsync(() => { ErrorOrSuccesTextBlock.Text += ex; });
            }
        }

        private void _getGroupsByNameWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                //  GroupInfoListView.Dispatcher.InvokeAsync(() => { GroupInfoListView.ItemsSource = Group.GetCurrentGroupByName(GroupNameTextBox.Text); });
            }
            catch (Exception ex) { ErrorOrSuccesTextBlock.Dispatcher.InvokeAsync(() => { ErrorOrSuccesTextBlock.Text += ex; }); }
        }

        private void GroupNameTextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            _getGroupsByNameWorker.RunWorkerAsync();
        }

        private void StudenNameTextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            _getStudentByGroupNameWorker.RunWorkerAsync();
        }

        private void SendMessage_OnClick(object sender, RoutedEventArgs e)
        {
            MessageTextBlock.Text += "\n" + InputMessageTextBox.Text;
            MessagesScrollViewer.ScrollToEnd();
            InputMessageTextBox.Text = "";
        }
    }
}
