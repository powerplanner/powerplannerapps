using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace InterfacesUWP.ArrowButtonFolder
{
    public class ArrowButtonBase : MyControl
    {
        public event EventHandler Click;

        public ArrowButtonBase()
        {
            base.DefaultStyleKey = typeof(ArrowButtonBase);

            
            base.Tapped += ArrowButtonBase_Tapped;
        }

        protected override void OnApplyTemplate()
        {
            this.Background = BackgroundNormal;
            this.Foreground = ForegroundNormal;
        }



        public SolidColorBrush BackgroundNormal
        {
            get { return (SolidColorBrush)GetValue(BackgroundNormalProperty); }
            set { SetValue(BackgroundNormalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundNormal.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundNormalProperty =
            DependencyProperty.Register("BackgroundNormal", typeof(SolidColorBrush), typeof(ArrowButtonBase), new PropertyMetadata(null));



        public SolidColorBrush BackgroundHover
        {
            get { return (SolidColorBrush)GetValue(BackgroundHoverProperty); }
            set { SetValue(BackgroundHoverProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundHover.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundHoverProperty =
            DependencyProperty.Register("BackgroundHover", typeof(SolidColorBrush), typeof(ArrowButtonBase), new PropertyMetadata(null));




        public SolidColorBrush BackgroundMouseDown
        {
            get { return (SolidColorBrush)GetValue(BackgroundMouseDownProperty); }
            set { SetValue(BackgroundMouseDownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundMouseDown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundMouseDownProperty =
            DependencyProperty.Register("BackgroundMouseDown", typeof(SolidColorBrush), typeof(ArrowButtonBase), new PropertyMetadata(null));




        public SolidColorBrush ForegroundNormal
        {
            get { return (SolidColorBrush)GetValue(ForegroundNormalProperty); }
            set { SetValue(ForegroundNormalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForegroundNormal.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForegroundNormalProperty =
            DependencyProperty.Register("ForegroundNormal", typeof(SolidColorBrush), typeof(ArrowButtonBase), new PropertyMetadata(null));



        public SolidColorBrush ForegroundMouseDown
        {
            get { return (SolidColorBrush)GetValue(ForegroundMouseDownProperty); }
            set { SetValue(ForegroundMouseDownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForegroundMouseDown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForegroundMouseDownProperty =
            DependencyProperty.Register("ForegroundMouseDown", typeof(SolidColorBrush), typeof(ArrowButtonBase), new PropertyMetadata(null));




        public string Character
        {
            get { return (string)GetValue(CharacterProperty); }
            set { SetValue(CharacterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Character.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CharacterProperty =
            DependencyProperty.Register("Character", typeof(string), typeof(ArrowButtonBase), new PropertyMetadata(null));





        void ArrowButtonBase_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Click != null)
                Click(this, new EventArgs());
        }

        protected override void OnMouseOverChanged(PointerRoutedEventArgs e)
        {
            if (IsMouseOver)
                base.Background = BackgroundHover;
            else
                base.Background = BackgroundNormal;
        }

        protected override void OnMouseDownChanged(PointerRoutedEventArgs e)
        {
            if (IsMouseDown)
            {
                this.Background = BackgroundMouseDown;
                this.Foreground = ForegroundMouseDown;
            }

            else
            {
                this.Background = BackgroundNormal;
                this.Foreground = ForegroundNormal;
            }
        }
    }
}
