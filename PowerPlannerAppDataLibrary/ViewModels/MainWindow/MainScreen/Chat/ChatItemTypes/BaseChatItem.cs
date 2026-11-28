using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes
{
    internal class BaseChatItem
    {
        [Description("A unique id for the item.")]
        public Guid Id { get; set; }

        public BaseChatItem(BaseViewItem item)
        {
            Id = item.Identifier;
        }
    }
}
