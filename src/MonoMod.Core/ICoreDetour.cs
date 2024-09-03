using System;
using System.Reflection;

namespace MonoMod.Core
{
    /// <summary>
    /// A single method-to-method managed detour.
    /// </summary>
    [CLSCompliant(true)]
    public interface ICoreDetour : ICoreDetourBase
    {
        /// <summary>
        /// The source method.
        /// </summary>
        MethodBase Source { get; }
        /// <summary>
        /// The target method.
        /// </summary>
        MethodBase Target { get; }
    }

    /// <summary>
    /// An <see cref="ICoreDetour"/> that additionally provides <see cref="SourceMethodClone"/>.
    /// </summary>
    /// <remarks>
    /// An <see cref="ICoreDetour"/> may implement this interface without actually providing a source clone.
    /// </remarks>
    /// <seealso cref="CreateDetourRequest.CreateSourceClone"/>
    public interface ICoreDetourWithClone : ICoreDetour
    {
        /// <summary>
        /// A clone of <see cref="ICoreDetour.Source"/>, which behaves as-if it had not been detoured.
        /// </summary>
        /// <remarks>
        /// This method will not be available unless <see cref="CreateDetourRequest.CreateSourceClone"/> was
        /// set when the detour was created. If the <see cref="IDetourFactory"/> does not support that option,
        /// this may not be set anyway.
        /// </remarks>
        /// <seealso cref="CreateDetourRequest.CreateSourceClone"/>
        MethodInfo? SourceMethodClone { get; }
    }
}
