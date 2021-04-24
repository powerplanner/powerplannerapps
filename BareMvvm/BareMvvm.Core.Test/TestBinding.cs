using BareMvvm.Core.Binding;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using ToolsPortable;

namespace BareMvvm.Core.Test
{
    [TestClass]
    public class TestBinding
    {
        public class MyTask : BindableBase
        {
            private string _name;
            public string Name
            {
                get => _name;
                set => SetProperty(ref _name, value, nameof(Name));
            }

            private MyClass _class;
            public MyClass Class
            {
                get => _class;
                set => SetProperty(ref _class, value, nameof(Class));
            }

            private double _percentComplete;
            public double PercentComplete
            {
                get => _percentComplete;
                set => SetProperty(ref _percentComplete, value, nameof(PercentComplete));
            }
        }

        public class MyClass : BindableBase
        {
            private string _name;
            public string Name
            {
                get => _name;
                set => SetProperty(ref _name, value, nameof(Name));
            }

            private byte[] _color;
            public byte[] Color
            {
                get => _color;
                set => SetProperty(ref _color, value, nameof(Color));
            }

            private MyTeacher _teacher;
            public MyTeacher Teacher
            {
                get => _teacher;
                set => SetProperty(ref _teacher, value, nameof(Teacher));
            }

            private int _priority;
            public int Priority
            {
                get => _priority;
                set => SetProperty(ref _priority, value, nameof(Priority));
            }
        }

        public class MyTeacher : BindableBase
        {
            private string _name;
            public string Name
            {
                get => _name;
                set => SetProperty(ref _name, value, nameof(Name));
            }
        }

        [TestMethod]
        public void TestOneLevelBinding()
        {
            var task = new MyTask()
            {
                Name = "Bookwork",
                PercentComplete = 0.3
            };

            BindingHost bindingHost = new BindingHost()
            {
                DataContext = task
            };

            int nameBindingExecutions = 0;
            int percentCompleteBindingExecutions = 0;

            bindingHost.SetBinding<string>(nameof(MyTask.Name), name =>
            {
                Assert.AreEqual(task.Name, name);
                nameBindingExecutions++;
            });

            bindingHost.SetBinding<double>(nameof(MyTask.PercentComplete), percentComplete =>
            {
                Assert.AreEqual(task.PercentComplete, percentComplete);
                percentCompleteBindingExecutions++;
            });

            task.Name = "Bookwork updated";
            task.PercentComplete = 0.6;

            Assert.AreEqual(2, nameBindingExecutions);
            Assert.AreEqual(2, percentCompleteBindingExecutions);

            bindingHost.Unregister();

            // Now any changes shouldn't trigger anything
            task.Name = "Bookwork 2";
            task.PercentComplete = 1;

            Assert.AreEqual(2, nameBindingExecutions);
            Assert.AreEqual(2, percentCompleteBindingExecutions);
        }

        [TestMethod]
        public void TestMultiLevelBinding()
        {
            var teacher1 = new MyTeacher()
            {
                Name = "Steven"
            };

            var teacher2 = new MyTeacher()
            {
                Name = "Stephanie"
            };

            var class1 = new MyClass()
            {
                Name = "Math",
                Color = new byte[] { 4, 3, 2 },
                Teacher = teacher1
            };

            var class2 = new MyClass()
            {
                Name = "Science",
                Color = new byte[] { 5, 6, 7 },
                Teacher = teacher2
            };

            var task = new MyTask()
            {
                Name = "Bookwork",
                PercentComplete = 0.3,
                Class = class1
            };

            var bindingHost = new BindingHost()
            {
                DataContext = task
            };

            int classNameExecutions = 0;
            int classTeacherExecutions = 0;
            int classTeacherNameExecutions = 0;

            bindingHost.SetBinding<string>("Class.Name", className =>
            {
                Assert.AreEqual(task.Class.Name, className);
                classNameExecutions++;
            });

            bindingHost.SetBinding<MyTeacher>("Class.Teacher", teacher =>
            {
                Assert.ReferenceEquals(task.Class.Teacher, teacher);
                classTeacherExecutions++;
            });

            bindingHost.SetBinding<string>("Class.Teacher.Name", teacherName =>
            {
                Assert.AreEqual(task.Class.Teacher.Name, teacherName);
                classTeacherNameExecutions++;
            });

            task.Class = class2; // This should trigger all bindings
            task.Class.Teacher = teacher1; // This should trigger only teacher bindings

            // These shouldn't trigger any of the bindings anymore since they're no longer referenced
            class1.Name = "Spanish";
            class1.Teacher = teacher2;
            teacher2.Name = "Bob";

            Assert.AreEqual(2, classNameExecutions);
            Assert.AreEqual(3, classTeacherExecutions);
            Assert.AreEqual(3, classTeacherNameExecutions);

            bindingHost.Unregister();

            // Now any of these changes shouldn't trigger anything
            task.Class = class1;

            Assert.AreEqual(2, classNameExecutions);
            Assert.AreEqual(3, classTeacherExecutions);
            Assert.AreEqual(3, classTeacherNameExecutions);
        }

