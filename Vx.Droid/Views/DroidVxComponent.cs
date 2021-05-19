using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vx.Droid.Views
{
    public class DroidVxComponent : DroidView<Vx.Views.VxComponent, View>
    {
        public DroidVxComponent(Vx.Views.VxComponent component) : base(component.Render())
        {

        }
    }
}