using Foundation;
using InterfacesiOS.Binding;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.Converters;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years;
using PowerPlanneriOS.Helpers;
using PowerPlanneriOS.Views;
using System;
using ToolsPortable;
using UIKit;

namespace PowerPlanneriOS.Controllers
{
    public class YearsViewController : PopupViewControllerWithScrolling<YearsViewModel>
    {
        private UIActivityIndicatorView _loadingIndicator;

        public YearsViewController()
        {
            Title = "Years";

            HideBackButton();

            _loadingIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Color = ColorResources.PowerPlannerAccentBlue
            };
            View.Add(_loadingIndicator);
            _loadingIndicator.StretchWidthAndHeight(View);
            _loadingIndicator.StartAnimating();

            AutomaticallyAdjustsScrollViewInsets = false;
        }

        protected override int AdditionalTopPadding => 16;
        protected override int LeftPadding => 16;
        protected override int RightPadding => 16;

        private BindingHost _schoolBindingHost;

        private BareUIStackViewItemsSourceAdapter<UIYearView> _itemsSourceYears;
        private object _tabBarHeightListener;

        public override void OnViewModelLoadedOverride()
        {
            _loadingIndicator.StopAnimating();
            _loadingIndicator.RemoveFromSuperview();
            _loadingIndicator = null;

            _schoolBindingHost = new BindingHost()
            {
                BindingObject = ViewModel.YearsViewItemsGroup.School
            };

            var overallText = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredTitle1,
                AdjustsFontSizeToFitWidth = true,
                Lines = 1
            };
            Action updateOverallText = delegate
            {
                overallText.Text = "Overall: " + GpaToStringConverter.Convert(ViewModel.YearsViewItemsGroup.School.GPA, includeGpa: true) + " / " + CreditsToStringConverter.Convert(ViewModel.YearsViewItemsGroup.School.CreditsEarned, includeCredits: true);
            };
            _schoolBindingHost.SetBinding(nameof(ViewModel.YearsViewItemsGroup.School.GPA), updateOverallText);
            _schoolBindingHost.SetBinding(nameof(ViewModel.YearsViewItemsGroup.School.CreditsEarned), updateOverallText);
            StackView.AddArrangedSubview(overallText);
            overallText.StretchWidth(StackView);

            StackView.AddArrangedSubview(new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            }.SetHeight(16));

            var stackViewYears = new UIStackView()
            {
                Axis = UILayoutConstraintAxis.Vertical,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Spacing = 16
            };
            _itemsSourceYears = new BareUIStackViewItemsSourceAdapter<UIYearView>(stackViewYears);
            _itemsSourceYears.OnViewCreated += _itemsSourceYears_OnViewCreated;
            _itemsSourceYears.ItemsSource = ViewModel.YearsViewItemsGroup.School.Years;
            StackView.AddArrangedSubview(stackViewYears);
            stackViewYears.StretchWidth(StackView);

            var buttonAddYear = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            buttonAddYear.SetTitle("Add Year", UIControlState.Normal);
            buttonAddYear.TouchUpInside += new WeakEventHandler(delegate { ViewModel.AddYear(); }).Handler;
            StackView.AddArrangedSubview(buttonAddYear);
            buttonAddYear.StretchWidth(StackView);

            MainScreenViewController.ListenToTabBarHeightChanged(ref _tabBarHeightListener, delegate
            {
                _scrollView.ContentInset = new UIEdgeInsets(0, 0, MainScreenViewController.TAB_BAR_HEIGHT, 0);
            });

            base.OnViewModelLoadedOverride();
        }

        private void _itemsSourceYears_OnViewCreated(object sender, UIYearView yearView)
        {
            yearView.OnRequestAddSemester += new WeakEventHandler<ViewItemYear>(YearView_OnRequestAddSemester).Handler;
            yearView.OnRequestOpenSemester += new WeakEventHandler<ViewItemSemester>(YearView_OnRequestOpenSemester).Handler;
            yearView.OnRequestEditYear += new WeakEventHandler<ViewItemYear>(YearView_OnRequestEditYear).Handler;
            yearView.OnRequestEditSemester += new WeakEventHandler<ViewItemSemester>(YearView_OnRequestEditSemester).Handler;
        }

        private void YearView_OnRequestEditSemester(object sender, ViewItemSemester e)
        {
            ViewModel.EditSemester(e);
        }

        private void YearView_OnRequestEditYear(object sender, ViewItemYear e)
        {
            ViewModel.EditYear(e);
        }

        private void YearView_OnRequestOpenSemester(object sender, ViewItemSemester e)
        {
            ViewModel.OpenSemester(e.Identifier);
        }

        private void YearView_OnRequestAddSemester(object sender, ViewItemYear e)
        {
            ViewModel.AddSemester(e.Identifier);
        }
    }
}