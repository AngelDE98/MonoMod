using System;
using System.Diagnostics.CodeAnalysis;
using static MonoMod.Core.Interop.CoreCLR;

namespace MonoMod.Core.Platforms.Runtimes
{
    [SuppressMessage("Performance", "CA1852", Justification = "This type will be derived for .NET 10.")]
    internal class Core90Runtime : Core80Runtime
    {
        public Core90Runtime(ISystem system, IArchitecture arch) : base(system, arch) { }

        // src/coreclr/inc/jiteeversionguid.h line 46
        // f43f9022-8795-4791-ba55-c450d76cfeb9
        private static readonly Guid JitVersionGuid = new(
            0xf43f9022,
            0x8795,
            0x4791,
            0xba, 0x55, 0xc4, 0x50, 0xd7, 0x6c, 0xfe, 0xb9
        );

        protected override Guid ExpectedJitVersion => JitVersionGuid;

        protected override int VtableIndexICorJitInfoAllocMem => V90.ICorJitInfoVtable.AllocMemIndex;
        protected override int ICorJitInfoFullVtableCount => V90.ICorJitInfoVtable.TotalVtableCount;
    }
}
