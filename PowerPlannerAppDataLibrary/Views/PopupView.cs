using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Views
{
    public abstract class PopupView : VxComponent
    {
        public VxState<string> Title { get; private set; } = new VxState<string>("");

        protected override VxView Render()
        {
            return new VxGrid()
                .Children(

                    new VxGrid()
                        .Children(
                            new VxTextBlock(Title.Value),
                            new VxButton("Close")
                        ),

                    RenderContent()
                );
        }

        protected abstract VxView RenderContent();
    }
}
