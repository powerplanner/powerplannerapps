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
            Title = PowerPlannerResources.GetString("MainMenuItem_Years");
        }

        protected override View Render()
        {
            if (YearsViewItemsGroup == null)
            {
                return null;
            }

            // Subscribe to the school
            Subscribe(YearsViewItemsGroup.School);

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
                                        FontWeight = FontWeights.Bold,
                                        TextAlignment = HorizontalAlignment.Right
                                    },

                                    new TextBlock
                                    {
                                        Text = CreditsToStringConverter.ConvertWithCredits(YearsViewItemsGroup.School.CreditsEarned),
                                        TextAlignment = HorizontalAlignment.Right
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
                Text = "+ " + PowerPlannerResources.GetString("YearsPage_ButtonAddYear.Content"),
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
                                    FontSize = Theme.Current.TitleFontSize,
                                    WrapText = false,
                                    Margin = new Thickness(0, 0, 6, 0)
                                }.LinearLayoutWeight(1),

                                new LinearLayout
                                {
                                    Children =
                                    {
                                        new LinearLayout
                                        {
                                            Orientation = Orientation.Horizontal,
                                            HorizontalAlignment = HorizontalAlignment.Right,
                                            Children =
                                            {
                                                year.OverriddenGPA != PowerPlannerSending.Grade.UNGRADED && year.CalculatedGPA != -1 ? new TextBlock
                                                {
                                                    Text = GpaToStringConverter.Convert(year.CalculatedGPA),
                                                    TextColor = Theme.Current.SubtleForegroundColor,
                                                    Strikethrough = true,
                                                    Margin = new Thickness(0, 0, 6, 0)
                                                } : null,

                                                new TextBlock
                                                {
                                                    Text = GpaToStringConverter.Convert(year.GPA, includeGpa: true),
                                                    TextColor = Theme.Current.SubtleForegroundColor,
                                                    FontWeight = FontWeights.Bold
                                                }
                                            }
                                        },


                                        new LinearLayout
                                        {
                                            Orientation = Orientation.Horizontal,
                                            HorizontalAlignment = HorizontalAlignment.Right,
                                            Children =
                                            {
                                                year.OverriddenCredits != PowerPlannerSending.Grade.UNGRADED && year.CalculatedCreditsEarned != -1 ? new TextBlock
                                                {
                                                    Text = CreditsToStringConverter.Convert(year.CalculatedCreditsEarned),
                                                    TextColor = Theme.Current.SubtleForegroundColor,
                                                    Strikethrough = true,
                                                    Margin = new Thickness(0, 0, 6, 0),
                                                    FontSize = Theme.Current.CaptionFontSize
                                                } : null,

                                                new TextBlock
                                                {
                                                    Text = CreditsToStringConverter.Convert(year.CreditsEarned, includeCredits: true),
                                                    TextColor = Theme.Current.SubtleForegroundColor,
                                                    FontSize = Theme.Current.CaptionFontSize
                                                }
                                            }
                                        }
                                    }
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
                Text = "+ " + PowerPlannerResources.GetString("YearView_ButtonAddSemester.Content"),
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
                                    FontSize = 22,
                                    WrapText = false,
                                    Margin = new Thickness(0, 0, 6, 0)
                                }.LinearLayoutWeight(1),

                                new TextBlock
                                {
                                    Text = SemesterToSemesterViewStartEndStringConverter.Convert(semester),
                                    FontSize = Theme.Current.CaptionFontSize,
                                    TextColor = Theme.Current.SubtleForegroundColor,
                                    TextAlignment = HorizontalAlignment.Right
                                }
                            }
                        }
                    }
                }
            };

            linearLayout.Children.Add(RenderClassRow(
                PowerPlannerResources.GetString("SemesterView_HeaderClass.Text"),
                PowerPlannerResources.GetString("SemesterView_HeaderCredits.Text"),
                PowerPlannerResources.GetString("SemesterView_HeaderGPA.Text"), isSubtle: true));

            SubscribeToCollection(semester.Classes);
            foreach (var c in semester.Classes)
            {
                Subscribe(c);
                linearLayout.Children.Add(RenderClassRow(c.Name, c.CreditsStringForYearsPage, c.GpaStringForTableDisplay));
            }

            bool displayCrossedOutCredits = semester.OverriddenCredits != PowerPlannerSending.Grade.UNGRADED;
            bool displayCrossedOutGpa = semester.OverriddenGPA != PowerPlannerSending.Grade.UNGRADED;
            if (displayCrossedOutCredits || displayCrossedOutGpa)
            {
                linearLayout.Children.Add(RenderClassRow(
                    PowerPlannerResources.GetString("SemesterView_Total.Text"),
                    displayCrossedOutCredits ? CreditsToStringConverter.Convert(semester.CalculatedCreditsEarned) : "",
                    displayCrossedOutGpa ? GpaToStringConverter.Convert(semester.CalculatedGPA) : "",
                    isSubtle: true,
                    isBig: true,
                    strikethrough: true));
            }

            linearLayout.Children.Add(RenderClassRow(PowerPlannerResources.GetString("SemesterView_Total.Text"), CreditsToStringConverter.Convert(semester.CreditsEarned), GpaToStringConverter.Convert(semester.GPA), isBig: true));

            linearLayout.Children.Add(new AccentButton
            {
                Text = PowerPlannerResources.GetString("SemesterView_ButtonOpenSemester.Content"),
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

        private View RenderClassRow(string str1, string str2, string str3, bool isSubtle = false, bool isBig = false, bool strikethrough = false)
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
                        FontWeight = FontWeights.SemiBold,
                        Strikethrough = strikethrough
                    }.LinearLayoutWeight(2),

                    new TextBlock
                    {
                        Text = str2,
                        FontSize = fontSize,
                        TextColor = isSubtle ? Theme.Current.SubtleForegroundColor : Theme.Current.ForegroundColor,
                        WrapText = false,
                        TextAlignment = HorizontalAlignment.Right,
                        FontWeight = FontWeights.SemiBold,
                        Strikethrough = strikethrough
                    }.LinearLayoutWeight(1),

                    new TextBlock
                    {
                        Text = str3,
                        FontSize = fontSize,
                        TextColor = isSubtle ? Theme.Current.SubtleForegroundColor : Theme.Current.ForegroundColor,
                        WrapText = false,
                        TextAlignment = HorizontalAlignment.Right,
                        FontWeight = FontWeights.SemiBold,
                        Strikethrough = strikethrough
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
