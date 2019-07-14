using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UpgradeFromSilverlight
{
    [CollectionDataContract(Namespace = "http://schemas.datacontract.org/2004/07/ToolsPortableMost")]
    public class FakeList<T> : List<T>
    {

    }
}
