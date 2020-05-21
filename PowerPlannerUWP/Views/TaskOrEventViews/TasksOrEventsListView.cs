using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PowerPlannerUWP.Views.TaskOrEventViews
{
    public class TasksOrEventsListView : ListView
    {
        public TasksOrEventsListView()
        {
            Style = (Style)Application.Current.Resources["BlankListViewStyle"];
            ItemTemplate = (DataTemplate)Application.Current.Resources["DataTemplateTaskOrEvent"];
        }
    }
}
