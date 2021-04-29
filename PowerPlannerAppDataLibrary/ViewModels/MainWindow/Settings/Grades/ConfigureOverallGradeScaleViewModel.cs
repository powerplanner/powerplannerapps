using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
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
    public class ConfigureOverallGradeScaleViewModel : ComponentViewModel
    {
        private enum States
        {
            Loading,
            Initial, // Can return to this after editing anything
            Saving,
            Applied
        }

        public MyObservableList<EditingGradeScale> GradeScales { get; private set; }
        public AccountDataItem Account { get; private set; }
        private VxState<MyObservableList<ViewItemYear>> _years = new VxState<MyObservableList<ViewItemYear>>(null);
        private HashSet<ViewItemClass> _selectedClasses = new HashSet<ViewItemClass>();
        private int _totalClasses;
        private VxState<States> _state = new VxState<States>(States.Loading);

        public ConfigureOverallGradeScaleViewModel(BaseViewModel parent) : base(parent)
        {
            Title = "Default grade scale";
            Account = MainScreenViewModel.CurrentAccount;

            GradeScales = new MyObservableList<EditingGradeScale>(Account.DefaultGradeScale.Select(i => new EditingGradeScale()
            {
                StartingGrade = i.StartGrade,
                GPA = i.GPA
            }));
        }

        protected override async System.Threading.Tasks.Task LoadAsyncOverride()
        {
            var yearsGroup = await YearsViewItemsGroup.LoadAsync(Account.LocalAccountId);
            _years.Value = yearsGroup.School.Years;
            foreach (var c in _years.Value.SelectMany(i => i.Semesters).SelectMany(i => i.Classes))
            {
                _selectedClasses.Add(c);
                _totalClasses++;
            }
            _state.Value = States.Initial;
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
                IsEnabled = _state.Value == States.Initial || _state.Value == States.Applied
            });

            string applyText;

            switch (_state.Value)
            {
                case States.Loading:
                case States.Initial:
                    applyText = $"Apply to {(_selectedClasses.Count == _totalClasses ? "all" : _selectedClasses.Count.ToString())} classes";
                    break;

                case States.Saving:
                    applyText = "Applying...";
                    break;

                case States.Applied:
                    applyText = "Applied!";
                    break;

                default:
                    throw new NotImplementedException();
            }

            layout.Children.Add(new AccentButton
            {
                Text = applyText,
                Margin = new Thickness(0, 24, 0, 0),
                Click = ApplyGradeScale,
                IsEnabled = _state.Value == States.Initial
            });

            if (_years.Value != null)
            {
                foreach (var year in _years.Value)
                {
                    foreach (var semester in year.Semesters)
                    {
                        if (semester.Classes.Count > 0)
                        {
                            layout.Children.Add(new TextBlock
                            {
                                Text = year.Name + " > " + semester.Name,
                                Margin = new Thickness(0, 12, 0, 0)
                            });

                            foreach (var c in semester.Classes)
                            {
                                layout.Children.Add(new CheckBox
                                {
                                    Text = c.Name,
                                    IsChecked = _selectedClasses.Contains(c),
                                    IsCheckedChanged = isChecked =>
                                    {
                                        if (isChecked)
                                        {
                                            _selectedClasses.Add(c);
                                            MarkDirty();
                                        }
                                        else
                                        {
                                            _selectedClasses.Remove(c);
                                            MarkDirty();
                                        }
                                    },
                                    IsEnabled = _state.Value != States.Saving
                                });
                            }
                        }
                    }
                }
            }

            // When any other property changed, we reset to initial
            if (_state.Value == States.Applied)
            {
                _state.SetValueSilently(States.Initial);
            }

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

        public async void ApplyGradeScale()
        {
            try
            {
                if (!AreScalesValid())
                {
                    await new PortableMessageDialog(PowerPlannerResources.GetString("String_InvalidGradeScalesMessageBody"), PowerPlannerResources.GetString("String_InvalidGradeScalesMessageHeader")).ShowAsync();
                    return;
                }

                _state.Value = States.Saving;

                GradeScale[] newScales = GradeScales.Select(i => new GradeScale { StartGrade = i.StartingGrade.Value, GPA = i.GPA.Value }).ToArray();

                DataChanges changes = new DataChanges();

                foreach (var c in _selectedClasses)
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

                    _state.Value = States.Applied;
                });
            }

            catch (Exception ex)
            {
                _state.Value = States.Initial;
                TelemetryExtension.Current?.TrackException(ex);
                await new PortableMessageDialog("Error encountered while saving. Your error report has been sent to the developer.", "Error").ShowAsync();
            }
        }
    }
}
