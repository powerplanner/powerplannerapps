using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Homework;
using PowerPlannerAppDataLibrary.ViewItems;
using InterfacesiOS.Views;
using InterfacesiOS.Binding;
using PowerPlanneriOS.Helpers;
using ToolsPortable;

namespace PowerPlanneriOS.Controllers
{
    public class ViewHomeworkViewController : PopupViewController<ViewHomeworkViewModel>
    {
        private UIScrollView _scrollView;
        private UIStackView _stackView;

        private BindingHost _itemBindingHost;
        private BindingHost _classBindingHost;
        private UISlider _completionSlider;
        private UIImageView _incompleteImageView;
        private UIImageView _completeImageView;

        // Note that the slider circle is 32.
        private const int CIRCLE_BUTTON_HEIGHT = 48;

        public override void OnViewModelLoadedOverride()
        {
            bool isHomework = ViewModel.Item is ViewItemHomework;

            Title = isHomework ? "View Task" : "View Event";

            var buttonEdit = new UIBarButtonItem(UIBarButtonSystemItem.Edit);
            buttonEdit.Clicked += new WeakEventHandler(delegate { ViewModel.Edit(); }).Handler;

            var buttonDelete = new UIBarButtonItem(UIBarButtonSystemItem.Trash);
            buttonDelete.Clicked += new WeakEventHandler(ButtonDelete_Clicked).Handler;

            NavItem.RightBarButtonItems = new UIBarButtonItem[]
            {
                buttonDelete,
                buttonEdit
            };

            int bottomSliderHeight = isHomework ? CIRCLE_BUTTON_HEIGHT + 16 + 16 : 0;

            _scrollView = new UIScrollView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                ShowsHorizontalScrollIndicator = false
            };
            base.ContentView.AddSubview(_scrollView);
            _scrollView.StretchWidthAndHeight(base.ContentView, bottom: bottomSliderHeight);

            _stackView = new UIStackView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Axis = UILayoutConstraintAxis.Vertical
            };
            _scrollView.AddSubview(_stackView);
            _stackView.ConfigureForVerticalScrolling(_scrollView, top: 16, bottom: 16, left: 16, right: 16);

            _itemBindingHost = new BindingHost()
            {
                BindingObject = ViewModel.Item
            };
            _classBindingHost = new BindingHost();
            _itemBindingHost.SetBinding(nameof(ViewItemHomework.Class), delegate
            {
                _classBindingHost.BindingObject = ViewModel.Item.GetClassOrNull();
            });

