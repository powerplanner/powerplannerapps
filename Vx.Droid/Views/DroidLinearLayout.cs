using Vx.Droid.DroidViews;

namespace Vx.Droid.Views
{
    public class DroidLinearLayout : DroidView<Vx.Views.LinearLayout, DroidVxLinearLayout>
    {
        public DroidLinearLayout() : base(new DroidVxLinearLayout(VxDroidExtensions.ApplicationContext))
        {
        }

        protected override void ApplyProperties(Vx.Views.LinearLayout oldView, Vx.Views.LinearLayout newView)
        {
            base.ApplyProperties(oldView, newView);

            View.SetBackgroundColor(newView.BackgroundColor.ToDroid());
            View.Orientation = newView.Orientation;

            ReconcileChildren(oldView?.Children, newView.Children, View);
        }
    }
}