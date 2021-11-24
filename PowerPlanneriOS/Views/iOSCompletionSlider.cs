using System;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.Views;
using ToolsPortable;
using UIKit;

namespace PowerPlanneriOS.Views
{
    public class iOSCompletionSlider : Vx.iOS.Views.iOSView<CompletionSlider, UIView>
    {
        private UISlider _completionSlider;
        private UIImageView _incompleteImageView;
        private UIImageView _completeImageView;

        // Note that the slider circle is 32.
        private const int CIRCLE_BUTTON_HEIGHT = 48;

        public iOSCompletionSlider()
        {
            View.SetHeight(CIRCLE_BUTTON_HEIGHT);

            _completionSlider = new UISlider()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                MaxValue = 1,
                MinValue = 0,
                MinimumTrackTintColor = UIColor.FromRGB(42 / 255f, 222 / 255f, 42 / 255f),
                ThumbTintColor = UIColor.FromRGB(42 / 255f, 222 / 255f, 42 / 255f)

            };
            _completionSlider.TouchUpInside += new WeakEventHandler(CompletionSlider_ValueCommitted).Handler;
            _completionSlider.TouchUpOutside += new WeakEventHandler(CompletionSlider_ValueCommitted).Handler;
            View.Add(_completionSlider);
            _completionSlider.StretchHeight(View);
            _completionSlider.StretchWidth(View, left: CIRCLE_BUTTON_HEIGHT + 8, right: CIRCLE_BUTTON_HEIGHT + 8);

            var incompleteImageContainer = new UIControl()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            {
                _incompleteImageView = new UIImageView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    ContentMode = UIViewContentMode.ScaleAspectFit,
                    TintColor = UIColor.LightGray
                };
                incompleteImageContainer.Add(_incompleteImageView);
                _incompleteImageView.StretchHeight(incompleteImageContainer);
                _incompleteImageView.SetWidth(CIRCLE_BUTTON_HEIGHT);
                _incompleteImageView.PinToLeft(incompleteImageContainer);
            }
            incompleteImageContainer.TouchUpInside += new WeakEventHandler(delegate { _completionSlider.Value = 0; ReportValueChanged(0); UpdateSliderImages(); }).Handler;
            View.Add(incompleteImageContainer);
            incompleteImageContainer.StretchHeight(View);
            incompleteImageContainer.PinToLeft(View);
            incompleteImageContainer.SetWidth(CIRCLE_BUTTON_HEIGHT);

            var completeImageContainer = new UIControl()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            {
                _completeImageView = new UIImageView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    ContentMode = UIViewContentMode.ScaleAspectFit,
                    TintColor = UIColor.LightGray
                };
                completeImageContainer.Add(_completeImageView);
                _completeImageView.StretchHeight(completeImageContainer);
                _completeImageView.SetWidth(CIRCLE_BUTTON_HEIGHT);
                _completeImageView.PinToRight(completeImageContainer);
            }
            completeImageContainer.TouchUpInside += new WeakEventHandler(delegate { _completionSlider.Value = 1; ReportValueChanged(1); UpdateSliderImages(); }).Handler;
            View.Add(completeImageContainer);
            completeImageContainer.StretchHeight(View);
            completeImageContainer.PinToRight(View);
            completeImageContainer.SetWidth(CIRCLE_BUTTON_HEIGHT);

            _completionSlider.ValueChanged += new WeakEventHandler(delegate { UpdateSliderImages(); }).Handler;
        }

        private UIImage _sliderImageIncomplete;
        private UIImage GetSliderImageIncomplete() { return GetImageHelper("SliderIncomplete", ref _sliderImageIncomplete, true); }
        private UIImage _sliderImageIncompleteFilled;
        private UIImage GetSliderImageIncompleteFilled() { return GetImageHelper("SliderIncompleteFilled", ref _sliderImageIncompleteFilled, false); }
        private UIImage _sliderImageComplete;
        private UIImage GetSliderImageComplete() { return GetImageHelper("SliderComplete", ref _sliderImageComplete, true); }
        private UIImage _sliderImageCompleteFilled;
        private UIImage GetSliderImageCompleteFilled() { return GetImageHelper("SliderCompleteFilled", ref _sliderImageCompleteFilled, false); }
        private void UpdateSliderImages()
        {
            if (_completionSlider.Value == 0)
            {
                SetMinValueImage(GetSliderImageIncompleteFilled());
                SetMaxValueImage(GetSliderImageComplete());
            }

            else if (_completionSlider.Value < 1)
            {
                SetMinValueImage(GetSliderImageIncomplete());
                SetMaxValueImage(GetSliderImageComplete());
            }

            else
            {
                SetMinValueImage(GetSliderImageIncomplete());
                SetMaxValueImage(GetSliderImageCompleteFilled());
            }
        }

        private UIImage GetImageHelper(string imageBundleName, ref UIImage storage, bool useTintColor)
        {
            if (storage == null)
            {
                storage = UIImage.FromBundle(imageBundleName);
                if (useTintColor)
                {
                    storage = storage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                }
            }
            return storage;
        }

        private void SetMaxValueImage(UIImage img)
        {
            if (_completeImageView.Image != img)
            {
                _completeImageView.Image = img;
            }
        }

        private void SetMinValueImage(UIImage img)
        {
            if (_incompleteImageView.Image != img)
            {
                _incompleteImageView.Image = img;
            }
        }

        private void CompletionSlider_ValueCommitted(object sender, EventArgs e)
        {
            ReportValueChanged((sender as UISlider).Value);
            UpdateSliderImages();
        }

        private void ReportValueChanged(float newValue)
        {
            if (VxView?.PercentComplete != null && VxView.PercentComplete.Value != newValue)
            {
                VxView.PercentComplete.ValueChanged?.Invoke(newValue);
            }
        }

        protected override void ApplyProperties(CompletionSlider oldView, CompletionSlider newView)
        {
            base.ApplyProperties(oldView, newView);

            _completionSlider.Value = (float)(newView.PercentComplete?.Value ?? 0);
            UpdateSliderImages();
        }
    }
}
