using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using BareMvvm.Core.ViewModels;
using InterfacesiOS.App;
using System.ComponentModel;
using ToolsPortable;
using InterfacesiOS.Binding;
using InterfacesiOS.Views;
using InterfacesiOS.Helpers;

namespace InterfacesiOS.Controllers
{
#if DEBUG
    internal static class ActiveViewControllers
    {
        private static readonly WeakReferenceList<UIViewController> ACTIVE_CONTROLLERS = new WeakReferenceList<UIViewController>();

        static ActiveViewControllers()
        {
            OutputActiveControllers();
        }

        public static void Track(UIViewController controller)
        {
            ACTIVE_CONTROLLERS.Add(controller);
        }

        private static string _prevResult;
        private static async void OutputActiveControllers()
        {
            while (true)
            {
                await System.Threading.Tasks.Task.Delay(1500);

                string result = "";
                int count = 0;
                foreach (var c in ACTIVE_CONTROLLERS)
                {
                    result += c.GetType().Name + "\n";
                    count++;
                }
                result = $"ACTIVE CONTROLLERS ({count})\n" + result;
                if (_prevResult != result)
                {
                    _prevResult = result;
                    System.Diagnostics.Debug.WriteLine(result);
                }
            }
        }
    }
#endif

    public abstract class BareMvvmUIViewController<T> : UIViewController where T : BaseViewModel
    {
        public BareMvvmUIViewController()
        {
            base.View.BackgroundColor = UIColorCompat.SystemBackgroundColor;

            _shouldReturnCondition = new UITextFieldCondition(ShouldReturn);

#if DEBUG
            ActiveViewControllers.Track(this);
#endif
        }

        public BindingHost BindingHost { get; private set; } = new BindingHost();
        private T _viewModel;
        public T ViewModel
        {
            get { return _viewModel; }
            set
            {
                if (_viewModel == value)
                {
                    return;
                }

                _viewModel = value;
                BindingHost.DataContext = value;
                OnViewModelSetOverride();
                TriggerViewModelLoaded(value);
            }
        }

        public virtual void OnViewModelSetOverride()
        {
            // Nothing
        }

        public virtual void OnViewModelLoadedOverride()
        {
            // Nothing
        }

        private bool _hasCalledViewModelAndViewLoaded;
        public virtual void OnViewModelAndViewLoadedOverride()
        {
            // Nothing
        }

        public override void ViewDidLoad()
        {
            if (IsViewModelLoaded && !_hasCalledViewModelAndViewLoaded)
            {
                _hasCalledViewModelAndViewLoaded = true;
                OnViewModelAndViewLoadedOverride();
            }

            base.ViewDidLoad();
        }

        public bool IsViewModelLoaded { get; private set; }

        private async void TriggerViewModelLoaded(BaseViewModel viewModel)
        {
            try
            {
                await viewModel.LoadAsync();
            }
            catch (Exception ex)
            {
                ExceptionHelper.OnHandledExceptionOccurred?.Invoke(ex);
                return;
            }

            IsViewModelLoaded = true;

            OnViewModelLoadedOverride();

            if (IsViewLoaded && !_hasCalledViewModelAndViewLoaded)
            {
                _hasCalledViewModelAndViewLoaded = true;
                OnViewModelAndViewLoadedOverride();
            }
        }

        protected virtual void OnViewReturnedTo()
        {
            // Nothing
        }

        private UIScrollView _scrollViewForKeyboardOffset;
        private NSObject _keyboardObserverWillShow;
        private NSObject _keyboardObserverWillHide;
        private nfloat _keyboardTopOffset = 0;

