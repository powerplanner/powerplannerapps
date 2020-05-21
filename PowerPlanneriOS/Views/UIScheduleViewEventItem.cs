using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.ViewLists;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using ToolsPortable;
using CoreGraphics;
using PowerPlannerAppDataLibrary.ViewItems;

namespace PowerPlanneriOS.Views
{
    public abstract class UIScheduleViewEventItem : UIView
    {
        private UIAdditionalItemsView _additionalItemsView;
        private const int CORNER_RADIUS = 10;

        public const string TELEMETRY_ON_CLICK_EVENT_NAME = "Click_ScheduleEventItem";

        public UIScheduleViewEventItem()
        {
            _additionalItemsView = new UIAdditionalItemsView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            base.Add(_additionalItemsView);
            _additionalItemsView.PinToTop(this);

            // Width constraint will be added when item assigned
        }

        public override UIView HitTest(CGPoint point, UIEvent uievent)
        {
            // Ignore touches on this view itself so that class schedules behind this
            // can still be touched
            // https://stackoverflow.com/questions/7719412/how-to-ignore-touch-events-and-pass-them-to-another-subviews-uicontrol-objects

            UIView hitView = base.HitTest(point, uievent);

            // If hit view is THIS view, return null and allow it to continue traversing
            if (hitView == this)
            {
                return null;
            }

            // Otherwise return the hit view (as it could be one of the touchable items in the view)
            return hitView;
        }

        private bool CanExpand()
        {
            return Item != null && Item.CanExpand();
        }

        private DayScheduleItemsArranger.EventItem _item;
        public DayScheduleItemsArranger.EventItem Item
        {
            get { return _item; }
            set
            {
                if (_item != null)
                {
                    throw new InvalidOperationException("Item cannot be changed");
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _item = value;

                _additionalItemsView.AdditionalItems = value?.AdditionalItems;
                
                var content = GenerateContent(value);
                content.TranslatesAutoresizingMaskIntoConstraints = false;
                content.Layer.CornerRadius = CORNER_RADIUS;
                this.Add(content);
                content.SetHeight((float)value.Height);
                this.SetMinimumHeight(1000); // We set height to some absurd amount so that when expanded touch events get captured within that height
                content.PinToTop(this);

                if (this is UIScheduleViewEventItemCollapsed)
                {
                    if (value.AdditionalItems != null)
                    {
                        base.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[content]-2-[additionalItems]", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                            "content", content,
                            "additionalItems", _additionalItemsView));
                        content.SetWidth(UIScheduleViewEventItemCollapsed.WIDTH_OF_COLLAPSED_ITEM);
                    }
                    else
                    {
                        content.PinToLeft(this);
                        content.SetWidth(UIScheduleViewEventItemCollapsed.WIDTH_OF_COLLAPSED_ITEM);
                    }
                }
                else
                {
                    if (value.AdditionalItems != null)
                    {
                        base.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[content]-2-[additionalItems]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                            "content", content,
                            "additionalItems", _additionalItemsView));
                    }
                    else
                    {
                        content.StretchWidth(this);
                    }
                }

