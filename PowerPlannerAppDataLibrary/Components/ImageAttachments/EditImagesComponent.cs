using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.ImageAttachments;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Components.ImageAttachments
{
    internal class EditImagesComponent : VxComponent
    {
        public ObservableCollection<BaseEditingImageAttachmentViewModel> ImageAttachments { get; set; }

        public Action RequestAddImage { get; set; }

        protected override View Render()
        {
            SubscribeToCollection(ImageAttachments);

            var wrapGrid = ImagesComponent.GenerateWrapGrid();
            var itemMargin = new Thickness(ImagesComponent.ItemSpacing / 2);

            foreach (var a in ImageAttachments)
            {
                wrapGrid.Children.Add(new FrameLayout
                {
                    Children =
                    {
                        new ImageComponent
                        {
                            ImageAttachment = a.ImageAttachment,
                            IsThumbnail = true,
                            DontIncludeMargin = true
                        },

                        new TransparentContentButton
                        {
                            Content = new Border
                            {
                                BackgroundColor = Color.Black.Opacity(0.2),
                                Content = new FontIcon
                                {
                                    Glyph = MaterialDesign.MaterialDesignIcons.Delete,
                                    Color = Color.White,
                                    FontSize = Theme.Current.TitleFontSize,
                                    Margin = new Thickness(6)
                                }
                            },
                            Click = () => a.RemoveThisImageAttachment(),
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Top
                        }
                    },
                    Margin = itemMargin
                });
            }

            wrapGrid.Children.Add(new Border
            {
                BackgroundColor = Color.Black,
                Content = new FontIcon
                {
                    Glyph = MaterialDesign.MaterialDesignIcons.Add,
                    Color = Color.White,
                    FontSize = Theme.Current.TitleFontSize,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                },
                Tapped = RequestAddImage,
                Margin = itemMargin
            });

            return wrapGrid;
        }
    }
}
