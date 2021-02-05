using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;

namespace PowerPlannerUWP.Helpers
{
    class TaskOrEventFlyout
    {
        private ViewItemTaskOrEvent _item;
        public TaskOrEventFlyout(ViewItemTaskOrEvent item)
        {
            _item = item;
        }

        /* Actions For Flyout buttons */

        private void Flyout_Edit(object sender, RoutedEventArgs e)
        {
            PowerPlannerApp.Current.GetMainScreenViewModel()?.EditTaskOrEvent(_item);
        }

        private void Flyout_Duplicate(object sender, RoutedEventArgs e)
        {
            PowerPlannerApp.Current.GetMainScreenViewModel()?.DuplicateTaskOrEvent(_item);
        }

        private async void Flyout_Delete(object sender, RoutedEventArgs e)
        {
            if (await App.ConfirmDelete(LocalizedResources.GetString("String_ConfirmDeleteItemMessage"), LocalizedResources.GetString("String_ConfirmDeleteItemHeader")))
            {
                PowerPlannerApp.Current.GetMainScreenViewModel()?.DeleteItem(_item);
            }
        }

        private void Flyout_ConvertType(object sender, RoutedEventArgs e)
        {
            PowerPlannerApp.Current.GetMainScreenViewModel()?.ConvertTaskOrEventType(_item);
        }

        private void Flyout_ToggleComplete(object sender, RoutedEventArgs e)
        {
            // New percent complete toggles completion; If there's any progress, remove it, otherwise set it to complete
            double newPercentComplete = _item.IsComplete ? 0 : 1;
            PowerPlannerApp.Current.GetMainScreenViewModel()?.SetTaskOrEventPercentComplete(_item, newPercentComplete);
        }

        private void Flyout_GoToClass(object sender, RoutedEventArgs e)
        {
            // Get initial page (if it's a task/event, go to that page)
            ClassViewModel.ClassPages? initialPage = null;
            if (_item.IsTask) initialPage = ClassViewModel.ClassPages.Tasks;
            else if (_item.IsTask) initialPage = ClassViewModel.ClassPages.Events;

            // Navigate to class
            PowerPlannerApp.Current.GetMainScreenViewModel()?.ViewClass(_item.Class, initialPage);
        }

        /* Generate Flyout Menu */

        public MenuFlyout GetFlyout(bool showGoToClass = false)
        {
            MenuFlyout flyout = new MenuFlyout();

            // We cannot add items with `Click` bindings directly to `flyout`
            // because the `Click` property cannot be assigned but is rather appended to
            var editItem = new MenuFlyoutItem()
            {
                Text = "Edit"
            };
            editItem.Click += Flyout_Edit;
            flyout.Items.Add(editItem);

            var duplicateItem = new MenuFlyoutItem()
            {
                Text = "Duplicate",
                Icon = new SymbolIcon(Symbol.Copy)
            };
            duplicateItem.Click += Flyout_Duplicate;
            flyout.Items.Add(duplicateItem);

            var deleteItem = new MenuFlyoutItem()
            {
                Text = "Delete",
                Icon = new SymbolIcon(Symbol.Delete)
            };
            deleteItem.Click += Flyout_Delete;
            flyout.Items.Add(deleteItem);

            flyout.Items.Add(new MenuFlyoutSeparator());

            var gradeWeightFlyout = new MenuFlyoutSubItem
            {
                Text = "Grade Weight Category",
            };

            // Populate flyout subitem
            foreach (var weight in GetGradeWeightItems(_item)) { gradeWeightFlyout.Items.Add(weight); };
            flyout.Items.Add(gradeWeightFlyout);

            flyout.Items.Add(new MenuFlyoutSeparator());

            var convertTypeItem = new MenuFlyoutItem
            {
                Text = !_item.IsTask ? LocalizedResources.GetString("String_ConvertToTask") : LocalizedResources.GetString("String_ConvertToEvent")
            };
            convertTypeItem.Click += Flyout_ConvertType;
            flyout.Items.Add(convertTypeItem);

            // Only show "Toggle Complete" item if it's a task
            if (_item.IsTask)
            {
                flyout.Items.Add(new MenuFlyoutSeparator());

                var toggleCompleteItem = new MenuFlyoutItem
                {
                    // We want to mark something complete only if it isn't complete
                    Text = !_item.IsComplete ? "Mark Complete" : "Mark Incomplete",
                    Icon = new SymbolIcon(!_item.IsComplete ? Symbol.Accept : Symbol.Cancel)
                };
                toggleCompleteItem.Click += Flyout_ToggleComplete;
                flyout.Items.Add(toggleCompleteItem);
            }

            if (showGoToClass)
            {
                flyout.Items.Add(new MenuFlyoutSeparator());
                var goToClassItem = new MenuFlyoutItem
                {
                    Text = "Go To Class"
                };
                goToClassItem.Click += Flyout_GoToClass;
                flyout.Items.Add(goToClassItem);
            }

            return flyout;
        }

        /* Utilities */

        public static List<RadioMenuFlyoutItem> GetGradeWeightItems(ViewItemTaskOrEvent item)
        {
            // Set the new grade weight of item
            Action<object, RoutedEventArgs> setNewGradeWeight(ViewItemWeightCategory newWeightCategory) 
            {
                return delegate (object sender, RoutedEventArgs e)
                {
                    DataChanges changes = new DataChanges();

                    var dataItem = (DataItemMegaItem)item.DataItem;
                    dataItem.WeightCategoryIdentifier = newWeightCategory.Identifier;

                    changes.Add(dataItem);

                    PowerPlannerApp.Current.SaveChanges(changes);
                };
            };

            // Map grade weight categories to RadioMenuFlyoutItems
            return  GetWeightCategories(item.Class)
                    .Select(weight =>
                        {
                            var flyout = new RadioMenuFlyoutItem
                            {
                                Text = weight.Name,
                                GroupName = "GradeWeightCategories",
                                IsChecked = weight.Identifier == item.WeightCategory?.Identifier,
                            };

                            flyout.Click += new RoutedEventHandler(setNewGradeWeight(weight));

                            return flyout;
                        }
                    ).ToList();
        }

        // From AddTaskOrEventViewModel. May be able to clean up with a reference
        private static ViewItemWeightCategory[] GetWeightCategories(ViewItemClass c)
        {
            if (c.WeightCategories == null)
            {
                throw new NullReferenceException("ViewItemClass.WeightCategories was null. ClassId: " + c.Identifier + ".");
            }

            List<ViewItemWeightCategory> answer = new List<ViewItemWeightCategory>(c.WeightCategories.Count + 2);

            answer.Add(ViewItemWeightCategory.UNASSIGNED);

            answer.AddRange(c.WeightCategories);

            answer.Add(ViewItemWeightCategory.EXCLUDED);

            return answer.ToArray();
        }
    }
}
