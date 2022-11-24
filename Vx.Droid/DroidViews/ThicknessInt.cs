using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vx.Droid.DroidViews
{
    public struct ThicknessInt
    {
        public ThicknessInt(int uniformLength)
        {
            Left = uniformLength;
            Top = uniformLength;
            Right = uniformLength;
            Bottom = uniformLength;
        }

        public ThicknessInt(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int Bottom { get; set; }
        public int Left { get; set; }
        public int Right { get; set; }
        public int Top { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is ThicknessInt otherThickness)
            {
                return Equals(otherThickness);
            }

            return base.Equals(obj);
        }
        public bool Equals(ThicknessInt thickness)
        {
            return Left == thickness.Left
                && Top == thickness.Top
                && Right == thickness.Right
                && Bottom == thickness.Bottom;
        }

        public override int GetHashCode()
        {
            return (Left + Top * 10 + Right * 100 + Bottom * 1000).GetHashCode();
        }

        public override string ToString()
        {
            return $"{Left}, {Top}, {Right}, {Bottom}";
        }

        public static bool operator ==(ThicknessInt t1, ThicknessInt t2)
        {
            return t1.Equals(t2);
        }

        public static bool operator !=(ThicknessInt t1, ThicknessInt t2)
        {
            return !t1.Equals(t2);
        }

        public int Width => Left + Right;
        public int Height => Top + Bottom;

        public ThicknessInt Combine(ThicknessInt other)
        {
            return new ThicknessInt(
                Left + other.Left,
                Top + other.Top,
                Right + other.Right,
                Bottom + other.Bottom);
        }
    }
}