        [TestMethod]
        public void TestBindingToNulls()
        {
            var task = new MyTask()
            {
                Name = "Bookwork",
                PercentComplete = 0.3
            };

            BindingHost bindingHost = new BindingHost()
            {
                DataContext = task
            };

            int classNameBindingExecutions = 0;
            int classPriorityBindingExecutions = 0;
            int classPriorityObjBindingExecutions = 0;

            // When property doesn't exist, should be default value of the type
            bindingHost.SetBinding<string>("Class.Name", className =>
            {
                Assert.AreEqual(task.Class?.Name, className);
                classNameBindingExecutions++;
            });

            bindingHost.SetBinding<int>("Class.Priority", classPriority =>
            {
                Assert.AreEqual(task.Class?.Priority ?? default, classPriority);
                classPriorityBindingExecutions++;
            });

            // And with this binder, it should be null if not found
            bindingHost.SetBinding("Class.Priority", classPriorityObj =>
            {
                Assert.AreEqual(task.Class?.Priority, classPriorityObj);
                classPriorityObjBindingExecutions++;
            });

            // That should trigger another execution
            task.Class = new MyClass()
            {
                Name = "Spanish",
                Priority = 3
            };

            // And another execution (this time back to null)
            task.Class = null;

            Assert.AreEqual(3, classNameBindingExecutions);
            Assert.AreEqual(3, classPriorityBindingExecutions);
            Assert.AreEqual(3, classPriorityObjBindingExecutions);
        }

        [TestMethod]
        public void TestSettingDataContextLater()
        {
            var task = new MyTask()
            {
                Name = "Bookwork",
                PercentComplete = 0.3,
                Class = new MyClass()
                {
                    Name = "Math"
                }
            };

            BindingHost bindingHost = new BindingHost();

            int taskNameBindingExecutions = 0;
            int classNameBindingExecutions = 0;

            // Shouldn't execute until I set data context
            bindingHost.SetBinding<string>("Name", name =>
            {
                if (taskNameBindingExecutions == 0)
                {
                    Assert.AreEqual(task.Name, name);
                }
                else
                {
                    Assert.IsNull(name);
                }

                taskNameBindingExecutions++;
            });

            bindingHost.SetBinding<string>("Class.Name", className =>
            {
                if (classNameBindingExecutions == 0)
                {
                    Assert.AreEqual(task.Class.Name, className);
                }
                else
                {
                    Assert.IsNull(className);
                }

                classNameBindingExecutions++;
            });

            // This should cause them to execute
            bindingHost.DataContext = task;

            Assert.AreEqual(1, taskNameBindingExecutions);
            Assert.AreEqual(1, classNameBindingExecutions);

            // And then setting it to null should cause everything to execute again
            bindingHost.DataContext = null;

            Assert.AreEqual(2, taskNameBindingExecutions);
            Assert.AreEqual(2, classNameBindingExecutions);

            // And changing values shouldn't do anything
            task.Name = "Changed";
            task.Class.Name = "Changed";

            Assert.AreEqual(2, taskNameBindingExecutions);
            Assert.AreEqual(2, classNameBindingExecutions);
        }

