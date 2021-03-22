using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace VxSampleApp
{
    public class WelcomeView : VxComponent
    {
        public override VxView Render()
        {
            return new VxStackPanel()
                .Children(
                    new VxButton("Log in")
                        .Click(() => ShowPopup(new LoginView())),
                    new VxButton("Create account")
                        .Click(() => ShowPopup(new LoginView()))
                );
        }

        
    }
}