                content.TouchUpInside += new WeakEventHandler(TouchTarget_TouchUpInside).Handler;
            }
        }

        private void TouchTarget_TouchUpInside(object sender, EventArgs e)
        {
            if (CanExpand())
            {
                ShowFull();
            }
            else
            {
                PowerPlannerApp.Current.GetMainScreenViewModel()?.ShowItem(Item.Item);
                TelemetryExtension.Current?.TrackEvent(TELEMETRY_ON_CLICK_EVENT_NAME);
            }
        }

        private UIView _fullView;
        private void ShowFull()
        {
            if (_fullView == null)
            {
                _fullView = new UIView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    BackgroundColor = UIColor.White
                };
                _fullView.Layer.CornerRadius = CORNER_RADIUS;

                var fullContent = GenerateFullContent();
                _fullView.Add(fullContent);
                fullContent.StretchWidthAndHeight(_fullView, top: CORNER_RADIUS, bottom: CORNER_RADIUS);

                this.Add(_fullView);
                _fullView.StretchWidth(this);
                _fullView.PinToTop(this);
            }

            _fullView.Hidden = false;
        }

        public bool HideFull()
        {
            if (_fullView != null)
            {
                if (!_fullView.Hidden)
                {
                    _fullView.Hidden = true;
                    return true;
                }
            }

            return false;
        }

        protected abstract UIControl GenerateContent(DayScheduleItemsArranger.EventItem item);

        private UIView GenerateFullContent()
        {
            var stackView = new UIStackView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Axis = UILayoutConstraintAxis.Vertical,
                Spacing = 1
            };
            AddSingleFullItemView(stackView, Item.Item);
            if (Item.AdditionalItems != null)
            {
                foreach (var i in Item.AdditionalItems)
                {
                    AddSingleFullItemView(stackView, i);
                }
            }

            return stackView;
        }

        private void AddSingleFullItemView(UIStackView stackView, ViewItemTaskOrEvent item)
        {
            var view = new UIMainCalendarItemView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                DataContext = item,

                // After opened, we hide this popup, otherwise when the user presses back, it'll close the popup rather than the task
                // Ideally we would implement the back handling as part of the view model like we did for UWP, but for simplicity we're going
                // to leave it like this for now
                AfterOpenedTaskOrEventAction = delegate { HideFull(); }
            };
            stackView.AddArrangedSubview(view);
            view.StretchWidth(stackView);
        }

        internal static CGColor GetBackgroundCGColor(ViewItemTaskOrEvent item)
        {
            if (item.IsComplete)
            {
                return new CGColor(180 / 255f, 1);
            }

            else
            {
                return BareUIHelper.ToCGColor(item.Class?.Color);
            }
        }

        internal static UIColor GetBackgroundColor(ViewItemTaskOrEvent item)
        {
            return new UIColor(GetBackgroundCGColor(item));
        }

        private class UIAdditionalItemsView : UIStackView
        {
            private BareUIStackViewItemsSourceAdapter _itemsSourceAdapter;

            public const float WIDTH = 7;

            public UIAdditionalItemsView()
            {
                base.Spacing = 2;
                _itemsSourceAdapter = new BareUIStackViewItemsSourceAdapter(this, CreateCircle);
                this.SetWidth(WIDTH);
            }

            private IEnumerable<ViewItemTaskOrEvent> _additionalItems;
            public IEnumerable<ViewItemTaskOrEvent> AdditionalItems
            {
                get { return _additionalItems; }
                set
                {
                    _additionalItems = value;
                    _itemsSourceAdapter.ItemsSource = value;
                }
            }

            private UIView CreateCircle(object item)
            {
                return CreateCircle(item as ViewItemTaskOrEvent);
            }

            private UIView CreateCircle(ViewItemTaskOrEvent item)
            {
                var view = new BareUIEllipseView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                };
                view.SetWidth(WIDTH);
                view.SetHeight(WIDTH);

                view.FillColor = GetBackgroundCGColor(item);

                return view;
            }
        }
    }

    public class UIScheduleViewEventItemFull : UIScheduleViewEventItem
    {
        protected override UIControl GenerateContent(DayScheduleItemsArranger.EventItem item)
        {
            var container = new UIControl()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = GetBackgroundColor(item.Item)
            };

            var label = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                AttributedText = new NSAttributedString(item.Item.Name, strikethroughStyle: item.Item.IsComplete ? NSUnderlineStyle.Single : NSUnderlineStyle.None),
                TextColor = UIColor.White,
                Font = UIFont.PreferredCaption1
            };
            container.Add(label);
            label.StretchWidth(container, left: 6, right: 6);
            label.PinToTop(container, top: 6);

            return container;
        }
    }

    public class UIScheduleViewEventItemCollapsed : UIScheduleViewEventItem
    {
        public const float WIDTH_OF_COLLAPSED_ITEM = 36;
        public static readonly float SPACING_WITH_NO_ADDITIONAL = WIDTH_OF_COLLAPSED_ITEM + 6;
        public static readonly float SPACING_WITH_ADDITIONAL = WIDTH_OF_COLLAPSED_ITEM + 16;

        protected override UIControl GenerateContent(DayScheduleItemsArranger.EventItem item)
        {
            var container = new UIControl()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = GetBackgroundColor(item.Item)
            };

            var label = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = item.Item.Name.Length > 0 ? item.Item.Name.Substring(0, 1) : "",
                TextColor = UIColor.White,
                TextAlignment = UITextAlignment.Center
            };
            container.Add(label);
            label.StretchWidthAndHeight(container, left: 6, top: 6, right: 9, bottom: 6);

            return container;
        }
    }
}