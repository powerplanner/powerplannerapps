using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public enum VxBindingMode
    {
        /// <summary>
        /// Normal two-way binding will occur, the component will re-render upon changes.
        /// </summary>
        Default = 0,
        
        /// <summary>
        /// When the view's value changes, it will silently set the state (NOT triggering a re-render).
        /// </summary>
        Silent = 1
    }
}
