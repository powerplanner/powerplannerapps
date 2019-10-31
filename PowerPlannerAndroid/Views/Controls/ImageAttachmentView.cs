using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using ImageViews.Photo;
using StorageEverywhere;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.ImageAttachments;
using ToolsPortable;

namespace PowerPlannerAndroid.Views.Controls
{
    public class ImageAttachmentThumbnailView : ImageView
    {
        private MyImageAttachmentViewHelper _helper;

        public ImageAttachmentViewModel ViewModel { get; private set; }

        public ImageAttachmentThumbnailView(Context context, ImageAttachmentViewModel viewModel) : base(context)
        {
            ViewModel = viewModel;
            base.SetScaleType(ScaleType.CenterCrop);
            _helper = new MyImageAttachmentViewHelper(this, viewModel);
        }
    }

    public class ImageAttachmentZoomableView : PhotoView
    {
        private MyImageAttachmentViewHelper _helper;
        public ImageAttachmentZoomableView(Context context, ImageAttachmentViewModel viewModel) : base(context)
        {
            _helper = new MyImageAttachmentViewHelper(this, viewModel);
        }
    }

    internal class MyImageAttachmentViewHelper
    {
        private ImageView _imageView;
        private ImageAttachmentViewModel _viewModel;

        public MyImageAttachmentViewHelper(ImageView imageView, ImageAttachmentViewModel viewModel)
        {
            _imageView = imageView;
            _imageView.SetBackgroundColor(Color.Black);
            _viewModel = viewModel;
            viewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;
            viewModel.StartLoad();
            Update();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ImageAttachmentViewModel.Status))
            {
                Update();
            }
        }

        private void Update()
        {
            switch (_viewModel.Status)
            {
                case ImageAttachmentStatus.NotStarted:
                    break;

                case ImageAttachmentStatus.Loaded:
                    DisplayImage(_viewModel.File);
                    break;

                case ImageAttachmentStatus.Downloading:
                    DisplayLoadingImage();
                    break;

                case ImageAttachmentStatus.Offline:
                    DisplayOffline();
                    break;

                default:
                    DisplayNotFound();
                    break;
            }
        }

        private void DisplayLoadingImage()
        {
            _imageView.SetImageResource(Resource.Drawable.ImageLoading);
        }

        private void DisplayImage(IFile file)
        {
            try
            {
                Glide.With(_imageView.Context).Load(file.Path).Into(_imageView);
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void DisplayNotFound()
        {
            _imageView.SetImageResource(Resource.Drawable.ImageNotFound);
        }

        private void DisplayOffline()
        {
            _imageView.SetImageResource(Resource.Drawable.ImageOffline);
        }
    }
}