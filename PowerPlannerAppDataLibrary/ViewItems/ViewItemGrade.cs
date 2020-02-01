using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerSending;

namespace PowerPlannerAppDataLibrary.ViewItems
{
    public class ViewItemGrade : BaseViewItemHomeworkExamGrade, IComparable<ViewItemGrade>
    {
        public ViewItemGrade(DataItemGrade dataItem) : base(dataItem) { }

        public byte[] ColorWhenInWhatIfMode
        {
            get
            {
                if (WasChanged)
                {
                    if (WeightCategory != null && WeightCategory.Class != null)
                        return WeightCategory.Class.Color;
                }

                return new byte[] { 190, 190, 190 };
            }
        }

        //internal override void HandleDataChangedEvent(DataChangedEvent e)
        //{
        // TODO - the base class should handle the item being edited?

        /// I could probably have attributes on all the properties or something, so that the base class can compare the current values and find out what changed,
        /// and then intelligently send out the notify property changed events for those specific properties. No use in having each class implement that logic
        /// themselves.
        /// 
        /// Or should item itself even process its own edited event? In many cases the parent would re-insert right? Or maybe we'll have the 
        /// parent intelligently remove and then re-insert... yeah, we're not going to create a completely new item just because of that, we'll still
        /// reuse the existing item, and thus its properties need to be updated (some views might be referring to the item itself)
        //}

        protected override void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            base.PopulateFromDataItemOverride(dataItem);
        }

        public override int CompareTo(object obj)
        {
            if (obj is ViewItemGrade)
                return CompareTo(obj as ViewItemGrade);

            return base.CompareTo(obj);
        }

        public int CompareTo(ViewItemGrade other)
        {
            if (this.Date < other.Date)
                return -1;

            else if (this.Date > other.Date)
                return 1;

            return base.CompareTo(other);
        }
    }
}
