using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.ViewItems;
using InterfacesiOS.Binding;

namespace PowerPlanneriOS.Views
{
    public class UIWeightHeaderCell : UITableViewHeaderFooterView
    {
        private UILabel _labelName;
        private BindingHost _bindingHost = new BindingHost();

        public UIWeightHeaderCell(NSString reuseIdentifier) : base(reuseIdentifier)
        {
            _labelName = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredSubheadline.Bold()
            };
            _bindingHost.SetLabelTextBinding(_labelName, nameof(ViewItemWeightCategory.Name));
            ContentView.Add(_labelName);
            _labelName.StretchWidthAndHeight(ContentView, left: 15, top: 8, right: 0, bottom: 8);
        }

        public ViewItemWeightCategory WeightCategory
        {
            get { return _bindingHost.DataContext as ViewItemWeightCategory; }
            set { _bindingHost.DataContext = value; }
        }
    }
}