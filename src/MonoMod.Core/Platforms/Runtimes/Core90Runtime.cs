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
        // 488a17ce-26c9-4ad0-a7b7-79bf320ea4d1
        private static readonly Guid JitVersionGuid = new(
            0x488a17ce,
            0x26c9,
            0x4ad0,
            0xa7, 0xb7, 0x79, 0xbf, 0x32, 0x0e, 0xa4, 0xd1
        );

        protected override Guid ExpectedJitVersion => JitVersionGuid;

        protected override int VtableIndexICorJitInfoAllocMem => V90.ICorJitInfoVtable.AllocMemIndex;
        protected override int ICorJitInfoFullVtableCount => V90.ICorJitInfoVtable.TotalVtableCount;
    }
}
