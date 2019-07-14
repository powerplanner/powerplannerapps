using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.DataLayer
{
    public class ColorItem : IEquatable<byte[]>
    {
        public static readonly List<ColorItem> DefaultColors = new List<ColorItem>()
        {
            new ColorItem("Blue", new byte[] { 27,161,226 }),
            new ColorItem("Red", new byte[] { 229,20,0 }),
            new ColorItem("Green", new byte[] { 51,153,51 }),
            new ColorItem("Purple", new byte[] { 162,0,255 }),
            new ColorItem("Pink", new byte[] { 230,113,184 }),
            new ColorItem("Mango", new byte[] { 240,150,9 }),
            new ColorItem("Teal", new byte[] { 0,171,169 }),
            new ColorItem("Lime", new byte[] { 140,191,38 }),
            new ColorItem("Magenta", new byte[] { 255,0,151 }),
            new ColorItem("Brown", new byte[] { 160,80,0 }),
            new ColorItem("Gray", new byte[] { 75,75,75 }),
            new ColorItem("Nokia", new byte[] { 16,128,221 }),
            new ColorItem("HTC", new byte[] { 105,180,15 })
        };

        public string Text { get; set; }
        public byte[] Color { get; set; }

        public ColorItem(string text, byte[] color)
        {
            Text = text;
            Color = color;
        }

        public bool Equals(byte[] other)
        {
            if (Color == null || other == null)
                return false;

            if (other.Length == 4)
            {
                other = other.Skip(1).ToArray();
            }

            return Color.SequenceEqual(other);
        }
    }
}
