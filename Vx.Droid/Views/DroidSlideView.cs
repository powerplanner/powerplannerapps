using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager2.Widget;
using InterfacesDroid.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vx.Views;

namespace Vx.Droid.Views
{
    public class DroidSlideView : DroidView<SlideView, ViewPager2>
    {


        public DroidSlideView() : base(new ViewPager2(VxDroidExtensions.ApplicationContext))
        {
            View.OffscreenPageLimit = 1; // This means views on left, so 1 is actually 2 total offscreen views
            View.RegisterOnPageChangeCallback(new PageChangeCallback(this));
        }

        private class PageChangeCallback : ViewPager2.OnPageChangeCallback
        {
            private DroidSlideView _droidSlideView;

            public PageChangeCallback(DroidSlideView droidSlideView)
            {
                _droidSlideView = droidSlideView;
            }

            public override void OnPageSelected(int position)
            {
                _droidSlideView.VxView?.Position?.ValueChanged?.Invoke(GetVxPosition(position));
            }
        }

        private static int GetVxPosition(int droidPosition)
        {
            return droidPosition - int.MaxValue / 2;
        }

        private static int GetDroidPosition(int vxPosition)
        {
            return vxPosition + int.MaxValue / 2;
        }

        protected override void ApplyProperties(SlideView oldView, SlideView newView)
        {
            base.ApplyProperties(oldView, newView);

            if (!object.ReferenceEquals(oldView?.ItemTemplate, newView.ItemTemplate))
            {
                View.Adapter = new DroidSwipeViewAdapter(newView.ItemTemplate);
            }

            int droidPosition = GetDroidPosition(newView.Position.Value);
            if (View.CurrentItem != droidPosition)
            {
                View.SetCurrentItem(droidPosition, false);
            }
        }

        private class DroidSwipeViewAdapter : RecyclerView.Adapter
        {
            public override int ItemCount => int.MaxValue;
            private Func<object, Vx.Views.View> _itemTemplate;

            public DroidSwipeViewAdapter(Func<int, Vx.Views.View> itemTemplate)
            {
                _itemTemplate = i =>
                {
                    if (i == null)
                    {
                        return null;
                    }

                    return itemTemplate((int)i);
                };
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                ((holder.ItemView as INativeComponent).Component as DataTemplateHelper.VxDataTemplateComponent).Data = GetVxPosition(position);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var view = new DataTemplateHelper.VxDataTemplateComponent
                {
                    Template = _itemTemplate
                }.Render();

                view.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);

                return new GenericRecyclerViewHolder(view);
            }
        }
    }
}