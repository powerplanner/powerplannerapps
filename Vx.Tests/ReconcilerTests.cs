using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Reconciler;
using Vx.Views;

namespace Vx.Tests
{
    [TestClass]
    public class ReconcilerTests
    {
        [TestMethod]
        public void TestBasicReconcilation()
        {
            VxReconciler.Reconcile(new VxTextBlock("Andrew"),

                new VxTextBlock("Andrew Leader"));
        }

        [TestMethod]
        public void TestMovingItemsDown()
        {
            // This should keep the existing item and move it down somehow...
            // Important that this works for cases where sometimes a top level view is added, the entire
            // tree should be kept but just moved down
            // But maybe this is just too tough to support.
            VxReconciler.Reconcile(new VxTextBlock("Andrew"),

                new VxGrid()
                    .Children(new VxTextBlock("Andrew")));
        }
    }
}
