using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public enum InputScope
    {
        /// <summary>
        /// The normal input scope for most text. Spell check enabled, text prediction enabled.
        /// </summary>
        Normal,

        /// <summary>
        /// Scope for entering email. Spell check disabled, text prediction enabled.
        /// </summary>
        Email,

        /// <summary>
        /// Scope for entering username. Spell check disabled, text prediction enabled.
        /// </summary>
        Username
    }
}
