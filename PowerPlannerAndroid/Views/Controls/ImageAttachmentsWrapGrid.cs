using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Adapters;
using InterfacesDroid.DataTemplates;
using InterfacesDroid.Themes;
using InterfacesDroid.Views;
using PowerPlannerAndroid.Helpers;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.ImageAttachments;
using ToolsPortable;

namespace PowerPlannerAndroid.Views.Controls
{
    public class EditingImageAttachmentsWrapGrid : GridViewLayout
    {
        public event EventHandler RequestedAddImage;
        private ItemsControlWrapper _itemsControlWrapper;
        private const string AddImageConstant = "AddImage";

        public EditingImageAttachmentsWrapGrid(Context context) : base(context)
        {
            Initialize();
        }

        public EditingImageAttachmentsWrapGrid(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        private void Initialize()
        {
            _itemsControlWrapper = new ItemsControlWrapper(this)
            {
                ItemTemplate = new CustomDataTemplate<object>(CreateImageView)
            };
            ImageAttachmentsWrapGrid.SharedInitialize(this);
        }

        private IEnumerable<BaseEditingImageAttachmentViewModel> _imageAttachments = new BaseEditingImageAttachmentViewModel[0];
        public IEnumerable<BaseEditingImageAttachmentViewModel> ImageAttachments
        {
            get { return _imageAttachments; }
            set
            {
                if (object.ReferenceEquals(value, _imageAttachments))
                {
                    return;
                }

                if (value == null)
                {
                    value = new BaseEditingImageAttachmentViewModel[0];
                }

                _imageAttachments = value;
                _itemsControlWrapper.ItemsSource = new MyAppendedObservableLists<object>(ImageAttachments, new string[] { AddImageConstant });
            }
        }

        private View CreateImageView(ViewGroup parent, object imageObj)
        {
            View answer;
            if (imageObj is string strConst && strConst == AddImageConstant)
            {
                answer = new FrameLayout(parent.Context);
                answer.SetBackgroundColor(Color.Black);

                var plusView = new ImageView(parent.Context);
                plusView.SetImageResource(Android.Resource.Drawable.IcInputAdd);
                plusView.SetColorFilter(Color.White);
                plusView.LayoutParameters = new FrameLayout.LayoutParams(ThemeHelper.AsPx(parent.Context, 30), ThemeHelper.AsPx(parent.Context, 30), GravityFlags.Center);
                (answer as FrameLayout).AddView(plusView);
                answer.Click += new WeakEventHandler(AddImage_Click).Handler;
            }
            else if (imageObj is BaseEditingImageAttachmentViewModel editingViewModel)
            {
                var thumbnail = new ImageAttachmentThumbnailView(parent.Context, editingViewModel.ImageAttachment)
                {
                    LayoutParameters = new FrameLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
                };
                thumbnail.Click += new WeakEventHandler(Thumbnail_Click).Handler;

                FrameLayout imageAndDeleteButtonFrame = new FrameLayout(parent.Context);
                imageAndDeleteButtonFrame.AddView(thumbnail);

                int deleteButtonSize = ThemeHelper.AsPx(parent.Context, 48);
                int deleteButtonMargin = ThemeHelper.AsPx(parent.Context, 6);
                var deleteButton = new RemoveAttachmentButton(parent.Context, editingViewModel)
                {
                    LayoutParameters = new FrameLayout.LayoutParams(deleteButtonSize, deleteButtonSize, GravityFlags.Top | GravityFlags.Right)
                    {
                        TopMargin = deleteButtonMargin,
                        RightMargin = deleteButtonMargin,
                        LeftMargin = deleteButtonMargin
                    }
                };
                imageAndDeleteButtonFrame.AddView(deleteButton);

                answer = imageAndDeleteButtonFrame;
            }
            else
            {
                return null;
            }
            return ImageAttachmentsWrapGrid.SharedWrapImageView(answer);
        }

        private class RemoveAttachmentButton : ImageButton
        {
            private BaseEditingImageAttachmentViewModel _viewModel;

            public RemoveAttachmentButton(Context context, BaseEditingImageAttachmentViewModel viewModel) : base(context)
            {
                _viewModel = viewModel;

                this.SetBackgroundResource(Resource.Drawable.circle);
                if (Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    this.BackgroundTintList = new Android.Content.Res.ColorStateList(new int[][] { new int[0] }, new int[] { new Color(0, 0, 0, (int)(255 * 0.4)) });
                }
                this.SetImageResource(Resource.Drawable.ic_close_white_36dp);
                this.Click += new WeakEventHandler(RemoveAttachmentButton_Click).Handler;
            }

            private void RemoveAttachmentButton_Click(object sender, EventArgs e)
            {
                try
                {
                    _viewModel.RemoveThisImageAttachment();
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }
        }

        private void Thumbnail_Click(object sender, EventArgs e)
        {
            ImageAttachmentsWrapGrid.HandleImageClick(sender as ImageAttachmentThumbnailView, ImageAttachments.Select(i => i.ImageAttachment));
        }

        private void AddImage_Click(object sender, EventArgs e)
        {
            try
            {
                RequestedAddImage?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }

    public class ImageAttachmentsWrapGrid : GridViewLayout
    {
        private ItemsControlWrapper _itemsControlWrapper;

        public ImageAttachmentsWrapGrid(Context context) : base(context)
        {
            Initialize();
        }

        public ImageAttachmentsWrapGrid(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        private void Initialize()
        {
            _itemsControlWrapper = new ItemsControlWrapper(this)
            {
                ItemTemplate = new CustomDataTemplate<ImageAttachmentViewModel>(CreateImageView)
            };
            SharedInitialize(this);
        }

        internal static void SharedInitialize(GridViewLayout gridViewLayout)
        {
            gridViewLayout.SetColumnWidth(ThemeHelper.AsPx(gridViewLayout.Context, 103));
            gridViewLayout.UseDefaultMargins = true;
            int negPadding = ThemeHelper.AsPx(gridViewLayout.Context, -3);
            gridViewLayout.SetPadding(negPadding, negPadding, negPadding, negPadding);
        }

        private IEnumerable<ImageAttachmentViewModel> _imageAttachments = new ImageAttachmentViewModel[0];
        public IEnumerable<ImageAttachmentViewModel> ImageAttachments
        {
            get { return _imageAttachments; }
            set
            {
                if (value == null)
                {
                    value = new ImageAttachmentViewModel[0];
                }

                _imageAttachments = value;
                _itemsControlWrapper.ItemsSource = value;
            }
        }

        private View CreateImageView(ViewGroup parent, ImageAttachmentViewModel imageObj)
        {
            View answer;
            {
                answer = new ImageAttachmentThumbnailView(parent.Context, imageObj as ImageAttachmentViewModel);
                answer.Click += new WeakEventHandler(Image_Click).Handler;
            }
            return SharedWrapImageView(answer);
        }

        internal static View SharedWrapImageView(View view)
        {
            FrameLayout wrapper = new FrameLayout(view.Context);
            int padding = ThemeHelper.AsPx(view.Context, 3);
            wrapper.SetPadding(padding, padding, padding, padding);
            view.LayoutParameters = new ViewGroup.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
            wrapper.AddView(view);
            wrapper.LayoutParameters = new ViewGroup.LayoutParams(ThemeHelper.AsPx(view.Context, 103), ThemeHelper.AsPx(view.Context, 103));
            return wrapper;
        }

        internal static void HandleImageClick(ImageAttachmentThumbnailView thumbnailView, IEnumerable<ImageAttachmentViewModel> imageAttachments)
        {
            try
            {
                ImageAttachmentViewModel img = thumbnailView.ViewModel;

                PowerPlannerApp.Current.ShowImage(img, imageAttachments.ToArray());
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void Image_Click(object sender, EventArgs e)
        {
            HandleImageClick(sender as ImageAttachmentThumbnailView, ImageAttachments);
        }
    }
}