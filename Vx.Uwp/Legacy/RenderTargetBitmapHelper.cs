using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace InterfacesUWP
{
    public static class RenderTargetBitmapHelper
    {
        public static async Task<InMemoryRandomAccessStream> ExportToStream(UIElement elToRender, Guid encoderId)
        {
            RenderTargetBitmap renderer = new RenderTargetBitmap();
            await renderer.RenderAsync(elToRender);

            IBuffer pixelsBuffer = await renderer.GetPixelsAsync();
            byte[] pixels = new byte[pixelsBuffer.Length];
            DataReader reader = DataReader.FromBuffer(pixelsBuffer);
            reader.ReadBytes(pixels);

            var randomAccessStream = new InMemoryRandomAccessStream();

            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, randomAccessStream);

            encoder.SetPixelData(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Ignore,
                (uint)renderer.PixelWidth,
                (uint)renderer.PixelHeight,
                96,
                96,
                pixels);

            await encoder.FlushAsync();

            randomAccessStream.Seek(0);
            return randomAccessStream;
        }
    }
}
