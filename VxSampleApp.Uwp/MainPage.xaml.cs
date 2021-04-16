using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vx.Uwp;
using Vx.Views;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VxSampleApp.Uwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            ComboBoxComponents.ItemsSource = new Choice[]
            {
                new Choice<VxSampleComponent>(),
                new Choice<VxAddingComponent>(),
                new Choice<VxCombinedComponent>(),
                new Choice<VxGradeScalesComponent>(),
                new Choice<VxGradeOptionsComponent>()
            };

            ComboBoxComponents.SelectionChanged += ComboBoxComponents_SelectionChanged;
        }

        private void ComboBoxComponents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Container.Children.Clear();
            var comp = Activator.CreateInstance((ComboBoxComponents.SelectedItem as Choice).Type) as VxComponent;
            Container.Children.Add(comp.Render());
        }

        private abstract class Choice
        {
            public abstract Type Type { get; }
        }

        private class Choice<T> : Choice
        {
            public override Type Type => typeof(T);
            public string Name => Type.Name;
        }
    }
}
