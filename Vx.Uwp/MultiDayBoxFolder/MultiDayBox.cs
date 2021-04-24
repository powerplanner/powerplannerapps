using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace InterfacesUWP
{
    public class MultiDayBox : ListBox
    {
        public MultiDayBox()
        {
            base.SelectionMode = Windows.UI.Xaml.Controls.SelectionMode.Multiple;

            base.ItemsSource = new DayOfWeek[]
            {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday,
                DayOfWeek.Saturday,
                DayOfWeek.Sunday
            };
        }

        public List<DayOfWeek> SelectedDays
        {
            get
            {
                if (base.SelectedItems == null)
                    return new List<DayOfWeek>();

                List<DayOfWeek> answer = base.SelectedItems.OfType<DayOfWeek>().Distinct().ToList();
                answer.Sort();
                return answer;
            }

            set
            {
                if (value == null)
                {
                    base.SelectedItems.Clear();
                }

                else
                {
                    //add all the items that are not already selected
                    foreach (DayOfWeek newDay in value.Except(base.SelectedItems.OfType<DayOfWeek>()).ToArray())
                        base.SelectedItems.Add(newDay);

                    //and remove any currently selected items that aren't in the new value
                    foreach (DayOfWeek oldDay in base.SelectedItems.OfType<DayOfWeek>().Except(value).ToArray())
                        base.SelectedItems.Remove(oldDay);
                }
            }
        }
    }
}
