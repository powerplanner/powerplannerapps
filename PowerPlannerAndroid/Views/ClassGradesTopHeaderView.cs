using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Views;
using InterfacesDroid.DataTemplates;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using PowerPlannerAndroid.Views.ListItems;
using Android.Util;

namespace PowerPlannerAndroid.Views
{
    public class ClassGradesTopHeaderView : InflatedViewWithBinding
    {
        public event EventHandler ButtonWhatIfModeClick;
        public event EventHandler ButtonEditGradeOptionsClick;

        private ItemsControlWrapper _itemsControlWeightCategoriesSummary;

        public ClassGradesTopHeaderView(ViewGroup root, bool hideWhatIfModeButton) : base(Resource.Layout.ClassGradesTopHeader, root)
        {
            var viewGroupWeightCategoriesSummary = FindViewById<ViewGroup>(Resource.Id.ViewGroupWeightCategoriesSummary);
            _itemsControlWeightCategoriesSummary = new ItemsControlWrapper(viewGroupWeightCategoriesSummary)
            {
                ItemTemplate = new CustomDataTemplate<ViewItemWeightCategory>(CreateWeightCategorySummary)
            };

            var buttonWhatIfMode = FindViewById<Button>(Resource.Id.ButtonWhatIfMode);
            if (hideWhatIfModeButton)
            {
                buttonWhatIfMode.Visibility = ViewStates.Gone;
            }
            else
            {
                buttonWhatIfMode.Click += ButtonWhatIfMode_Click;
            }

            FindViewById<Button>(Resource.Id.ButtonEditGradeOptions).Click += ClassGradesTopHeaderView_Click;
        }

        private void ClassGradesTopHeaderView_Click(object sender, EventArgs e)
        {
            ButtonEditGradeOptionsClick?.Invoke(this, new EventArgs());
        }

        private void ButtonWhatIfMode_Click(object sender, EventArgs e)
        {
            ButtonWhatIfModeClick?.Invoke(this, new EventArgs());
        }

        protected override void OnDataContextChanged(object oldValue, object newValue)
        {
            var viewModel = newValue as ClassGradesViewModel;

            if (viewModel == null)
            {
                var whatIfViewModel = newValue as ClassWhatIfViewModel;
                if (whatIfViewModel != null)
                {
                    _itemsControlWeightCategoriesSummary.ItemsSource = whatIfViewModel.Class.WeightCategories;
                }
                else
                {
                    _itemsControlWeightCategoriesSummary.ItemsSource = null;
                }
            }
            else
                _itemsControlWeightCategoriesSummary.ItemsSource = viewModel.Class.WeightCategories;
        }

        private View CreateWeightCategorySummary(ViewGroup parent, ViewItemWeightCategory weightCategory)
        {
            return new ListItemWeightCategorySummaryView(parent)
            {
                DataContext = weightCategory
            };
        }
    }
}