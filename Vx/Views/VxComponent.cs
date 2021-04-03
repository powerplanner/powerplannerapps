using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Vx.Views
{
    public abstract class VxComponent : ContentView
    {
        private VxView _current;

        public VxComponent()
        {
            SubscribeToStates();
        }

        protected override void OnParentSet()
        {
            lock (this)
            {
                if (!_dirty)
                {
                    return;
                }
            }

            RenderActual();
        }

        private void SubscribeToStates()
        {
            var stateType = typeof(VxState);
            foreach (var prop in this.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).Where(i => stateType.IsAssignableFrom(i.PropertyType)))
            {
                var state = prop.GetValue(this) as VxState;
                state.ValueChanged += State_ValueChanged;
            }
            foreach (var prop in this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(i => stateType.IsAssignableFrom(i.FieldType)))
            {
                var state = prop.GetValue(this) as VxState;
                state.ValueChanged += State_ValueChanged;
            }
        }

        protected abstract VxView Render();

        private void State_ValueChanged(object sender, EventArgs e)
        {
            MarkDirty();
        }

        private bool _dirty;

        /// <summary>
        /// Marks this component for re-render on next UI cycle
        /// </summary>
        protected void MarkDirty()
        {
            lock (this)
            {
                if (_dirty)
                {
                    return;
                }

                _dirty = true;
            }

            MainThread.BeginInvokeOnMainThread(RenderActual);
        }

        private void RenderActual()
        {
            lock (this)
            {
                _dirty = false;
            }

            VxView newView = Render();
            VxView oldView = _current;
            _current = newView;

            if (oldView == null || oldView.GetType() != newView.GetType())
            {
                Content = newView.CreateView();
            }

            else
            {
                oldView.ApplyDifferentView(newView);
            }
        }
    }
}