        [TestMethod]
        public void TestChangingDataContext()
        {
            var class1 = new MyClass()
            {
                Name = "Math"
            };

            var class2 = new MyClass()
            {
                Name = "Science"
            };

            var task1 = new MyTask()
            {
                Name = "Bookwork",
                Class = class1
            };

            var task2 = new MyTask()
            {
                Name = "Essay",
                Class = class2
            };

            var bindingHost = new BindingHost()
            {
                DataContext = task1
            };

            int nameExecutions = 0;
            int classNameExecutions = 0;

            bindingHost.SetBinding<string>("Name", name =>
            {
                if (nameExecutions == 0)
                {
                    Assert.AreEqual(task1.Name, name);
                }
                else
                {
                    Assert.AreEqual(task2.Name, name);
                }

                nameExecutions++;
            });

            bindingHost.SetBinding<string>("Class.Name", className =>
            {
                if (classNameExecutions == 0)
                {
                    Assert.AreEqual(task1.Class.Name, className);
                }
                else
                {
                    Assert.AreEqual(task2.Class.Name, className);
                }

                classNameExecutions++;
            });

            Assert.AreEqual(1, nameExecutions);
            Assert.AreEqual(1, classNameExecutions);

            // This should re-trigger both bindings
            bindingHost.DataContext = task2;
            Assert.AreEqual(2, nameExecutions);
            Assert.AreEqual(2, classNameExecutions);

            // Changing values in original task shouldn't trigger anything
            task1.Name = "Doesn't matter";
            class1.Name = "Class 1 doesn't matter";
            Assert.AreEqual(2, nameExecutions);
            Assert.AreEqual(2, classNameExecutions);

            // Changing values in new task should trigger
            task2.Name = "Task 2 updated";
            task2.Class = class1;
            Assert.AreEqual(3, nameExecutions);
            Assert.AreEqual(3, classNameExecutions);

            // Changing value in old no longer referenced class shouldn't trigger
            class2.Name = "No longer referenced";
            Assert.AreEqual(3, nameExecutions);
            Assert.AreEqual(3, classNameExecutions);
        }

        [TestMethod]
        public void TestSettingValuesThroughBinding()
        {
            var task = new MyTask()
            {
                Name = "Bookwork",
                PercentComplete = 0.3,
                Class = new MyClass()
                {
                    Name = "Math"
                }
            };

            BindingHost bindingHost = new BindingHost()
            {
                DataContext = task
            };

            int name1Executions = 0;
            int name2Executions = 0;
            int className1Executions = 0;
            int className2Executions = 0;

            var name1Registration = bindingHost.SetBinding<string>("Name", name =>
            {
                Assert.AreEqual(task.Name, name);
                name1Executions++;
            });

            var name2Registration = bindingHost.SetBinding<string>("Name", name =>
            {
                Assert.AreEqual(task.Name, name);
                name2Executions++;
            });

            var className1Registration = bindingHost.SetBinding<string>("Class.Name", className =>
            {
                Assert.AreEqual(task.Class.Name, className);
                className1Executions++;
            });

            var className2Registration = bindingHost.SetBinding<string>("Class.Name", className =>
            {
                Assert.AreEqual(task.Class.Name, className);
                className2Executions++;
            });

            // Should only have initial executions so far
            Assert.AreEqual(1, name1Executions);
            Assert.AreEqual(1, name2Executions);
            Assert.AreEqual(1, className1Executions);
            Assert.AreEqual(1, className2Executions);

            name1Registration.SetSourceValue("Bookwork updated");

            // Only ones I didn't set through should have updated
            Assert.AreEqual("Bookwork updated", task.Name);
            Assert.AreEqual(1, name1Executions);
            Assert.AreEqual(2, name2Executions);
            Assert.AreEqual(1, className1Executions);
            Assert.AreEqual(1, className2Executions);

            className2Registration.SetSourceValue("Math updated");

            // Only ones I didn't set through should have updated
            Assert.AreEqual("Math updated", task.Class.Name);
            Assert.AreEqual(1, name1Executions);
            Assert.AreEqual(2, name2Executions);
            Assert.AreEqual(2, className1Executions);
            Assert.AreEqual(1, className2Executions);

            // And vice versa
            name2Registration.SetSourceValue("Bookwork updated 2");
            className1Registration.SetSourceValue("Math updated 2");
            Assert.AreEqual("Bookwork updated 2", task.Name);
            Assert.AreEqual("Math updated 2", task.Class.Name);
            Assert.AreEqual(2, name1Executions);
            Assert.AreEqual(2, name2Executions);
            Assert.AreEqual(2, className1Executions);
            Assert.AreEqual(2, className2Executions);

            // And setting programmatically should update all
            task.Name = "Bookwork 3";
            task.Class.Name = "Math 3";
            Assert.AreEqual(3, name1Executions);
            Assert.AreEqual(3, name2Executions);
            Assert.AreEqual(3, className1Executions);
            Assert.AreEqual(3, className2Executions);
        }



