using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Vx.Views;
using Xamarin.Forms;

namespace Vx.Test
{
    [TestClass]
    public class PerfTests
    {
        [TestMethod]
        public void AbcInitializer()
        {
        }

        [TestMethod]
        public void CreatingFormsViews()
        {
            List<View> views = new List<View>();

            for (int i = 0; i < 5000; i++)
            {
                views.Add(new Grid());
            }
        }

        [TestMethod]
        public void CreatingVxViews()
        {
            List<VxView> views = new List<VxView>();

            for (int i = 0; i < 5000; i++)
            {
                views.Add(new VxLabel());
            }
        }
    }
}
