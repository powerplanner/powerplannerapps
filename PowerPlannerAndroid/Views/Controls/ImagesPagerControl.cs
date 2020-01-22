using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.ViewPager.Widget;
using InterfacesDroid.Views;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.ImageAttachments;

namespace PowerPlannerAndroid.Views.Controls
{
    public class ImagesPagerControl : ViewPager
    {
        public ImagesPagerControl(Context context) : base(context)
        {
            Initialize();
        }

        public ImagesPagerControl(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        private void Initialize()
        {
            this.OffscreenPageLimit = 3;

            this.PageSelected += ImagesPagerControl_PageSelected;

            this.SetBackgroundColor(Color.Black);
        }

        private void ImagesPagerControl_PageSelected(object sender, PageSelectedEventArgs e)
        {

        }

        public void Initialize(ImageAttachmentViewModel currentImage, ImageAttachmentViewModel[] allImages)
        {
            AllImages = allImages;
            CurrentImage = currentImage;

            var adapter = new ImagesPagerAdapter(allImages);
            this.Adapter = adapter;
            this.SetCurrentItem(adapter.GetPosition(currentImage), false);
        }

        public ImageAttachmentViewModel[] AllImages { get; private set; }

        public ImageAttachmentViewModel CurrentImage { get; private set; }

        private class ImagesPagerAdapter : PagerAdapter
        {
            public ImageAttachmentViewModel[] ItemsSource { get; private set; }

            public ImagesPagerAdapter(ImageAttachmentViewModel[] allImages)
            {
                ItemsSource = allImages;
            }

            public override int Count
            {
                get
                {
                    return ItemsSource.Length;
                }
            }

            public override bool IsViewFromObject(View view, Java.Lang.Object objectValue)
            {
                return view == objectValue;
            }

            public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
            {
                var img = GetImage(position);

                // TODO: Someday support recycling views
                View control = null;

                if (control == null)
                {
                    control = new ImageAttachmentZoomableView(container.Context, img);
                }

                container.AddView(control);

                return control;
            }

            // Using a strong reference list so that neither the Java nor the C# object gets disposed (Java shouldn't get disposed
            // since the C# one has a strong reference).
            //private List<SingleDayControl> _destroyedControls = new List<SingleDayControl>();

            public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object objectValue)
            {
                var control = (View)objectValue;
                //control.Deinitialize();
                container.RemoveView(control);
                //_destroyedControls.Add(control);
            }

            public ImageAttachmentViewModel GetImage(int position)
            {
                return ItemsSource[position];
            }

            public int GetPosition(ImageAttachmentViewModel img)
            {
                for (int i = 0; i < ItemsSource.Length; i++)
                {
                    if (ItemsSource[i] == img)
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
    }
}