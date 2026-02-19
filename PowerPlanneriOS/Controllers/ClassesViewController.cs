using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Classes;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using ToolsPortable;
using System.Collections.Specialized;
using PowerPlannerAppDataLibrary.Extensions;
using CoreAnimation;
using CoreGraphics;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary;

namespace PowerPlanneriOS.Controllers
{
    public class ClassesViewController : PopupViewController<ClassesViewModel>
    {
        private UILabel _labelNoClasses;
        private UITableView _tableViewClasses;
        private object _tabBarHeightListener;

        public ClassesViewController()
        {
            Title = PowerPlannerResources.GetString("MainMenuItem_Classes");
            HideBackButton();
        }

        public override void OnViewModelLoadedOverride()
        {
            var buttonAdd = new UIBarButtonItem(UIBarButtonSystemItem.Add);
            buttonAdd.Clicked += new WeakEventHandler<EventArgs>(delegate { ViewModel.AddClass(); }).Handler;
            NavItem.RightBarButtonItem = buttonAdd;

            _labelNoClasses = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = PowerPlannerResources.GetString("ClassesPage_TextBlockNoClassesDescription.Text"),
                Font = UIFont.PreferredCallout,
                TextAlignment = UITextAlignment.Center
            };
            ContentView.Add(_labelNoClasses);
            _labelNoClasses.StretchWidthAndHeight(ContentView, left: 16, top: 16, right: 16, bottom: 16);

            _tableViewClasses = new UITableView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                CellLayoutMarginsFollowReadableWidth = false
            };
            _tableViewClasses.TableFooterView = new UIView(); // Eliminate extra separators on bottom of view
            var tableViewClassesSource = new BareUITableViewSource<UIClassView>(_tableViewClasses, ViewModel.MainScreenViewModel.Classes);
            tableViewClassesSource.ItemSelected += new WeakEventHandler<object>(TableViewClassesSource_ItemSelected).Handler;
            _tableViewClasses.Source = tableViewClassesSource;
            ContentView.Add(_tableViewClasses);
            _tableViewClasses.StretchWidthAndHeight(ContentView);

            MainScreenViewController.ListenToTabBarHeightChanged(ref _tabBarHeightListener, delegate
            {
                _tableViewClasses.ContentInset = new UIEdgeInsets(0, 0, MainScreenViewController.TAB_BAR_HEIGHT, 0);
            });

            ViewModel.MainScreenViewModel.Classes.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(Classes_CollectionChanged).Handler;
            UpdateNoClassesVisual();

            base.OnViewModelLoadedOverride();
        }

        private void TableViewClassesSource_ItemSelected(object sender, object e)
        {
            if (e is ViewItemClass)
            {
                ViewModel.MainScreenViewModel.SelectClassWithinSemester(e as ViewItemClass, allowGoingBack: true);

                // Immediately unselect it
                _tableViewClasses.SelectRow(null, true, UITableViewScrollPosition.None);
            }
        }

        private void Classes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                UpdateNoClassesVisual();
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void UpdateNoClassesVisual()
        {
            _labelNoClasses.Hidden = ViewModel.MainScreenViewModel.Classes.Count > 0;
            _tableViewClasses.Hidden = !_labelNoClasses.Hidden;
        }

        private class UIClassView : BareUIView
        {
            private CAShapeLayer _circle;
            private UILabel _labelName;

            public UIClassView()
            {
                const int CIRCLE_HEIGHT = 18;

                _circle = new CAShapeLayer();
                _circle.Path = CGPath.EllipseFromRect(new CGRect(16, 12, CIRCLE_HEIGHT, CIRCLE_HEIGHT));
                BindingHost.SetColorBinding(_circle, nameof(ViewItemClass.Color));
                base.Layer.AddSublayer(_circle);

                _labelName = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.PreferredTitle3,
                    Lines = 1
                };
                BindingHost.SetLabelTextBinding(_labelName, nameof(ViewItemClass.Name));
                this.Add(_labelName);
                _labelName.StretchWidthAndHeight(this, left: CIRCLE_HEIGHT + 16 + 16, right: 16);

                this.SetHeight(44);
            }
        }
    }
}