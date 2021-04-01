using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace InterfacesUWP
{
    public static class ImageDownloader
    {
        public static BitmapImage NewLoadingBitmap()
        {
            return new BitmapImage(new Uri("ms-appx:///Assets/ImageLoading.jpg"));
        }

        public static void SetBitmapToOffline(BitmapImage bmp)
        {
            bmp.UriSource = new Uri("ms-appx:///Assets/ImageOffline.jpg");
        }

        public static void SetBitmapToNotFound(BitmapImage bmp)
        {
            bmp.UriSource = new Uri("ms-appx:///Assets/ImageNotFound.jpg");
        }

        static ImageDownloader()
        { 
            pendingDownloads = new PendingDownloads();
            pendingDownloads.DownloadNeeded += pendingDownloads_DownloadNeeded;
        }

        private static void pendingDownloads_DownloadNeeded(object sender, PendingDownload e)
        {
            download(e);
        }

        private static async void download(PendingDownload pending)
        {
            try
            {
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    foreach (BitmapImage bmp in pending.BitmapImages)
                        SetBitmapToOffline(bmp);
                }

                else
                {
                    HttpWebRequest req = WebRequest.CreateHttp(pending.Url);

                    StorageFile file = await pending.Folder.CreateFileAsync(pending.FileName, CreationCollisionOption.ReplaceExisting);

                    using (WebResponse resp = await req.GetResponseAsync())
                    {
                        using (Stream storageStream = await file.OpenStreamForWriteAsync())
                        {
                            await resp.GetResponseStream().CopyToAsync(storageStream);
                        }
                    }

                    Uri uri = new Uri(file.Path);

                    foreach (BitmapImage bmp in pending.BitmapImages)
                        bmp.UriSource = uri;
                }
            }

            catch (WebException)
            {
                //set it to failed loading bitmap
                foreach (BitmapImage bmp in pending.BitmapImages)
                    SetBitmapToNotFound(bmp);
            }

            catch
            {
                
            }

            //mark it finished, which might trigger loading the next
            pendingDownloads.MarkFinished(pending);
        }

        private static PendingDownloads pendingDownloads;

        public static BitmapImage GetImage(StorageFolder folder, string fileName, string url)
        {
            BitmapImage bitmap = NewLoadingBitmap();

            //purposefully ignoring await, so it runs in background
            GetImagesWithoutBlocking(folder, fileName, url, bitmap);

            return bitmap;
        }

        private static async void GetImagesWithoutBlocking(StorageFolder folder, string fileName, string url, BitmapImage bmp)
        {
            try
            {
                await GetImageAsync(folder, fileName, url, bmp);
            }

            catch { }
        }

        public static async Task GetImageAsync(StorageFolder folder, string fileName, string url, BitmapImage bmp)
        {
            try
            {
                StorageFile file = await folder.GetFileAsync(fileName);

                if (file != null)
                {
                    if (bmp != null)
                        bmp.UriSource = new Uri(file.Path); //Path returns the entire C://Users/Andrew path, but that's fine
                    return;
                }
            }

            catch { }

            //doesn't exist, we need to download it or return already queued one


            //queue it up (handles duplicates)
            pendingDownloads.Queue(folder, fileName, url, bmp);
        }


        private class PendingDownloads
        {
            public event EventHandler<PendingDownload> DownloadNeeded;

            private object _lock = new object();
            private LinkedList<PendingDownload> _list = new LinkedList<PendingDownload>();

            public void Queue(StorageFolder folder, string fileName, string url, BitmapImage bmp)
            {
                PendingDownload pending;
                bool isFirstInQueue = false;

                lock (_lock)
                {
                    PendingDownload existing = _list.FirstOrDefault(i => i.FileName.Equals(fileName) && i.Folder.IsEqual(folder));

                    if (existing != null)
                    {
                        if (bmp != null)
                            existing.BitmapImages.AddLast(bmp);
                        return;
                    }

                    pending = new PendingDownload(folder, fileName, url, bmp);
                    _list.AddLast(pending);

                    if (_list.Count == 1)
                        isFirstInQueue = true;
                }

                //do it outside of lock so we don't keep locking for no reason
                if (isFirstInQueue)
                    sendDownloadedNeededEvent(pending);
            }

            private void sendDownloadedNeededEvent(PendingDownload pending)
            {
                DownloadNeeded(this, pending);
            }

            public void MarkFinished(PendingDownload pending)
            {
                PendingDownload nextPending = null;

                lock (_lock)
                {
                    _list.Remove(pending);

                    nextPending = _list.FirstOrDefault();
                }

                if (nextPending != null)
                    sendDownloadedNeededEvent(nextPending);
            }
        }

        private class PendingDownload
        {
            public StorageFolder Folder { get; private set; }
            public string FileName { get; private set; }
            public string Url { get; private set; }

            public LinkedList<BitmapImage> BitmapImages;

            public PendingDownload(StorageFolder folder, string fileName, string url, BitmapImage bmp)
            {
                Folder = folder;
                FileName = fileName;
                Url = url;

                BitmapImages = new LinkedList<Windows.UI.Xaml.Media.Imaging.BitmapImage>();

                if (bmp != null)
                    BitmapImages.AddFirst(bmp);
            }
        }
    }

    
}
