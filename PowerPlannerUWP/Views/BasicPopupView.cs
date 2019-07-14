using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using ToolsUniversal.Pages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace PowerPlannerUWP.Views
{
    public class BasicPopupView : ContentControl
    {
        public event EventHandler OnRequestClose;

        public BasicPopupView()
        {
            this.DefaultStyleKey = typeof(BasicPopupView);
            CloseCommand = new RelayCommand(Close);
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(object), typeof(BasicPopupView), null);

        public object Title
        {
            get { return GetValue(TitleProperty) as string; }
            set { SetValue(TitleProperty, value); }
        }

        public void Close()
        {
            if (OnRequestClose != null)
                OnRequestClose(this, null);
        }

        public static readonly DependencyProperty CloseCommandProperty = DependencyProperty.Register("CloseCommand", typeof(ICommand), typeof(BasicPopupView), null);

        public ICommand CloseCommand
        {
            get { return GetValue(CloseCommandProperty) as ICommand; }
            set { SetValue(CloseCommandProperty, value); }
        }
    }
}
