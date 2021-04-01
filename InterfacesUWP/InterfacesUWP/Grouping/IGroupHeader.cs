using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfacesUWP.Grouping
{
    public interface IGroupHeader : IComparable<IGroupHeader>
    {
        int CompareInsideHeader(object item, object compareTo);
    }
}
