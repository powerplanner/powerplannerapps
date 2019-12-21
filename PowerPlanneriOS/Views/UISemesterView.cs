using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.Converters;
using ToolsPortable;

namespace PowerPlanneriOS.Views
{
    public class UISemesterView : BareUIView
    {
        public event EventHandler<ViewItemSemester> OnRequestOpenSemester;
        public event EventHandler<ViewItemSemester> OnRequestEditSemester;

        public UISemesterView()
        {
            InitializeViews();
        }

        private BareUIStackViewItemsSourceAdapter<UIClassView> _itemsSourceClasses;
        private void InitializeViews()
        {
            base.TranslatesAutoresizingMaskIntoConstraints = false;
            base.BackgroundColor = new UIColor(247f / 255f, 1);

            // [ [labelName] [labelDates] ]
            // [ [class] [credits] [gpa]  ]
            // [ stackViewClasses         ]
            // [ buttonOpenSemester       ]

            // stackView
            // + viewNameAndDates
            //   + labelName
            //   + labelDates
            // + stackViewClassesTable
            //   + viewClassesTableHeader
            //     + labelHeaderClass
            //     + labelHeaderCredits
            //     + labelHeaderGpa
            //   + stackViewClasses
            //   + viewClassesTableFooter
            //     + labelTotal
            //     + labelTotalCredits
            //     + labelTotalGpa
            // + buttonAddSemester

            var stackView = new UIStackView()
            {
                Axis = UILayoutConstraintAxis.Vertical,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Spacing = 8
            };

            // Name and dates
            var viewNameAndDates = new UIControl()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            viewNameAndDates.TouchUpInside += new WeakEventHandler(delegate { OnRequestEditSemester?.Invoke(this, DataContext as ViewItemSemester); }).Handler;
            {
                var labelName = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                BindingHost.SetLabelTextBinding(labelName, nameof(ViewItemSemester.Name));
                viewNameAndDates.AddSubview(labelName);

                var labelDates = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.PreferredCaption2,
                    TextColor = UIColor.DarkGray,
                    Lines = 0,
                    TextAlignment = UITextAlignment.Right
                };
                BindingHost.SetLabelTextBinding(labelDates, nameof(ViewItemSemester.Start), delegate
                {
                    return SemesterToSemesterViewStartEndStringConverter.Convert(DataContext as ViewItemSemester);
                });
                viewNameAndDates.AddSubview(labelDates);

                viewNameAndDates.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[labelName]->=6-[labelDates]|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary(
                    "labelName", labelName,
                    "labelDates", labelDates)));
                labelName.StretchHeight(viewNameAndDates);
                labelDates.StretchHeight(viewNameAndDates);
            }
            stackView.AddArrangedSubview(viewNameAndDates);
            viewNameAndDates.StretchWidth(stackView);

            // Table
            var classesTouchContainer = new UIControl()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            classesTouchContainer.TouchUpInside += new WeakEventHandler(delegate { OnRequestOpenSemester?.Invoke(this, DataContext as ViewItemSemester); }).Handler;
            {
                var stackViewClassesTable = new UIStackView()
                {
                    Axis = UILayoutConstraintAxis.Vertical,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                {
                    // Table header
                    var viewClassesTableHeader = new UIView()
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false
                    };
                    {
                        var labelHeaderClass = new UILabel()
                        {
                            TranslatesAutoresizingMaskIntoConstraints = false,
                            Text = "Class",
                            Font = UIFont.PreferredCaption2,
                            TextColor = UIColor.DarkGray
                        };
                        viewClassesTableHeader.AddSubview(labelHeaderClass);

                        var labelHeaderCredits = new UILabel()
                        {
                            TranslatesAutoresizingMaskIntoConstraints = false,
                            Font = UIFont.PreferredCaption2,
                            Text = "Credits",
                            TextColor = UIColor.DarkGray,
                            TextAlignment = UITextAlignment.Right
                        };
                        viewClassesTableHeader.AddSubview(labelHeaderCredits);

                        var labelHeaderGpa = new UILabel()
                        {
                            TranslatesAutoresizingMaskIntoConstraints = false,
                            Font = UIFont.PreferredCaption2,
                            Text = "GPA",
                            TextColor = UIColor.DarkGray,
                            TextAlignment = UITextAlignment.Right
                        };
                        viewClassesTableHeader.AddSubview(labelHeaderGpa);

                        viewClassesTableHeader.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[labelHeaderClass]->=6-[labelHeaderCredits(50)]-6-[labelHeaderGpa(50)]|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary(
                            "labelHeaderClass", labelHeaderClass,
                            "labelHeaderCredits", labelHeaderCredits,
                            "labelHeaderGpa", labelHeaderGpa)));
                        labelHeaderClass.StretchHeight(viewClassesTableHeader);
                        labelHeaderCredits.StretchHeight(viewClassesTableHeader);
                        labelHeaderGpa.StretchHeight(viewClassesTableHeader);
                    }
                    stackViewClassesTable.AddArrangedSubview(viewClassesTableHeader);
                    viewClassesTableHeader.StretchWidth(stackViewClassesTable);

                    // Classes stack view
                    var stackViewClasses = new UIStackView()
                    {
                        Axis = UILayoutConstraintAxis.Vertical,
                        TranslatesAutoresizingMaskIntoConstraints = false
                    };
                    _itemsSourceClasses = new BareUIStackViewItemsSourceAdapter<UIClassView>(stackViewClasses);
                    stackViewClassesTable.AddArrangedSubview(stackViewClasses);
                    stackViewClasses.StretchWidth(stackViewClassesTable);

                    // Totals
                    var viewClassesTableFooter = new UIView()
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false
                    };
                    {
                        var labelTotal = new UILabel()
                        {
                            TranslatesAutoresizingMaskIntoConstraints = false,
                            Text = "Total",
                            Font = UIFont.PreferredFootnote
                        };
                        viewClassesTableFooter.AddSubview(labelTotal);

                        var labelTotalCredits = new UILabel()
                        {
                            TranslatesAutoresizingMaskIntoConstraints = false,
                            TextAlignment = UITextAlignment.Right,
                            Font = UIFont.PreferredFootnote
                        };
                        BindingHost.SetLabelTextBinding<double>(labelTotalCredits, nameof(ViewItemSemester.CreditsEarned), (credits) =>
                        {
                            return CreditsToStringConverter.Convert(credits);
                        });
                        viewClassesTableFooter.AddSubview(labelTotalCredits);

                        var labelTotalGpa = new UILabel()
                        {
                            TranslatesAutoresizingMaskIntoConstraints = false,
                            TextAlignment = UITextAlignment.Right,
                            Font = UIFont.PreferredFootnote
                        };
                        BindingHost.SetLabelTextBinding<double>(labelTotalGpa, nameof(ViewItemSemester.GPA), (gpa) =>
                        {
                            return GpaToStringConverter.Convert(gpa);
                        });
                        viewClassesTableFooter.AddSubview(labelTotalGpa);

                        viewClassesTableFooter.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[labelTotal]->=6-[labelTotalCredits(50)]-6-[labelTotalGpa(50)]|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary(
                            "labelTotal", labelTotal,
                            "labelTotalCredits", labelTotalCredits,
                            "labelTotalGpa", labelTotalGpa)));
                        labelTotal.StretchHeight(viewClassesTableFooter);
                        labelTotalCredits.StretchHeight(viewClassesTableFooter);
                        labelTotalGpa.StretchHeight(viewClassesTableFooter);
                    }
                    stackViewClassesTable.AddArrangedSubview(viewClassesTableFooter);
                    viewClassesTableFooter.StretchWidth(stackViewClassesTable);
                }
                classesTouchContainer.Add(stackViewClassesTable);
                stackViewClassesTable.StretchWidthAndHeight(classesTouchContainer);
            }
            stackView.AddArrangedSubview(classesTouchContainer);
            classesTouchContainer.StretchWidth(stackView);

            // Add semester button
            var buttonOpenSemester = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            buttonOpenSemester.SetTitle("Open Semester", UIControlState.Normal);
            buttonOpenSemester.TouchUpInside += new WeakEventHandler(ButtonOpenSemester_TouchUpInside).Handler;
            stackView.AddArrangedSubview(buttonOpenSemester);
            buttonOpenSemester.StretchWidth(stackView);

            base.AddSubview(stackView);
            stackView.StretchWidthAndHeight(this, top: 6, bottom: 6, left: 8, right: 8);
        }

        private void ButtonOpenSemester_TouchUpInside(object sender, EventArgs e)
        {
            OnRequestOpenSemester?.Invoke(this, DataContext as ViewItemSemester);
        }

        protected override void OnDataContextChanged()
        {
            _itemsSourceClasses.ItemsSource = (DataContext as ViewItemSemester)?.Classes;

            base.OnDataContextChanged();
        }

        private class UIClassView : BareUIView
        {
            public UIClassView()
            {
                InitializeViews();
            }

            private void InitializeViews()
            {
                var labelName = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.PreferredCaption1
                };
                BindingHost.SetLabelTextBinding(labelName, nameof(ViewItemClass.Name));
                this.AddSubview(labelName);

                var labelCredits = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.PreferredCaption1,
                    TextAlignment = UITextAlignment.Right
                };
                BindingHost.SetLabelTextBinding(labelCredits, nameof(ViewItemClass.CreditsStringForYearsPage));
                this.AddSubview(labelCredits);

                var labelGpa = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.PreferredCaption1,
                    TextAlignment = UITextAlignment.Right
                };
                BindingHost.SetLabelTextBinding(labelGpa, nameof(ViewItemClass.GpaStringForTableDisplay));
                this.AddSubview(labelGpa);

                this.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[labelName]->=6-[labelCredits(50)]-6-[labelGpa(50)]|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary(
                    "labelName", labelName,
                    "labelCredits", labelCredits,
                    "labelGpa", labelGpa)));
                labelName.StretchHeight(this);
                labelCredits.StretchHeight(this);
                labelGpa.StretchHeight(this);
            }
        }
    }
}