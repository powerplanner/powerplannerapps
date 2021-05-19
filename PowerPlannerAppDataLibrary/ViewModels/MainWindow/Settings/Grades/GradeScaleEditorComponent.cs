using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class GradeScaleEditorComponent : VxComponent
    {
        public class EditingGradeScale
        {
            public double? StartingGrade { get; set; }

            public double? GPA { get; set; }
        }

        private List<EditingGradeScale> GradeScales;

        private Func<IEnumerable<ViewItemClass>> _getAllClasses;
        private Action _changed;
        public GradeScaleEditorComponent(Func<IEnumerable<ViewItemClass>> getAllClasses, Action changed = null)
        {
            _getAllClasses = getAllClasses;
            _changed = changed;
        }

        public bool IsEnabled { get; set; }
        public GradeScale[] InitialGradeScale { get; set; }

        private GradeScale[] _currInitialGradeScale;
        private GradeScale[] _oldGradeScale;
        private View _presetScaleButton;
        protected override View Render()
        {
            if (_currInitialGradeScale != InitialGradeScale)
            {
                GradeScales = new List<EditingGradeScale>(InitialGradeScale.Select(i => new EditingGradeScale()
                {
                    StartingGrade = i.StartGrade,
                    GPA = i.GPA
                }));
                _currInitialGradeScale = InitialGradeScale;
                _oldGradeScale = InitialGradeScale;
            }
            else if (_changed != null)
            {
                var currGradeScales = GetGradeScales();
                if (!_oldGradeScale.SequenceEqual(currGradeScales))
                {
                    _oldGradeScale = currGradeScales;
                    _changed();
                }
            }

            var layout = new LinearLayout
            {
                Children =
                {
                    new TextButton
                    {
                        Text = PowerPlannerResources.GetString("Settings_GradeOptions_UsePresetScale"),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(0, 0, 0, 12),
                        ViewRef = view => _presetScaleButton = view,
                        IsEnabled = IsEnabled,
                        Click = () =>
                        {
                            var menu = new ContextMenu();
                            foreach (var preset in GetPresetGradeScales())
                            {
                                menu.Items.Add(new ContextMenuItem
                                {
                                    Text = preset.Name,
                                    Click = () =>
                                    {
                                        GradeScales = new List<EditingGradeScale>(preset.GradeScales.Select(i => new EditingGradeScale()
                                        {
                                            StartingGrade = i.StartGrade,
                                            GPA = i.GPA
                                        }));
                                        MarkDirty();
                                    }
                                });
                            }
                            menu.Show(_presetScaleButton);
                        }
                    },

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

            return layout;
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

        private IEnumerable<PresetGradeScale> GetPresetGradeScales()
        {
            yield return new PresetGradeScale
            {
                Name = "United States",
                GradeScales = GradeScale.GenerateDefaultScaleWithoutLetters()
            };

            yield return new PresetGradeScale
            {
                Name = "Eleven-Point System",
                GradeScales = GradeScale.GenerateElevenPointScale()
            };

            yield return new PresetGradeScale
            {
                Name = "Twelve-Point System",
                GradeScales = GradeScale.GenerateTwelvePointScale()
            };

            yield return new PresetGradeScale
            {
                Name = "Mexico - 100 Point",
                GradeScales = GradeScale.GenerateMexico100PointScale()
            };

            yield return new PresetGradeScale
            {
                Name = "Mexico - 10 Point",
                GradeScales = GradeScale.GenerateMexico10PointScale()
            };


            List<GradeScale> alreadySeen = new List<GradeScale>();
            var remainingClasses = _getAllClasses().ToList();

            while (remainingClasses.Count > 0)
            {
                var c = remainingClasses[0];
                var matching = remainingClasses.Skip(1).Where(otherC => c.GradeScales.SequenceEqual(otherC.GradeScales)).ToArray();

                string name = c.Name;

                if (matching.Length == 1)
                {
                    name += " / " + matching[0].Name;
                }
                else if (matching.Length > 1)
                {
                    name = PowerPlannerResources.GetStringWithParameters("String_ClassPlusXOthers", c.Name, matching.Length);
                }

                yield return new PresetGradeScale
                {
                    Name = name,
                    GradeScales = c.GradeScales
                };

                remainingClasses.RemoveAt(0);
                remainingClasses.RemoveAll(i => matching.Contains(i));
            }
        }

        private class PresetGradeScale
        {
            public string Name { get; set; }

            public GradeScale[] GradeScales { get; set; }
        }

        private void AddGradeScale()
        {
            GradeScales.Add(new EditingGradeScale()
            {
                StartingGrade = 0,
                GPA = 0
            });
            MarkDirty();
        }

        private void RemoveGradeScale(EditingGradeScale scale)
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

        /// <summary>
        /// Displays error if invalid
        /// </summary>
        /// <returns></returns>
        public bool CheckIfValid()
        {
            if (AreScalesValid())
            {
                return true;
            }

            _ = new PortableMessageDialog(PowerPlannerResources.GetString("String_InvalidGradeScalesMessageBody"), PowerPlannerResources.GetString("String_InvalidGradeScalesMessageHeader")).ShowAsync();
            return false;
        }

        public GradeScale[] GetGradeScales()
        {
            return GradeScales.Select(i => new GradeScale { StartGrade = i.StartingGrade.Value, GPA = i.GPA.Value }).ToArray();
        }
    }
}
