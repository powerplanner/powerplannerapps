using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Views.InputMethods;

namespace InterfacesDroid.Helpers
{
    public static class KeyboardHelper
    {
        public static void HideKeyboard(View view)
        {
            InputMethodManager im = view.Context.GetSystemService(Context.InputMethodService) as InputMethodManager;
            im.HideSoftInputFromWindow(view.WindowToken, HideSoftInputFlags.None);
        }

        private static void ShowKeyboard(View view)
        {
            InputMethodManager im = view.Context.GetSystemService(Context.InputMethodService) as InputMethodManager;
            im.ShowSoftInput(view, ShowFlags.Implicit);
        }

        public static void FocusAndShow(EditText editText)
        {
            if (editText == null)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif
                return;
            }

            try
            {
                EventHandler<View.ViewAttachedToWindowEventArgs> handler = null;
                handler = delegate
                {
                    try
                    {
                        editText.ViewAttachedToWindow -= handler;

                        if (editText.RequestFocus())
                        {
                            ShowKeyboard(editText);
                        }
                    }
                    catch
                    {
#if DEBUG
                        System.Diagnostics.Debugger.Break();
#endif
                    }
                };

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat && editText.IsAttachedToWindow)
                {
                    handler(editText, null);
                }
                else
                {
                    editText.ViewAttachedToWindow += handler;
                }
            }
            catch
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif
            }
        }
    }
}