using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes
{
    internal class BaseChatItem
    {
        [Description("A unique identifier for the item.")]
        public Guid Identifier { get; set; }

        [Description("The type of the item.")]
        public ItemType Discriminator { get; set; }
    }
}
