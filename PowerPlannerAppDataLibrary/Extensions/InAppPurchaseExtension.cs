using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Extensions
{
    public abstract class InAppPurchaseExtension
    {
        public static InAppPurchaseExtension Current;

        public abstract Task<bool> OwnsInAppPurchaseAsync();

        public abstract Task<bool> PromptPurchase();

        /// <summary>
        /// Whether the current device/account is able to make payments. Defaults to true.
        /// </summary>
        public virtual bool CanMakePayments => true;

        /// <summary>
        /// Returns the localized price string, or null if unavailable. Defaults to null.
        /// </summary>
        public virtual Task<string> GetPriceAsync() => Task.FromResult<string>(null);

        /// <summary>
        /// Whether restoring a previous purchase is supported (e.g. required on iOS). Defaults to false.
        /// </summary>
        public virtual bool SupportsRestore => false;

        /// <summary>
        /// Restores a previous purchase. By default behaves like <see cref="PromptPurchase"/>.
        /// </summary>
        public virtual Task<bool> PromptRestore() => PromptPurchase();
    }

    /// <summary>
    /// Thrown when an in app purchase failed with a user-displayable error message.
    /// </summary>
    public class InAppPurchaseException : Exception
    {
        public InAppPurchaseException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when there was an in app purchase error but the exception has already been handled and logged. Should still display info to end user though.
    /// </summary>
    public class InAppPurchaseHandledException : Exception { }
}