        [TestMethod]
        public void TestSettingValuesThroughBindingWithPreObtainedProperty()
        {
            var task = new MyTask()
            {
                Name = "Bookwork",
                PercentComplete = 0.3,
                Class = new MyClass()
                {
                    Name = "Math"
                }
            };

            BindingHost bindingHost = new BindingHost()
            {
                DataContext = task
            };

            int name1Executions = 0;
            int name2Executions = 0;
            int className1Executions = 0;
            int className2Executions = 0;

            var name1Registration = bindingHost.SetBinding<string>("Name", name =>
            {
                Assert.AreEqual(task.Name, name);
                name1Executions++;
            });

            var name2Registration = bindingHost.SetBinding<string>("Name", name =>
            {
                Assert.AreEqual(task.Name, name);
                name2Executions++;
            });

            var className1Registration = bindingHost.SetBinding<string>("Class.Name", className =>
            {
                Assert.AreEqual(task.Class.Name, className);
                className1Executions++;
            });

            var className2Registration = bindingHost.SetBinding<string>("Class.Name", className =>
            {
                Assert.AreEqual(task.Class.Name, className);
                className2Executions++;
            });

            // Should only have initial executions so far
            Assert.AreEqual(1, name1Executions);
            Assert.AreEqual(1, name2Executions);
            Assert.AreEqual(1, className1Executions);
            Assert.AreEqual(1, className2Executions);

            var name1Property = name1Registration.GetSourceProperty();
            name1Registration.SetSourceValue("Bookwork updated", name1Property);

            // Only ones I didn't set through should have updated
            Assert.AreEqual("Bookwork updated", task.Name);
            Assert.AreEqual(1, name1Executions);
            Assert.AreEqual(2, name2Executions);
            Assert.AreEqual(1, className1Executions);
            Assert.AreEqual(1, className2Executions);

            var className2Property = className2Registration.GetSourceProperty();
            className2Registration.SetSourceValue("Math updated", className2Property);

            // Only ones I didn't set through should have updated
            Assert.AreEqual("Math updated", task.Class.Name);
            Assert.AreEqual(1, name1Executions);
            Assert.AreEqual(2, name2Executions);
            Assert.AreEqual(2, className1Executions);
            Assert.AreEqual(1, className2Executions);
        }

        [TestMethod]
        public void TestBindingToDataContext()
        {
            var task1 = new MyTask()
            {
                Name = "Bookwork 1"
            };

            var task2 = new MyTask()
            {
                Name = "Bookwork 2"
            };

            BindingHost bindingHost = new BindingHost()
            {
                DataContext = task1
            };

            int timesInvoked = 0;

            bindingHost.SetBinding<MyTask>("", task =>
            {
                if (timesInvoked == 0)
                {
                    Assert.ReferenceEquals(task1, task);
                }
                else
                {
                    Assert.ReferenceEquals(task2, task);
                }

                timesInvoked++;
            });

            Assert.AreEqual(1, timesInvoked);

            bindingHost.DataContext = task2;

            Assert.AreEqual(2, timesInvoked);
        }

        [TestMethod]
        public void TestUnregisterBinding()
        {
            var task = new MyTask()
            {
                Name = "Bookwork"
            };

            var bindingHost = new BindingHost()
            {
                DataContext = task
            };

            int nameExecutions = 0;

            var nameBinding = bindingHost.SetBinding<string>("Name", name =>
            {
                Assert.AreEqual(task.Name, name);
                nameExecutions++;
            });

            Assert.AreEqual(1, nameExecutions);

            nameBinding.Unregister();

            task.Name = "Bookwork updated";

            Assert.AreEqual(1, nameExecutions);

            // Register again
            bindingHost.SetBinding<string>("Name", name =>
            {
                Assert.AreEqual(task.Name, name);
                nameExecutions++;
            });

            Assert.AreEqual(2, nameExecutions);

            task.Name = "Bookwork updated 2";

            Assert.AreEqual(3, nameExecutions);
        }

        [TestMethod]
        public void TestUnregisterMultiLevelBinding()
        {
            var c1 = new MyClass()
            {
                Name = "Math"
            };

            var c2 = new MyClass()
            {
                Name = "Science"
            };

            var task = new MyTask()
            {
                Name = "Bookwork",
                Class = c1
            };

            var bindingHost = new BindingHost()
            {
                DataContext = task
            };

            int classNameExecutions = 0;

            var classNameBinding = bindingHost.SetBinding<string>("Class.Name", className =>
            {
                Assert.AreEqual(task.Class.Name, className);
                classNameExecutions++;
            });

            Assert.AreEqual(1, classNameExecutions);

            classNameBinding.Unregister();

            // Changing these shouldn't do anything
            task.Class.Name = "Math updated";
            task.Class = c2;
            task.Name = "Bookwork updated";

            Assert.AreEqual(1, classNameExecutions);

            // Register again
            bindingHost.SetBinding<string>("Class.Name", className =>
            {
                Assert.AreEqual(task.Class.Name, className);
                classNameExecutions++;
            });

            // It should have ran now
            Assert.AreEqual(2, classNameExecutions);

            // And changing these should have an effect (2 additional runs)
            task.Class.Name = "Science updated";
            task.Class = c1;
            task.Name = "Bookwork updated 2";

            Assert.AreEqual(4, classNameExecutions);
        }

