using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.SyncLayer;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolsPortable;
using Vx;
using Vx.Views;
using static PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades.ConfigureClassGradeScaleViewModel;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class ConfigureDefaultGradeScaleViewModel : BaseConfigureDefaultGradesPageViewModel
    {
        public MyObservableList<EditingGradeScale> GradeScales { get; private set; }

        public ConfigureDefaultGradeScaleViewModel(BaseViewModel parent) : base(parent)
        {
            Title = PowerPlannerResources.GetString("Settings_DefaultGradeOptions_GradeScale");

            GradeScales = new MyObservableList<EditingGradeScale>(Account.DefaultGradeScale.Select(i => new EditingGradeScale()
            {
                StartingGrade = i.StartGrade,
                GPA = i.GPA
            }));
        }

        protected override View Render()
        {
            var layout = new LinearLayout
            {
                Margin = new Thickness(Theme.Current.PageMargin),
                Children =
                {
                    RenderRow(new TextBlock
                    {
                        Text = PowerPlannerResources.GetString("ClassPage_EditGrades_TextBlockStartingGrade.Text"),
                        FontWeight = FontWeights.Bold
                    }, new TextBlock
                    {
                        Text = PowerPlannerResources.GetString("ClassPage_EditGrades_TextBlockGPA.Text"),
                        FontWeight = FontWeights.Bold
                    }, new TransparentContentButton
                    {
                        Opacity = 0,
                        Content = new FontIcon
                        {
                            Glyph = MaterialDesign.MaterialDesignIcons.Close,
                            FontSize = 20
                        }
                    })
                }
            };

            if (VxPlatform.Current == Platform.Uwp)
            {
                layout.Children.Insert(0, new TextBlock
                {
                    Text = Title.ToUpper(),
                    WrapText = true,
                    Margin = new Thickness(0, 0, 0, 12)
                }.TitleStyle());
            }

            foreach (var entry in GradeScales)
            {
                layout.Children.Add(RenderRow(new NumberTextBox
                {
                    Number = Bind<double?>(nameof(entry.StartingGrade), entry)
                }, new NumberTextBox
                {
                    Number = Bind<double?>(nameof(entry.GPA), entry)
                }, new TransparentContentButton
                {
                    Content = new FontIcon
                    {
                        Glyph = MaterialDesign.MaterialDesignIcons.Close,
                        FontSize = 20,
                        Color = System.Drawing.Color.Red
                    },
                    Click = () => { RemoveGradeScale(entry); }
                }));
            }

            layout.Children.Add(new Button
            {
                Text = PowerPlannerResources.GetString("ClassPage_ButtonAddGradeScale.Content"),
                Margin = new Thickness(0, 12, 0, 0),
                Click = AddGradeScale,
                IsEnabled = IsEnabled
            });

            RenderApplyUI(layout);

            return new ScrollView(layout);
        }

        private static View RenderRow(View first, View second, View third)
        {
            first.Margin = new Thickness(0, 0, 6, 0);
            second.Margin = new Thickness(6, 0, 0, 0);

            var layout = new LinearLayout
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    first.LinearLayoutWeight(1),
                    second.LinearLayoutWeight(1)
                },
                Margin = new Thickness(0, 0, 0, 6)
            };

            if (third != null)
            {
                third.Margin = new Thickness(12, 0, 0, 0);
                layout.Children.Add(third);
            }

            return layout;
        }

        public void AddGradeScale()
        {
            GradeScales.Add(new EditingGradeScale()
            {
                StartingGrade = 0,
                GPA = 0
            });
            MarkDirty();
        }

        public void RemoveGradeScale(EditingGradeScale scale)
        {
            GradeScales.Remove(scale);
            MarkDirty();
        }

        private bool AreScalesValid()
        {
            if (GradeScales.Any(i => i.StartingGrade == null || i.GPA == null))
            {
                return false;
            }

            //check that the numbers are valid
            for (int i = 1; i < GradeScales.Count; i++)
                if (GradeScales[i].StartingGrade.Value >= GradeScales[i - 1].StartingGrade.Value) //if the current starting grade is equal to or greater than the previous starting grade
                    return false;

            return true;
        }

        protected override bool CanApply()
        {
            if (!AreScalesValid())
            {
                _ = new PortableMessageDialog(PowerPlannerResources.GetString("String_InvalidGradeScalesMessageBody"), PowerPlannerResources.GetString("String_InvalidGradeScalesMessageHeader")).ShowAsync();
                return false;
            }

            return true;
        }

        protected override async System.Threading.Tasks.Task Apply()
        {
            GradeScale[] newScales = GradeScales.Select(i => new GradeScale { StartGrade = i.StartingGrade.Value, GPA = i.GPA.Value }).ToArray();

            DataChanges changes = new DataChanges();

            foreach (var c in SelectedClasses)
            {
                if (!newScales.SequenceEqual(c.GradeScales))
                {
                    var change = new DataItemClass
                    {
                        Identifier = c.Identifier
                    };

                    change.SetGradeScales(newScales);

                    changes.Add(change);
                }
            }

            await TryHandleUserInteractionAsync("save", async delegate
            {
                await Account.SaveDefaultGradeScale(newScales);

                if (!changes.IsEmpty())
                {
                    await PowerPlannerApp.Current.SaveChanges(changes);
                }
                else
                {
                    _ = Sync.SyncSettings(Account, Sync.ChangedSetting.DefaultGradeScale);
                }
            });
        }
    }
}
