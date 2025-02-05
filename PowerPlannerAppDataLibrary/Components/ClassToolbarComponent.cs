using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Components
{
    public class ClassToolbarComponent : VxComponent
    {
        private VxState<int> _selectedIndex = new VxState<int>(0);
        public int SelectedIndex
        {
            get => _selectedIndex.Value;
            set => _selectedIndex.Value = value;
        }

        [VxSubscribe]
        public ClassViewModel ViewModel { get; set; }

        public Action OnPinClass { get; set; }
        public Action OnUnpinClass { get; set; }

        private VxState<bool> _isPinned = new VxState<bool>(false);
        public bool IsPinned
        {
            get => _isPinned;
            set => _isPinned.Value = value;
        }

        protected override View Render()
        {
            return new Toolbar
            {
                Title = ViewModel.ClassName,
                PrimaryCommands =
                {
                    SelectedIndex == 0 ? new MenuItem
                    {
                        Text = PowerPlannerResources.GetString("String_EditClass"),
                        Glyph = MaterialDesign.MaterialDesignIcons.Edit,
                        Click = ViewModel.EditClassWithDetails
                    } : null,

                    SelectedIndex == 1 ? new MenuItem
                    {
                        Text = PowerPlannerResources.GetString("String_EditDetails"),
                        Glyph = MaterialDesign.MaterialDesignIcons.Edit,
                        Click = ViewModel.EditDetails
                    } : null,

                    SelectedIndex == 2 ? new MenuItem
                    {
                        Text = PowerPlannerResources.GetString("String_EditTimes"),
                        Glyph = MaterialDesign.MaterialDesignIcons.Edit,
                        Click = ViewModel.EditTimes
                    } : null,

                    SelectedIndex >= 3 ? new MenuItem
                    {
                        Text = PowerPlannerResources.GetString(SelectedIndex == 3 ? "String_NewTask" : SelectedIndex == 4 ? "String_NewEvent" : "String_NewGrade"),
                        Glyph = MaterialDesign.MaterialDesignIcons.Add,
                        Click = () =>
                        {
                            if (SelectedIndex == 3)
                            {
                                ViewModel.TasksViewModel.Add();
                            }
                            else if (SelectedIndex == 4)
                            {
                                ViewModel.EventsViewModel.Add();
                            }
                            else
                            {
                                ViewModel.GradesViewModel.Add();
                            }
                        }
                    } : null
                },

                SecondaryCommands =
                {
                    OnPinClass != null && OnUnpinClass != null ? new MenuItem
                    {
                        Text = PowerPlannerResources.GetString(IsPinned ? "String_UnpinClass" : "String_PinClass"),
                        Click = IsPinned ? OnUnpinClass : OnPinClass
                    } : null,

                    new MenuItem
                    {
                        Text = PowerPlannerResources.GetString("String_DeleteClass"),
                        Click = async () =>
                        {
                            if (await PowerPlannerApp.ConfirmDeleteAsync(PowerPlannerResources.GetString("String_ConfirmDeleteClassMessage"), PowerPlannerResources.GetString("String_ConfirmDeleteClassHeader"), useConfirmationCheckbox: true))
                            {
                                ViewModel.DeleteClass();
                            }
                        }
                    }
                }
            }.InnerToolbarThemed();
        }
    }
}
