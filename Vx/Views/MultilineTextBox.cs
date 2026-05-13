using System;
using System.Collections.Generic;
using BareMvvm.Core;

namespace Vx.Views
{
    public class MultilineTextBox : TextBox
    {
        public MultilineTextBox() { }

        /// <summary>
        /// Creates a text box connected to the text field.
        /// </summary>
        /// <param name="textField"></param>
        public MultilineTextBox(TextField textField) : base(textField) { }

        /// <summary>
        /// Called when the user pastes or drops one or more images into the text box.
        /// Each tuple contains the raw image bytes and the MIME type (e.g. "image/png").
        /// </summary>
        public Action<IReadOnlyList<PastedImage>> OnImagesPasted { get; set; }
    }

    public class PastedImage
    {
        public byte[] Data { get; set; }
        public string MediaType { get; set; }
    }
}
