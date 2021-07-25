using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    //
    // Summary:
    //     Describes how a child element is vertically positioned or stretched within a
    //     parent's layout slot.
    public enum VerticalAlignment
    {
        //
        // Summary:
        //     The element is stretched to fill the entire layout slot of the parent element.
        Stretch = 0,
        //
        // Summary:
        //     The element is aligned to the top of the parent's layout slot.
        Top = 1,
        //
        // Summary:
        //     The element is aligned to the center of the parent's layout slot.
        Center = 2,
        //
        // Summary:
        //     The element is aligned to the bottom of the parent's layout slot.
        Bottom = 3
    }
}
