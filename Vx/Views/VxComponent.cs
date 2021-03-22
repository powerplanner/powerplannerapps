using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vx.Views
{
    public abstract class VxComponent : VxView
    {
        private Action<object> _onTopNativeViewChanged;
        private VxView _current;
        private VxNativeView _currentNativeView;

        public VxComponent()
        {
            SubscribeToStates();
        }

        private void SubscribeToStates()
        {
            var stateType = typeof(VxState);
            foreach (var prop in this.GetType().GetProperties().Where(i => stateType.IsAssignableFrom(i.PropertyType)))
            {
                var state = prop.GetValue(this) as VxState;
                state.ValueChanged += State_ValueChanged;
            }
        }

        private void State_ValueChanged(object sender, EventArgs e)
        {
            // TODO: Send to dispatcher first to bulk the state changes
            RenderActual();
        }

        protected abstract VxView Render();

        protected void ShowPopup(VxComponent view)
        {

        }

        public void SetOnTopNativeViewChanged(Action<object> onTopNativeViewChanged)
        {
            _onTopNativeViewChanged = onTopNativeViewChanged;
            RenderActual();
        }

        private void RenderActual()
        {
            VxView newView = Render();
            VxView oldView = _current;
            _current = newView;

            if (oldView == null || oldView.GetType() != newView.GetType())
            {
                _currentNativeView = VxNativeView.Create(newView);
                _onTopNativeViewChanged?.Invoke(_currentNativeView.NativeView);
            }

            else
            {
                _currentNativeView.ApplyDifferentView(newView);
            }
        }
    }
}
