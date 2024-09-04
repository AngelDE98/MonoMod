using Mono.Cecil;

namespace MonoMod.Utils
{
    public static partial class Extensions
    {

        public static IMetadataTokenProvider ImportReference(this ModuleDefinition mod, IMetadataTokenProvider mtp)
        {
            Helpers.ThrowIfArgumentNull(mod);
            if (mtp is TypeReference type)
                return mod.ImportReference(type);
            if (mtp is FieldReference field)
                return mod.ImportReference(field);
            if (mtp is MethodReference method)
                return mod.ImportReference(method);
            if (mtp is CallSite callsite)
                return mod.ImportReference(callsite);
            return mtp;
        }

        public static CallSite ImportReference(this ModuleDefinition mod, CallSite callsite)
        {
            Helpers.ThrowIfArgumentNull(mod);
            Helpers.ThrowIfArgumentNull(callsite);
            var cs = new CallSite(mod.ImportReference(callsite.ReturnType));
            cs.CallingConvention = callsite.CallingConvention;
            cs.ExplicitThis = callsite.ExplicitThis;
            cs.HasThis = callsite.HasThis;
            foreach (var param in callsite.Parameters)
            {
                var p = new ParameterDefinition(mod.ImportReference(param.ParameterType))
                {
                    Name = param.Name,
                    Attributes = param.Attributes,
                    Constant = param.Constant,
                    MarshalInfo = param.MarshalInfo,
                };

                // TODO: CAs
                cs.Parameters.Add(p);
            }
            return cs;
        }

    }
}
