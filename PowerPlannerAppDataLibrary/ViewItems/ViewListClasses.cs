using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewItems
{
    public class ViewListClasses : BaseViewItemWithoutData
    {
        public MyObservableList<ViewItemClass> Classes { get; private set; } = new MyObservableList<ViewItemClass>();

        public Guid SemesterId { get; private set; }

        public ViewListClasses(Guid semesterId)
        {
            SemesterId = semesterId;

            AddChildrenHelper(new ViewItemChildrenHelper<DataItemClass, ViewItemClass>(
                isChild: IsChild,
                addMethod: Add,
                removeMethod: Remove,
                createChildMethod: CreateClass,
                children: Classes));
        }

        private bool IsChild(DataItemClass dataItem)
        {
            return dataItem.UpperIdentifier == SemesterId;
        }

        private ViewItemClass CreateClass(DataItemClass dataItem)
        {
            return new ViewItemClass(dataItem);
        }

        private void Add(ViewItemClass c)
        {
            Classes.InsertSorted(c);
        }
        private void Remove(ViewItemClass c)
        {
            Classes.Remove(c);
        }
    }
}
