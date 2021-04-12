using System;
using System.Collections.Generic;
using System.Text;
using ToolsPortable;

namespace BareMvvm.Core
{
    public interface IInputValidator
    {
        InputValidationState Validate(string text);
    }
}
