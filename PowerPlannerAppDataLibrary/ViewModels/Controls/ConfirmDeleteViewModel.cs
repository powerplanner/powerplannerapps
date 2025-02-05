using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.Controls
{
    public class ConfirmDeleteViewModel : PopupComponentViewModel
    {
        private readonly string _title;
        private readonly string _message;
        private readonly string _confirmationCheckBox;

        private TaskCompletionSource<bool> _taskCompletionSource = new TaskCompletionSource<bool>();

        private VxState<bool> _isChecked = new VxState<bool>(false);

        private ConfirmDeleteViewModel(PagedViewModelWithPopups parent, string message, string title, string confirmationCheckBox) : base(parent)
        {
            _title = title;
            _message = message;
            _confirmationCheckBox = confirmationCheckBox;

            Title = title;

            // Listen for if the popup is dismissed
            parent.Popups.CollectionChanged += Popups_CollectionChanged;
        }

        private void Popups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var parent = (Parent as PagedViewModelWithPopups);

            // If the popup has been dismissed
            if (!parent.Popups.Contains(this))
            {
                // If we haven't already set the result
                if (!_taskCompletionSource.Task.IsCompleted)
                {
                    // Set it as false
                    _taskCompletionSource.TrySetResult(false);
                }

                // Unsubscribe from the popup changes
                parent.Popups.CollectionChanged -= Popups_CollectionChanged;
            }
        }

        protected override View Render()
        {
            return new LinearLayout
            {
                Children =
                {
                    new ScrollView
                    {
                        Content = new LinearLayout
                        {
                            Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, Theme.Current.PageMargin + NookInsets.Top, Theme.Current.PageMargin + NookInsets.Right, Theme.Current.PageMargin),
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = _message
                                }
                            }
                        }
                    }.LinearLayoutWeight(1),

                    new LinearLayout
                    {
                        Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, Theme.Current.PageMargin, Theme.Current.PageMargin + NookInsets.Right, Theme.Current.PageMargin + NookInsets.Bottom),
                        Children =
                        {
                            new CheckBox
                            {
                                IsChecked = VxValue.Create(_isChecked.Value, v => _isChecked.Value = v),
                                Text = _confirmationCheckBox,
                                Margin = new Thickness(0, 12, 0, 6)
                            },

                            new LinearLayout
                            {
                                Orientation = Orientation.Horizontal,
                                Children =
                                {
                                    new DestructiveButton
                                    {
                                        IsEnabled = _isChecked.Value,
                                        Text = R.S("MenuItemDelete"),
                                        Click = OnDelete,
                                        Margin = new Thickness(0, 0, 6, 0)
                                    }.LinearLayoutWeight(1),

                                    new Button
                                    {
                                        Text = R.S("MenuItemCancel"),
                                        Click = OnCancel,
                                        Margin = new Thickness(6, 0, 0, 0)
                                    }.LinearLayoutWeight(1)
                                }
                            }

                        }
                    }
                }
            };
        }

        private void OnDelete()
        {
            _taskCompletionSource.TrySetResult(true);
            RemoveViewModel();
        }

        private void OnCancel()
        {
            _taskCompletionSource.TrySetResult(false);
            RemoveViewModel();
        }

        public static Task<bool> ShowForResultAsync(string message, string title)
        {
            var parent = PowerPlannerApp.Current.GetMainWindowViewModel().FinalContent.GetPopupViewModelHost();
            var viewModel = new ConfirmDeleteViewModel(parent, message, title, R.S("Settings_DeleteAccountPage_Description.Text"));
            parent.ShowPopup(viewModel);

            return viewModel._taskCompletionSource.Task;
        }
    }
}
