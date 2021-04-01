using System;
using BareMvvm.Core;
using CoreGraphics;
using ToolsPortable;
using UIKit;

namespace InterfacesiOS.Views
{
    public class BareUITextField : UITextField
    {
        public BareUITextField()
        {
            // Listen to text change and update TextField
            // Note that need to listen to EditingDidEnd so that autosuggest corrections are consumed: https://github.com/MvvmCross/MvvmCross/pull/2682
            this.AddTarget(UpdateTextField, UIControlEvent.EditingChanged);
            this.AddTarget(UpdateTextField, UIControlEvent.EditingDidEnd);

            this.AddTarget(EndedEditing, UIControlEvent.EditingDidEnd); // This is fired when clicking a different view
            this.AddTarget(EndedEditing, UIControlEvent.EditingDidEndOnExit); // This is fired when clicking "Done" on keyboard
            this.AddTarget(StartedEditing, UIControlEvent.EditingDidBegin);
        }

        private void StartedEditing(object sender, EventArgs e)
        {
            if (TextField != null)
            {
                try
                {
                    TextField.HasFocus = true;
                }
                catch { }
            }
        }

        private void EndedEditing(object sender, EventArgs e)
        {
            if (TextField != null)
            {
                try
                {
                    TextField.HasFocus = false;
                }
                catch { }
            }
        }

        private void UpdateTextField(object sender, EventArgs e)
        {
            if (TextField != null)
            {
                try
                {
                    TextField.Text = Text;
                }
                catch { }
            }
        }

        private UIButton _errorIcon;

        private void CreateErrorIcon()
        {
            var errorButton = new UIButton(UIButtonType.Custom)
            {
                Frame = new CGRect(0, 0, Frame.Size.Height, Frame.Size.Height),
                TintColor = UIColor.Red
            };

            errorButton.SetImage(UIImage.FromBundle("baseline_error_black_18pt").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            errorButton.TouchUpInside += ErrorButton_TouchUpInside;

            _errorIcon = errorButton;
        }

        private void ErrorButton_TouchUpInside(object sender, EventArgs e)
        {
            ShowErrorPopup();
        }

        private bool _listeningToTextField;
        private TextField _textField;
        public TextField TextField
        {
            get => _textField;
            set
            {
                if (_textField != value)
                {
                    StopListeningToTextField();

                    _textField = value;

                    StartListeningToTextField();
                    UpdateFromTextField();
                }
            }
        }

        private void StartListeningToTextField()
        {
            if (TextField != null)
            {
                if (!_listeningToTextField)
                {
                    TextField.PropertyChanged += SourceTextField_PropertyChanged;
                    _listeningToTextField = true;
                }
            }
            else
            {
                _listeningToTextField = false;
            }
        }

        private void StopListeningToTextField()
        {
            if (TextField != null)
            {
                if (_listeningToTextField)
                {
                    TextField.PropertyChanged -= SourceTextField_PropertyChanged;
                    _listeningToTextField = false;
                }
            }
            else
            {
                _listeningToTextField = false;
            }
        }

        public override void WillMoveToWindow(UIWindow window)
        {
            if (window == null)
            {
                StopListeningToTextField();
            }
            else
            {
                StartListeningToTextField();
            }

            base.WillMoveToWindow(window);
        }

        private void SourceTextField_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateFromTextField();
        }

        private void UpdateFromTextField()
        {
            if (TextField != null)
            {
                Text = TextField.Text;
                Error = TextField.ValidationState?.ErrorMessage;
            }
        }

        private string _error;
        public string Error
        {
            get => _error;
            set
            {
                // Using excellent code from https://stackoverflow.com/a/56742493/1454643
                if (_error != value)
                {
                    _error = value;

                    if (value != null)
                    {
                        if (_errorIcon == null)
                        {
                            CreateErrorIcon();
                        }

                        this.RightView = _errorIcon;
                        this.RightViewMode = UITextFieldViewMode.Always;
                    }
                    else
                    {
                        if (_errorIcon != null)
                        {
                            this.RightView = null;
                        }
                    }
                }
            }
        }

        private void ShowErrorPopup()
        {
            if (Error != null)
            {
                new PortableMessageDialog(Error).Show();
            }
        }
    }
}