using PowerPlannerAppDataLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace PowerPlannerUWP.Controls
{
    public class AbTestControl : UserControl
    {
        private TimeSpan _durationWithFocus = new TimeSpan();

        private UIElement _disabledContent;
        public UIElement DisabledContent
        {
            get => _disabledContent;
            set
            {
                _disabledContent = value;
                ApplyIfReady();
            }
        }

        private UIElement _enabledContent;
        public UIElement EnabledContent
        {
            get => _enabledContent;
            set
            {
                _enabledContent = value;
                ApplyIfReady();
            }
        }

        private string _testName;
        public string TestName
        {
            get => _testName;
            set
            {
                _testName = value;
                ApplyIfReady();
            }
        }

        private bool _hasApplied;
        private void ApplyIfReady()
        {
            if (_hasApplied)
            {
                return;
            }

            if (DisabledContent != null && EnabledContent != null && TestName != null)
            {
                _hasApplied = true;

                bool enabled = true;
                try
                {
                    // Wrap in try/catch, mostly just so that designer view renders
                    enabled = AbTestHelper.IsEnabled(TestName);
                }
                catch { }

                if (enabled)
                {
                    Content = EnabledContent;
                }
                else
                {
                    Content = DisabledContent;
                }
            }
        }

        private DateTime _gotFocus;
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            _gotFocus = DateTime.UtcNow;
            System.Diagnostics.Debug.WriteLine("GotFocus");
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            _durationWithFocus += DateTime.UtcNow - _gotFocus;
            _gotFocus = DateTime.MinValue;
            System.Diagnostics.Debug.WriteLine("LostFocus");
            base.OnLostFocus(e);
        }

        public TimeSpan GetDurationWithFocus()
        {
            if (_gotFocus != DateTime.MinValue)
            {
                return _durationWithFocus + (DateTime.UtcNow - _gotFocus);
            }
            else
            {
                return _durationWithFocus;
            }
        }
    }
}
