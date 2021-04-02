using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.CommunityToolkit.Markup;

namespace PowerPlannerAppDataLibrary.Views.Settings
{
    public abstract class BaseSettingPageView<T> : ScrollView
        where T : BaseViewModel
    {
        public T ViewModel
        {
            get => BindingContext as T;
            set
            {
                BindingContext = value;
                if (value != null)
                {
                    CreateIfNeeded();
                }
            }
        }

        public BaseSettingPageView()
        {
            Padding = new Thickness(12);
        }

        private bool _created;
        private void CreateIfNeeded()
        {
            if (!_created)
            {
                base.Content = new StackLayout
                {
                    Children =
                    {
                        new Label
                        {
                            Text = Title
                        },

                        Content
                    }
                };
            }
        }

        protected abstract string Title { get; }

        protected new abstract View Content { get; }
    }
}
