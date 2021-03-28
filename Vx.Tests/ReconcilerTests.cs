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
        public void TestAddedToBottom()
        {
            var oldViewTextBlock = new VxTextBlock("Enter your name");

            var newViewTextBlock = new VxTextBlock("Enter your name");
            var newView = new VxTextBox();

            var changes = VxReconciler.ReconcileList(new List<VxView>()
            {
                oldViewTextBlock
            }, new List<VxView>()
            {
                newViewTextBlock,
                newView
            });

            AssertListChanges(new List<VxReconcilerBaseListChange>()
            {
                new VxReconcilerUpdateListItem(oldViewTextBlock, newViewTextBlock),
                new VxReconcilerInsertListItem(1, newView)
            }, changes);
        }


        [TestMethod]
        public void TestAddedToTop()
        {
            var oldViewTextBox = new VxTextBox();

            var newView = new VxTextBlock("Enter your name");
            var newViewTextBox = new VxTextBox();

            var changes = VxReconciler.ReconcileList(new List<VxView>()
            {
                oldViewTextBox
            }, new List<VxView>()
            {
                newView,
                newViewTextBox
            });

            AssertListChanges(new List<VxReconcilerBaseListChange>()
            {
                new VxReconcilerInsertListItem(0, newView),
                new VxReconcilerUpdateListItem(1, oldViewTextBox, newViewTextBox)
            }, changes);
        }


        [TestMethod]
        public void TestUpdatedAtTop()
        {
            var oldView = new VxTextBlock("Enter your name:");
            var oldViewTb = new VxTextBox();
            var newView = new VxTextBlock("Name");
            var newViewTb = new VxTextBox();

            var changes = VxReconciler.ReconcileList(new List<VxView>()
            {
                oldView,
                oldViewTb
            }, new List<VxView>()
            {
                newView,
                newViewTb
            });

            AssertListChanges(new List<VxReconcilerBaseListChange>()
            {
                new VxReconcilerUpdateListItem(0, oldView, newView),
                new VxReconcilerUpdateListItem(1, oldViewTb, newViewTb)
            }, changes);
        }


        [TestMethod]
        public void TestReplacedAtTop()
        {
            var oldViewTextBlock = new VxTextBlock("Enter your name:");
            var oldViewTextBox = new VxTextBox();

            var newViewComboBox = new VxComboBox();
            var newViewTextBox = new VxTextBox();

            var changes = VxReconciler.ReconcileList(new List<VxView>()
            {
                oldViewTextBlock,
                oldViewTextBox
            }, new List<VxView>()
            {
                newViewComboBox,
                newViewTextBox
            });

            AssertListChanges(new List<VxReconcilerBaseListChange>()
            {
                new VxReconcilerReplaceListItem(0, newViewComboBox),
                new VxReconcilerUpdateListItem(1, oldViewTextBox, newViewTextBox)
            }, changes);
        }


        [TestMethod]
        public void TestReplacedAtBottom()
        {
            var oldViewTextBlock = new VxTextBlock("Enter your name:");
            var oldViewTextBox = new VxTextBox();

            var newViewTextBlock = new VxTextBlock("Enter your name:");
            var newViewComboBox = new VxComboBox();

            var changes = VxReconciler.ReconcileList(new List<VxView>()
            {
                oldViewTextBlock,
                oldViewTextBox
            }, new List<VxView>()
            {
                newViewTextBlock,
                newViewComboBox
            });

            AssertListChanges(new List<VxReconcilerBaseListChange>()
            {
                new VxReconcilerUpdateListItem(1, oldViewTextBlock, newViewTextBlock),
                new VxReconcilerReplaceListItem(1, newViewComboBox)
            }, changes);
        }

        private void AssertListChanges(List<VxReconcilerBaseListChange> expected, List<VxReconcilerBaseListChange> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                var expectedChange = expected[i];
                var actualChange = actual[i];

                Assert.IsInstanceOfType(actualChange, expectedChange.GetType());

                Assert.AreEqual(expectedChange, actualChange);
            }
        }
    }
}
