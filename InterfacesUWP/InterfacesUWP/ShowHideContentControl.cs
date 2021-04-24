using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace InterfacesUWP
{
    public class ShowHideContentControl : ContentControl
    {
        public ShowHideContentControl()
        {
            
        }



        public bool IsShown
        {
            get { return (bool)GetValue(IsShownProperty); }
            set { SetValue(IsShownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsShown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsShownProperty =
            DependencyProperty.Register("IsShown", typeof(bool), typeof(ShowHideContentControl), new PropertyMetadata(false, OnIsShownChanged));

        private static void OnIsShownChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as ShowHideContentControl).OnIsShownChanged(e);
        }

        private void OnIsShownChanged(DependencyPropertyChangedEventArgs e)
        {
        }





        public Storyboard ShowAnimation
        {
            get { return (Storyboard)GetValue(ShowAnimationProperty); }
            set { SetValue(ShowAnimationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowAnimation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowAnimationProperty =
            DependencyProperty.Register("ShowAnimation", typeof(Storyboard), typeof(ShowHideContentControl), new PropertyMetadata(null));



        public Storyboard HideAnimation
        {
            get { return (Storyboard)GetValue(HideAnimationProperty); }
            set { SetValue(HideAnimationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HideAnimation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HideAnimationProperty =
            DependencyProperty.Register("HideAnimation", typeof(Storyboard), typeof(ShowHideContentControl), new PropertyMetadata(null));




    }
}
