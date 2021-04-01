using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace InterfacesUWP
{
    public class WeakTypedEventHandler<TSender, TResult>
    {
        private WeakReference _targetReference;
        private MethodInfo _methodInfo;

        public WeakTypedEventHandler(TypedEventHandler<TSender, TResult> callback)
        {
            // Since we originally weak referenced the event handler, the event handler will obviously be disposed unless the calling
            // subscriber holds a reference to it.

            // We'll hold a weak reference to the subscriber target
            _targetReference = new WeakReference(callback.Target);

            // And also obtain the actual method info
            _methodInfo = callback.GetMethodInfo();
        }

        public void Handler(TSender sender, TResult e)
        {
            if (_targetReference != null)
            {
                object target = _targetReference.Target;
                if (target != null)
                {
                    _methodInfo.Invoke(target, new object[] { sender, e });
                    return;
                }
            }

            // Otherwise clean up
            _targetReference = null;
            _methodInfo = null;
        }
    }
}
