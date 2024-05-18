using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class ImageView : View
    {
        public ImageSource Source { get; set; }
    }

    public abstract class ImageSource
    {
    }

    public class UriImageSource : ImageSource, IEquatable<UriImageSource>
    {
        public string UwpUri { get; set; }
        public string AndroidUri { get; set; }
        public string IosBundleName { get; set; }
        public string Uri
        {
            get
            {
                switch (VxPlatform.Current)
                {
                    case Platform.Uwp: return UwpUri;
                    case Platform.Android: return AndroidUri;
                    case Platform.iOS: return IosBundleName;
                    default: throw new NotImplementedException();
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Equals(obj as UriImageSource);
        }

        public bool Equals(UriImageSource other)
        {
            if (other == null)
            {
                return false;
            }

            return Uri == other.Uri;
        }

        public override int GetHashCode()
        {
            return Uri.GetHashCode();
        }
    }
}
