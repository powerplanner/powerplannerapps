using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Views;

namespace InterfacesDroid.Dialogs
{
    public class CustomColorPickerDialog
    {
        public event EventHandler<Color> ColorChosen;
        private Activity m_activity;
        private CustomColorPickerView m_customColorPickerView;
        private AlertDialog m_dialog;

        public CustomColorPickerDialog(Activity activity)
        {
            m_activity = activity;
        }

        public void Show(Color color)
        {
            if (m_dialog == null)
            {
                m_customColorPickerView = new CustomColorPickerView(m_activity);
                m_dialog = new AlertDialog.Builder(m_activity).SetView(m_customColorPickerView)
                    .SetPositiveButton("OK", delegate
                    {
                        ColorChosen?.Invoke(this, m_customColorPickerView.Color);
                    })
                    .SetNegativeButton("Cancel", delegate
                    {

                    }).Create();
            }

            m_customColorPickerView.Color = color;

            m_dialog.Show();
        }
    }
}