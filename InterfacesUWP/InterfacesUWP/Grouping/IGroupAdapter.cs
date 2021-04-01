using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfacesUWP.Grouping
{
    public interface IGroupAdapter
    {
        IGroupHeader GenerateHeader(object forItem);
    }
}
