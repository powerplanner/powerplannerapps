using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Components
{
    public class DeleteWithConfirmationButton : DestructiveButton
    {
        private View _viewRef;

        public Action Delete { get; set; }

        /// <summary>
        /// Do NOT use this. Use <see cref="Delete"/> instead.
        /// </summary>
        public new Action Click
        {
            get => throw new InvalidOperationException();
            set => throw new InvalidOperationException();
        }

        public DeleteWithConfirmationButton()
        {
            base.Click = ConfirmDelete;
            ViewRef = v => _viewRef = v;
        }

        private void ConfirmDelete()
        {
            new ContextMenu
            {
                Items =
                {
                    new MenuItem
                    {
                        Text = PowerPlannerResources.GetString("String_YesDelete"),
                        Click = Delete
                    }
                }
            }.Show(_viewRef);
        }
    }
}
