using System;
using InterfacesiOS.Helpers;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSTimePicker : iOSView<Vx.Views.TimePicker, UIView>
    {
        private UILabel _text;

        public iOSTimePicker()
        {
            UITapGestureRecognizer tapRecognizer = new UITapGestureRecognizer();

            tapRecognizer.AddTarget(() => ShowTimePicker());

            View.AddGestureRecognizer(tapRecognizer);

            _text = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.Add(_text);
            _text.StretchWidthAndHeight(View, 6, 6, 6, 6);

            View.BackgroundColor = UIColorCompat.SystemGroupedBackgroundColor;
            View.Layer.CornerRadius = 10;
            View.ClipsToBounds = true;
        }

        private void ShowTimePicker()
        {
            // TODO
        }

        protected override void ApplyProperties(TimePicker oldView, TimePicker newView)
        {
            base.ApplyProperties(oldView, newView);

            _text.Text = DateTime.Today.Add(newView.Value).ToString("t");
        }
    }
}
