using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Grade;
using ToolsPortable;
using PowerPlanneriOS.Helpers;
using InterfacesiOS.Binding;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.ViewItems;

namespace PowerPlanneriOS.Controllers
{
    public class ViewGradeViewController : PopupViewControllerWithScrolling<ViewGradeViewModel>
    {
        private BindingHost _itemBindingHost = new BindingHost();
        private BindingHost _classBindingHost = new BindingHost();

        protected override int LeftPadding => 16;
        protected override int AdditionalTopPadding => 16;
        protected override int RightPadding => 16;

        public ViewGradeViewController()
        {
            Title = "View Grade";

            var buttonEdit = new UIBarButtonItem(UIBarButtonSystemItem.Edit);
            buttonEdit.Clicked += new WeakEventHandler(delegate { ViewModel.Edit(); }).Handler;

            var buttonDelete = new UIBarButtonItem(UIBarButtonSystemItem.Trash);
            buttonDelete.Clicked += new WeakEventHandler(ButtonDelete_Clicked).Handler;

            NavItem.RightBarButtonItems = new UIBarButtonItem[]
            {
                buttonDelete,
                buttonEdit
            };

            var labelTitle = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredTitle3,
                Lines = 0
            };
            _itemBindingHost.SetLabelTextBinding(labelTitle, nameof(ViewModel.Grade.Name));
            StackView.AddArrangedSubview(labelTitle);
            labelTitle.StretchWidth(StackView);

            StackView.AddArrangedSubview(new UIView().SetHeight(4));

            var labelSubtitle = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredSubheadline,
                Lines = 0
            };
            _itemBindingHost.SetLabelTextBinding(labelSubtitle, nameof(ViewItemGrade.GradeSubtitle));
            _classBindingHost.SetBinding<byte[]>(nameof(ViewItemClass.Color), (color) =>
            {
                labelSubtitle.TextColor = BareUIHelper.ToColor(color);
            });
            StackView.AddArrangedSubview(labelSubtitle);
            labelSubtitle.StretchWidth(StackView);

            StackView.AddArrangedSubview(new UIView().SetHeight(4));

            var labelDate = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredSubheadline,
                TextColor = UIColor.DarkGray
            };
            _itemBindingHost.SetBinding<DateTime>(nameof(ViewItemGrade.Date), (date) =>
            {
                labelDate.Text = date.ToString("d");
            });
            StackView.AddArrangedSubview(labelDate);
            labelDate.StretchWidth(StackView);

            StackView.AddArrangedSubview(new UIView().SetHeight(4));

            var labelDetails = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredCaption1,
                Lines = 0,
                TextColor = UIColor.DarkGray
            };
            _itemBindingHost.SetLabelTextBinding(labelDetails, nameof(ViewItemGrade.Details));
            StackView.AddArrangedSubview(labelDetails);
            labelDetails.StretchWidth(StackView);
        }

        private void ButtonDelete_Clicked(object sender, EventArgs e)
        {
            PowerPlannerUIHelper.ConfirmDeleteQuick(this, NavItem.RightBarButtonItems.First(), ViewModel.Delete);
        }

        public override void OnViewModelLoadedOverride()
        {
            _itemBindingHost.BindingObject = ViewModel.Grade;
            _classBindingHost.BindingObject = ViewModel.Grade.WeightCategory.Class;

            base.OnViewModelLoadedOverride();
        }
    }
}