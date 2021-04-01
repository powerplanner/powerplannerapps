using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using CoreGraphics;
using InterfacesiOS.Helpers;
using Intents;
using System.ComponentModel;

namespace InterfacesiOS.Views
{
    public static class BareUIHelper
    {
        public const int STATUS_AND_NAV_BAR_HEIGHT = 44;

        public static UIView ConfigureForVerticalScrolling(this UIView view, UIScrollView parentScrollView, int left = 0, int top = 0, int right = 0, int bottom = 0)
        {
            // https://developer.apple.com/library/content/documentation/UserExperience/Conceptual/AutolayoutPG/WorkingwithScrollViews.html#//apple_ref/doc/uid/TP40010853-CH24-SW1

            // This define's the scroll view's content area, but doesn't cause the width to match the scroll viewer's width, width and height are still auto at this point
            view.StretchWidthAndHeight(parentScrollView, left: left, top: top, right: right, bottom: bottom);

            // Therefore we need to set the width equal to the scroll view's width as described in step 5 of the link above
            parentScrollView.AddConstraint(NSLayoutConstraint.Create(
                view,
                NSLayoutAttribute.Width,
                NSLayoutRelation.Equal,
                parentScrollView,
                NSLayoutAttribute.Width,
                1,
                (left + right) * -1));

            return view;
        }

        public static UIView ConfigureForHorizontalScrolling(this UIView view, UIScrollView parentScrollView, int left = 0, int top = 0, int right = 0, int bottom = 0)
        {
            // https://developer.apple.com/library/content/documentation/UserExperience/Conceptual/AutolayoutPG/WorkingwithScrollViews.html#//apple_ref/doc/uid/TP40010853-CH24-SW1

            // This define's the scroll view's content area, but doesn't cause the width to match the scroll viewer's width, width and height are still auto at this point
            view.StretchWidthAndHeight(parentScrollView, left: left, top: top, right: right, bottom: bottom);

            // Therefore we need to set the height equal to the scroll view's height as described in step 5 of the link above
            parentScrollView.AddConstraint(NSLayoutConstraint.Create(
                view,
                NSLayoutAttribute.Height,
                NSLayoutRelation.Equal,
                parentScrollView,
                NSLayoutAttribute.Height,
                1,
                (top + bottom) * -1));

            return view;
        }

        public static UIView ConfigureForMultiDirectionScrolling(this UIView view, UIScrollView parentScrollView, int left = 0, int top = 0, int right = 0, int bottom = 0)
        {
            // This define's the scroll view's content area, but doesn't cause the width to match the scroll viewer's width, width and height are still auto at this point
            view.StretchWidthAndHeight(parentScrollView, left: left, top: top, right: right, bottom: bottom);

            // In this case we don't need to assign the width of anything, we leave it as auto

            return view;
        }

        public static UIView StretchWidthAndHeight(this UIView view, UIView parentView, float left = 0, float top = 0, float right = 0, float bottom = 0)
        {
            // https://gist.github.com/twostraws/a02d4cc09fc7bc16859c
            // http://commandshift.co.uk/blog/2013/01/31/visual-format-language-for-autolayout/
            //parentView.AddConstraints(NSLayoutConstraint.FromVisualFormat($"H:|-({left})-[view]-({right})-|", NSLayoutFormatOptions.AlignAllCenterX, null, new NSDictionary("view", view)));
            //parentView.AddConstraints(NSLayoutConstraint.FromVisualFormat($"V:|-({top})-[view]-({bottom})-|", NSLayoutFormatOptions.AlignAllCenterX, null, new NSDictionary("view", view)));

            view.StretchWidth(parentView, left: left, right: right);
            view.StretchHeight(parentView, top: top, bottom: bottom);

            return view;
        }

        public static UIView PinToTop(this UIView view, UIView parentView, float top = 0)
        {
            // http://commandshift.co.uk/blog/2013/01/31/visual-format-language-for-autolayout/
            parentView.AddConstraints(NSLayoutConstraint.FromVisualFormat($"V:|-({top})-[view]", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary("view", view)));

            return view;
        }

