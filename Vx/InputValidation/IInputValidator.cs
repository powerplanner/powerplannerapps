using System;
using System.Collections.Generic;
using System.Text;
using ToolsPortable;

namespace Vx.InputValidation
{
    public interface IInputValidator
    {
        InputValidationState Validate(string text);
    }
}
