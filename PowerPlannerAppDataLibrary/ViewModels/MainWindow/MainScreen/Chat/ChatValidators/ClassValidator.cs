using System;
using System.Collections.Generic;
using System.Text;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatValidators
{
    internal class ClassValidator
    {
        public static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Class name cannot be empty or whitespace.");
            }

            if (name.Length > 150)
            {
                throw new ArgumentException("Class name cannot be longer than 150 characters.");
            }
        }

        public static byte[] GetColor(string hexColor)
        {
            if (string.IsNullOrWhiteSpace(hexColor))
            {
                throw new ArgumentException("Class color cannot be empty or whitespace.");
            }
            return ToolsPortable.ColorHelper.ToBytes(hexColor);
        }
    }
}
