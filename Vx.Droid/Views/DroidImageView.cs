using Android.App;
using Android.Content.Res;
using Android.Widget;
using Bumptech.Glide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace Vx.Droid.Views
{
    internal class DroidImageView : DroidView<Vx.Views.ImageView, ImageView>
    {
        public DroidImageView() : base(new ImageView(VxDroidExtensions.ApplicationContext))
        {
        }

        protected override void ApplyProperties(Vx.Views.ImageView oldView, Vx.Views.ImageView newView)
        {
            base.ApplyProperties(oldView, newView);

            if (!object.Equals(newView.UseFilePictureViewThumbnail, oldView?.UseFilePictureViewThumbnail))
            {
                if (newView.UseFilePictureViewThumbnail)
                {
                    View.SetScaleType(ImageView.ScaleType.CenterCrop);
                }
                else
                {
                    View.SetScaleType(ImageView.ScaleType.CenterInside);
                }
            }

            if (!object.Equals(newView.Source, oldView?.Source))
            {
                SetImageSource(View, newView.Source);
            }
        }

        internal static void SetImageSource(ImageView view, Vx.Views.ImageSource source)
        {
            if (source is Vx.Views.UriImageSource uriImageSource)
            {
                if (uriImageSource.AndroidUri != null)
                {
                    try
                    {
                        // Need to resolve any relative ".."'s from the path, otherwise Glide won't display the image
                        var path = System.IO.Path.GetFullPath(uriImageSource.AndroidUri);

                        // And then display the image
                        Glide.With(view.Context).Load(path).Into(view);
                    }
                    catch (Exception ex)
                    {
                        ExceptionHelper.ReportHandledException(ex);
                        view.SetImageDrawable(null);
                    }
                }
                else if (uriImageSource.AndroidResourceName != null)
                {
                    try
                    {
                        // Untested code, I haven't used resource IDs yet
                        // https://nsian.medium.com/use-variables-for-resource-id-in-android-5e64bba59284
                        var resId = Application.Context.Resources.GetIdentifier(uriImageSource.AndroidResourceName, "drawable", null);
                        if (resId != 0)
                        {
                            view.SetImageResource(resId);
                        }
                        else
                        {
                            view.SetImageDrawable(null);
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionHelper.ReportHandledException(ex);
                        view.SetImageDrawable(null);
                    }
                }
                else
                {
                    view.SetImageDrawable(null);
                }
            }
            else
            {
                view.SetImageDrawable(null);
            }
        }
    }
}