            var labelTitle = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredTitle3,
                Lines = 0
            };
            _itemBindingHost.SetLabelTextBinding(labelTitle, nameof(ViewModel.Item.Name));
            _stackView.AddArrangedSubview(labelTitle);
            labelTitle.StretchWidth(_stackView);

            _stackView.AddArrangedSubview(new UIView().SetHeight(4));

            var labelSubtitle = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredSubheadline,
                Lines = 0
            };
            _itemBindingHost.SetLabelTextBinding(labelSubtitle, nameof(ViewItemHomework.Subtitle));
            _classBindingHost.SetBinding<byte[]>(nameof(ViewItemClass.Color), (color) =>
            {
                labelSubtitle.TextColor = BareUIHelper.ToColor(color);
            });
            _stackView.AddArrangedSubview(labelSubtitle);
            labelSubtitle.StretchWidth(_stackView);

            _stackView.AddArrangedSubview(new UIView().SetHeight(4));

            var labelDetails = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredCaption1,
                Lines = 0,
                TextColor = UIColor.DarkGray
            };
            _itemBindingHost.SetLabelTextBinding(labelDetails, nameof(ViewItemHomework.Details));
            _stackView.AddArrangedSubview(labelDetails);
            labelDetails.StretchWidth(_stackView);

            if (ViewModel.IsUnassigedMode)
            {
                var buttonConvertToGrade = new UIButton(UIButtonType.System)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                buttonConvertToGrade.SetTitle("Convert To Grade", UIControlState.Normal);
                buttonConvertToGrade.SetTitleColor(new UIColor(1, 1), UIControlState.Normal);
                buttonConvertToGrade.BackgroundColor = ColorResources.PowerPlannerAccentBlue;
                buttonConvertToGrade.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { ViewModel.ConvertToGrade(); }).Handler;
                base.ContentView.Add(buttonConvertToGrade);

                // https://stackoverflow.com/questions/46344381/ios-11-layout-guidance-about-safe-area-for-iphone-x
                if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
                {
                    NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[] {
                        buttonConvertToGrade.LeftAnchor.ConstraintEqualTo(base.ContentView.SafeAreaLayoutGuide.LeftAnchor, 16),
                        buttonConvertToGrade.RightAnchor.ConstraintEqualTo(base.ContentView.SafeAreaLayoutGuide.RightAnchor, -16),
                        buttonConvertToGrade.BottomAnchor.ConstraintEqualTo(base.ContentView.SafeAreaLayoutGuide.BottomAnchor, -16)
                    });
                }
                else
                {
                    buttonConvertToGrade.StretchWidth(base.ContentView, left: 16, right: 16);
                    buttonConvertToGrade.PinToBottom(base.ContentView, bottom: 16);
                }
            }

            else if (isHomework)
            {
                var completionSliderContainer = new UIView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                {
                    _completionSlider = new UISlider()
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        MaxValue = 1,
                        MinValue = 0,
                        MinimumTrackTintColor = UIColor.FromRGB(42 / 255f, 222 / 255f, 42 / 255f),
                        ThumbTintColor = UIColor.FromRGB(42 / 255f, 222 / 255f, 42 / 255f)

                    };
                    _itemBindingHost.SetSliderBinding(_completionSlider, nameof(ViewItemHomework.PercentComplete));
                    _completionSlider.TouchUpInside += new WeakEventHandler(CompletionSlider_ValueCommitted).Handler;
                    _completionSlider.TouchUpOutside += new WeakEventHandler(CompletionSlider_ValueCommitted).Handler;
                    completionSliderContainer.Add(_completionSlider);
                    _completionSlider.StretchHeight(completionSliderContainer);
                    _completionSlider.StretchWidth(completionSliderContainer, left: CIRCLE_BUTTON_HEIGHT + 8, right: CIRCLE_BUTTON_HEIGHT + 8);

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
                    incompleteImageContainer.TouchUpInside += new WeakEventHandler(delegate { _completionSlider.Value = 0; ViewModel.SetPercentComplete(0); UpdateSliderImages(); }).Handler;
                    completionSliderContainer.Add(incompleteImageContainer);
                    incompleteImageContainer.StretchHeight(completionSliderContainer);
                    incompleteImageContainer.PinToLeft(completionSliderContainer);
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
                    completeImageContainer.TouchUpInside += new WeakEventHandler(delegate { _completionSlider.Value = 1; ViewModel.SetPercentComplete(1); UpdateSliderImages(); }).Handler;
                    completionSliderContainer.Add(completeImageContainer);
                    completeImageContainer.StretchHeight(completionSliderContainer);
                    completeImageContainer.PinToRight(completionSliderContainer);
                    completeImageContainer.SetWidth(CIRCLE_BUTTON_HEIGHT);

                    _completionSlider.ValueChanged += new WeakEventHandler(delegate { UpdateSliderImages(); }).Handler;
                }
                base.ContentView.Add(completionSliderContainer);
                completionSliderContainer.SetHeight(CIRCLE_BUTTON_HEIGHT);

                // https://stackoverflow.com/questions/46344381/ios-11-layout-guidance-about-safe-area-for-iphone-x
                if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
                {
                    NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[] {
                        completionSliderContainer.LeftAnchor.ConstraintEqualTo(base.ContentView.SafeAreaLayoutGuide.LeftAnchor, 16),
                        completionSliderContainer.RightAnchor.ConstraintEqualTo(base.ContentView.SafeAreaLayoutGuide.RightAnchor, -16),
                        completionSliderContainer.BottomAnchor.ConstraintEqualTo(base.ContentView.SafeAreaLayoutGuide.BottomAnchor, -16)
                    });
                }
                else
                {
                    completionSliderContainer.StretchWidth(base.ContentView, left: 16, right: 16);
                    completionSliderContainer.PinToBottom(base.ContentView, bottom: 16);
                }

                _itemBindingHost.SetBinding(nameof(ViewItemHomework.PercentComplete), UpdateSliderImages);
            }

            base.OnViewModelLoadedOverride();
        }

        private void ButtonDelete_Clicked(object sender, EventArgs e)
        {
            PowerPlannerUIHelper.ConfirmDeleteQuick(this, NavItem.RightBarButtonItems.First(), ViewModel.Delete);
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
            ViewModel.SetPercentComplete((sender as UISlider).Value);
        }
    }
}