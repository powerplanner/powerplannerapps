using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public abstract class VxBindingComponent<T> : VxComponent
    {
        public new T BindingContext
        {
            get => (T)base.BindingContext;
            set => base.BindingContext = value;
        }

        protected override bool IsDependentOnBindingContext => true;
    }
}
