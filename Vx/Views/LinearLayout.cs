﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Vx.Views
{
    public class LinearLayout : View
    {
        public Orientation Orientation { get; set; }

        public List<View> Children { get; } = new List<View>();

        public Color BackgroundColor { get; set; }

        public float TotalWeight()
        {
            return Children.Sum(i => GetWeight(i));
        }

        public static void SetWeight(View view, float weight)
        {
            view.SetAttachedProperty("LinearLayout.Weight", weight);
        }

        public static float GetWeight(View view)
        {
            return view.GetAttachedProperty("LinearLayout.Weight", 0f);
        }
    }

    public static class LinearLayoutExtensions
    {
        public static T LinearLayoutWeight<T>(this T view, float weight) where T : View
        {
            LinearLayout.SetWeight(view, weight);
            return view;
        }
    }
}
