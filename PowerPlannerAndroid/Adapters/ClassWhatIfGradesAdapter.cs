using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Adapters;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAndroid.Views.ListItems;
using InterfacesDroid.Views;
using PowerPlannerAndroid.Views.ListItemHeaders;
using PowerPlannerAndroid.Views;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;

namespace PowerPlannerAndroid.Adapters
{
    public class ClassWhatIfGradesAdapter : ObservableRecyclerViewAdapter
    {
        private const int GRADE_ITEM_TYPE = 0;
        private const int TOP_HEADER_ITEM_TYPE = 1;
        private const int SECTION_HEADER_ITEM_TYPE = 2;

        public event EventHandler<BaseViewItemMegaItem> ItemClick;

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            switch (viewType)
            {
                case GRADE_ITEM_TYPE:
                    var gradeView = new ListItemGradeView(parent);
                    gradeView.Click += GradeView_Click;
                    return new GenericRecyclerViewHolder(gradeView);

                case TOP_HEADER_ITEM_TYPE:
                    var header = new ClassWhatIfTopHeaderView(parent);
                    return new GenericRecyclerViewHolder(header);

                case SECTION_HEADER_ITEM_TYPE:
                    return new GenericRecyclerViewHolder(new ListItemHeaderWeightCategoryView(parent));

                default:
                    return base.OnCreateViewHolder(parent, viewType);
            }
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

            if (item is ViewItemTaskOrEvent)
            {
                return GRADE_ITEM_TYPE;
            }

            return TOP_HEADER_ITEM_TYPE;
        }
    }
}