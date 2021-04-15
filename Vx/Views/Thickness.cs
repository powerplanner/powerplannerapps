using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public struct Thickness
    {
        public Thickness(double uniformLength)
        {
            Left = uniformLength;
            Top = uniformLength;
            Right = uniformLength;
            Bottom = uniformLength;
        }

        public Thickness(double left, double top, double right, double bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public double Bottom { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
        public double Top { get; set; }

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
    }
}
