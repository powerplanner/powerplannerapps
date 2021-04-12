using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace Vx.Views
{
    public class VxBindings : Dictionary<BindableProperty, VxBinding>
    {
        public static void SetBinding(BindableObject target, BindableProperty property, VxBinding binding)
        {
            var bindings = GetVxBindings(target);
            if (bindings == null)
            {
                bindings = new VxBindings();
                SetVxBindings(target, bindings);
            }

            if (binding == null)
            {
                target.RemoveBinding(property);
                bindings.Remove(property);
            }
            else
            {
                bindings[property] = binding;
            }

            ApplyBinding(target, property, binding);
        }

        private static void ApplyBinding(BindableObject target, BindableProperty property, VxBinding binding)
        {
            if (binding == null)
            {
                target.RemoveBinding(property);
            }
            else
            {
                if (binding.PropertyPath != null)
                {
                    target.SetBinding(property, new Binding()
                    {
                        Path = binding.PropertyPath,
                        Source = binding.Source,
                        Mode = BindingMode.TwoWay
                    });
                }
                else
                {
                    target.SetValue(property, binding.State.Value);
                    target.SetBinding(property, new Binding()
                    {
                        Path = nameof(binding.BindingValue),
                        Source = binding,
                        Mode = BindingMode.OneWayToSource
                    });
                }
            }
        }

        public static void TransferBindings(BindableObject virtualView, BindableObject renderedView)
        {
#if DEBUG
            try
            {
#endif
                var virtualBindings = GetVxBindings(virtualView);
                var renderedBindings = GetVxBindings(renderedView);

                if ((virtualBindings == null || virtualBindings.Count == 0) && (renderedBindings == null || renderedBindings.Count == 0))
                {
                    return;
                }

                if (virtualBindings == null)
                {
                    virtualBindings = new VxBindings();
                }

                if (renderedBindings == null)
                {
                    renderedBindings = new VxBindings();
                    SetVxBindings(renderedView, renderedBindings);
                }

                // Update or add new bindings
                foreach (var virtualBinding in virtualBindings.ToArray())
                {
                    // Clear binding from virtual view just to keep things clean
                    virtualView.RemoveBinding(virtualBinding.Key);

                    if (renderedBindings.TryGetValue(virtualBinding.Key, out VxBinding renderedBinding))
                    {
                        bool changed = !renderedBinding.Equals(virtualBinding);

                        // Update binding
                        renderedBinding.State = virtualBinding.Value.State;
                        renderedBinding.BindingMode = virtualBinding.Value.BindingMode;
                        renderedBinding.PropertyPath = virtualBinding.Value.PropertyPath;
                        renderedBinding.Source = virtualBinding.Value.Source;

                        if (renderedBinding.PropertyPath != null)
                        {
                            if (changed)
                            {
                                ApplyBinding(renderedView, virtualBinding.Key, renderedBinding);
                            }
                        }
                        else
                        {
                            renderedView.SetValue(virtualBinding.Key, virtualBinding.Value.State.Value);
                        }
                    }
                    else
                    {
                        // Add binding
                        renderedBindings[virtualBinding.Key] = virtualBinding.Value;
                        ApplyBinding(renderedView, virtualBinding.Key, virtualBinding.Value);
                    }
                }

                // Remove bindings
                foreach (var renderedBinding in renderedBindings.ToArray())
                {
                    if (!virtualBindings.ContainsKey(renderedBinding.Key))
                    {
                        renderedBindings.Remove(renderedBinding.Key);
                        renderedView.RemoveBinding(renderedBinding.Key);
                    }
                }

                // Remove bindings from virtual view to clean up
                foreach (var virtualBinding in virtualBindings.ToArray())
                {
                    virtualView.RemoveBinding(virtualBinding.Key);
                }

                // Clear virtual view bindings just to clean up
                virtualBindings.Clear();
#if DEBUG
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                System.Diagnostics.Debugger.Break();
            }
#endif
        }

        public static VxBindings GetVxBindings(BindableObject target)
        {
            return target.GetValue(VxBindingsProperty) as VxBindings;
        }

        public static void SetVxBindings(BindableObject target, VxBindings value)
        {
            target.SetValue(VxBindingsProperty, value);
        }

        public static readonly BindableProperty VxBindingsProperty = BindableProperty.CreateAttached("VxBindings", typeof(VxBindings), typeof(VxBindings), null);
    }
}
