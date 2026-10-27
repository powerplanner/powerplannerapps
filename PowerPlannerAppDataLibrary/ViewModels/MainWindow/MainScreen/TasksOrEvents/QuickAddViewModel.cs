using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewItems;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents
{
    public class QuickAddViewModel : PopupComponentViewModel
    {
        public QuickAddViewModel(BaseViewModel parent) : base(parent)
        {
            Title = R.S("QuickAddPage.Title");
        }

        protected override View Render()
        {
            if (!IsLoaded)
            {
                return new LinearLayout();
            }

            bool noClasses = MainScreenViewModel.Classes == null || MainScreenViewModel.Classes.Count == 0;

            return new LinearLayout
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(
                    Theme.Current.PageMargin + NookInsets.Left,
                    Theme.Current.PageMargin,
                    Theme.Current.PageMargin + NookInsets.Right,
                    Theme.Current.PageMargin + NookInsets.Bottom),
                Children =
                {
                    noClasses ? new TextBlock
                    {
                        Text = "No classes",
                        TextAlignment = HorizontalAlignment.Center,
                        FontSize = Theme.Current.SubtitleFontSize
                    } : null,

                    noClasses ? new TextBlock
                    {
                        Text = "You don't have any classes. Make sure you've opened a semester that has classes.",
                        TextAlignment = HorizontalAlignment.Center,
                        FontSize = Theme.Current.SubtitleFontSize,
                        TextColor = Theme.Current.SubtleForegroundColor,
                        Margin = new Thickness(0, 6, 0, 0)
                    } : null,

                    noClasses ? null : new AccentButton
                    {
                        Text = R.S("String_AddTask"),
                        Click = AddTask
                    },

                    noClasses ? null : new AccentButton
                    {
                        Text = R.S("String_AddEvent"),
                        Click = AddEvent,
                        Margin = new Thickness(0, 12, 0, 0)
                    }
                }
            };
        }

        public void AddTask()
        {
            AddItem(TaskOrEventType.Task);
        }

        public void AddEvent()
        {
            AddItem(TaskOrEventType.Event);
        }

        private async void AddItem(TaskOrEventType type)
        {
            MainScreenViewModel.ShowPopup(AddTaskOrEventViewModel.CreateForAdd(MainScreenViewModel, new AddTaskOrEventViewModel.AddParameter()
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
