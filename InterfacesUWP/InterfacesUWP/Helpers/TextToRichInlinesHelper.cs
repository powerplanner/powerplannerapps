using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ToolsPortable.Helpers;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace InterfacesUWP.Helpers
{
    /// <summary>
    /// Detects urls and emails in text, converting it into rich inlines.
    /// </summary>
    public static class TextToRichInlinesHelper
    {
        public static IEnumerable<Inline> Convert(string text, Brush hyperlinkColor = null)
        {
            var portableRuns = LinkDetectionHelper.DetectRuns(text);

            foreach (var run in portableRuns)
            {
                if (run is PortableHyperlinkRun hl)
                {
                    var answer = new Hyperlink()
                    {
                        NavigateUri = hl.Uri,
                        Inlines =
                        {
                            new Run()
                            {
                                Text = hl.Text
                            }
                        }
                    };

                    if (hyperlinkColor != null)
                    {
                        answer.Foreground = hyperlinkColor;
                    }

                    yield return answer;
                }
                else
                {
                    yield return new Run()
                    {
                        Text = run.Text
                    };
                }
            }
        }
    }
}
