using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using static Xamarin.Forms.Button;

namespace Vx.Views
{
    public class VxButton : VxView<Button>
    {
        //
        // Summary:
        //     Gets or sets the corner radius for the button, in device-independent units.
        //
        // Value:
        //     The corner radius for the button, in device-independent units.
        //
        // Remarks:
        //     To be added.
        public int CornerRadius { get; set; }
        //
        // Summary:
        //     Allows you to display a bitmap image on the Button.
        //
        // Value:
        //     To be added.
        //
        // Remarks:
        //     To be added.
        public ImageSource ImageSource { get; set; }
        //
        // Summary:
        //     Gets or sets the Font for the Label text. This is a bindable property.
        //
        // Value:
        //     The Xamarin.Forms.Font value for the button. The default is null, which represents
        //     the default font on the platform.
        //
        // Remarks:
        //     To be added.
        public Font Font { get; set; }
        //
        // Summary:
        //     Gets or sets the parameter to pass to the Command property. This is a bindable
        //     property.
        //
        // Value:
        //     A object to pass to the command property. The default value is null.
        //
        // Remarks:
        //     To be added.
        public object CommandParameter { get; set; }
        //
        // Summary:
        //     Gets or sets the command to invoke when the button is activated. This is a bindable
        //     property.
        //
        // Value:
        //     A command to invoke when the button is activated. The default value is null.
        //
        // Remarks:
        //     This property is used to associate a command with an instance of a button. This
        //     property is most often set in the MVVM pattern to bind callbacks back into the
        //     ViewModel. Xamarin.Forms.VisualElement.IsEnabled is controlled by the Command
        //     if set.
        public ICommand Command { get; set; }
        //
        // Summary:
        //     Gets or sets an object that controls the position of the button image and the
        //     spacing between the button's image and the button's text.
        //
        // Value:
        //     To be added.
        //
        // Remarks:
        //     To be added.
        public ButtonContentLayout ContentLayout { get; set; }
        //
        // Summary:
        //     Gets or sets the width of the border. This is a bindable property.
        //
        // Value:
        //     The width of the button border; the default is 0.
        //
        // Remarks:
        //     Set this value to a non-zero value in order to have a visible border.
        public double BorderWidth { get; set; }
        //
        // Summary:
        //     Gets or sets the size of the font of the button text.
        //
        // Value:
        //     To be added.
        //
        // Remarks:
        //     To be added.
        [TypeConverter(typeof(FontSizeConverter))]
        public double FontSize { get; set; }
        //
        // Summary:
        //     Gets or sets the padding for the button.
        //
        // Value:
        //     The padding for the button.
        //
        // Remarks:
        //     To be added.
        public Thickness Padding { get; set; }
        //
        // Summary:
        //     Gets or sets the Xamarin.Forms.Color for the text of the button. This is a bindable
        //     property.
        //
        // Value:
        //     The Xamarin.Forms.Color value.
        //
        // Remarks:
        //     To be added.
        public Color TextColor { get; set; }
        //
        // Summary:
        //     To be added.
        //
        // Value:
        //     To be added.
        //
        // Remarks:
        //     To be added.
        public double CharacterSpacing { get; set; }
        //
        // Summary:
        //     For internal use by the Xamarin.Forms platform.
        //
        // Value:
        //     To be added.
        //
        // Remarks:
        //     To be added.
        public bool IsPressed { get; }
        //
        // Summary:
        //     To be added.
        //
        // Value:
        //     To be added.
        //
        // Remarks:
        //     To be added.
        public TextTransform TextTransform { get; set; }
        //
        // Summary:
        //     Gets a value that indicates whether the font for the button text is bold, italic,
        //     or neither.
        //
        // Value:
        //     To be added.
        //
        // Remarks:
        //     To be added.
        public FontAttributes FontAttributes { get; set; }
        //
        // Summary:
        //     Gets the font family to which the font for the button text belongs.
        //
        // Value:
        //     To be added.
        //
        // Remarks:
        //     To be added.
        public string FontFamily { get; set; }
        public FileImageSource Image { get; set; }
        //
        // Summary:
        //     Gets or sets a color that describes the border stroke color of the button. This
        //     is a bindable property.
        //
        // Value:
        //     The color that is used as the border stroke color; the default is Xamarin.Forms.Color.Default.
        //
        // Remarks:
        //     This property has no effect if Xamarin.Forms.Button.BorderWidth is set to 0.
        //     On Android this property will not have an effect unless Xamarin.Forms.VisualElement.BackgroundColor
        //     is set to a non-default color.
        public Color BorderColor { get; set; }
        //
        // Summary:
        //     Gets or sets the Text displayed as the content of the button. This is a bindable
        //     property.
        //
        // Value:
        //     The text displayed in the button. The default value is null.
        //
        // Remarks:
        //     Changing the Text of a button will trigger a layout cycle.
        public string Text { get; set; }
    }

    public static class VxButtonExtensions
    {

    }
}
