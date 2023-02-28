using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Components
{
    public static class TaskOrEventListItemComponent
    {
        private const string IMAGE_ATTACHMENT_SYMBOL = "\uD83D\uDCF7";

        public static View Render(ViewItemTaskOrEvent Item, BaseMainScreenViewModelDescendant ViewModel, bool IncludeDate = true, bool IncludeClass = true, bool IncludeMargin = true, Action InterceptOnTapped = null)
        {
            if (Item == null || ViewModel == null)
            {
                return null;
            }

            string details = GetDetails(Item);
            var subtitleColor = Item.Class.Color.ToColor().Opacity(Item.IsComplete ? 0.7 : 1);

            return new Border
            {
                Margin = IncludeMargin ? new Thickness(Theme.Current.PageMargin, 3, Theme.Current.PageMargin, 3) : new Thickness(0, 3, 0, 3),
                BackgroundColor = Theme.Current.BackgroundAlt1Color,
                CornerRadius = 4,
                Tapped = () =>
                {
                    if (InterceptOnTapped != null)
                    {
                        InterceptOnTapped();
                    }
                    else
                    {
                        ViewModel.MainScreenViewModel.ShowItem(Item);
                    }
                },
                Content = new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        CompletionBar(Item),

                        new LinearLayout
                        {
                            Margin = new Thickness(6, 3, 0, 5),
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
                                    Text = GetSubtitle(Item, IncludeDate, IncludeClass),
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

                ContextMenu = () => TaskOrEventContextMenu.Generate(Item, ViewModel)
            };
        }

        private static string GetSubtitle(ViewItemTaskOrEvent Item, bool IncludeDate, bool IncludeClass)
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

            if (!IncludeClass || Item.Class == null || Item.Class.IsNoClassClass)
            {
                if (txt.StartsWith(" - "))
                {
                    txt = txt.Substring(" - ".Length);
                }
            }
            else
            {
                txt = Item.Class.Name + txt;
            }

            return txt;
        }

        private static string GetDetails(ViewItemTaskOrEvent Item)
        {
            if (string.IsNullOrWhiteSpace(Item.Details) && !HasImageAttachments(Item))
            {
                return null;
            }

            string details = Item.Details.Replace("\n", "  ").Trim();

            if (HasImageAttachments(Item))
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

        private static bool HasImageAttachments(ViewItemTaskOrEvent Item)
        {
            return Item.ImageNames != null && Item.ImageNames.Length > 0;
        }

        private static View CompletionBar(ViewItemTaskOrEvent item)
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
                        new Border().LinearLayoutWeight((float)item.PercentComplete / 2),

                        new Border
                        {
                            BackgroundColor = item.Class.Color.ToColor()
                        }.LinearLayoutWeight(incomplete),

                        new Border().LinearLayoutWeight((float)item.PercentComplete/ 2)
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
