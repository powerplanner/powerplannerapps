using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views.DragDrop
{
    public class DragStartingEventArgs
    {
        public DataPackage Data { get; } = new DataPackage();
    }

    public class DataPackage
    {
        public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();
    }

    public class DragEventArgs
    {
        public DataPackage Data { get; }

        public DragDropModifiers Modifiers { get; }

        public DataPackageOperation AcceptedOperation { get; set; } = DataPackageOperation.None;

        public DragEventArgs(DataPackage data, DragDropModifiers modifiers)
        {
            Data = data;
            Modifiers = modifiers;
        }
    }

    public enum DragDropModifiers : uint
    {
        //
        // Summary:
        //     No modifiers.
        None = 0x0u,
        //
        // Summary:
        //     The shift key.
        //Shift = 0x1u,
        //
        // Summary:
        //     The control key.
        Control = 0x2u,
        //
        // Summary:
        //     The alt key.
        //Alt = 0x4u,
        //
        // Summary:
        //     The left mouse button.
        //LeftButton = 0x8u,
        //
        // Summary:
        //     The middle mouse button.
        //MiddleButton = 0x10u,
        //
        // Summary:
        //     The right mouse button.
        //RightButton = 0x20u
    }

    public enum DataPackageOperation : uint
    {
        //
        // Summary:
        //     No action. Typically used when the DataPackage object requires delayed rendering.
        None = 0x0u,
        //
        // Summary:
        //     Copies the content to the target destination. When implementing clipboard functionality,
        //     this corresponds to the "Copy" command.
        Copy = 0x1u,
        //
        // Summary:
        //     Copies the content to the target destination and deletes the original. When implementing
        //     clipboard functionality, this corresponds to the "Cut" command.
        Move = 0x2u,
        //
        // Summary:
        //     Creates a link for the data.
        //Link = 0x4u
    }
}
