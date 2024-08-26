using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.ImageAttachments;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Vx;
using Vx.Views;
using static System.Net.Mime.MediaTypeNames;

namespace PowerPlannerAppDataLibrary.Components.ImageAttachments
{
    internal class ImagesComponent : VxComponent
    {
        public ImageAttachmentViewModel[] ImageAttachments { get; set; }

        internal const float ItemSpacing = 8;

        protected override View Render()
        {
            if (ImageAttachments == null || ImageAttachments.Length == 0)
            {
                return null;
            }

            float ITEM_SIZE = 120 + ItemSpacing;

            var frame = new WrapGrid
            {
                Margin = new Thickness(ItemSpacing / -2),
                ItemHeight = ITEM_SIZE,
                ItemWidth = ITEM_SIZE
            };

            for (int i = 0; i < ImageAttachments.Length; i++)
            {
                frame.Children.Add(new Border
                {
                    BackgroundColor = Color.Black,
                    Margin = new Thickness(i * ITEM_SIZE + i * ItemSpacing, 0, 0, 0),
                    Content = new TextBlock
                    {
                        Text = "hi",
                        TextColor = Color.White
                    }
                });
            }
            return frame;

            var wrapGrid = new WrapGrid
            {
                ItemWidth = ITEM_SIZE,
                ItemHeight = ITEM_SIZE,
                Margin = new Thickness(ItemSpacing / -2)
            };

            if (VxPlatform.Current == Platform.Uwp)
            {
                // UWP displays thumbnails at 190x130, so maintain that aspect ratio (and accomodate for 
                wrapGrid.ItemWidth = 152 + ItemSpacing;
                wrapGrid.ItemHeight = 104 + ItemSpacing;
            }

            foreach (var a in ImageAttachments)
            {
                wrapGrid.Children.Add(new ImagePreviewComponent
                {
                    ImageAttachment = a,
                    AllImageAttachments = ImageAttachments
                });
            }

            return wrapGrid;
        }

        private class ImagePreviewComponent : VxComponent
        {
            [VxSubscribe]
            public ImageAttachmentViewModel ImageAttachment { get; set; }

            public ImageAttachmentViewModel[] AllImageAttachments { get; set; }

            protected override void Initialize()
            {
                ImageAttachment.StartLoad();
            }

            protected override View Render()
            {
                // Ensure this is called for when ImageAttachment changes
                ImageAttachment.StartLoad();

                var margin = new Thickness(ImagesComponent.ItemSpacing / 2);

                if (ImageAttachment.Status == Helpers.ImageAttachmentStatus.Loaded && false)
                {
                    return new Border
                    {
                        BackgroundColor = Color.Black,
                        Content = new ImageView
                        {
                            Source = UriImageSource.FromFilePath(ImageAttachment.File.Path),
                            UseFilePictureViewThumbnail = true
                        },
                        Tapped = delegate
                        {
                            PowerPlannerApp.Current.ShowImage(ImageAttachment, AllImageAttachments);
                        },
                        Margin = margin
                    };
                }

                return new Border
                {
                    BackgroundColor = Color.Black,
                    Content = new FontIcon
                    {
                        Glyph = GetGlyph(ImageAttachment.Status),
                        Color = Color.White,
                        FontSize = 18,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    Margin = margin
                };
            }

            private string GetGlyph(Helpers.ImageAttachmentStatus status)
            {
                switch (status)
                {
                    case Helpers.ImageAttachmentStatus.NotFound:
                        return MaterialDesign.MaterialDesignIcons.Error;

                    case Helpers.ImageAttachmentStatus.Offline:
                        return MaterialDesign.MaterialDesignIcons.WifiOff;

                    case Helpers.ImageAttachmentStatus.Downloading:
                        return MaterialDesign.MaterialDesignIcons.Downloading;

                    case Helpers.ImageAttachmentStatus.NotStarted:
                        return "";

                    default:
                        return MaterialDesign.MaterialDesignIcons.Error;
                }
            }
        }
    }
}
