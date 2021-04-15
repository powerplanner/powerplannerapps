using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public interface IStackLayout
    {
        List<View> Children { get; }
    }
    public class LinearLayout : View, IStackLayout
    {
        public List<View> Children { get; } = new List<View>();

        public static void SetWeight(View view, float weight)
        {
            view.SetAttachedProperty("LinearLayout.Weight", weight);
        }

        public static float GetWeight(View view)
        {
            return view.GetAttachedProperty("LinearLayout.Weight", 0);
        }
    }
}
