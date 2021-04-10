using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerApp.Pages
{
    public abstract class MainScreenSubpage : VxPage
    {
        private MainScreenPage _mainScreenPage;
        protected MainScreenPage MainScreenPage
        {
            get
            {
                if (_mainScreenPage == null)
                {
                    _mainScreenPage = FindAncestor<MainScreenPage>();
                }

                return _mainScreenPage;
            }
        }
    }
}
