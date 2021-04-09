using PowerPlannerApp.DataLayer.DataItems.BaseItems;
using PowerPlannerApp.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerApp.ViewItems
{
    public class ViewItemChildren<D, V> where D : BaseDataItem where V : BaseViewItem
    {
        public IEnumerable<D> PotentialInitialChildren { get; private set; }

        public Func<D, V> CreateChildMethod { get; private set; }

        public ViewItemChildren(IEnumerable<D> potentialInitialChildren, Func<D, V> createChildMethod)
        {
            PotentialInitialChildren = potentialInitialChildren;
            CreateChildMethod = createChildMethod;
        }
    }
}
