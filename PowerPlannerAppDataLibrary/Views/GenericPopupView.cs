using BareMvvm.Forms.Extensions;
using BareMvvm.Forms.Views;
using PowerPlannerAppDataLibrary.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolsPortable;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;

namespace PowerPlannerAppDataLibrary.Views
{
    public class GenericPopupView : ViewModelView
    {
        private const double TOP_BAR_HEIGHT = 48;

        private Grid _content;

        //private Border _mainContentContainer;

        //private Border _secondaryOptionsButtonContainer;
        //protected View SecondaryOptionsButtonContainer => _secondaryOptionsButtonContainer;
        //private Border _buttonCloseContainer;

        //private Grid _topTitleBar;
        //private StackLayout _topPrimaryCommands;

        //private Border _fullScreenTitle;

        //private CommandBar _commandBar;

        private PopupTitleBar _titleBar = new PopupTitleBar();

        public GenericPopupView()
        {
            Content = new Grid()
            {
                RowDefinitions =
                {
                    new RowDefinition() { Height = GridLength.Auto },
                    new RowDefinition() { Height = GridLength.Star }
                },

                Children =
                {
                    new PopupTitleBar() { IsFullScreenStyle = false }
                        .Bind(PopupTitleBar.TitleProperty, nameof(Title), source: this)
                        .Bind(PopupTitleBar.PrimaryCommandsProperty)
                }
            };
        }

        public string Title
        {
            get => GetValue(TitleProperty) as string;
            set => SetValue(TitleProperty, value);
        }

        public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(GenericPopupView));

        public List<PopupCommandBarItem> PrimaryCommands
        {
            get => GetValue(PrimaryCommandsProperty) as List<PopupCommandBarItem>;
            set => SetValue(PrimaryCommandsProperty, value);
        }

        public static readonly BindableProperty PrimaryCommandsProperty = BindableProperty.Create(nameof(PrimaryCommands), typeof(List<PopupCommandBarItem>), typeof(GenericPopupView));
    }

    
}
