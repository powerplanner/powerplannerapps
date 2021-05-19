using CoreGraphics;
using InterfacesiOS.Controllers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ToolsPortable;
using UIKit;

namespace InterfacesiOS.Views
{
    public class BareUIInlineColorPickerView : UIView
    {
        public event EventHandler<CGColor> SelectionChanged;

        private BareUIInlinePickerView m_picker;
        private ColorItem m_pickCustomColorItem = new ColorItem("Pick Custom Color", new CGColor(0, 0, 0));
        private ColorItem m_customColorItem = new ColorItem("Custom", new CGColor(0, 0, 0));
        private ObservableCollection<ColorItem> m_colorItems;
        private UIViewController m_controller;

        public BareUIInlineColorPickerView(UIViewController controller, int left = 0, int right = 0)
        {
            m_controller = controller;
            m_colorItems = new ObservableCollection<ColorItem>(ColorItem.DefaultColors.Append(m_pickCustomColorItem));

            m_picker = new BareUIInlinePickerView(controller, left: left, right: right)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                HeaderText = "Color",
                ItemToPreviewStringConverter = ConvertColorToPreviewText,
                ItemToViewConverter = ConvertColorToInlineView,
                ItemsSource = m_colorItems
            };
            SelectedColor = m_colorItems.First().Color;
            m_picker.SelectionChanged += M_picker_SelectionChanged;
            m_picker.SelectionChanging += M_picker_SelectionChanging;
            AddSubview(m_picker);
            m_picker.StretchWidth(this);
            m_picker.StretchHeight(this);
        }

        private void M_picker_SelectionChanging(object sender, BareUIInlinePickerView.SelectionChangingEventArgs e)
        {
            if (e.NewSelection == m_pickCustomColorItem)
            {
                e.Cancel = true;
                ShowCustomColorPicker();
            }
        }

        private void M_picker_SelectionChanged(object sender, object e)
        {
            SelectedColor = (m_picker.SelectedItem as ColorItem).Color;
        }

        private ModalCustomColorPickerViewController m_modalCustomPicker;
        private void ShowCustomColorPicker()
        {
            if (m_modalCustomPicker == null)
            {
                m_modalCustomPicker = new ModalCustomColorPickerViewController(HeaderText, m_controller);
                m_modalCustomPicker.OnModalEditSubmitted += new WeakEventHandler(ModalCustomPicker_OnModalEditSubmitted).Handler;
            }
            m_modalCustomPicker.ColorPicker.Color = SelectedColor;
            m_modalCustomPicker.ShowAsModal();
        }

        private void ModalCustomPicker_OnModalEditSubmitted(object sender, EventArgs e)
        {
            SelectedColor = (sender as ModalCustomColorPickerViewController).ColorPicker.Color;
        }

        public string HeaderText
        {
            get { return m_picker.HeaderText; }
            set { m_picker.HeaderText = value; }
        }

        public IEnumerable<ColorItem> AvailableColors
        {
            get { return m_colorItems.Where(i => i != m_pickCustomColorItem && i != m_customColorItem); }
            set
            {
                m_colorItems = new ObservableCollection<ColorItem>(value.Append(m_pickCustomColorItem));

                if (!m_colorItems.Any(i => i.Matches(SelectedColor)))
                {
                    m_customColorItem.Color = SelectedColor;
                    m_colorItems.Insert(m_colorItems.Count - 1, m_customColorItem);
                }

                m_picker.ItemsSource = m_colorItems;
            }
        }

        private CGColor m_selectedColor;
        public CGColor SelectedColor
        {
            get { return m_selectedColor; }
            set
            {
                if (m_selectedColor != value)
                {
                    m_selectedColor = value;

                    ColorItem matching = m_colorItems.FirstOrDefault(i => i.Matches(value));
                    if (matching != null && matching != m_pickCustomColorItem)
                    {
                        m_picker.SelectedItem = matching;
                    }
                    else
                    {
                        var existingCustom = m_colorItems.FirstOrDefault(i => i == m_customColorItem);
                        if (existingCustom != null)
                        {
                            existingCustom.Color = value;
                        }
                        else
                        {
                            m_customColorItem.Color = value;
                            m_colorItems.Insert(m_colorItems.Count - 1, m_customColorItem);
                        }
                        m_picker.SelectedItem = m_customColorItem;
                    }

                    // Update the color display
                    m_picker.LabelDisplayValue.TextColor = new UIColor(m_selectedColor);

                    // Fire selection changed
                    SelectionChanged?.Invoke(this, m_selectedColor);
                }
            }
        }

        private static string ConvertColorToPreviewText(object item)
        {
            var c = item as ColorItem;

            return c.Text;
        }

        private static UIView ConvertColorToInlineView(object item, UIView recycledView)
        {
            var c = item as ColorItem;

            var view = recycledView as BareUIPickerViewItemTextWithColorCircle;
            if (view == null)
            {
                view = new BareUIPickerViewItemTextWithColorCircle(c.Text, c.Color);
                view.BindingHost.SetBinding(nameof(c.Color), delegate
                {
                    view.Color = (view.DataContext as ColorItem)?.Color;
                });
            }
            view.Text = c.Text;
            view.DataContext = c;
            return view;
        }

        public class ColorItem : BindableBase, IEquatable<CGColor>
        {
            public static readonly List<ColorItem> DefaultColors = new List<ColorItem>()
            {
                new ColorItem("Blue", CreateCGColor(27,161,226)),
                new ColorItem("Red", CreateCGColor(229,20,0)),
                new ColorItem("Green", CreateCGColor(51,153,51)),
                new ColorItem("Purple", CreateCGColor(162,0,255)),
                new ColorItem("Pink", CreateCGColor(230,113,184)),
                new ColorItem("Mango", CreateCGColor(240,150,9)),
                new ColorItem("Teal", CreateCGColor(0,171,169)),
                new ColorItem("Lime", CreateCGColor(140,191,38)),
                new ColorItem("Magenta", CreateCGColor(255,0,151)),
                new ColorItem("Brown", CreateCGColor(160,80,0)),
                new ColorItem("Gray", CreateCGColor(75,75,75)),
                new ColorItem("Nokia", CreateCGColor(16,128,221)),
                new ColorItem("HTC", CreateCGColor(105,180,15))
            };

            private static CGColor CreateCGColor(byte red, byte green, byte blue)
            {
                return new CGColor(red / 255f, green / 255f, blue / 255f);
            }

            public string Text { get; set; }

            private CGColor m_color;
            public CGColor Color
            {
                get { return m_color; }
                set { SetProperty(ref m_color, value, nameof(Color)); }
            }

            public ColorItem(string text, CGColor color)
            {
                Text = text;
                Color = color;
            }

            public bool Matches(CGColor color)
            {
                return Color != null && color != null && Color.Components.SequenceEqual(color.Components);
            }

            public bool Equals(CGColor other)
            {
                if (Color == null || other == null)
                    return false;

                return Color.Components.SequenceEqual(other.Components);
            }
        }
    }
}
