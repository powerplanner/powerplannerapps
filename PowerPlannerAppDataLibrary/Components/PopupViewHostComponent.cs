using PowerPlannerAppDataLibrary.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Vx;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Components
{
    public class PopupViewHostComponent : VxComponent
    {
        private ObservableCollection<PopupCommand> _primaryCommands;
        public ObservableCollection<PopupCommand> PrimaryCommands
        {
            get => _primaryCommands;
            set
            {
                if (value == _primaryCommands)
                {
                    return;
                }

                UnsubscribeFromCollection(_primaryCommands);
                _primaryCommands = value;
                if (_primaryCommands != null)
                {
                    SubscribeToCollection(value);
                }
                MarkDirty();
            }
        }

        private ObservableCollection<PopupCommand> _secondaryCommands;
        public ObservableCollection<PopupCommand> SecondaryCommands
        {
            get => _secondaryCommands;
            set
            {
                if (value == _secondaryCommands)
                {
                    return;
                }

                UnsubscribeFromCollection(_secondaryCommands);
                _secondaryCommands = value;
                if (_secondaryCommands != null)
                {
                    SubscribeToCollection(value);
                }
                MarkDirty();
            }
        }

        public Action OnClose { get; set; }


        private VxState<string> _title = new VxState<string>("");
        public string Title
        {
            get => _title;
            set => _title.Value = value;
        }

        private VxState<object> _nativeContent = new VxState<object>(null);
        /// <summary>
        /// For UWP, this should be a UIElement. Note that iOS/Android don't implement native content containers yet.
        /// </summary>
        public object NativeContent
        {
            get => _nativeContent.Value;
            set => _nativeContent.Value = value;
        }

        protected override View Render()
        {
            var titlebar = new Toolbar
            {
                Title = Title,
                OnClose = OnClose
            };
            if (PrimaryCommands != null)
            {
                titlebar.PrimaryCommands.AddRange(HandleQuickConfirmDelete(PrimaryCommands));
            }
            if (SecondaryCommands != null)
            {
                titlebar.SecondaryCommands.AddRange(HandleQuickConfirmDelete(SecondaryCommands));
            }

            return new LinearLayout
            {
                BackgroundColor = Theme.Current.PopupPageBackgroundColor,
                Children =
                {
                    titlebar,
                    new NativeContentContainer(NativeContent).LinearLayoutWeight(1)
                }
            };
        }

        private IEnumerable<PopupCommand> HandleQuickConfirmDelete(IEnumerable<PopupCommand> commands)
        {
            foreach (var command in commands)
            {
                if (command.UseQuickConfirmDelete)
                {
                    yield return new PopupCommand
                    {
                        SubItems =
                        {
                            new MenuItem
                            {
                                Text = PowerPlannerResources.GetCapitalizedString("String_YesDelete"),
                                Click = command.Click
                            }
                        },
                        Text = command.Text,
                        Glyph = command.Glyph,
                        Style = command.Style
                    };
                }
                else
                {
                    yield return command;
                }
            }
        }
    }
}
