using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class VxState<T>
    {
        public VxState(T value)
        {

        }

        public T Value { get; set; }
    }
}
