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

        private Func<object, View> _renderClassMobileTemplate;
        private Func<object, View> _renderClassTemplate;

        public ClassesViewModel(BaseViewModel parent) : base(parent)
        {
            _renderClassMobileTemplate = RenderClassMobile;
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
            MainScreenViewModel.ViewClass(viewItemClass);
        }

        protected override View Render()
        {
            if (VxPlatform.Current == Platform.iOS || VxPlatform.Current == Platform.Android)
            {
                View mobileContent;

                // These platforms always are in compact mode, and they have headers built-in
                if (!HasClasses)
                {
                    mobileContent = RenderInformationalText();
                }
                else
                {
                    SubscribeToCollection(MainScreenViewModel.Classes);

                    float floatingActionButtonOffset = VxPlatform.Current == Platform.Android ? Theme.Current.PageMargin + FloatingActionButton.DefaultSize : 0;

                    mobileContent = new ListView
                    {
                        Items = MainScreenViewModel.Classes,
                        Padding = new Thickness(0, Theme.Current.PageMargin / 2f, 0, Theme.Current.PageMargin / 2f + floatingActionButtonOffset),
                        ItemTemplate = _renderClassMobileTemplate,
                        ItemClicked = (cObj) =>
                        {
                            var c = cObj as ViewItemClass;
                            OpenClass(c);
                        }
                    };
                }

                if (VxPlatform.Current == Platform.Android)
                {
                    return new FrameLayout
                    {
                        Children =
                        {
                            mobileContent,

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

                return mobileContent;
            }

            View content;

            if (!HasClasses || !MainScreenViewModel.IsCompactMode)
            {
                content = RenderInformationalText();
            }
            else
            {
                SubscribeToCollection(MainScreenViewModel.Classes);

                content = new ListView
                {
                    Items = MainScreenViewModel.Classes,
                    Padding = new Thickness(0, Theme.Current.PageMargin / 2f, 0, Theme.Current.PageMargin / 2f),
                    ItemTemplate = _renderClassTemplate,
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

        private View RenderClass(object classObject)
        {
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
        }

        private View RenderClassMobile(object classObject)
        {
            var c = classObject as ViewItemClass;
            return new LinearLayout
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(Theme.Current.PageMargin, Theme.Current.PageMargin / 2f, Theme.Current.PageMargin, Theme.Current.PageMargin / 2f),
                Children =
                {
                    new Border
                    {
                        BackgroundColor = ColorBytesHelper.ToColor(c.Color),
                        Width = 36,
                        Height = 36,
                        VerticalAlignment = VerticalAlignment.Center,
                        CornerRadius = 36,
                        Content = new TextBlock
                        {
                            Text = "" + c.Name?.FirstOrDefault(),
                            TextColor = System.Drawing.Color.White,
                            FontSize = 16,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    },
                    new TextBlock
                    {
                        Text = c.Name,
                        FontSize = Theme.Current.SubtitleFontSize,
                        Margin = new Thickness(12,0,0,0),
                        WrapText = false,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            };
        }

        private View RenderInformationalText()
        {
            return new Border
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
    }
}
