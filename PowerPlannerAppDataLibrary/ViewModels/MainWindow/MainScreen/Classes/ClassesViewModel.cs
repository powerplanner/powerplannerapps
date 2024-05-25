using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Classes
{
    public class ClassesViewModel : BaseMainScreenViewModelChild
    {
        private bool _hasClasses;
        public bool HasClasses
        {
            get => _hasClasses;
            set => SetProperty(ref _hasClasses, value, nameof(HasClasses));
        }

        public ClassesViewModel(BaseViewModel parent) : base(parent)
        {
            MainScreenViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(MainScreenViewModel_PropertyChanged).Handler;
            MainScreenViewModel.Classes.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(Classes_CollectionChanged).Handler;
            HasClasses = MainScreenViewModel.Classes.Any();
        }

        private void MainScreenViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainScreenViewModel.IsCompactMode))
            {
                MarkDirty();
            }
        }

        private void Classes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HasClasses = MainScreenViewModel.Classes.Any();
        }

        public void AddClass()
        {
            MainScreenViewModel.AddClass();
        }

        public void OpenClass(ViewItemClass viewItemClass)
        {
            MainScreenViewModel.ViewClass(viewItemClass);
        }

        protected override View Render()
        {
            View content;

            if (!HasClasses || !MainScreenViewModel.IsCompactMode)
            {
                content = new Border
                {
                    Content = new LinearLayout
                    {
                        Margin = new Thickness(Theme.Current.PageMargin),
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = R.S(HasClasses ? "SelectClassPage_TextBlockHeader.Text" : "ClassesPage_TextBlockNoClassesHeader.Text"),
                                TextAlignment = HorizontalAlignment.Center,
                                WrapText = true
                            }.TitleStyle(),

                            new TextBlock
                            {
                                Text = R.S(HasClasses ? "SelectClassPage_TextBlockExplanation.Text" : "ClassesPage_TextBlockNoClassesDescription.Text"),
                                TextAlignment = HorizontalAlignment.Center,
                                WrapText = true,
                                TextColor = Theme.Current.SubtleForegroundColor
                            }
                        }
                    }
                };
            }
            else
            {
                SubscribeToCollection(MainScreenViewModel.Classes);

                content = new ListView
                {
                    Items = MainScreenViewModel.Classes,
                    Padding = new Thickness(0, Theme.Current.PageMargin / 2f, 0, Theme.Current.PageMargin / 2f),
                    ItemTemplate = (cObj) =>
                    {
                        var c = cObj as ViewItemClass;
                        return new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(Theme.Current.PageMargin, Theme.Current.PageMargin / 2f, Theme.Current.PageMargin, Theme.Current.PageMargin / 2f),
                            Children =
                            {
                            new Border
                            {
                                BorderColor = System.Drawing.Color.Black,
                                BorderThickness = new Thickness(1),
                                BackgroundColor = ColorBytesHelper.ToColor(c.Color),
                                Width = 14,
                                Height = 14,
                                VerticalAlignment = VerticalAlignment.Center
                            },
                            new TextBlock
                            {
                                Text = c.Name,
                                FontSize = 14,
                                Margin = new Thickness(12,0,0,0),
                                WrapText = false,
                                VerticalAlignment = VerticalAlignment.Center
                            }
                            }
                        };
                    },
                    ItemClicked = (cObj) =>
                    {
                        var c = cObj as ViewItemClass;
                        OpenClass(c);
                    }
                };
            }

            return new LinearLayout
            {
                Children =
                {
                    new Toolbar
                    {
                        Title = MainScreenViewModel.MainMenuItemToString(NavigationManager.MainMenuSelections.Classes),
                        PrimaryCommands =
                        {
                            new MenuItem
                            {
                                Text = R.S("SchedulePage_ButtonAddClass.Content"),
                                Glyph = MaterialDesign.MaterialDesignIcons.Add,
                                Click = AddClass
                            }
                        }
                    }.InnerToolbarThemed(),

                    content.LinearLayoutWeight(1)
                }
            };
        }
    }
}
