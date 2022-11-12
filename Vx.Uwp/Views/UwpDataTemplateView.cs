using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace Vx.Uwp.Views
{
    public class UwpDataTemplateView : ContentControl
    {
        public UwpDataTemplateView()
        {
            HorizontalContentAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
        }

        public static DataTemplate GetDataTemplate(string elementWithTemplateName)
        {
            var dataTemplateString =
@"<DataTemplate
    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
    xmlns:local=""using:Vx.Uwp.Views"">
    <local:UwpDataTemplateView Data=""{Binding}"" ItemTemplate=""{Binding DataContext, ElementName=" + elementWithTemplateName + @"}""/>
</DataTemplate>";

            return XamlReader.Load(dataTemplateString) as DataTemplate;
        }

        private DataTemplateHelper.VxDataTemplateComponent _component = new DataTemplateHelper.VxDataTemplateComponent();

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(UwpDataTemplateView), new PropertyMetadata(null, OnDataChanged));

        private static void OnDataChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as UwpDataTemplateView).OnDataChanged();
        }

        private void OnDataChanged()
        {
            _component.Data = Data;
            RenderIfNeeded();
        }

        public Func<object, Vx.Views.View> ItemTemplate
        {
            get { return (Func<object, Vx.Views.View>)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(Func<object, Vx.Views.View>), typeof(UwpDataTemplateView), new PropertyMetadata(null, OnItemTemplateChanged));


        private static void OnItemTemplateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as UwpDataTemplateView).OnItemTemplateChanged();
        }

        private void OnItemTemplateChanged()
        {
            _component.Template = ItemTemplate;
            RenderIfNeeded();
        }

        private void RenderIfNeeded()
        {
            if (Content == null && ItemTemplate != null && Data != null)
            {
                Content = _component.Render();
            }
        }
    }
}