        [TestMethod]
        public void TestUnregisteringJustOneBinding()
        {
            var class1 = new MyClass()
            {
                Name = "Math"
            };

            var class2 = new MyClass()
            {
                Name = "Science"
            };

            var task = new MyTask()
            {
                Name = "Bookwork",
                Class = class1
            };

            var bindingHost = new BindingHost()
            {
                DataContext = task
            };

            int name1Executions = 0;
            int name2Executions = 0;
            int className1Executions = 0;
            int className2Executions = 0;


            var name1Binding = bindingHost.SetBinding<string>("Name", name =>
            {
                Assert.AreEqual(task.Name, name);
                name1Executions++;
            });

            var name2Binding = bindingHost.SetBinding<string>("Name", name =>
            {
                Assert.AreEqual(task.Name, name);
                name2Executions++;
            });

            var className1Binding = bindingHost.SetBinding<string>("Class.Name", name =>
            {
                Assert.AreEqual(task.Class.Name, name);
                className1Executions++;
            });

            var className2Binding = bindingHost.SetBinding<string>("Class.Name", name =>
            {
                Assert.AreEqual(task.Class.Name, name);
                className2Executions++;
            });

            Assert.AreEqual(1, name1Executions);
            Assert.AreEqual(1, name2Executions);
            Assert.AreEqual(1, className1Executions);
            Assert.AreEqual(1, className2Executions);

            name1Binding.Unregister();
            className1Binding.Unregister();

            task.Name = "Bookwork updated";
            task.Class = class2;

            Assert.AreEqual(1, name1Executions);
            Assert.AreEqual(2, name2Executions);
            Assert.AreEqual(1, className1Executions);
            Assert.AreEqual(2, className2Executions);
        }

        [TestMethod]
        public void TestEmptyRegistration()
        {
            var class1 = new MyClass()
            {
                Name = "Math"
            };

            var task = new MyTask()
            {
                Name = "Bookwork",
                Class = class1
            };

            var bindingHost = new BindingHost()
            {
                DataContext = task
            };

            int name1Executions = 0;
            int className1Executions = 0;


            var name1Binding = bindingHost.SetBinding<string>("Name", name =>
            {
                Assert.AreEqual(task.Name, name);
                name1Executions++;
            });

            var className1Binding = bindingHost.SetBinding<string>("Class.Name", name =>
            {
                Assert.AreEqual(task.Class.Name, name);
                className1Executions++;
            });

            var emptyNameRegistration = bindingHost.GetEmptyRegistration("Name");
            var emptyClassNameRegistration = bindingHost.GetEmptyRegistration("Class.Name");

            Assert.AreEqual(1, name1Executions);
            Assert.AreEqual(1, className1Executions);

            emptyNameRegistration.SetSourceValue("Bookwork updated");

            Assert.AreEqual("Bookwork updated", task.Name);
            Assert.AreEqual(2, name1Executions);
            Assert.AreEqual(1, className1Executions);

            emptyClassNameRegistration.SetSourceValue("Math updated");

            Assert.AreEqual("Math updated", task.Class.Name);
            Assert.AreEqual(2, name1Executions);
            Assert.AreEqual(2, className1Executions);
        }

