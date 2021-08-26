using PowerPlannerAppDataLibrary.ViewItemsGroups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.App;
using ToolsPortable;
using Vx.Views;
using PowerPlannerAppDataLibrary.Converters;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years
{
    public class YearsViewModel : PopupComponentViewModel
    {
        private YearsViewItemsGroup _yearsViewItemsGroup;
        public YearsViewItemsGroup YearsViewItemsGroup
        {
            get { return _yearsViewItemsGroup; }
            set { SetProperty(ref _yearsViewItemsGroup, value, "YearsViewItemsGroup"); }
        }

        public YearsViewModel(BaseViewModel parent) : base(parent)
        {
            Title = "Years";
        }

        protected override View Render()
        {
            if (YearsViewItemsGroup == null)
            {
                return null;
            }

            // Subscribe to the school
            Subscribe(YearsViewItemsGroup.School);

            // TODO: Subscribe to School property changes
            var linearLayout = new LinearLayout
            {
                Margin = new Thickness(Theme.Current.PageMargin),
                Children =
                {
                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = PowerPlannerResources.GetString("YearsPage_TextBlockOverall.Text"),
                                Margin = new Thickness(0, 0, 12, 0),
                                FontSize = Theme.Current.TitleFontSize,
                                VerticalAlignment = VerticalAlignment.Center
                            },

                            new LinearLayout
                            {
                                VerticalAlignment = VerticalAlignment.Center,
                                Children =
                                {
                                    new TextBlock
                                    {
                                        Text = GpaToStringConverter.ConvertWithGpa(YearsViewItemsGroup.School.GPA),
                                        FontWeight = FontWeights.Bold
                                    },

                                    new TextBlock
                                    {
                                        Text = CreditsToStringConverter.ConvertWithCredits(YearsViewItemsGroup.School.CreditsEarned)
                                    }
                                }
                            }.LinearLayoutWeight(1)
                        }
                    }
                }
            };

            var adaptiveGridPanel = new AdaptiveGridPanel();

            SubscribeToCollection(YearsViewItemsGroup.School.Years);
            foreach (var year in YearsViewItemsGroup.School.Years)
            {
                adaptiveGridPanel.Children.Add(RenderYear(year));
            }

            linearLayout.Children.Add(adaptiveGridPanel);

            linearLayout.Children.Add(new Button
            {
                Text = "+ add year",
                Margin = new Thickness(0, 24, 0, 0),
                Click = AddYear
            });

            return new ScrollView
            {
                Content = linearLayout
            };
        }

        private View RenderYear(ViewItemYear year)
        {
            Subscribe(year);

            var linearLayout = new LinearLayout
            {
                Children =
                {
                    new TransparentContentButton
                    {
                        Click = () => EditYear(year),
                        Content = new LinearLayout
                        {
                            Margin = new Thickness(12),
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = year.Name,
                                    FontSize = Theme.Current.TitleFontSize
                                }.LinearLayoutWeight(1),

                                new TextBlock
                                {
                                    Text = GpaToStringConverter.Convert(year.GPA),
                                    FontSize = Theme.Current.TitleFontSize,
                                    TextColor = Theme.Current.SubtleForegroundColor
                                }
                            }
                        }
                    }
                }
            };

            SubscribeToCollection(year.Semesters);
            foreach (var semester in year.Semesters)
            {
                linearLayout.Children.Add(RenderSemester(semester));
            }

            linearLayout.Children.Add(new Button
            {
                Text = "+ add semester",
                Margin = new Thickness(12, 0, 12, 12),
                Click = () => AddSemester(year.Identifier)
            });

            return new Border
            {
                BackgroundColor = Theme.Current.PopupPageBackgroundColor,
                Content = linearLayout,
                Margin = new Thickness(0, 24, 0, 0),
                VerticalAlignment = VerticalAlignment.Top
            };
        }

        private View RenderSemester(ViewItemSemester semester)
        {
            Subscribe(semester);

            var linearLayout = new LinearLayout
            {
                Children =
                {
                    new TransparentContentButton
                    {
                        Click = () => EditSemester(semester),
                        Content = new LinearLayout
                        {
                            Margin = new Thickness(12),
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = semester.Name,
                                    FontSize = 22
                                }.LinearLayoutWeight(1),

                                new TextBlock
                                {
                                    Text = "TODO date span",
                                    FontSize = Theme.Current.CaptionFontSize,
                                    TextColor = Theme.Current.SubtleForegroundColor,
                                    TextAlignment = HorizontalAlignment.Right
                                }
                            }
                        }
                    }
                }
            };

            // TODO: Localize
            linearLayout.Children.Add(RenderClassRow("Class", "Credits", "GPA", isSubtle: true));

            SubscribeToCollection(semester.Classes);
            foreach (var c in semester.Classes)
            {
                Subscribe(c);
                linearLayout.Children.Add(RenderClassRow(c.Name, CreditsToStringConverter.Convert(c.Credits), GpaToStringConverter.Convert(c.GPA)));
            }

            // TODO: Localize
            linearLayout.Children.Add(RenderClassRow("Total", CreditsToStringConverter.Convert(semester.CreditsEarned), GpaToStringConverter.Convert(semester.GPA), isBig: true));

            linearLayout.Children.Add(new AccentButton
            {
                Text = "Open semester",
                Margin = new Thickness(12, 12, 12, 12),
                Click = () => OpenSemester(semester.Identifier)
            });

            return new Border
            {
                Margin = new Thickness(12, 0, 12, 12),
                BackgroundColor = Theme.Current.PopupPageBackgroundAltColor,
                Content = linearLayout
            };
        }

        private View RenderClassRow(string str1, string str2, string str3, bool isSubtle = false, bool isBig = false)
        {
            var textColor = isSubtle ? Theme.Current.SubtleForegroundColor : Theme.Current.ForegroundColor;
            var fontSize = isBig ? 16 : Theme.Current.CaptionFontSize;

            return new LinearLayout
            {
                Margin = new Thickness(12, 0, 12, 0),
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new TextBlock
                    {
                        Text = str1,
                        FontSize = fontSize,
                        TextColor = textColor,
                        WrapText = false,
                        FontWeight = FontWeights.SemiBold
                    }.LinearLayoutWeight(2),

                    new TextBlock
                    {
                        Text = str2,
                        FontSize = fontSize,
                        TextColor = isSubtle ? Theme.Current.SubtleForegroundColor : Theme.Current.ForegroundColor,
                        WrapText = false,
                        TextAlignment = HorizontalAlignment.Right,
                        FontWeight = FontWeights.SemiBold
                    }.LinearLayoutWeight(1),

                    new TextBlock
                    {
                        Text = str3,
                        FontSize = fontSize,
                        TextColor = isSubtle ? Theme.Current.SubtleForegroundColor : Theme.Current.ForegroundColor,
                        WrapText = false,
                        TextAlignment = HorizontalAlignment.Right,
                        FontWeight = FontWeights.SemiBold
                    }.LinearLayoutWeight(1)
                }
            };
        }

        protected override async Task LoadAsyncOverride()
        {
            YearsViewItemsGroup = await YearsViewItemsGroup.LoadAsync(MainScreenViewModel.CurrentLocalAccountId);
        }

        public void AddYear()
        {
            ShowPopup(AddYearViewModel.CreateForAdd(MainScreenViewModel));
        }

        public async void AddSemester(Guid yearIdentifier)
        {
            await TryHandleUserInteractionAsync("AddSemester" + yearIdentifier, async (cancellationToken) =>
            {
                // If not full version and they already have a semester
                if (YearsViewItemsGroup.School.Years.Any(i => i.Semesters.Any()) && !await PowerPlannerApp.Current.IsFullVersionAsync())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    PowerPlannerApp.Current.PromptPurchase(PowerPlannerResources.GetString("MessageFreeSemesterLimitReached"));
                    return;
                }

                ShowPopup(AddSemesterViewModel.CreateForAdd(MainScreenViewModel, new AddSemesterViewModel.AddParameter()
                {
                    YearIdentifier = yearIdentifier
                }));
            });
        }

        public void EditYear(ViewItemYear year)
        {
            ShowPopup(AddYearViewModel.CreateForEdit(MainScreenViewModel, year));
        }

        public void EditSemester(ViewItemSemester semester)
        {
            ShowPopup(AddSemesterViewModel.CreateForEdit(MainScreenViewModel, semester));
        }

        public async void OpenSemester(Guid semesterId)
        {
            await TryHandleUserInteractionAsync("OpenSemester" + semesterId, async delegate
            {
                await MainScreenViewModel.SetCurrentSemester(semesterId, alwaysNavigate: true);
            });
        }

        public override bool GoBack()
        {
            if (PowerPlannerApp.DoNotShowYearsInTabItems && !MainScreenViewModel.AvailableItems.Any())
            {
                return false;
            }

            return base.GoBack();
        }
    }
}
