using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public struct Thickness
    {
        public Thickness(float uniformLength)
        {
            Left = uniformLength;
            Top = uniformLength;
            Right = uniformLength;
            Bottom = uniformLength;
        }

        public Thickness(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public float Bottom { get; set; }
        public float Left { get; set; }
        public float Right { get; set; }
        public float Top { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Thickness otherThickness)
            {
                return Equals(otherThickness);
            }

            return base.Equals(obj);
        }
        public bool Equals(Thickness thickness)
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

        public static bool operator ==(Thickness t1, Thickness t2)
        {
            return t1.Equals(t2);
        }

        public static bool operator !=(Thickness t1, Thickness t2)
        {
            return !t1.Equals(t2);
        }

        public float Width => Left + Right;
        public float Height => Top + Bottom;

        public Thickness AsModified()
        {
            return new Thickness(Theme.Current.MarginModifier * Left, Theme.Current.MarginModifier * Top, Theme.Current.MarginModifier * Right, Theme.Current.MarginModifier * Bottom);
        }

        public Thickness Combine(Thickness other)
        {
            return new Thickness(
                Left + other.Left,
                Top + other.Top,
                Right + other.Right,
                Bottom + other.Bottom);
        }
    }
}
