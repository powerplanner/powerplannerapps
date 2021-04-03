using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Vx.Views;
using Xamarin.Forms;

namespace Vx.Test
{
    [TestClass]
    public class VxComponentTests
    {
        public class BasicComponent : VxComponent
        {
            protected override VxView Render()
            {
                return new VxLabel()
                    .Text("Hello world");

                return new VxView<Label>()
                    .Text("Hello world");
            }
        }

        [TestMethod]
        public void TestBasicComponent()
        {
            new Button()
            {
                Text = "Cool",
                Command = new MyCommand()
            };
        }

        private class MyCommand : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                throw new NotImplementedException();
            }

            public void Execute(object parameter)
            {
                throw new NotImplementedException();
            }
        }
    }
}
