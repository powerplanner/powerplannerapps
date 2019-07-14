using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Homework
{
    public class QuickAddViewModel : BaseMainScreenViewModelChild
    {
        public QuickAddViewModel(BaseViewModel parent) : base(parent)
        {
        }

        public void AddHomework()
        {
            AddItem(AddHomeworkViewModel.ItemType.Homework);
        }

        public void AddExam()
        {
            AddItem(AddHomeworkViewModel.ItemType.Exam);
        }

        private async void AddItem(AddHomeworkViewModel.ItemType type)
        {
            if (MainScreenViewModel.Classes == null || MainScreenViewModel.Classes.Count == 0)
            {
                await new PortableMessageDialog("You don't have any classes. Make sure you've opened a semester that has classes.", "No classes").ShowAsync();
                return;
            }

            MainScreenViewModel.ShowPopup(AddHomeworkViewModel.CreateForAdd(MainScreenViewModel, new AddHomeworkViewModel.AddParameter()
            {
                SemesterIdentifier = MainScreenViewModel.CurrentSemesterId,
                Classes = MainScreenViewModel.Classes,
                SelectedClass = null,
                Type = type,
                DueDate = DateTime.Today
            }));

            RemoveViewModel();
        }
    }
}
