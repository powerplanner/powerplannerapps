using System;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;

namespace PowerPlannerAppDataLibrary.ViewModels
{
    public class PopupComponentViewModel : ComponentViewModel
    {
        /// <summary>
        /// Optional, if null, typical back behavior will be used. Can use this to specify "Cancel" or other options.
        /// </summary>
        public Tuple<string, Action> BackOverride { get; protected set; }

        public PopupCommand PrimaryCommand { get; protected set; }

        public PopupComponentViewModel(BaseViewModel parent) : base(parent)
        {
        }

        protected void UseCancelForBack()
        {
            BackOverride = new Tuple<string, Action>("Cancel", null);
        }
    }

    public class PopupCommand
    {
        public string Text { get; set; }
        public Action Action { get; set; }
        public string Glyph { get; set; }

        public static PopupCommand Save(Action action)
        {
            return new PopupCommand
            {
                Text = PowerPlannerResources.GetStringSave(),
                Glyph = MaterialDesign.MaterialDesignIcons.Check,
                Action = action
            };
        }
    }
}
