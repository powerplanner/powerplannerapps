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

    public UIProgressBarWithIndeterminate()
    {
        progressView = new UIProgressView(UIProgressViewStyle.Default)
        {
            TranslatesAutoresizingMaskIntoConstraints = false
        };

        indeterminateView = new UIView
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            BackgroundColor = foreground,
            Hidden = true
        };

        AddSubview(progressView);
        AddSubview(indeterminateView);

        NSLayoutConstraint.ActivateConstraints(new[]
        {
            progressView.LeadingAnchor.ConstraintEqualTo(LeadingAnchor),
            progressView.TrailingAnchor.ConstraintEqualTo(TrailingAnchor),
            progressView.CenterYAnchor.ConstraintEqualTo(CenterYAnchor),

            indeterminateView.LeadingAnchor.ConstraintEqualTo(LeadingAnchor),
            indeterminateView.TrailingAnchor.ConstraintEqualTo(TrailingAnchor),
            indeterminateView.TopAnchor.ConstraintEqualTo(TopAnchor),
            indeterminateView.BottomAnchor.ConstraintEqualTo(BottomAnchor),
        });

        UpdateProgress();
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
                StartIndeterminateAnimation();
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

    private void StartIndeterminateAnimation()
    {
        indeterminateView.Layer.RemoveAllAnimations();
        var animation = new CABasicAnimation
        {
            KeyPath = "position.x",
            From = NSNumber.FromNFloat(-Frame.Width),
            To = NSNumber.FromNFloat(Frame.Width),
            Duration = 1.2,
            RepeatCount = float.MaxValue,
            AutoReverses = false
        };
        indeterminateView.Layer.AddAnimation(animation, "indeterminate");
    }
}
