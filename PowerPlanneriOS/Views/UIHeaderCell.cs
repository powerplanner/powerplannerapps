using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using InterfacesiOS.Views;
using UIKit;

namespace PowerPlanneriOS.Views
{
    public class UIHeaderCell : BareUITableViewCell
    {
        private UILabel _labelText;

        public UIHeaderCell(string cellId) : base(cellId)
        {
            // Don't allow clicking on this header cell
            UserInteractionEnabled = false;

            ContentView.BackgroundColor = new UIColor(0.95f, 1);

            _labelText = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredSubheadline,
                TextColor = UIColor.DarkGray
            };
            ContentView.AddSubview(_labelText);
            _labelText.StretchWidthAndHeight(ContentView, left: 16, top: 8, bottom: 8);

            ContentView.SetHeight(44);
        }

        public string Text
        {
            get => _labelText.Text;
            set => _labelText.Text = value;
        }
    }
}