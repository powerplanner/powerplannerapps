using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class BaseSettingsSplitViewModel : BaseSettingsViewModelWithAccount
    {
        public BaseSettingsSplitViewModel(BaseViewModel parent) : base(parent)
        {
        }

        private BaseViewModel _content;
        public BaseViewModel Content
        {
            get { return _content; }
            set
            {
                SetProperty(ref _content, value, nameof(Content));

                if (value == null && State == ViewState.Content)
                {
                    State = ViewState.Items;
                    ThisBackButtonVisibility = RequestedBackButtonVisibility.Inherit;
                }
                else if (value != null && State == ViewState.Items)
                {
                    State = ViewState.Content;
                    ThisBackButtonVisibility = RequestedBackButtonVisibility.Visible;
                }
            }
        }

        private BaseViewModel[] _items;
        public BaseViewModel[] Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value, nameof(Items)); }
        }

        public enum ViewState
        {
            Items,
            Content,
            Both
        }

        private ViewState _state = ViewState.Items;
        public ViewState State
        {
            get { return _state; }
            private set { SetProperty(ref _state, value, nameof(State)); }
        }

        public void SwitchToSingle()
        {
            if (State == ViewState.Both)
            {
                if (Content != null)
                {
                    State = ViewState.Content;
                    ThisBackButtonVisibility = RequestedBackButtonVisibility.Visible;
                }
                else
                {
                    State = ViewState.Items;
                    ThisBackButtonVisibility = RequestedBackButtonVisibility.Inherit;
                }
            }
        }

        public void SwitchToSplit()
        {
            if (State != ViewState.Both)
            {
                State = ViewState.Both;
                if (Content == null)
                {
                    Content = Items.First();
                }
                ThisBackButtonVisibility = RequestedBackButtonVisibility.Inherit;
            }
        }

        protected override BaseViewModel GetChildContent()
        {
            return Content;
        }

        public override bool GoBack()
        {
            if (State == ViewState.Content)
            {
                Content = null;
                return true;
            }

            return base.GoBack();
        }
    }
}
