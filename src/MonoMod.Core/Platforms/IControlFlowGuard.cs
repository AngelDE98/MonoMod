using System;

namespace MonoMod.Core.Platforms
{
    /// <summary>
    /// An interface enabling interaction with features like Windows' Control Flow Guard.
    /// </summary>
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/secbp/control-flow-guard"/>
    public interface IControlFlowGuard
    {
        /// <summary>
        /// <see langword="true"/> if this feature is supported, <see langword="false"/> otherwise.
        /// </summary>
        bool IsSupported { get; }

        /// <summary>
        /// Gets the alignment requirement for registerable entry points.
        /// </summary>
        int TargetAlignmentRequirement { get; }

        /// <summary>
        /// Registers a memory region's valid indirect call targets.
        /// </summary>
        /// <remarks>
        /// Callers are responsible for ensuring that provided entrypoints are aligned according to <see cref="TargetAlignmentRequirement"/>.
        /// </remarks>
        /// <param name="memoryRegionStart">A pointer to the start of the memory region to register.</param>
        /// <param name="memoryRegionLength">The length of the memory region to register.</param>
        /// <param name="validTargetsInMemoryRegion">A set of <see cref="TargetAlignmentRequirement"/>-aligned valid entrypoints,
        /// as offsets relative to <paramref name="memoryRegionStart"/>.</param>
        unsafe void RegisterValidIndirectCallTargets(
            void* memoryRegionStart, nint memoryRegionLength,
            ReadOnlySpan<nint> validTargetsInMemoryRegion);
    }
}
