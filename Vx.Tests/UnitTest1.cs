using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vx.Views;

namespace Vx.Tests
{
    [TestClass]
    public class UnitTest1
    {
        public class SampleClass
        {
            public bool IsNoClassClass { get; set; }
        }

        private SampleClass _class;

        [TestMethod]
        public void TestMethod1()
        {
            bool includeLoveIt = false;

            bool includeMore = false;

            // Note would have to define the builder methods as extension methods so if I extend classes, they return the correct type
            new VxStackPanel()
                .Children(
                    new VxTextBlock("Cool"),
                    
                    includeLoveIt ? new VxTextBlock("I love it") : null,

                    includeMore ? (VxView)new VxStackPanel()
                        .Children(
                            new VxTextBlock("Coolio"),
                            new VxTextBlock("Sweet")
                        )
                        : (VxView)new VxTextBlock("No more"),

                    !_class.IsNoClassClass ? new VxComboBox()
                        .Header("Weight category") : null,
                    
                    new VxButton("Login")
                        .Click(() => Login()));
        }

        private void Login()
        {

        }
    }
}