        [TestMethod]
        public void TestSwappingTask()
        {
            var c = new MyClass()
            {
                Name = "Math",
                Color = new byte[] { 93, 25, 13 }
            };

            var originalTask = new MyTask()
            {
                Name = "Original",
                Class = c
            };

            var replacedTask = new MyTask()
            {
                Name = "Replaced",
                Class = c
            };

            var bindingHost = new BindingHost();

            int nameExecutions = 0;
            int colorExecutions = 0;

            bindingHost.SetBinding<string>("Name", name =>
            {
                if (nameExecutions == 0)
                {
                    Assert.AreEqual("Original", name);
                }
                else
                {
                    Assert.AreEqual("Replaced", name);
                }

                nameExecutions++;
            });

            bindingHost.SetBinding<byte[]>("Class.Color", color =>
            {
                Assert.AreEqual(c.Color, color);
                colorExecutions++;
            });

            Assert.AreEqual(0, nameExecutions);
            Assert.AreEqual(0, colorExecutions);

            bindingHost.DataContext = originalTask;

            Assert.AreEqual(1, nameExecutions);
            Assert.AreEqual(1, colorExecutions);

            bindingHost.DataContext = replacedTask;

            // Technically the class didn't change, so class bindings shouldn't re-trigger
            Assert.AreEqual(2, nameExecutions);
            Assert.AreEqual(1, colorExecutions);
        }

        [TestMethod]
        public void TestSwappingTaskWithDetach()
        {
            var c = new MyClass()
            {
                Name = "Math",
                Color = new byte[] { 93, 25, 13 }
            };

            var originalTask = new MyTask()
            {
                Name = "Original",
                Class = c
            };

            var replacedTask = new MyTask()
            {
                Name = "Replaced",
                Class = c
            };

            var bindingHost = new BindingHost();

            int nameExecutions = 0;
            int colorExecutions = 0;

            bindingHost.SetBinding<string>("Name", name =>
            {
                if (nameExecutions == 0)
                {
                    Assert.AreEqual("Original", name);
                }
                else
                {
                    Assert.AreEqual("Replaced", name);
                }

                nameExecutions++;
            });

            bindingHost.SetBinding<byte[]>("Class.Color", color =>
            {
                Assert.AreEqual(c.Color, color);
                colorExecutions++;
            });

            Assert.AreEqual(0, nameExecutions);
            Assert.AreEqual(0, colorExecutions);

            bindingHost.DataContext = originalTask;

            Assert.AreEqual(1, nameExecutions);
            Assert.AreEqual(1, colorExecutions);

            // Editing class color should trigger
            c.Color = new byte[] { 15, 30, 25 };
            Assert.AreEqual(1, nameExecutions);
            Assert.AreEqual(2, colorExecutions);

            // Detach (for example, view was removed/recycled)
            bindingHost.Detach();

            originalTask.Name = "Edited";
            c.Color = new byte[] { 90, 30, 25 };

            // Shouldn't have triggered anything
            Assert.AreEqual(1, nameExecutions);
            Assert.AreEqual(2, colorExecutions);

            // Now view added with the new task
            bindingHost.DataContext = replacedTask;

            // Even though class itself didn't change, properties of the class might have changed
            // while we were detached (as they did)... thus we need to retrigger those too.
            Assert.AreEqual(2, nameExecutions);
            Assert.AreEqual(3, colorExecutions);
        }

        [TestMethod]
        public void TestSwappingTaskAndClass()
        {
            var c1 = new MyClass()
            {
                Name = "Math",
                Color = new byte[] { 93, 25, 13 }
            };

            var c2 = new MyClass()
            {
                Name = "Spanish",
                Color = new byte[] { 80, 40, 20 }
            };

            var originalTask = new MyTask()
            {
                Name = "Original",
                Class = c1
            };

            var replacedTask = new MyTask()
            {
                Name = "Replaced",
                Class = c2
            };

            var bindingHost = new BindingHost();

            int nameExecutions = 0;
            int colorExecutions = 0;

            bindingHost.SetBinding<string>("Name", name =>
            {
                if (nameExecutions == 0)
                {
                    Assert.AreEqual("Original", name);
                }
                else
                {
                    Assert.AreEqual("Replaced", name);
                }

                nameExecutions++;
            });

            bindingHost.SetBinding<byte[]>("Class.Color", color =>
            {
                if (colorExecutions == 0)
                {
                    Assert.IsTrue(new byte[] { 93, 25, 13 }.SequenceEqual(color));
                }
                else
                {
                    Assert.IsTrue(new byte[] { 80, 40, 20 }.SequenceEqual(color));
                }

                colorExecutions++;
            });

            Assert.AreEqual(0, nameExecutions);
            Assert.AreEqual(0, colorExecutions);

            bindingHost.DataContext = originalTask;

            Assert.AreEqual(1, nameExecutions);
            Assert.AreEqual(1, colorExecutions);

            bindingHost.DataContext = replacedTask;

            Assert.AreEqual(2, nameExecutions);
            Assert.AreEqual(2, colorExecutions);
        }
    }
}
