﻿using MonoMod.Cil;
using MonoMod.Core;
using System;
using System.Reflection;

namespace MonoMod.RuntimeDetour {
    internal interface IDetourBase {
        IDetourFactory Factory { get; }
        DetourConfig? Config { get; }
    }

    internal interface IDetour : IDetourBase {
        MethodInfo PublicTarget { get; }
        MethodInfo InvokeTarget { get; }
        MethodBase NextTrampoline { get; }
    }

    internal interface IILHook : IDetourBase {
        ILContext.Manipulator Manip { get; }
    }

    internal interface INativeDetour : IDetourBase {
        IntPtr Function { get; }
        Type NativeDelegateType { get; }
        Delegate Invoker { get; }
        bool HasOrigParam { get; }
    }
}
