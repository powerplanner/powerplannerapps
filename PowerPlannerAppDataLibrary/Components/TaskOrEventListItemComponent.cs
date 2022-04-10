using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Components
{
    public class TaskOrEventListItemComponent : VxComponent
    {
        private const string IMAGE_ATTACHMENT_SYMBOL = "\uD83D\uDCF7";

        public ViewItemTaskOrEvent Item { get; set; }
        public BaseMainScreenViewModelDescendant ViewModel { get; set; }
        public bool IncludeDate { get; set; } = true;
        public bool IncludeClass { get; set; } = true;

        protected override View Render()
        {
            string details = GetDetails();
            var subtitleColor = Item.Class.Color.ToColor().Opacity(Item.IsComplete ? 0.7 : 1);

            return new LinearLayout
            {
                Children =
                {
                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            CompletionBar(Item),

                            new LinearLayout
                            {
                                Margin = new Thickness(6,3,0,5),
                                Children =
                                {
                                    new TextBlock
                                    {
                                        Text = Item.Name,
                                        FontWeight = FontWeights.SemiBold,
                                        WrapText = false,
                                        Strikethrough = Item.IsComplete,
                                        TextColor = !Item.IsComplete ? Theme.Current.ForegroundColor : Theme.Current.SubtleForegroundColor
                                    },

                                    new TextBlock
                                    {
                                        Text = GetSubtitle(),
                                        FontWeight = FontWeights.SemiBold,
                                        TextColor = subtitleColor,
                                        WrapText = false
                                    },

                                    !string.IsNullOrWhiteSpace(details) ? new TextBlock
                                    {
                                        Text = details,
                                        WrapText = false,
                                        TextColor = Theme.Current.SubtleForegroundColor
                                    } : null
                                }
                            }.LinearLayoutWeight(1)
                        }
                    },

                    Divider()
                },

                ContextMenu = () => TaskOrEventContextMenu.Generate(Item, ViewModel)
            };
        }

        private string GetSubtitle()
        {
            string txt;

            if (IncludeDate)
            {
                txt = Item.GetType().GetProperty("SubtitleDueDate").GetValue(Item) as string;
            }
            else
            {
                txt = Item.GetType().GetProperty("SubtitleDueTime").GetValue(Item) as string;
            }

            if (!IncludeClass)
            {
                txt = txt.Substring(" - ".Length);
            }
            else
            {
                txt = Item.Class.Name + txt;
            }

            return txt;
        }

        private string GetDetails()
        {
            if (string.IsNullOrWhiteSpace(Item.Details) && !HasImageAttachments())
            {
                return null;
            }

            string details = Item.Details.Replace("\n", "  ").Trim();

            if (HasImageAttachments())
            {
                if (string.IsNullOrWhiteSpace(details))
                    return IMAGE_ATTACHMENT_SYMBOL + " Image Attachment";
                else
                    return IMAGE_ATTACHMENT_SYMBOL + " " + details;
            }
            else
            {
                return details;
            }
        }

        private bool HasImageAttachments()
        {
            return Item.ImageNames != null && Item.ImageNames.Length > 0;
        }

        private View CompletionBar(ViewItemTaskOrEvent item)
        {
            bool isAbsolute = item.IsComplete || item.IsEvent || item.PercentComplete == 0;
            const int width = 8;
            var doneColor = Theme.Current.SubtleForegroundColor.Opacity(0.3);

            if (isAbsolute)
            {
                return new Border
                {
                    Width = width,
                    BackgroundColor = item.IsComplete ? doneColor : item.Class.Color.ToColor()
                };
            }
            else
            {
                float incomplete = (float)(1 - item.PercentComplete);

                return new LinearLayout
                {
                    BackgroundColor = doneColor,
                    Width = width,
                    Children =
                    {
                        new Border().LinearLayoutWeight(incomplete / 2),

                        new Border
                        {
                            BackgroundColor = item.Class.Color.ToColor(),
                            Width = width
                        }.LinearLayoutWeight((float)item.PercentComplete),

                        new Border().LinearLayoutWeight(incomplete / 2)
                    }
                };
            }
        }

        public static View Divider()
        {
            return new Border
            {
                BackgroundColor = Theme.Current.SubtleForegroundColor.Opacity(0.3),
                Height = 1
            };
        }
    }
}
