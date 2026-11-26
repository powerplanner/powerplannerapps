using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes
{
    internal class ChatClassItem : BaseChatItem
    {
        [Description("The name of the class.")]
        public string Name { get; set; }

        public ChatClassItem(ViewItemClass c)
        {
            Name = c.Name;
        }
    }
}