        public static UIView PinToBottom(this UIView view, UIView parentView, int bottom = 0)
        {
            // http://commandshift.co.uk/blog/2013/01/31/visual-format-language-for-autolayout/
            parentView.AddConstraints(NSLayoutConstraint.FromVisualFormat($"V:[view]-({bottom})-|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary("view", view)));

            return view;
        }

        public static UIView RemovePinToBottom(this UIView view, UIView parentView)
        {
            var existing = parentView.Constraints.FirstOrDefault(i => i.FirstAttribute == NSLayoutAttribute.Bottom && i.SecondAttribute == NSLayoutAttribute.Bottom
                && ((i.FirstItem == view && i.SecondItem == parentView) || i.FirstItem == parentView && i.SecondItem == view));

            if (existing != null)
            {
                parentView.RemoveConstraint(existing);
            }

            return view;
        }

        public static UIView RemoveAllConstraints(this UIView view)
        {
            view.RemoveConstraints(view.Constraints);
            return view;
        }

        public static UIView PinToLeft(this UIView view, UIView parentView, int left = 0)
        {
            parentView.AddConstraints(NSLayoutConstraint.FromVisualFormat($"H:|-({left})-[view]", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary("view", view)));

            return view;
        }

        /// <summary>
        /// Sets the left edge of the view to the right edge of the provided view. Must also provide the parent view which hosts the constraint.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="viewToTheLeft"></param>
        /// <param name="parentView"></param>
        /// <returns></returns>
        public static UIView SetToRightOf(this UIView view, UIView viewToTheLeft, UIView parentView, int spacing = 0)
        {
            foreach (var c in parentView.Constraints)
            {
                if (c.FirstItem == view && c.FirstAttribute == NSLayoutAttribute.Left && c.Relation == NSLayoutRelation.Equal && c.SecondItem == viewToTheLeft && c.SecondAttribute == NSLayoutAttribute.Right)
                {
                    parentView.RemoveConstraint(c);
                }
            }

            parentView.AddConstraint(NSLayoutConstraint.Create(
                view,
                NSLayoutAttribute.Left,
                NSLayoutRelation.Equal,
                viewToTheLeft,
                NSLayoutAttribute.Right,
                1,
                spacing));

            return view;
        }

        /// <summary>
        /// Sets the top edge of this view to the bottom edge of the provided view. Must also provide the parent view which hosts the constraint.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="viewAbove"></param>
        /// <param name="parentView"></param>
        /// <param name="spacing"></param>
        /// <returns></returns>
        public static UIView SetBelow(this UIView view, UIView viewAbove, UIView parentView, int spacing = 0)
        {
            parentView.AddConstraint(NSLayoutConstraint.Create(
                view,
                NSLayoutAttribute.Top,
                NSLayoutRelation.Equal,
                viewAbove,
                NSLayoutAttribute.Bottom,
                1,
                spacing));

            return view;
        }

        public static UIView PinToRight(this UIView view, UIView parentView, int right = 0)
        {
            parentView.AddConstraints(NSLayoutConstraint.FromVisualFormat($"H:[view]-({right})-|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary("view", view)));

            return view;
        }

        public static UIView StretchWithToReadableContentGuide(this UIView view, UIView parentView, int fallbackLeft = 16, int fallbackRight = 16)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                // https://useyourloaf.com/blog/readable-content-guides/
                var guide = parentView.ReadableContentGuide;
                guide.LeadingAnchor.ConstraintEqualTo(view.LeadingAnchor).Active = true;
                guide.TrailingAnchor.ConstraintEqualTo(view.TrailingAnchor).Active = true;
            }
            else
            {
                StretchWidth(view, parentView, fallbackLeft, fallbackRight);
            }

            return view;
        }

        public static UIView StretchWidth(this UIView view, UIView parentView, float left = 0, float right = 0)
        {
            parentView.AddConstraints(NSLayoutConstraint.FromVisualFormat($"H:|-({left})-[view]-({right})-|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary("view", view)));

            return view;
        }

        public static UIView StretchHeight(this UIView view, UIView parentView, float top = 0, float bottom = 0)
        {
            parentView.AddConstraints(NSLayoutConstraint.FromVisualFormat($"V:|-({top})-[view]-({bottom})-|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary("view", view)));

            return view;
        }

        public static UIView SetWidth(this UIView view, float width)
        {
            // Remove any existing duplicative constraint
            foreach (var c in view.Constraints)
            {
                if (c.FirstAttribute == NSLayoutAttribute.Width && c.Relation == NSLayoutRelation.Equal)
                {
                    view.RemoveConstraint(c);
                }
            }

            view.AddConstraint(NSLayoutConstraint.Create(
                view,
                NSLayoutAttribute.Width,
                NSLayoutRelation.Equal,
                1,
                width));

            return view;
        }

        public static UIView SetHeight(this UIView view, float height)
        {
            // Remove any existing duplicative constraint
            foreach (var c in view.Constraints)
            {
                if (c.FirstAttribute == NSLayoutAttribute.Height && c.Relation == NSLayoutRelation.Equal)
                {
                    view.RemoveConstraint(c);
                }
            }

            view.AddConstraint(NSLayoutConstraint.Create(
                view,
                NSLayoutAttribute.Height,
                NSLayoutRelation.Equal,
                1,
                height));

            return view;
        }

        public static UIView SetMinimumHeight(this UIView view, float minHeight)
        {
            view.AddConstraint(NSLayoutConstraint.Create(
                view,
                NSLayoutAttribute.Height,
                NSLayoutRelation.GreaterThanOrEqual,
                1,
                minHeight));

            return view;
        }

        public static UIView SetMaxWidth(this UIView view, nfloat maxWidth)
        {
            view.AddConstraint(NSLayoutConstraint.Create(
                view,
                NSLayoutAttribute.Width,
                NSLayoutRelation.LessThanOrEqual,
                1,
                maxWidth));

            return view;
        }

        public static UIView WrapInPadding(this UIView view, float left = 0, float top = 0, float right = 0, float bottom = 0)
        {
            view.TranslatesAutoresizingMaskIntoConstraints = false;

            UIView wrapper = new UIView();
            wrapper.Add(view);
            view.StretchWidthAndHeight(wrapper, left, top, right, bottom);
            return wrapper;
        }

        public static void ActuallyRemoveArrangedSubview(this UIStackView stackView, UIView subview)
        {
            // Have to also RemoveFromSuperview since removing arranged doesn't remove the subview, it simply stops controlling the layout
            // https://developer.apple.com/documentation/uikit/uistackview/1616235-removearrangedsubview

            stackView.RemoveArrangedSubview(subview);
            subview.RemoveFromSuperview();
        }

        public static UIColor ToColor(byte[] colorBytes)
        {
            return new UIColor(ToCGColor(colorBytes));
        }

        public static CGColor ToCGColor(byte[] colorBytes)
        {
            if (colorBytes == null)
                return new CGColor(1, 0);

            if (colorBytes.Length == 3)
                return new CGColor(colorBytes[0] / 255f, colorBytes[1] / 255f, colorBytes[2] / 255f);
            else if (colorBytes.Length == 4)
                return new CGColor(colorBytes[1] / 255f, colorBytes[2] / 255f, colorBytes[3] / 255f, colorBytes[0] / 255f);

            return new CGColor(1, 0);
        }

        /// <summary>
        /// Returns an RGB array
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static byte[] ToColorBytes(CGColor color)
        {
            if (color == null)
            {
                return new byte[3];
            }

            if (color.NumberOfComponents == 1)
            {
                return new byte[]
                {
                    (byte)(color.Components[0] * 255),
                    (byte)(color.Components[0] * 255),
                    (byte)(color.Components[0] * 255)
                };
            }

            // We always just want RGB
            return new byte[]
            {
                (byte)(color.Components[0] * 255),
                (byte)(color.Components[1] * 255),
                (byte)(color.Components[2] * 255)
            };
        }

        public static DateTime NSDateToDateTime(NSDate date)
        {
            // https://forums.xamarin.com/discussion/27184/convert-nsdate-to-datetime
            DateTime reference = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return reference.AddSeconds(date.SecondsSinceReferenceDate).ToLocalTime();
        }

        public static NSDate DateTimeToNSDate(DateTime date)
        {
            DateTime reference = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return NSDate.FromTimeIntervalSinceReferenceDate(
                (date.ToUniversalTime() - reference).TotalSeconds);
        }

        public static UIFont WithTraits(this UIFont font, UIFontDescriptorSymbolicTraits traits)
        {
            // https://stackoverflow.com/questions/18862868/setting-bold-font-on-ios-uilabel/21776838#21776838
            var descriptor = font.FontDescriptor.CreateWithTraits(traits);
            return UIFont.FromDescriptor(descriptor, 0);
        }

        public static UIFont Bold(this UIFont font)
        {
            return WithTraits(font, UIFontDescriptorSymbolicTraits.Bold);
        }

        public static IEnumerable<UIView> Descendants(this UIView view)
        {
            foreach (var subview in view.Subviews)
            {
                yield return subview;
                foreach (var descendant in subview.Descendants())
                {
                    yield return descendant;
                }
            }
        }

        public static void ClearAllSubviews(this UIView view)
        {
            foreach (var subview in view.Subviews.ToArray())
            {
                subview.RemoveFromSuperview();
            }
        }

        public static readonly UIColor InputDividerColor = UIColorCompat.SeparatorColor;
        public static readonly UIColor InputSectionDividerColor = UIColorCompat.SystemGroupedBackgroundColor;

        public static void AddDivider(this UIStackView stackView, bool fullWidth = false)
        {
            var divider = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = InputDividerColor
            };
            stackView.AddArrangedSubview(divider);
            divider.StretchWidth(stackView, left: fullWidth ? 0 : 16);
            divider.SetHeight(1f);
        }

        public static void AddSpacing(this UIStackView stackView, float spacingHeight)
        {
            var spacing = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            stackView.AddArrangedSubview(spacing);
            spacing.SetHeight(spacingHeight);
        }

        public static void AddSectionDivider(this UIStackView stackView)
        {
            var divider = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = InputSectionDividerColor
            };
            divider.Layer.BorderColor = InputSectionDividerColor.CGColor;
            divider.Layer.BorderWidth = 1;
            stackView.AddArrangedSubview(divider);
            divider.StretchWidth(stackView, left: -1, right: -1);
            divider.SetHeight(35);
        }

        public static void AddTopSectionDivider(this UIStackView stackView)
        {
            AddSectionDividerWithoutBorder(stackView);
            stackView.AddDivider(fullWidth: true);
        }

        public static void AddBottomSectionDivider(this UIStackView stackView)
        {
            stackView.AddDivider(fullWidth: true);
            AddSectionDividerWithoutBorder(stackView);
        }

        private static void AddSectionDividerWithoutBorder(UIStackView stackView)
        {
            var divider = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = InputSectionDividerColor
            };
            stackView.AddArrangedSubview(divider);
            divider.StretchWidth(stackView);
            divider.SetHeight(34);
        }

        public static void AddUnderVisiblity(this UIStackView stackView, UIView view, Binding.BindingHost bindingHost, string propertyName, bool invert = false)
        {
            BareUIVisibilityContainer container = new BareUIVisibilityContainer()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Child = view
            };
            bindingHost.SetVisibilityBinding(container, propertyName, invert);
            stackView.AddArrangedSubview(container);
            container.StretchWidth(stackView);
        }

        public static void AddUnderLazyVisibility(this UIStackView stackView, Binding.BindingHost bindingHost, string propertyName, Func<UIView> createView)
        {
            BareUIVisibilityContainer container = new BareUIVisibilityContainer()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            bindingHost.SetBinding<bool>(propertyName, (isVisible) =>
            {
                if (isVisible && container.Child == null)
                {
                    container.Child = createView();
                }
            });

            bindingHost.SetVisibilityBinding(container, propertyName);
            stackView.AddArrangedSubview(container);
            container.StretchWidth(stackView);
        }
    }
}