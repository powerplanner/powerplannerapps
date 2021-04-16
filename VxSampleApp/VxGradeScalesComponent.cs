using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace VxSampleApp
{
    public class VxGradeScalesComponent : VxComponent
    {
        private List<GradeScaleEntry> _gradeScaleEntries;

        protected override void Initialize()
        {
            base.Initialize();

            Reset();
        }

        private void Reset()
        {
            _gradeScaleEntries = new List<GradeScaleEntry>()
            {
                new GradeScaleEntry { StartingGrade = "90", Gpa = "4"},
                new GradeScaleEntry { StartingGrade = "80", Gpa = "3"},
                new GradeScaleEntry { StartingGrade = "70", Gpa = "2"},
                new GradeScaleEntry { StartingGrade = "60", Gpa = "1"},
                new GradeScaleEntry { StartingGrade = "0", Gpa = "0"},
            };

            MarkDirty();
        }

        protected override View Render()
        {
            var layout = new LinearLayout
            {
                Children =
                {
                    new Button
                    {
                        Text = "Reset",
                        Click = Reset
                    },

                    RenderRow(new TextBlock
                    {
                        Text = "Starting grade"
                    }, new TextBlock
                    {
                        Text = "GPA"
                    }, null)
                }
            };

            foreach (var entry in _gradeScaleEntries)
            {
                layout.Children.Add(RenderRow(new TextBox
                {
                    Text = Bind<string>(nameof(entry.StartingGrade), entry)
                }, new TextBox
                {
                    Text = Bind<string>(nameof(entry.Gpa), entry)
                }, new Button
                {
                    Text = "X",
                    Click = () => { _gradeScaleEntries.Remove(entry); MarkDirty(); }
                }));
            }

            layout.Children.Add(new Button
            {
                Text = "Add grade scale",
                Click = () => { _gradeScaleEntries.Add(new GradeScaleEntry() { StartingGrade = "0", Gpa = "0" }); MarkDirty(); }
            });

            foreach (var entry in _gradeScaleEntries)
            {
                layout.Children.Add(RenderRow(new TextBlock
                {
                    Text = entry.StartingGrade,
                    FontWeight = FontWeights.Bold
                }, new TextBlock
                {
                    Text = entry.Gpa,
                    FontWeight = FontWeights.Bold
                }, null));
            }

            return layout;
        }

        private class GradeScaleEntry
        {
            public string StartingGrade { get; set; }
            public string Gpa { get; set; }
        }

        private static View RenderRow(View first, View second, View third)
        {
            first.Margin = new Thickness(0, 0, 3, 0);
            second.Margin = new Thickness(3, 0, 0, 0);

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
                third.Margin = new Thickness(6, 0, 0, 0);
                layout.Children.Add(third);
            }

            return layout;
        }
    }
}
