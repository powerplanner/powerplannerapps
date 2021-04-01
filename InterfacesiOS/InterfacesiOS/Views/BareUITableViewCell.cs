using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.Binding;
using ToolsPortable;

namespace InterfacesiOS.Views
{
#if DEBUG
    internal static class ActiveCells
    {
        private static readonly WeakReferenceList<UITableViewCell> ACTIVE_CELLS = new WeakReferenceList<UITableViewCell>();

        static ActiveCells()
        {
            OutputActiveCells();
        }

        public static void Track(UITableViewCell cell)
        {
            ACTIVE_CELLS.Add(cell);
        }

        private static string _prevResult;
        private static async void OutputActiveCells()
        {
            while (true)
            {
                await System.Threading.Tasks.Task.Delay(1500);

                string result = "";
                int count = 0;
                foreach (var v in ACTIVE_CELLS)
                {
                    count++;
                }
                result = $"ACTIVE CELLS ({count})";
                if (_prevResult != result)
                {
                    _prevResult = result;
                    System.Diagnostics.Debug.WriteLine(result);
                }
            }
        }
    }
#endif

    public class BareUITableViewCell<T> : BareUITableViewCell
    {
        public BareUITableViewCell(string cellId) : base(cellId)
        {

        }

        public new T DataContext
        {
            get { return base.DataContext == null ? default(T) : (T)base.DataContext; }
            set { base.DataContext = value; }
        }
    }

    public class BareUITableViewCell : UITableViewCell
    {
        public BareUITableViewCell(string cellId) : base(UITableViewCellStyle.Default, cellId)
        {
#if DEBUG
            ActiveCells.Track(this);
#endif
        }

        public BindingHost BindingHost { get; private set; } = new BindingHost();
        private object _dataContext;
        public object DataContext
        {
            get { return _dataContext; }
            set
            {
                if (object.ReferenceEquals(value, _dataContext))
                {
                    OnDataContextAssigned();
                    return;
                }

                _dataContext = value;
                BindingHost.DataContext = value;

                OnDataContextChanged();
                OnDataContextAssigned();
            }
        }

        protected virtual void OnDataContextChanged()
        {
            // Nothing
        }

        /// <summary>
        /// This gets called even if the data context is identical
        /// </summary>
        protected virtual void OnDataContextAssigned()
        {
            // Nothing
        }
    }
}