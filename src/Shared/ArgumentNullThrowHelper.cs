using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Extensions.Caching.Dapr.Shared
{
    internal static partial class ArgumentNullThrowHelper
    {
        /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
        /// <param name="argument">The reference type argument to validate as non-null.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        public static void ThrowIfNull(
            [NotNull] object? argument, 
            [CallerArgumentExpression("argument")] string? paramName = null)
        {
            ArgumentNullException.ThrowIfNull(argument, paramName);
        }

        [DoesNotReturn]
        internal static void Throw(string? paramName) => throw new ArgumentNullException(paramName);
    }
}
