using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.SyncLayer;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using ToolsPortable;
using Vx;
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

        private Func<object, View> _renderClassTemplate;

        public bool IsDesktop { get; } = VxPlatform.Current == Platform.Uwp
                || (VxPlatform.Current == Platform.iOS && SyncExtensions.GetPlatform() == "Mac");

        public ClassesViewModel(BaseViewModel parent) : base(parent)
        {
            _renderClassTemplate = RenderClass;
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
            MainScreenViewModel.ViewClass(viewItemClass, ClassViewModel.LastSelectedPage ?? ClassViewModel.ClassPages.Overview);
        }

        protected override View Render()
        {
            var currentlyHasDesktopSidebar = IsDesktop && !MainScreenViewModel.IsCompactMode;

            View content;

            if (!HasClasses || currentlyHasDesktopSidebar)
            {
                content = RenderInformationalText();
            }
            else
            {
                SubscribeToCollection(MainScreenViewModel.Classes);

                float floatingActionButtonOffset = VxPlatform.Current == Platform.Android ? Theme.Current.PageMargin + FloatingActionButton.DefaultSize : 0;

                content = new LinearLayout
                {
                    BackgroundColor = Theme.Current.BackgroundAlt2Color,
                    Children =
                    {
                        new ListView
                        {
                            Items = MainScreenViewModel.Classes,
                            Padding = new Thickness(0, Theme.Current.PageMargin / 2f, 0, Theme.Current.PageMargin / 2f + floatingActionButtonOffset),
                            ItemTemplate = _renderClassTemplate,
                            ItemClicked = (cObj) =>
                            {
                                var c = cObj as ViewItemClass;
                                OpenClass(c);
                            }
                        }.LinearLayoutWeight(1),

                        new TextButton
                        {
                            Text = R.S("String_ViewYearsAndSemesters"),
                            Margin = new Thickness(Theme.Current.PageMargin, Theme.Current.PageMargin / 2f, Theme.Current.PageMargin, Theme.Current.PageMargin),
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Click = OpenYears
                        }
                    }
                };
            }

            if (VxPlatform.Current == Platform.Android)
            {
                return new FrameLayout
                {
                    Children =
                    {
                        content,

                        new FloatingActionButton
                        {
                            Click = AddClass,
                            Margin = new Thickness(Theme.Current.PageMargin),
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Bottom
                        }
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

        private void OpenYears()
        {
            TelemetryExtension.Current?.TrackEvent("Action_OpenYearsFromClassesPage");
            MainScreenViewModel.OpenOrShowYears();
        }

        private View RenderClass(object classObject)
        {
            float CIRCLE_SIZE = IsDesktop ? 16f : 32f;
            float TEXT_SIZE = IsDesktop ? Theme.Current.BodyFontSize : Theme.Current.SubtitleFontSize;

            var c = classObject as ViewItemClass;
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
                        Width = CIRCLE_SIZE,
                        Height = CIRCLE_SIZE,
                        VerticalAlignment = VerticalAlignment.Center,
                        CornerRadius = CIRCLE_SIZE / 2f
                    },
                    new TextBlock
                    {
                        Text = c.Name,
                        FontSize = TEXT_SIZE,
                        Margin = new Thickness(12,0,0,0),
                        WrapText = false,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            };
        }

        private View RenderInformationalText()
        {
            var currentlyHasDesktopSidebar = IsDesktop && !MainScreenViewModel.IsCompactMode;
            bool showViewYearsAndSemesters;
            if (currentlyHasDesktopSidebar)
            {
                showViewYearsAndSemesters = false;
            }
            else if (!HasClasses)
            {
                showViewYearsAndSemesters = true;
            }
            else
            {
                showViewYearsAndSemesters = false;
            }

            return new LinearLayout
            {
                BackgroundColor = Theme.Current.BackgroundAlt2Color,
                Children =
                {
                    new Border
                    {
                        Content = new LinearLayout
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(Theme.Current.PageMargin),
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
                    }.LinearLayoutWeight(1),

                    showViewYearsAndSemesters ? new TextButton
                    {
                        Text = R.S("String_ViewYearsAndSemesters"),
                        Margin = new Thickness(Theme.Current.PageMargin, 0, Theme.Current.PageMargin, Theme.Current.PageMargin),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Click = OpenYears
                    } : null
                }
            };
        }
    }
}
