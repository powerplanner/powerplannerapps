﻿using System;
using InterfacesiOS.Views;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSSlideView : iOSView<SlideView, VxSlideView>
    {
        public iOSSlideView()
        {
            View.MovedToNext += View_MovedToNext;
            View.MovedToPrev += View_MovedToPrev;
        }

        private void View_MovedToPrev(object sender, EventArgs e)
        {
            VxView.Position?.ValueChanged?.Invoke(VxView.Position.Value - 1);
        }

        private void View_MovedToNext(object sender, EventArgs e)
        {
            VxView.Position?.ValueChanged?.Invoke(VxView.Position.Value + 1);
        }

        protected override void ApplyProperties(SlideView oldView, SlideView newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Position = newView.Position?.Value ?? 0;

            if (!object.ReferenceEquals(oldView?.ItemTemplate, newView.ItemTemplate))
            {
                View.ItemTemplate = newView.ItemTemplate;
            }
        }
    }

    public class VxSlideView : BareUISlideView<UIView>
    {
        public event EventHandler MovedToNext, MovedToPrev;

        private int _position;
        public int Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    UpdateAllViews();
                }
            }
        }

        private Func<object, View> _objItemTemplate;
        private Func<int, View> _itemTemplate;
        public Func<int, View> ItemTemplate
        {
            get => _itemTemplate;
            set
            {
                if (!object.ReferenceEquals(_itemTemplate, value))
                {
                    _itemTemplate = value;
                    _objItemTemplate = i =>
                    {
                        if (i == null)
                        {
                            return null;
                        }

                        return _itemTemplate((int)i);
                    };
                    UpdateAllViews();
                }
            }
        }

        protected override UIView CreateView()
        {
            return new DataTemplateHelper.VxDataTemplateComponent().Render();
        }

        protected override void OnMovedToNext()
        {
            MovedToNext?.Invoke(this, new EventArgs());
        }

        protected override void OnMovedToPrevious()
        {
            MovedToPrev?.Invoke(this, new EventArgs());
        }

        protected override void UpdateCurrView(UIView curr)
        {
            var comp = GetDataTemplateComponent(curr);
            comp.Data = Position;
            comp.Template = _objItemTemplate;
        }

        protected override void UpdateNextView(UIView next)
        {
            var comp = GetDataTemplateComponent(next);
            comp.Data = Position + 1;
            comp.Template = _objItemTemplate;
        }

        protected override void UpdatePrevView(UIView prev)
        {
            var comp = GetDataTemplateComponent(prev);
            comp.Data = Position - 1;
            comp.Template = _objItemTemplate;
        }

        private DataTemplateHelper.VxDataTemplateComponent GetDataTemplateComponent(UIView view)
        {
            return (view as INativeComponent).Component as DataTemplateHelper.VxDataTemplateComponent;
        }
    }
}
