using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents;
using PowerPlannerAppDataLibrary.ViewItems;
using InterfacesiOS.Views;
using InterfacesiOS.Binding;
using PowerPlanneriOS.Helpers;
using ToolsPortable;
using InterfacesiOS.Helpers;

namespace PowerPlanneriOS.Controllers
{
    public class ViewTaskOrEventViewController : PopupViewController<ViewTaskOrEventViewModel>
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

        private string StringWithCapitals(string str)
        {
            var words = str.Split(' ');
            for (var i = 0; i < words.Length; i++)
            {
                words[i] = words[i][0].ToString().ToUpper() + words[i].Substring(1).ToLower();
            }
            return string.Join(" ", words);
        }

        public override void OnViewModelLoadedOverride()
        {
            bool isTask = ViewModel.Item.Type == TaskOrEventType.Task;

            BindingHost.SetBinding<string>(nameof(ViewModel.PageTitle), (t) => Title = StringWithCapitals(t));

            var buttonEdit = new UIBarButtonItem(UIBarButtonSystemItem.Edit);
            buttonEdit.Clicked += new WeakEventHandler(delegate { ViewModel.Edit(); }).Handler;

            var buttonMore = new UIBarButtonItem(UIImage.FromBundle("MenuVerticalIcon").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIBarButtonItemStyle.Plain, new WeakEventHandler(ButtonMore_Clicked).Handler);

            NavItem.RightBarButtonItems = new UIBarButtonItem[]
            {
                buttonMore,
                buttonEdit
            };

            int bottomSliderHeight = isTask ? CIRCLE_BUTTON_HEIGHT + 16 + 16 : 0;

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
            _itemBindingHost.SetBinding(nameof(ViewItemTaskOrEvent.Class), delegate
            {
                _classBindingHost.BindingObject = ViewModel.Item.Class;
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
            _itemBindingHost.SetLabelTextBinding(labelSubtitle, nameof(ViewItemTaskOrEvent.Subtitle));
            _classBindingHost.SetBinding<byte[]>(nameof(ViewItemClass.Color), (color) =>
            {
                labelSubtitle.TextColor = BareUIHelper.ToColor(color);
            });
            _stackView.AddArrangedSubview(labelSubtitle);
            labelSubtitle.StretchWidth(_stackView);

            _stackView.AddArrangedSubview(new UIView().SetHeight(4));

            _stackView.AddSpacing(12);

            var textViewDetails = new UITextView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredBody,
                TextColor = UIColorCompat.SecondaryLabelColor,
                Editable = false,
                ScrollEnabled = false,

                // Link detection: http://iosdevelopertips.com/user-interface/creating-clickable-hyperlinks-from-a-url-phone-number-or-address.html
                DataDetectorTypes = UIDataDetectorType.All
            };

            // Lose the padding: https://stackoverflow.com/questions/746670/how-to-lose-margin-padding-in-uitextview
            textViewDetails.TextContainerInset = UIEdgeInsets.Zero;
            textViewDetails.TextContainer.LineFragmentPadding = 0;

            _itemBindingHost.SetTextViewTextBinding(textViewDetails, nameof(ViewItemTaskOrEvent.Details));
            _stackView.AddArrangedSubview(textViewDetails);
            textViewDetails.StretchWidth(_stackView);

            if (ViewModel.IsUnassigedMode)
            {
                var buttonAddGrade = new UIButton(UIButtonType.System)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                buttonAddGrade.SetTitle("Add Grade", UIControlState.Normal);
                buttonAddGrade.SetTitleColor(new UIColor(1, 1), UIControlState.Normal);
                buttonAddGrade.BackgroundColor = ColorResources.PowerPlannerAccentBlue;
                buttonAddGrade.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { ViewModel.AddGrade(); }).Handler;
                base.ContentView.Add(buttonAddGrade);

                // https://stackoverflow.com/questions/46344381/ios-11-layout-guidance-about-safe-area-for-iphone-x
                if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
                {
                    NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[] {
                        buttonAddGrade.LeftAnchor.ConstraintEqualTo(base.ContentView.SafeAreaLayoutGuide.LeftAnchor, 16),
                        buttonAddGrade.RightAnchor.ConstraintEqualTo(base.ContentView.SafeAreaLayoutGuide.RightAnchor, -16),
                        buttonAddGrade.BottomAnchor.ConstraintEqualTo(base.ContentView.SafeAreaLayoutGuide.BottomAnchor, -16)
                    });
                }
                else
                {
                    buttonAddGrade.StretchWidth(base.ContentView, left: 16, right: 16);
                    buttonAddGrade.PinToBottom(base.ContentView, bottom: 16);
                }
            }

            else
            {
                var completionSliderVisibilityContainer = new BareUIVisibilityContainer()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
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
                        _itemBindingHost.SetSliderBinding(_completionSlider, nameof(ViewItemTaskOrEvent.PercentComplete));
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

                    completionSliderContainer.SetHeight(CIRCLE_BUTTON_HEIGHT);

                    completionSliderVisibilityContainer.Child = completionSliderContainer;
                }
                BindingHost.SetVisibilityBinding(completionSliderVisibilityContainer, nameof(ViewModel.IsCompletionSliderVisible));
                base.ContentView.Add(completionSliderVisibilityContainer);

                // https://stackoverflow.com/questions/46344381/ios-11-layout-guidance-about-safe-area-for-iphone-x
                if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
                {
                    NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[] {
                        completionSliderVisibilityContainer.LeftAnchor.ConstraintEqualTo(base.ContentView.SafeAreaLayoutGuide.LeftAnchor, 16),
                        completionSliderVisibilityContainer.RightAnchor.ConstraintEqualTo(base.ContentView.SafeAreaLayoutGuide.RightAnchor, -16),
                        completionSliderVisibilityContainer.BottomAnchor.ConstraintEqualTo(base.ContentView.SafeAreaLayoutGuide.BottomAnchor, -16)
                    });
                }
                else
                {
                    completionSliderVisibilityContainer.StretchWidth(base.ContentView, left: 16, right: 16);
                    completionSliderVisibilityContainer.PinToBottom(base.ContentView, bottom: 16);
                }

                _itemBindingHost.SetBinding(nameof(ViewItemTaskOrEvent.PercentComplete), UpdateSliderImages);
            }

            base.OnViewModelLoadedOverride();
        }

        private void ConfirmDelete()
        {
            PowerPlannerUIHelper.ConfirmDeleteQuick(this, NavItem.RightBarButtonItems.First(), ViewModel.Delete, "Yes, delete");
        }

        private void ButtonMore_Clicked(object sender, EventArgs e)
        {
            // https://developer.xamarin.com/recipes/ios/standard_controls/alertcontroller/#ActionSheet_Alert
            UIAlertController actionSheetMoreOptions = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            actionSheetMoreOptions.AddAction(UIAlertAction.Create(ViewModel.ConvertTypeButtonText, UIAlertActionStyle.Default, delegate { ViewModel.ConvertType(); }));
            actionSheetMoreOptions.AddAction(UIAlertAction.Create("Delete", UIAlertActionStyle.Destructive, delegate { ConfirmDelete(); }));
            actionSheetMoreOptions.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            // Required for iPad - You must specify a source for the Action Sheet since it is
            // displayed as a popover
            UIPopoverPresentationController presentationPopover = actionSheetMoreOptions.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.BarButtonItem = NavItem.RightBarButtonItems.First();
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Display the alert
            this.PresentViewController(actionSheetMoreOptions, true, null);
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