using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Components;
using PowerPlannerAppDataLibrary.Converters;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Grade;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class
{
    public class ClassGradesViewModel : BaseClassContentViewModel
    {
        public static readonly string UNASSIGNED_ITEMS_HEADER = "Unassigned items";

        [VxSubscribe]
        public ViewItemsGroups.ClassViewItemsGroup ViewItemsGroup { get; private set; }

        public ClassGradesViewModel(ClassViewModel parent) : base(parent)
        {
            ViewItemsGroup = ClassViewModel.ViewItemsGroupClass;
        }

        protected override async Task LoadAsyncOverride()
        {
            await ClassViewModel.ViewItemsGroupClass.LoadGradesTask;

            Class.WeightCategories.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(WeightCategories_CollectionChanged).Handler;
            UpdateShowWeightCategoriesSummary();

            await base.LoadAsyncOverride();
        }

        private void WeightCategories_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                UpdateShowWeightCategoriesSummary();
            }
            catch { }
        }

        private void UpdateShowWeightCategoriesSummary()
        {
            if (Class.WeightCategories.Count > 1)
            {
                ShowWeightCategoriesSummary = true;
            }
            else if (Class.WeightCategories.Count == 1 && Class.WeightCategories[0].WeightValue != 100)
            {
                ShowWeightCategoriesSummary = true;
            }
            else
            {
                ShowWeightCategoriesSummary = false;
            }
        }

        public ViewItemClass Class
        {
            get { return ClassViewModel.ViewItemsGroupClass.Class; }
        }

        /// <summary>
        /// Opens the list view for editing all the class grade options.
        /// </summary>
        public void ConfigureGrades()
        {
            ClassViewModel.ConfigureGrades();
        }

        private bool _showWeightCategoriesSummary = false;
        public bool ShowWeightCategoriesSummary
        {
            get => _showWeightCategoriesSummary;
            set => SetProperty(ref _showWeightCategoriesSummary, value, nameof(ShowWeightCategoriesSummary));
        }

        private MyAppendedObservableLists<object> _itemsWithHeaders;
        public MyAppendedObservableLists<object> ItemsWithHeaders
        {
            get
            {
                if (_itemsWithHeaders == null)
                {
                    // Shouldn't be null unless exception loading occurred
                    if (Class.WeightCategories == null)
                        _itemsWithHeaders = new MyAppendedObservableLists<object>(new object[] { });
                    else
                    {
                        _itemsWithHeaders = new MyAppendedObservableLists<object>(
                            new ListWithItemSelector(new MySublistsToFlatHeaderedList<ViewItemWeightCategory, BaseViewItemMegaItem>(Class.WeightCategories, SelectGrades, this), (item) =>
                            {
                                if (item is ViewItemTaskOrEvent)
                                {
                                    (item as ViewItemTaskOrEvent).IsUnassignedItem = false;
                                }
                                return item;
                            }),
                            new ListWithItemSelector(new UnassignedItemsHeaderList(ClassViewModel.ViewItemsGroupClass.UnassignedItems)),
                            new ListWithItemSelector(ClassViewModel.ViewItemsGroupClass.UnassignedItems, (item) =>
                            {
                                (item as ViewItemTaskOrEvent).IsUnassignedItem = true;
                                return item;
                            }));
                    }
                }

                return _itemsWithHeaders;
            }
        }

        private class UnassignedItemsHeaderList : ObservableCollection<string>
        {
            public UnassignedItemsHeaderList(MyObservableList<ViewItemTaskOrEvent> unassignedItems)
            {
                unassignedItems.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(UnassignedItems_CollectionChanged).Handler;

                Update(unassignedItems);
            }

            private void UnassignedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                if ((sender as MyObservableList<ViewItemTaskOrEvent>).Count > 0)
                {
                    if (Count == 0)
                    {
                        Add(UNASSIGNED_ITEMS_HEADER);
                    }
                }

                else
                {
                    if (Count > 0)
                    {
                        RemoveAt(0);
                    }
                }
            }

            private void Update(MyObservableList<ViewItemTaskOrEvent> list)
            {
                if (list.Count > 0)
                {
                    if (Count == 0)
                    {
                        Add(UNASSIGNED_ITEMS_HEADER);
                    }
                }

                else
                {
                    if (Count > 0)
                    {
                        RemoveAt(0);
                    }
                }
            }
        }

        private MyObservableList<BaseViewItemMegaItem> SelectGrades(ViewItemWeightCategory weightCategory)
        {
            return weightCategory.Grades;
        }

        public async void Add()
        {
            await TryHandleUserInteractionAsync("Add", async (cancellationToken) =>
            {
                if (Class.WeightCategories.SelectMany(i => i.Grades).Count() >= 5 && !await PowerPlannerApp.Current.IsFullVersionAsync())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    PowerPlannerApp.Current.PromptPurchase(PowerPlannerResources.GetString("MessageFreeGradesLimitReached"));
                    return;
                }

                MainScreenViewModel.ShowPopup(AddGradeViewModel.CreateForAdd(MainScreenViewModel, new AddGradeViewModel.AddParameter()
                {
                    Class = Class
                }));
            });
        }

        public void ShowItem(BaseViewItemMegaItem e)
        {
            MainScreenViewModel.ShowPopup(ViewGradeViewModel.Create(MainScreenViewModel, e));
        }

        public void ShowUnassignedItem(ViewItemTaskOrEvent item)
        {
            MainScreenViewModel.ShowPopup(ViewTaskOrEventViewModel.CreateForUnassigned(MainScreenViewModel, item));
        }

        public void OpenWhatIf()
        {
            if (PowerPlannerApp.ShowClassesAsPopups || VxPlatform.Current == Platform.iOS)
            {
                MainScreenViewModel.ShowPopup(new ClassWhatIfViewModel(MainScreenViewModel, Class));
            }
            else
            {
                MainScreenViewModel.Navigate(new ClassWhatIfViewModel(MainScreenViewModel, Class));
            }
        }

        private const float WidthBreakpoint = 640;

        public override IEnumerable<float> SubscribeToWidthBreakpoints => new float[] { WidthBreakpoint };

        protected override void Initialize()
        {
            _ = LoadAsync();

            base.Initialize();
        }

        protected override View Render()
        {
            if (!IsLoaded)
            {
                return null;
            }

            View finalView;
            float floatingActionButtonOffset = VxPlatform.Current == Platform.Android ? Theme.Current.PageMargin + FloatingActionButton.DefaultSize : 0;

            var topMargin = VxPlatform.Current == Platform.iOS ? Theme.Current.PageMargin : 0;

            // If there's only one default weight category, we show the weight header as a sum rather than /100, since the /100 value is dupe of the percent value.
            bool showWeightHeaderAsSum = !ShowWeightCategoriesSummary && !Class.ShouldAverageGradeTotals;

            // Android/iOS don't support the multi-column views
            if (Size.Width < WidthBreakpoint || VxPlatform.Current == Platform.Android || VxPlatform.Current == Platform.iOS)
            {
                finalView = new ListView
                {
                    Items = ItemsWithHeaders,
                    Padding = new Thickness(0, topMargin, 0, Theme.Current.PageMargin + floatingActionButtonOffset),
                    ItemTemplate = item =>
                    {
                        if (item is ViewItemGrade g)
                        {
                            return RenderGrade(g, i => ShowItem(i), isInWhatIfMode: false);
                        }

                        if (item is ViewItemWeightCategory w)
                        {
                            return RenderHeader(w, showWeightHeaderAsSum);
                        }

                        if (item is string str && str == UNASSIGNED_ITEMS_HEADER)
                        {
                            return RenderUnassignedHeader();
                        }

                        if (item is ViewItemTaskOrEvent t)
                        {
                            if (t.IsUnassignedItem)
                            {
                                return RenderUnassignedItem(t);
                            }
                            else
                            {
                                return RenderGrade(t, i => ShowItem(i), isInWhatIfMode: false);
                            }
                        }

                        return new GradesSummaryComponent
                        {
                            Class = Class,
                            OnRequestConfigureGrades = ConfigureGrades,
                            OnRequestOpenWhatIf = OpenWhatIf,
                            Margin = new Thickness(Theme.Current.PageMargin, 0, Theme.Current.PageMargin, 0)
                        };
                    }
                };
            }

            // Otherwise full-size
            else
            {
                const float minColumnWidth = 280;
                const float columnSpacing = 24;

                var gridPanel = new AdaptiveGradesListComponent
                {
                    Class = Class,
                    OnRequestViewGrade = g => ShowItem(g),
                    ShowWeightHeaderAsSum = showWeightHeaderAsSum
                };

                var unassignedPanel = VxPlatform.Current == Platform.Uwp ? (View)new AdaptiveGridPanel
                {
                    MinColumnWidth = minColumnWidth,
                    ColumnSpacing = columnSpacing
                } : (View)new AdaptiveGridPanelComponent
                {
                    MinColumnWidth = minColumnWidth,
                    ColumnSpacing = columnSpacing
                };

                List<View> unassignedPanelChildren = VxPlatform.Current == Platform.Uwp ? (unassignedPanel as AdaptiveGridPanel).Children : (unassignedPanel as AdaptiveGridPanelComponent).Children;

                foreach (var t in ViewItemsGroup.UnassignedItems)
                {
                    unassignedPanelChildren.Add(RenderUnassignedItem(t, includeMargin: false));
                }

                finalView = new ScrollView
                {
                    Content = new LinearLayout
                    {
                        Margin = new Thickness(Theme.Current.PageMargin, Theme.Current.PageMargin, Theme.Current.PageMargin, Theme.Current.PageMargin + floatingActionButtonOffset),
                        Children =
                        {
                            new GradesSummaryComponent
                            {
                                Class = Class,
                                OnRequestOpenWhatIf = OpenWhatIf,
                                OnRequestConfigureGrades = ConfigureGrades
                            },

                            gridPanel,

                            ViewItemsGroup.HasUnassignedItems ? RenderUnassignedHeader(includeMargin: false) : null,

                            ViewItemsGroup.HasUnassignedItems ? unassignedPanel : null
                        }
                    }
                };
            }

            // Add floating action button for Android
            return WrapInFloatingActionButtonIfNeeded(finalView, Add, new Thickness());
        }

        internal static View WrapInFloatingActionButtonIfNeeded(View view, Action addAction, Thickness nookInsets)
        {
            if (VxPlatform.Current == Platform.Android)
            {
                return new FrameLayout
                {
                    Children =
                    {
                        view,

                        new FloatingActionButton
                        {
                            Click = addAction,
                            Margin = new Thickness(Theme.Current.PageMargin).Combine(nookInsets),
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Bottom
                        }
                    }
                };
            }

            return view;
        }

        private View RenderUnassignedItem(ViewItemTaskOrEvent t, bool includeMargin = true)
        {
            return Components.TaskOrEventListItemComponent.Render(t, this, IncludeMargin: includeMargin, InterceptOnTapped: () => ShowUnassignedItem(t), AllowDrag: true);
        }

        private View RenderUnassignedHeader(bool includeMargin = true)
        {
            return new Border
            {
                BackgroundColor = Theme.Current.BackgroundAlt2Color,
                Content = new TextBlock
                {
                    Text = PowerPlannerResources.GetString("ClassGrades_UnassignedItemsHeader"),
                    FontSize = Theme.Current.SubtitleFontSize,
                    Margin = new Thickness(12, 6, 6, 6),
                    WrapText = false
                },
                Margin = new Thickness(includeMargin ? Theme.Current.PageMargin : 0, 18, includeMargin ? Theme.Current.PageMargin : 0, 3),
                BorderColor = Theme.Current.ForegroundColor.Opacity(0.1),
                BorderThickness = new Thickness(1)
            };
        }

        private class WeightHeaderComponent : VxComponent
        {
            [VxSubscribe] // Subscribing is needed for What If? mode
            public ViewItemWeightCategory Weight { get; set; }

            public bool IncludeMargin { get; set; }

            public bool ShowWeightHeaderAsSum { get; set; }

            protected override View Render()
            {
                return new Border
                {
                    BackgroundColor = Theme.Current.BackgroundAlt2Color,
                    Content = new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                    {
                        new TextBlock
                        {
                            Text = Weight.Name,
                            FontSize = Theme.Current.SubtitleFontSize,
                            Margin = new Thickness(12, 6, 6, 6),
                            WrapText = false
                        }.LinearLayoutWeight(1),

                        new TextBlock
                        {
                            Text = ShowWeightHeaderAsSum ? Weight.WeightAchievedAndTotalStringAsSum : Weight.WeightAchievedAndTotalString,
                            FontSize = Theme.Current.SubtitleFontSize,
                            Margin = new Thickness(0, 6, 6, 6),
                            TextColor = Theme.Current.SubtleForegroundColor,
                            WrapText = false
                        }
                    }
                    },
                    Margin = IncludeMargin ? new Thickness(Theme.Current.PageMargin, 12, Theme.Current.PageMargin, 0) : new Thickness(0, 12, 0, 0),
                    BorderColor = Theme.Current.ForegroundColor.Opacity(0.1),
                    BorderThickness = new Thickness(1)
                };
            }
        }

        internal static View RenderHeader(ViewItemWeightCategory w, bool showWeightHeaderAsSum, bool includeMargin = true)
        {
            return new WeightHeaderComponent
            {
                Weight = w,
                IncludeMargin = includeMargin,
                ShowWeightHeaderAsSum = showWeightHeaderAsSum
            }.AllowDropMegaItemOnWeight(w);
        }

        private static View RenderGrade(BaseViewItemMegaItem i, Action<BaseViewItemMegaItem> onRequestViewGrade, bool isInWhatIfMode, bool includeMargin = true)
        {
            return new GradeListViewItemComponent
            {
                Item = i,
                OnRequestViewGrade = () => onRequestViewGrade(i),
                Margin = includeMargin ? new Thickness(Theme.Current.PageMargin, 0, Theme.Current.PageMargin, 0) : new Thickness(),
                IsInWhatIfMode = isInWhatIfMode
            }.AllowDragViewItem(i);
        }

        public GradesSummaryComponent SummaryComponent { get; private set; }

        internal class AdaptiveGradesListComponent : VxComponent
        {
            public const float MinColumnWidth = 280;
            public const float ColumnSpacing = 24;

            public ViewItemClass Class { get; set; }

            public Action<BaseViewItemMegaItem> OnRequestViewGrade { get; set; }

            public bool IsInWhatIfMode { get; set; }

            public bool ShowWeightHeaderAsSum { get; set; }

            protected override View Render()
            {
                SubscribeToCollection(Class.WeightCategories);

                var gridPanel = VxPlatform.Current == Platform.Uwp ? (View)new AdaptiveGridPanel
                {
                    MinColumnWidth = MinColumnWidth,
                    ColumnSpacing = ColumnSpacing
                } : (View)new AdaptiveGridPanelComponent
                {
                    MinColumnWidth = VxPlatform.Current == Platform.iOS ? 50000 : MinColumnWidth, // iOS doesn't work with this component at this time
                    ColumnSpacing = ColumnSpacing
                };

                List<View> gridPanelChildren = VxPlatform.Current == Platform.Uwp ? (gridPanel as AdaptiveGridPanel).Children : (gridPanel as AdaptiveGridPanelComponent).Children;

                foreach (var weight in Class.WeightCategories)
                {
                    SubscribeToCollection(weight.Grades);

                    var linLayout = new LinearLayout
                    {
                        Children =
                    {
                        RenderHeader(weight, ShowWeightHeaderAsSum, includeMargin: false)
                    }
                    };

                    foreach (var g in weight.Grades)
                    {
                        linLayout.Children.Add(RenderGrade(g, OnRequestViewGrade, IsInWhatIfMode, includeMargin: false));
                    }

                    gridPanelChildren.Add(linLayout);
                }

                return gridPanel;
            }
        }

        public class GradesSummaryComponent : VxComponent
        {
            [VxSubscribe]
            public ViewItemClass Class { get; set; }

            public Action OnRequestConfigureGrades { get; set; }

            public Action OnRequestOpenWhatIf { get; set; }

            protected override View Render()
            {
                SubscribeToCollection(Class.WeightCategories);

                return new LinearLayout
                {
                    Children =
                    {
                        new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                Class.OverriddenGrade != PowerPlannerSending.Grade.UNGRADED ? new LinearLayout
                                {
                                    Children =
                                    {
                                        new TextBlock
                                        {
                                            Text = GradeToStringConverter.Convert(Class.Grade),
                                            FontSize = Theme.Current.TitleFontSize,
                                            WrapText = false
                                        },

                                        new TextBlock
                                        {
                                            Text = GradeToStringConverter.Convert(Class.CalculatedGrade),
                                            FontSize = Theme.Current.CaptionFontSize,
                                            TextColor = Theme.Current.SubtleForegroundColor,
                                            WrapText = false,
                                            Strikethrough = true
                                        }
                                    }
                                }.LinearLayoutWeight(1) as View : new TextBlock
                                {
                                    Text = GradeToStringConverter.Convert(Class.Grade),
                                    FontSize = Theme.Current.TitleFontSize,
                                    WrapText = false,
                                    VerticalAlignment = VerticalAlignment.Top
                                }.LinearLayoutWeight(1) as View,

                                new LinearLayout
                                {
                                    Margin = new Thickness(12, 0, 0, 0),
                                    Children =
                                    {
                                        new TextBlock
                                        {
                                            Text = Class.GpaString,
                                            FontSize = Theme.Current.TitleFontSize,
                                            WrapText = false,
                                            HorizontalAlignment = HorizontalAlignment.Right
                                        },

                                        Class.OverriddenGPA != PowerPlannerSending.Grade.UNGRADED && Class.GpaType == PowerPlannerSending.GpaType.Standard ? new TextBlock
                                        {
                                            Text = GpaToStringConverter.ConvertWithGpa(Class.CalculatedGPA),
                                            FontSize = Theme.Current.CaptionFontSize,
                                            TextColor = Theme.Current.SubtleForegroundColor,
                                            WrapText = false,
                                            Strikethrough = true,
                                            HorizontalAlignment = HorizontalAlignment.Right
                                        } : null,

                                        new TextBlock
                                        {
                                            Text = CreditsToStringConverter.ConvertWithCredits(Class.Credits),
                                            FontSize = Theme.Current.CaptionFontSize,
                                            WrapText = false,
                                            HorizontalAlignment = HorizontalAlignment.Right
                                        },

                                        //OnRequestConfigureGrades != null ? new TextButton
                                        //{
                                        //    Text = PowerPlannerResources.GetString("AppBarButtonEdit.Label"),
                                        //    HorizontalAlignment = HorizontalAlignment.Right,
                                        //    Margin = new Thickness(0, 6, 0, 0),
                                        //    Click = () => OnRequestConfigureGrades()
                                        //} : null
                                    }
                                }
                            }
                        },

                        OnRequestOpenWhatIf != null && OnRequestConfigureGrades != null ? new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                // On Droid AccentButton needs to be in a container to respect the left alignment
                                new Border
                                {
                                    Content = new AccentButton
                                    {
                                        Text = PowerPlannerResources.GetString("ClassPage_ButtonWhatIfMode.Content"),
                                        HorizontalAlignment = HorizontalAlignment.Left,
                                        Click = () => OnRequestOpenWhatIf()
                                    }
                                }.LinearLayoutWeight(1),

                                new TextButton
                                {
                                    Text = PowerPlannerResources.GetString("AppBarButtonEdit.Label"),
                                    HorizontalAlignment = HorizontalAlignment.Right,
                                    Margin = new Thickness(0, 6, 0, 0),
                                    Click = () => OnRequestConfigureGrades()
                                }
                            }
                        } : null,

                        RenderWeightCategories()
                    }
                };
            }

            private View RenderWeightCategories()
            {
                if (Class.WeightCategories.Count <= 1)
                {
                    return null;
                }

                var layout = new LinearLayout
                {
                    Margin = new Thickness(0, 12, 0, 0)
                };

                foreach (var weight in Class.WeightCategories)
                {
                    layout.Children.Add(new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = weight.Name,
                                WrapText = false,
                                FontSize = Theme.Current.CaptionFontSize
                            }.LinearLayoutWeight(1),

                            new TextBlock
                            {
                                Text = weight.WeightAchievedAndTotalString,
                                WrapText = false,
                                FontSize = Theme.Current.CaptionFontSize,
                                TextColor = Theme.Current.SubtleForegroundColor,
                                Margin = new Thickness(12, 0, 0, 0)
                            }
                        }
                    });
                }

                return layout;
            }
        }
    }
}
