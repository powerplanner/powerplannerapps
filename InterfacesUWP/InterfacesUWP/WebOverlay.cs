using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using ToolsUniversal;

namespace InterfacesUWP
{
    public class WebOverlay
    {
        public static async Task<T> Download<K, T>(string message, string url, K postData, ApiKeyCombo apiKey)
        {
            LoadingPopup popup = new LoadingPopup()
            {
                Text = message
            };

            popup.Show();


            try
            {
                return await ToolsPortable.WebHelper.Download<K, T>(url, postData, apiKey);
            }

            finally
            {
                popup.Close();
            }
        }
    }
}
