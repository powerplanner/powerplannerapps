using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BareMvvm.Core.Snackbar;
using CoreGraphics;
using Foundation;
using ToolsPortable;
using UIKit;

namespace InterfacesiOS.Views
{
    public class BareSnackbarPresenter : UIView
    {
        public static UIColor ButtonTextColor { get; set; } = UIColor.SystemBlueColor;
        public static nfloat BottomOffset { get; set; } = 0;

        public BareSnackbarManager SnackbarManager { get; private set; }

        public BareSnackbarPresenter(BareSnackbarManager manager)
        {
            SnackbarManager = manager;

            manager.PropertyChanged += Manager_PropertyChanged;
        }

        public override UIView HitTest(CGPoint point, UIEvent uievent)
        {
            if (_currentSnackbar != null)
            {
                var hitSnackbar = _currentSnackbar.HitTest(point, uievent);
                if (hitSnackbar != null)
                {
                    return hitSnackbar;
                }
            }

            var hitView = base.HitTest(point, uievent);

            if (hitView == this)
            {
                return null;
            }

            return hitView;
        }

        private void Manager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SnackbarManager.CurrentSnackbar):
                    RemoveSnackbar();

                    if (SnackbarManager.CurrentSnackbar != null)
                    {
                        ShowSnackbar(SnackbarManager.CurrentSnackbar);
                    }

                    break;
            }
        }

        private UIView _currentSnackbar;

        private void ShowSnackbar(BareSnackbar snackbar)
        {
            UIView snackbarContainer = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.Black,
                Alpha = 0
            };
            snackbarContainer.Layer.CornerRadius = 5;
            this.AddSubview(snackbarContainer);
            snackbarContainer.StretchWidth(this, left: 12, right: 12);
            snackbarContainer.PinToBottom(this, bottom: 12 + (int)BottomOffset);

            UILabel label = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = snackbar.Message,
                Font = UIFont.PreferredCaption1,
                TextColor = UIColor.White
            };
            snackbarContainer.AddSubview(label);
            label.StretchHeight(snackbarContainer, top: 12, bottom: 12);

            if (snackbar.ButtonText == null)
            {
                label.StretchWidth(snackbarContainer, left: 12, right: 12);
            }
            else
            {
                UIButton button = new UIButton()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.PreferredCaption1.Bold()
                };
                button.SetTitle(snackbar.ButtonText, UIControlState.Normal);
                button.SetTitleColor(ButtonTextColor, UIControlState.Normal);
                snackbarContainer.AddSubview(button);
                button.StretchHeight(snackbarContainer, top: 12, bottom: 12);
                button.TouchUpInside += delegate
                {
                    try
                    {
                        SnackbarManager.Close(snackbar); // Make sure to remove it
                        snackbar.ButtonCallback();
                    }
                    catch { }
                };

                snackbarContainer.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-12-[message]->=12-[button]-12-|", NSLayoutFormatOptions.SpacingEdgeToEdge, null, new NSDictionary(
                    "message", label,
                    "button", button)));
            }

            _currentSnackbar = snackbarContainer;

            UIView.Animate(duration: 0.5, delegate
            {
                snackbarContainer.Alpha = 1;
            });
        }

        private void RemoveSnackbar()
        {
            if (_currentSnackbar != null)
            {
                var snackbarToFadeOut = _currentSnackbar;
                _currentSnackbar = null;

                UIView.Animate(duration: 0.5, delegate
                {
                    snackbarToFadeOut.Alpha = 0;
                }, delegate
                {
                    snackbarToFadeOut.RemoveFromSuperview();
                });
            }
        }
    }
}