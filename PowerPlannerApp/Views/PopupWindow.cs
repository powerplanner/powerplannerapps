﻿using MaterialDesign;
using PowerPlannerApp.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vx.Views;
using Xam.Plugin;
using Xamarin.Forms;

namespace PowerPlannerApp.Views
{
    public class PopupWindow : VxComponent
    {
        public string Title { get; set; }

        public new View Content { get; set; }

        public PopupWindowCommand[] Commands { get; set; }

        public PopupWindowCommand[] SecondaryCommands { get; set; }

        public bool AutoScrollAndPad { get; set; }

        protected override View Render()
        {
            return new Grid
            {
                Children =
                {
                    new Xamarin.Forms.Shapes.Rectangle
                    {
                        Fill = new SolidColorBrush(new Color(0, 0, 0, 0.3))
                    }.Tap(() => RemoveThisPage()),

                    new Grid()
                    {
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        MinimumHeightRequest = 300,
                        WidthRequest = 400,
                        BackgroundColor = Color.White,
                        RowDefinitions =
                        {
                            new RowDefinition { Height = GridLength.Auto },
                            new RowDefinition { Height = GridLength.Star }
                        },
                        Children =
                        {
                            RenderTitleBar(),

                            RenderContent().Row(1)
                        }
                    }
                }
            };
        }

        private View RenderContent()
        {
            if (AutoScrollAndPad)
            {
                Content.Margin = new Thickness(20);

                return new ScrollView
                {
                    Content = Content
                };
            }

            else
            {
                return Content;
            }
        }

        private View RenderTitleBar()
        {
            var grid = new Grid
            {
                BackgroundColor = PowerPlannerColors.PowerPlannerBlue,
                HeightRequest = 48,
                ColumnSpacing = 0,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star }
                },
                Children =
                {
                    new Label
                    {
                        TextColor = Color.White,
                        Margin = new Thickness(12, 0, 12, 0),
                        Text = Title,
                        VerticalOptions = LayoutOptions.Center,
                        LineBreakMode = LineBreakMode.NoWrap
                    }
                }
            };

            var cmds = new List<PopupWindowCommand>();

            if (SecondaryCommands != null && SecondaryCommands.Length > 0)
            {
                var popupMenu = new PopupMenu();
                popupMenu.ItemsSource = SecondaryCommands.Select(i => i.ToString()).ToList();
                popupMenu.OnItemSelected += PopupMenu_OnItemSelected;

                cmds.Add(new PopupWindowCommand
                {
                    Glyph = MaterialDesignIcons.MoreVert,
                    Title = "More",
                    Action = delegate
                    {
                        popupMenu.ShowPopup(grid.Children[1]);
                    }
                });
            }

            if (Commands != null)
            {
                cmds.AddRange(Commands);
            }

            cmds.Add(new PopupWindowCommand
            {
                Glyph = MaterialDesignIcons.Close,
                Title = "Close",
                Action = RemoveThisPage
            });

            foreach (var cmd in cmds)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.Children.Add(CreatePrimaryCommand(cmd).Column(grid.ColumnDefinitions.Count - 1));
            }

            return grid;
        }

        private void PopupMenu_OnItemSelected(string item)
        {
            var cmd = SecondaryCommands.First(i => i.ToString() == item);
            cmd.Action();
        }

        private View CreatePrimaryCommand(PopupWindowCommand cmd)
        {
            return new ImageButton
            {
                Source = new FontImageSource()
                {
                    FontFamily = "MaterialIconsOutlined",
                    Glyph = cmd.Glyph,
                    Color = Color.White
                },
                Padding = new Thickness(12, 12, 4, 12),
                Command = CreateCommand(cmd.Action)
            };
        }
    }

    public class PopupWindowCommand
    {
        public string Glyph { get; set; }

        public string Title { get; set; }

        public Action Action { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}