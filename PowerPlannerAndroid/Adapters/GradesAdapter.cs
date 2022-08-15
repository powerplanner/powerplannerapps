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
using AndroidX.RecyclerView.Widget;
using InterfacesDroid.Adapters;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAndroid.Views.ListItems;
using InterfacesDroid.Views;
using PowerPlannerAndroid.Views.ListItemHeaders;
using PowerPlannerAndroid.Views;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using Vx.Droid;
using AndroidX.Lifecycle;

namespace PowerPlannerAndroid.Adapters
{
    public class GradesAdapter : ObservableRecyclerViewAdapter
    {
        private const int GRADE_ITEM_TYPE = 0;
        private const int TOP_HEADER_ITEM_TYPE = 1;
        private const int SECTION_HEADER_ITEM_TYPE = 2;
        private const int UNASSIGNED_ITEMS_HEADER_TYPE = 3;
        private const int UNASSIGNED_ITEM_TYPE = 4;

        public event EventHandler<BaseViewItemMegaItem> ItemClick;
        public event EventHandler<ViewItemTaskOrEvent> UnassignedItemClick;
        public event EventHandler ButtonWhatIfModeClick;
        public event EventHandler ButtonEditGradeOptionsClick;

        private ClassGradesViewModel _viewModel;

        public GradesAdapter(ClassGradesViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            switch (viewType)
            {
                case GRADE_ITEM_TYPE:
                    var gradeView = new ListItemGradeView(parent);
                    gradeView.Click += GradeView_Click;
                    return new GenericRecyclerViewHolder(gradeView);

                case TOP_HEADER_ITEM_TYPE:
                    var topHeaderView = new ClassGradesViewModel.GradesSummaryComponent
                    {
                        Class = _viewModel.Class,
                        OnRequestConfigureGrades = _viewModel.ConfigureGrades
                    }.Render();
                    topHeaderView.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                    return new GenericRecyclerViewHolder(topHeaderView);

                case SECTION_HEADER_ITEM_TYPE:
                    return new GenericRecyclerViewHolder(new ListItemHeaderWeightCategoryView(parent));

                case UNASSIGNED_ITEMS_HEADER_TYPE:
                    return new GenericRecyclerViewHolder(new InflatedViewWithBinding(Resource.Layout.ListItemUnassignedItemsHeader, parent));

                case UNASSIGNED_ITEM_TYPE:
                    var unassignedView = new ListItemTaskOrEventView(parent);
                    unassignedView.Click += UnassignedView_Click;
                    return new GenericRecyclerViewHolder(unassignedView);

                default:
                    return base.OnCreateViewHolder(parent, viewType);
            }
        }

        private void Header_ButtonEditGradeOptionsClick(object sender, EventArgs e)
        {
            ButtonEditGradeOptionsClick?.Invoke(this, new EventArgs());
        }

        private void Header_ButtonWhatIfModeClick(object sender, EventArgs e)
        {
            ButtonWhatIfModeClick?.Invoke(this, new EventArgs());
        }

        private void UnassignedView_Click(object sender, EventArgs e)
        {
            UnassignedItemClick?.Invoke(this, (sender as ListItemTaskOrEventView).DataContext as ViewItemTaskOrEvent);
        }

        private void GradeView_Click(object sender, EventArgs e)
        {
            ItemClick?.Invoke(this, (sender as ListItemGradeView).DataContext as BaseViewItemMegaItem);
        }

        protected override int GetItemViewType(object item)
        {
            if (item is ViewItemGrade)  
                return GRADE_ITEM_TYPE;

            if (item is ViewItemWeightCategory)
                return SECTION_HEADER_ITEM_TYPE;

            if (item == (ClassGradesViewModel.UNASSIGNED_ITEMS_HEADER as object))
                return UNASSIGNED_ITEMS_HEADER_TYPE;

            if (item is ViewItemTaskOrEvent)
            {
                if ((item as ViewItemTaskOrEvent).IsUnassignedItem)
                {
                    return UNASSIGNED_ITEM_TYPE;
                }
                else
                {
                    return GRADE_ITEM_TYPE;
                }
            }

            return TOP_HEADER_ITEM_TYPE;
        }
    }
}