using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.Extensions;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.ClassViews
{
    public sealed partial class ClassSchedulesEditing : UserControl
    {
        public event EventHandler OnRequestClose;

        public ClassSchedulesEditing()
        {
            this.InitializeComponent();
        }

        private IEnumerable<EditingClassScheduleItemView> scheduleEditors
        {
            get { return stackPanel.Children.OfType<EditingClassScheduleItemView>(); }
        }

        private ViewItemClass _class;
        public ViewItemClass Class
        {
            get { return _class; }
            set
            {
                _class = value;

                //remove any current displayed items
                stackPanel.Children.Clear();

                if (value == null)
                    return;

                if (value.Schedules.Count > 0)
                    TextBlockNoTimes.Visibility = Visibility.Collapsed;
                else
                    TextBlockNoTimes.Visibility = Visibility.Visible;

                List<ScheduleCreator> creators = new List<ScheduleCreator>();


                foreach (var s in value.Schedules)
                    addSchedule(s, creators);

                foreach (ScheduleCreator c in creators)
                    displayScheduleCreator(c);
            }
        }

        private UIElement displayScheduleCreator(ScheduleCreator c)
        {
            EditingClassScheduleItemView itemViewEditing = new EditingClassScheduleItemView()
            {
                ScheduleCreator = c,
                Margin = new Thickness(10, 0, 10, 20)
            };

            itemViewEditing.OnDelete += itemViewEditing_OnDelete;


            stackPanel.Children.Add(itemViewEditing);

            return itemViewEditing;
        }

        private List<ScheduleCreator> _deletedCreators = new List<ScheduleCreator>();

        void itemViewEditing_OnDelete(object sender, EventArgs e)
        {
            EditingClassScheduleItemView editing = sender as EditingClassScheduleItemView;

            stackPanel.Children.Remove(editing);

            _deletedCreators.Add(editing.DataContext as ScheduleCreator);

            if (stackPanel.Children.Count == 0)
                TextBlockNoTimes.Visibility = Visibility.Visible;
        }

        private void addSchedule(ViewItemSchedule s, List<ScheduleCreator> creators)
        {
            foreach (ScheduleCreator c in creators)
                if (c.Add(s))
                    return;

            creators.Add(new ScheduleCreator(s));
        }

        private void close()
        {
            if (OnRequestClose != null)
                OnRequestClose(this, new EventArgs());
        }

        public UIElement AddNewSchedule()
        {
            TextBlockNoTimes.Visibility = Visibility.Collapsed;

            return displayScheduleCreator(new ScheduleCreator());
        }

        private async void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            await Save();
        }

        public async System.Threading.Tasks.Task Save()
        {
            try
            {
                List<EditingClassScheduleItemView.ScheduleData> desiredSchedules = scheduleEditors.SelectMany(i => i.GetSchedules()).ToList();

                foreach (var s in desiredSchedules)
                {
                    if (s.StartTime.TimeOfDay > s.EndTime.TimeOfDay)
                    {
                        await new MessageDialog("End times must be greater than start times. Check your schedules and ensure that the end times are greater than the start times.", "Invalid end time").ShowAsync();
                        return;
                    }
                }

                List<DataItemSchedule> currentSchedules = Class.Schedules.Select(i => new DataItemSchedule()
                {
                    Identifier = i.Identifier
                }).ToList();


                DataChanges changes = new DataChanges();


                //if we have more desired schedules than existing, we add some extras
                for (int i = currentSchedules.Count; i < desiredSchedules.Count; i++)
                    currentSchedules.Add(new DataItemSchedule()
                    {
                        Identifier = Guid.NewGuid(),
                        UpperIdentifier = Class.Identifier
                    });


                //if we have less desired schedules than existing, we delete some existing
                while (currentSchedules.Count > desiredSchedules.Count)
                {
                    changes.DeleteItem(currentSchedules.Last().Identifier);
                    currentSchedules.RemoveAt(currentSchedules.Count - 1);
                }


                //now merge changes
                for (int i = 0; i < currentSchedules.Count; i++)
                {
                    DataItemSchedule current = currentSchedules[i];
                    var desired = desiredSchedules[i];

                    current.StartTime = desired.StartTime;
                    current.EndTime = desired.EndTime;
                    current.Room = desired.Room;
                    current.ScheduleWeek = desired.Week;
                    current.DayOfWeek = desired.Day;
                    current.ScheduleType = PowerPlannerSending.Schedule.Type.Normal;

                    changes.Add(current);
                }

                //save changes
                //await App.SaveChanges(changes);

                close();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                await new MessageDialog("Failed to save. Your error has been sent to the developer.", "Error").ShowAsync();
            }
        }
    }
}
