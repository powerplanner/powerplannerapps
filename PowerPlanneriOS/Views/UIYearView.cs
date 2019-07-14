using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.Binding;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.ViewItems;
using ToolsPortable;

namespace PowerPlanneriOS.Views
{
    public class UIYearView : BareUIView
    {
        public event EventHandler<ViewItemYear> OnRequestAddSemester;
        public event EventHandler<ViewItemSemester> OnRequestOpenSemester;
        public event EventHandler<ViewItemYear> OnRequestEditYear;
        public event EventHandler<ViewItemSemester> OnRequestEditSemester;

        public UIYearView()
        {
            InitializeViews();
        }

        private BareUIStackViewItemsSourceAdapter<UISemesterView> _itemsSourceSemesters;
        private void InitializeViews()
        {
            base.TranslatesAutoresizingMaskIntoConstraints = false;
            base.BackgroundColor = new UIColor(235f/255f, 1);

            // [ [labelName] [labelGPA] ]
            // [ semesterView           ]
            // [ buttonAddSemester      ]

            // stackView
            // + viewNameAndGpa
            //   + labelName
            //   + labelGpa
            // + semesterView
            // + buttonAddSemester

            var stackView = new UIStackView()
            {
                Axis = UILayoutConstraintAxis.Vertical,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Spacing = 12
            };

            // Name and GPA
            var viewNameAndGpa = new UIControl()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            viewNameAndGpa.TouchUpInside += new WeakEventHandler(delegate { OnRequestEditYear?.Invoke(this, DataContext as ViewItemYear); }).Handler;
            {
                var labelName = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                BindingHost.SetLabelTextBinding(labelName, nameof(ViewItemYear.Name));
                viewNameAndGpa.AddSubview(labelName);

                var labelGpa = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                BindingHost.SetLabelTextBinding(labelGpa, nameof(ViewItemYear.GPA), (gpa) =>
                {
                    return ((double)gpa).ToString("0.0##");
                });
                viewNameAndGpa.AddSubview(labelGpa);

                viewNameAndGpa.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[labelName]->=6-[labelGpa]|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary("labelName", labelName, "labelGpa", labelGpa)));
                labelName.StretchHeight(viewNameAndGpa);
                labelGpa.StretchHeight(viewNameAndGpa);
            }
            stackView.AddArrangedSubview(viewNameAndGpa);
            viewNameAndGpa.StretchWidth(stackView);

            // Semester
            var stackViewSemesters = new UIStackView()
            {
                Axis = UILayoutConstraintAxis.Vertical,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Spacing = 8
            };
            _itemsSourceSemesters = new BareUIStackViewItemsSourceAdapter<UISemesterView>(stackViewSemesters);
            _itemsSourceSemesters.OnViewCreated += _itemsSourceSemesters_OnViewCreated;
            stackView.AddArrangedSubview(stackViewSemesters);
            stackViewSemesters.StretchWidth(stackView);

            // Add semester button
            var buttonAddSemester = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            buttonAddSemester.SetTitle("Add Semester", UIControlState.Normal);
            buttonAddSemester.TouchUpInside += new WeakEventHandler(ButtonAddSemester_TouchUpInside).Handler;
            stackView.AddArrangedSubview(buttonAddSemester);
            buttonAddSemester.StretchWidth(stackView);

            base.AddSubview(stackView);
            stackView.StretchWidthAndHeight(this, top: 6, bottom: 6, left: 8, right: 8);
        }

        private void _itemsSourceSemesters_OnViewCreated(object sender, UISemesterView semesterView)
        {
            semesterView.OnRequestOpenSemester += new WeakEventHandler<ViewItemSemester>(SemesterView_OnRequestOpenSemester).Handler;
            semesterView.OnRequestEditSemester += new WeakEventHandler<ViewItemSemester>(SemesterView_OnRequestEditSemester).Handler;
        }

        private void SemesterView_OnRequestEditSemester(object sender, ViewItemSemester e)
        {
            OnRequestEditSemester?.Invoke(this, e);
        }

        private void SemesterView_OnRequestOpenSemester(object sender, ViewItemSemester e)
        {
            OnRequestOpenSemester?.Invoke(this, e);
        }

        protected override void OnDataContextChanged()
        {
            _itemsSourceSemesters.ItemsSource = (DataContext as ViewItemYear)?.Semesters;

            base.OnDataContextChanged();
        }

        private void ButtonAddSemester_TouchUpInside(object sender, EventArgs e)
        {
            OnRequestAddSemester?.Invoke(this, DataContext as ViewItemYear);
        }
    }
}