        protected void EnableKeyboardScrollOffsetHandling(UIScrollView scrollView, nfloat topOffset)
        {
            _scrollViewForKeyboardOffset = scrollView;
            _keyboardTopOffset = topOffset;

            _keyboardObserverWillShow = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, KeyboardWillShow);
            _keyboardObserverWillHide = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, KeyboardWillHide);
        }

        protected virtual void KeyboardWillShow(NSNotification notification)
        {
            var keyboardFrame = UIKeyboard.FrameEndFromNotification(notification);
            OnKeyboardChanged(keyboardFrame.Height);
        }

        protected virtual void KeyboardWillHide(NSNotification notification)
        {
            OnKeyboardChanged(0);
        }

        private void OnKeyboardChanged(nfloat height)
        {
            _scrollViewForKeyboardOffset.ContentInset = new UIEdgeInsets(
                _keyboardTopOffset, 0, height, 0);
        }

        private UITapGestureRecognizer _tapGestureRecognizer;
        private List<UITextField> _textFieldsWithShouldReturnCondition = new List<UITextField>();
        private UITextFieldCondition _shouldReturnCondition;
        private bool _appearedAtLeastOnce = false;
        public override void ViewDidAppear(bool animated)
        {
            // Dismiss keyboard upon selecting anything else
            if (_tapGestureRecognizer == null)
            {
                _tapGestureRecognizer = new UITapGestureRecognizer(new Action(delegate
                {
                    this.View.EndEditing(true);
                }))
                {
                    CancelsTouchesInView = false
                };
            }
            this.View.AddGestureRecognizer(_tapGestureRecognizer);

            foreach (var tf in _textFieldsWithShouldReturnCondition)
            {
                tf.ShouldReturn = _shouldReturnCondition;
            }

            if (_scrollViewForKeyboardOffset != null && _keyboardObserverWillHide == null)
            {
                _keyboardObserverWillShow = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, KeyboardWillShow);
                _keyboardObserverWillHide = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, KeyboardWillHide);
            }

            base.ViewDidAppear(animated);

            if (_appearedAtLeastOnce)
            {
                OnViewReturnedTo();
            }
            else
            {
                _appearedAtLeastOnce = true;
            }
        }

        public override void ViewDidDisappear(bool animated)
        {
            if (_tapGestureRecognizer != null)
            {
                this.View.RemoveGestureRecognizer(_tapGestureRecognizer);
            }

            foreach (var tf in _textFieldsWithShouldReturnCondition)
            {
                tf.ShouldReturn = null;
            }

            if (_keyboardObserverWillHide != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_keyboardObserverWillHide);
                _keyboardObserverWillHide = null;
            }
            if (_keyboardObserverWillShow != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_keyboardObserverWillShow);
                _keyboardObserverWillHide = null;
            }

            base.ViewDidDisappear(animated);
        }

        private bool ShouldReturn(UITextField textField)
        {
            if (textField.ReturnKeyType != UIReturnKeyType.Next)
            {
                if (!HandleKeyboardAction(textField.ReturnKeyType))
                {
                    textField.ResignFirstResponder();
                }
                return true;
            }

            var descendants = View.Descendants().OfType<UITextField>().ToArray();
            int index = descendants.FindIndex(i => i == textField);
            if (index == descendants.Length - 1)
            {
                // If there's no more text fields, close
                textField.ResignFirstResponder();
            }
            else
            {
                // Otherwise continue to next
                descendants[index + 1].BecomeFirstResponder();
            }
            return true;
        }

        protected virtual bool HandleKeyboardAction(UIReturnKeyType returnKeyType)
        {
            return false;
        }

        /// <summary>
        /// Adds a text field
        /// </summary>
        /// <param name="textField"></param>
        /// <param name="textBindingPropertyName"></param>
        /// <param name="firstResponder">Whether this text box should get the first focus on the page, causing the keyboard to appear</param>
        protected void AddTextField(UIStackView stackView, UITextField textField, string textBindingPropertyName = null, bool firstResponder = false)
        {
            textField.TranslatesAutoresizingMaskIntoConstraints = false;
            textField.AdjustsFontSizeToFitWidth = true;

            if (textBindingPropertyName != null)
            {
                BindingHost.SetTextFieldTextBinding(textField, textBindingPropertyName);
            }

            stackView.AddArrangedSubview(textField);
            textField.StretchWidth(stackView, left: 16, right: 16);
            textField.SetHeight(44);

            RegisterTextField(textField, firstResponder);
        }

        protected void RegisterTextField(UITextField textField, bool firstResponder = false)
        {
            if (firstResponder)
            {
                textField.BecomeFirstResponder();
                // The following would dismiss when scrolling
                //textField.EditingDidBegin += delegate
                //{
                //    _scrollView.Scrolled += delegate
                //    {
                //        textField.ResignFirstResponder();
                //    };
                //};
            }

            textField.ShouldReturn = _shouldReturnCondition;
            _textFieldsWithShouldReturnCondition.Add(textField);
        }
    }
}