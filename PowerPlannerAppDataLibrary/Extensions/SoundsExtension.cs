using System;
using System.Collections.Generic;
using System.Text;

namespace PowerPlannerAppDataLibrary.Extensions
{
    public abstract class SoundsExtension
    {
        public static SoundsExtension Current { get; set; }

        public abstract void TryPlayTaskCompletedSound();
    }
}
