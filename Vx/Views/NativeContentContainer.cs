using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    /// <summary>
    /// Container that hosts a native view from a native platform (like a UWP FrameworkElement).
    /// </summary>
    public class NativeContentContainer : View
    {
        public object NativeContent { get; set; }

        public NativeContentContainer()
        {

        }

        public NativeContentContainer(object nativeContent)
        {
            NativeContent = nativeContent;
        }
    }
}
