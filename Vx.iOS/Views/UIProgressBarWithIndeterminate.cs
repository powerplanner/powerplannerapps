using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

public class UIProgressBarWithIndeterminate : UIView
{
    private UIProgressView progressView;
    private UIView indeterminateView;
    private bool isIndeterminate;
    private double value;
    private double maximum = 100;
    private UIColor foreground = UIColor.Blue;
    private nfloat animatedWidth;

    public UIProgressBarWithIndeterminate()
    {
        ClipsToBounds = true;
        progressView = new UIProgressView(UIProgressViewStyle.Default);

        indeterminateView = new UIView
        {
            BackgroundColor = foreground,
            Hidden = true
        };

        AddSubview(progressView);
        AddSubview(indeterminateView);

        UpdateProgress();
    }

    public override void LayoutSubviews()
    {
        base.LayoutSubviews();

        nfloat progressHeight = progressView.IntrinsicContentSize.Height;
        if (progressHeight <= 0)
        {
            progressHeight = 4;
        }

        progressView.Frame = new CGRect(0, (Bounds.Height - progressHeight) / 2, Bounds.Width, progressHeight);
        nfloat segmentWidth = Bounds.Width / 3;
        indeterminateView.Frame = new CGRect(-segmentWidth, (Bounds.Height - progressHeight) / 2, segmentWidth, progressHeight);
        UpdateIndeterminateAnimation();
    }

    public override void MovedToWindow()
    {
        base.MovedToWindow();

        if (Window == null)
        {
            indeterminateView.Layer.RemoveAnimation("indeterminate");
        }
        else
        {
            SetNeedsLayout();
        }
    }

    /// <summary>
    /// Reports a thin natural height (matching the underlying UIProgressView) so the manual Vx
    /// layout engine doesn't collapse this control to zero height. Width stretches to the parent.
    /// </summary>
    public override CGSize SizeThatFits(CGSize size)
    {
        nfloat height = progressView.IntrinsicContentSize.Height;
        if (height <= 0)
        {
            height = 4;
        }

        // Stretch to the available width; report 0 when unbounded so we never inflate a parent.
        nfloat width = (size.Width > 0 && size.Width < Vx.iOS.Views.UIViewWrapper.UnboundedSize) ? size.Width : 0;
        return new CGSize(width, height);
    }

    public override CGSize IntrinsicContentSize
    {
        get
        {
            nfloat height = progressView.IntrinsicContentSize.Height;
            if (height <= 0)
            {
                height = 4;
            }
            return new CGSize(NoIntrinsicMetric, height);
        }
    }

    public bool IsIndeterminate
    {
        get => isIndeterminate;
        set
        {
            if (isIndeterminate == value) return;
            isIndeterminate = value;
            progressView.Hidden = isIndeterminate;
            indeterminateView.Hidden = !isIndeterminate;
            if (isIndeterminate)
            {
                SetNeedsLayout();
            }
            else
            {
                indeterminateView.Layer.RemoveAnimation("indeterminate");
                UpdateProgress();
            }
        }
    }

    public double Value
    {
        get => value;
        set
        {
            this.value = Math.Max(0, Math.Min(value, maximum));
            UpdateProgress();
        }
    }

    public double Maximum
    {
        get => maximum;
        set
        {
            if (value <= 0) return;
            maximum = value;
            UpdateProgress();
        }
    }

    public UIColor Foreground
    {
        get => foreground;
        set
        {
            foreground = value;
            progressView.ProgressTintColor = foreground;
            indeterminateView.BackgroundColor = foreground;
        }
    }

    private void UpdateProgress()
    {
        if (!isIndeterminate)
        {
            progressView.Progress = (float)(value / maximum);
        }
    }

    private void UpdateIndeterminateAnimation()
    {
        if (!isIndeterminate || Window == null || Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            indeterminateView.Layer.RemoveAnimation("indeterminate");
            return;
        }

        using var existingAnimation = indeterminateView.Layer.AnimationForKey("indeterminate");
        if (existingAnimation != null && animatedWidth == Bounds.Width)
        {
            return;
        }

        // Properties can be applied before layout; use the measured width, not the initial zero frame.
        animatedWidth = Bounds.Width;
        nfloat halfSegmentWidth = indeterminateView.Bounds.Width / 2;
        using var animation = new CABasicAnimation
        {
            KeyPath = "position.x",
            From = NSNumber.FromNFloat(-halfSegmentWidth),
            To = NSNumber.FromNFloat(Bounds.Width + halfSegmentWidth),
            Duration = 1.2,
            RepeatCount = float.MaxValue,
            AutoReverses = false
        };
        indeterminateView.Layer.AddAnimation(animation, "indeterminate");
    }
}
