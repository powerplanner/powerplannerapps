using System;
using System.Collections.Generic;
using System.Text;

namespace PowerPlannerApp.Extensions
{
    public abstract class SoundsExtension
    {
        public static SoundsExtension Current { get; set; }

        public abstract void TryPlayTaskCompletedSound();
    }
}
