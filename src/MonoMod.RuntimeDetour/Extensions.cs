using MonoMod.Utils;
using System.Reflection;

namespace MonoMod.RuntimeDetour
{
    internal static class Extensions
    {
        public static MethodInfo CreateILCopy(this MethodBase method)
        {
            using var dmd = new DynamicMethodDefinition(method);
            return dmd.Generate();
        }
    }
}
