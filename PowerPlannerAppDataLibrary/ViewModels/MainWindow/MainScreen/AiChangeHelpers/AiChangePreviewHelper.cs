using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.AiChangeHelpers
{
    internal static class AiChangePreviewHelper
    {
        internal static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "…";
        }

        //internal static string RenderChange(string fieldName, string oldValue, string newValue)
        //{
        //    oldValue = Truncate(oldValue, 30);
        //    newValue = Truncate(newValue, 30);
        //    return $"{fieldName}: \"{oldValue}\" → \"{newValue}\"";
        //}

        internal static View RenderChange(string fieldName, string oldValue, string newValue)
        {
            oldValue = Truncate(oldValue, 30);
            newValue = Truncate(newValue, 30);

            return new LinearLayout
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new TextBlock { Text = fieldName + ":", Margin = new Thickness(0, 0, 6, 0), FontWeight = FontWeights.Bold, WrapText = false, FontSize = Theme.Current.CaptionFontSize },
                    new LinearLayout
                    {
                        Children =
                        {
                            new TextBlock { Text = oldValue, Strikethrough = true, TextColor = Theme.Current.SubtleForegroundColor, FontSize = Theme.Current.CaptionFontSize },
                            new TextBlock { Text = newValue, FontSize = Theme.Current.CaptionFontSize }
                        }
                    }.LinearLayoutWeight(1)
                }
            };
        }

        internal static View RenderNewProperty(string fieldName, string value)
        {
            value = Truncate(value, 60);
            return new LinearLayout
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new TextBlock { Text = fieldName + ":", Margin = new Thickness(0, 0, 6, 0), FontWeight = FontWeights.Bold, WrapText = false, FontSize = Theme.Current.CaptionFontSize },
                    new TextBlock { Text = value, FontSize = Theme.Current.CaptionFontSize }.LinearLayoutWeight(1)
                }
            };
        }
    }
}
