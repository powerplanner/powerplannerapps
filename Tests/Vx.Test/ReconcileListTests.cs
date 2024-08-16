using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Vx.Views;

namespace Vx.Test
{
    [TestClass]
    public class ReconcileListTests
    {
        private class TestTheme : Theme
        {
            public override bool IsDarkTheme => false;

            public override Color ForegroundColor => Color.Black;

            public override Color SubtleForegroundColor => Color.DarkGray;

            public override Color PopupPageBackgroundColor => Color.LightGray;

            public override Color PopupPageBackgroundAltColor => Color.White;
        }

        private class NativeTestView : NativeView
        {
            public Action<View, View> OnApplyProperties { get; set; }

            private bool _isInitialApply = true;

            protected override void ApplyProperties(View oldView, View newView)
            {
                if (_isInitialApply)
                {
                    _isInitialApply = false;
                    return;
                }

                if (OnApplyProperties != null)
                {
                    OnApplyProperties(oldView, newView);
                }
            }
        }

        [TestInitialize] public void Init()
        {
            Theme.Current = new TestTheme();
        }

        [TestMethod]
        public void TestZeroToOne()
        {
            var tb = new TextBlock
            {
                Text = "Tacos"
            };

            TestReconcile(
                new List<View>(),
                new List<View>
                {
                    tb
                }, new ExpectedOperations
                {
                    Inserts = 1
                });
        }

        [TestMethod]
        public void TestOneToZero()
        {
            TestReconcile(
                new List<View>
                {
                    new TextBlock
                    {
                        Text = "Tacos"
                    }
                },
                new List<View>(),
                new ExpectedOperations
                {
                    Clears = 1
                });
        }

        [TestMethod]
        public void TestTwoToZero()
        {
            TestReconcile(
                new List<View>
                {
                    new TextBlock
                    {
                        Text = "Tacos"
                    },
                    new Button
                    {
                        Text = "Click me"
                    }
                },
                new List<View>(),
                new ExpectedOperations
                {
                    Clears = 1
                });
        }

        [TestMethod]
        public void TestChangeOrder()
        {
            TestReconcile(
                new List<View>
                {
                    new TextBlock
                    {
                        Text = "Tacos"
                    },
                    new Button
                    {
                        Text = "Click me"
                    }
                },
                new List<View>
                {
                    new Button
                    {
                        Text = "Click me"
                    },
                    new TextBlock
                    {
                        Text = "Tacos"
                    }
                },
                new ExpectedOperations
                {
                    Replaces = 2
                });
        }

        [TestMethod]
        public void TestNoChanges()
        {
            TestReconcile(new List<View>
            {
                new TextBlock
                {
                    Text = "Tacos"
                }
            }, new List<View>
            {
                new TextBlock
                {
                    Text = "Tacos"
                }
            }, new ExpectedOperations
            {
                ApplyProps = 1
            });
        }

        [TestMethod]
        public void TestChangeType()
        {
            TestReconcile(new List<View>
            {
                new TextBlock
                {
                    Text = "Tacos"
                }
            }, new List<View>
            {
                new Button
                {
                    Text = "Click me"
                }
            }, new ExpectedOperations
            {
                Replaces = 1
            });
        }

        private class ExpectedOperations
        {
            public int Inserts { get; set; }
            public int Removes { get; set; }
            public int Replaces { get; set; }
            public int Clears { get; set; }
            public int ApplyProps { get; set; }
        }

        private void TestReconcile(List<View> oldList, List<View> newList, ExpectedOperations expectedOperations)
        {
            var finalList = new List<View>(oldList);

            int actualInserts = 0, actualRemoves = 0, actualReplaces = 0, actualClears = 0, actualApplyProps = 0;

            foreach (var item in oldList)
            {
                item.SetNativeView(new NativeTestView
                {
                    OnApplyProperties = (oldView, newView) =>
                    {
                        var index = finalList.IndexOf(oldView);
                        if (index == -1)
                        {
                            Assert.Fail("Couldn't find old view");
                        }
                        finalList[index] = newView;
                        actualApplyProps++;
                    }
                });
            }

            NativeView.ReconcileList(
                oldList,
                newList,
                insert: (i, v) =>
                {
                    finalList.Insert(i, v);
                    actualInserts++;
                },
                remove: (i) =>
                {
                    finalList.RemoveAt(i);
                    actualRemoves++;
                },
                replace: (i, v) =>
                {
                    Assert.AreNotEqual(finalList[i].GetType(), v.GetType(), "When replacing, item types should be different.");
                    finalList[i] = v;
                    actualReplaces++;
                },
                clear: () =>
                {
                    finalList.Clear();
                    actualClears++;
                }, catchErrorsInDebug: false);

            Assert.AreEqual(expectedOperations.Inserts, actualInserts, "Incorrect number of inserts");
            Assert.AreEqual(expectedOperations.Removes, actualRemoves, "Incorrect number of removes");
            Assert.AreEqual(expectedOperations.Replaces, actualReplaces, "Incorrect number of replaces");
            Assert.AreEqual(expectedOperations.Clears, actualClears, "Incorrect number of clears");
            Assert.AreEqual(expectedOperations.ApplyProps, actualApplyProps, "Incorrect number of apply props");

            Assert.IsTrue(newList.SequenceEqual(finalList), "Final list didn't match expected.");
        }
    }
}
