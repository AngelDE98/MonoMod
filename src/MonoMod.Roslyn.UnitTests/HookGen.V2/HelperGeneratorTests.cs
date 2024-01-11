﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using MonoMod.HookGen.V2;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace MonoMod.Roslyn.UnitTests.HookGen.V2 {

#pragma warning disable IDE0065
    using Test = CSharpSourceGeneratorTest<Verifiers.Adapter<HookHelperGenerator>, XUnitVerifier>;
#pragma warning restore IDE0065

    public class HelperGeneratorTests {

        private static readonly Type generatorType = typeof(Verifiers.Adapter<HookHelperGenerator>);
        private static readonly (Type, string, string) attributesSource = (generatorType,
            HookHelperGenerator.GenHelperForTypeAttrFile, HookHelperGenerator.GenHelperForTypeAttributeSource);

        private static readonly MetadataReference SelfMetadataReference = MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().Location);
        private static readonly MetadataReference RuntimeDetourMetadataReference = MetadataReference.CreateFromFile(typeof(Hook).Assembly.Location);
        private static readonly MetadataReference UtilsMetadataReference = MetadataReference.CreateFromFile(typeof(Cil.ILContext).Assembly.Location);

        const string ThisTypeFile = "MonoMod.Roslyn.UnitTests_MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests.g.cs";
        const string ThisTypeHelperDelegates = """
                // <auto-generated />
                #nullable enable
                namespace MonoMod.HookGen;

                internal delegate global::System.Threading.Tasks.Task OrigHookSig_System_Threading_Tasks_Task_0_MonoMod_Roslyn_UnitTests_HookGen_V2_HelperGeneratorTests(
                    global::MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests @this);
                internal delegate global::System.Threading.Tasks.Task HookSig_System_Threading_Tasks_Task_0_MonoMod_Roslyn_UnitTests_HookGen_V2_HelperGeneratorTests(
                    OrigHookSig_System_Threading_Tasks_Task_0_MonoMod_Roslyn_UnitTests_HookGen_V2_HelperGeneratorTests orig,
                    global::MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests @this);
                
                """;
        const string ThisTypeHelpers = """
                // <auto-generated />
                #nullable enable
                namespace On
                {
                    namespace MonoMod.Roslyn.UnitTests.HookGen.V2
                    {
                        internal static partial class HelperGeneratorTests
                        {
                            public static global::MonoMod.RuntimeDetour.Hook NoAttributesNoGen(global::MonoMod.HookGen.HookSig_System_Threading_Tasks_Task_0_MonoMod_Roslyn_UnitTests_HookGen_V2_HelperGeneratorTests hook, bool applyByDefault = true)
                            {
                                var type = typeof(global::MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests);
                                var method = type.GetMethod("NoAttributesNoGen", (global::System.Reflection.BindingFlags)24, new global::System.Type[]
                                {
                                }
                                );
                                if (method is null) throw new global::System.MissingMethodException("MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests", "NoAttributesNoGen");
                                return new(method, hook, applyByDefault: applyByDefault);
                            }

                        }
                    }
                }


                """;

        [Fact]
        public async Task NoAttributesNoGen() {
            const string source = """
                using MonoMod.HookGen;

                // no body
                """;
            await new Test {
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
                TestState = {
                    Sources = { source },
                    AdditionalReferences = { SelfMetadataReference, RuntimeDetourMetadataReference, UtilsMetadataReference },
                    GeneratedSources = { attributesSource }
                },
                ExpectedDiagnostics = { }
            }.RunAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task CanGenerateWithExactName() {
            const string source = """
                using MonoMod.HookGen;

                [assembly: GenerateHookHelpers(typeof(MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests), 
                    Members = ["NoAttributesNoGen"])]
                """;
            await new Test {
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
                TestState = {
                    Sources = { source },
                    AdditionalReferences = { SelfMetadataReference, RuntimeDetourMetadataReference, UtilsMetadataReference },
                    GeneratedSources = {
                        attributesSource,
                        (generatorType, HookHelperGenerator.DelegateTypesFile, ThisTypeHelperDelegates),
                        (generatorType, ThisTypeFile, ThisTypeHelpers),
                    },
                },
                ExpectedDiagnostics = { }
            }.RunAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task CanGenerateWithPrefixName() {
            const string source = """
                using MonoMod.HookGen;

                [assembly: GenerateHookHelpers(typeof(MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests), 
                    MemberNamePrefixes = ["NoAttr"])]
                """;
            await new Test {
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
                TestState = {
                    Sources = { source },
                    AdditionalReferences = { SelfMetadataReference, RuntimeDetourMetadataReference, UtilsMetadataReference },
                    GeneratedSources = {
                        attributesSource,
                        (generatorType, HookHelperGenerator.DelegateTypesFile, ThisTypeHelperDelegates),
                        (generatorType, ThisTypeFile, ThisTypeHelpers),
                    },
                },
                ExpectedDiagnostics = { }
            }.RunAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task CanGenerateWithSuffixName() {
            const string source = """
                using MonoMod.HookGen;

                [assembly: GenerateHookHelpers(typeof(MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests), 
                    MemberNameSuffixes = ["NoGen"])]
                """;
            await new Test {
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
                TestState = {
                    Sources = { source },
                    AdditionalReferences = { SelfMetadataReference, RuntimeDetourMetadataReference, UtilsMetadataReference },
                    GeneratedSources = {
                        attributesSource,
                        (generatorType, HookHelperGenerator.DelegateTypesFile, ThisTypeHelperDelegates),
                        (generatorType, ThisTypeFile, ThisTypeHelpers),
                    },
                },
                ExpectedDiagnostics = { }
            }.RunAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task CanMatchMultipleWithPrefix() {
            const string source = """
                using MonoMod.HookGen;

                [assembly: GenerateHookHelpers(typeof(MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests), 
                    MemberNamePrefixes = ["CanGenerateWith"])]
                """;
            const string helpers = """
                // <auto-generated />
                #nullable enable
                namespace On
                {
                    namespace MonoMod.Roslyn.UnitTests.HookGen.V2
                    {
                        internal static partial class HelperGeneratorTests
                        {
                            public static global::MonoMod.RuntimeDetour.Hook CanGenerateWithExactName(global::MonoMod.HookGen.HookSig_System_Threading_Tasks_Task_0_MonoMod_Roslyn_UnitTests_HookGen_V2_HelperGeneratorTests hook, bool applyByDefault = true)
                            {
                                var type = typeof(global::MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests);
                                var method = type.GetMethod("CanGenerateWithExactName", (global::System.Reflection.BindingFlags)24, new global::System.Type[]
                                {
                                }
                                );
                                if (method is null) throw new global::System.MissingMethodException("MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests", "CanGenerateWithExactName");
                                return new(method, hook, applyByDefault: applyByDefault);
                            }

                            public static global::MonoMod.RuntimeDetour.Hook CanGenerateWithPrefixName(global::MonoMod.HookGen.HookSig_System_Threading_Tasks_Task_0_MonoMod_Roslyn_UnitTests_HookGen_V2_HelperGeneratorTests hook, bool applyByDefault = true)
                            {
                                var type = typeof(global::MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests);
                                var method = type.GetMethod("CanGenerateWithPrefixName", (global::System.Reflection.BindingFlags)24, new global::System.Type[]
                                {
                                }
                                );
                                if (method is null) throw new global::System.MissingMethodException("MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests", "CanGenerateWithPrefixName");
                                return new(method, hook, applyByDefault: applyByDefault);
                            }

                            public static global::MonoMod.RuntimeDetour.Hook CanGenerateWithSuffixName(global::MonoMod.HookGen.HookSig_System_Threading_Tasks_Task_0_MonoMod_Roslyn_UnitTests_HookGen_V2_HelperGeneratorTests hook, bool applyByDefault = true)
                            {
                                var type = typeof(global::MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests);
                                var method = type.GetMethod("CanGenerateWithSuffixName", (global::System.Reflection.BindingFlags)24, new global::System.Type[]
                                {
                                }
                                );
                                if (method is null) throw new global::System.MissingMethodException("MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests", "CanGenerateWithSuffixName");
                                return new(method, hook, applyByDefault: applyByDefault);
                            }

                        }
                    }
                }

                
                """;
            await new Test {
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
                TestState = {
                    Sources = { source },
                    AdditionalReferences = { SelfMetadataReference, RuntimeDetourMetadataReference, UtilsMetadataReference },
                    GeneratedSources = {
                        attributesSource,
                        (generatorType, HookHelperGenerator.DelegateTypesFile, ThisTypeHelperDelegates),
                        (generatorType, ThisTypeFile, helpers),
                    },
                },
                ExpectedDiagnostics = { }
            }.RunAsync().ConfigureAwait(false);
        }

        public static class TestClass {
            public static void Single() {

            }

            public static void Overloaded() {

            }

            public static void Overloaded(int i) {

            }
        }

        private const string TestClassFile = "MonoMod.Roslyn.UnitTests_MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests.TestClass.g.cs";
        private const string TestClassDelegates = """
            // <auto-generated />
            #nullable enable
            namespace MonoMod.HookGen;

            internal delegate void OrigHookSig_System_Void_0(
                );
            internal delegate void HookSig_System_Void_0(
                OrigHookSig_System_Void_0 orig);
            internal delegate void OrigHookSig_System_Void_1_System_Int32(
                int arg0);
            internal delegate void HookSig_System_Void_1_System_Int32(
                OrigHookSig_System_Void_1_System_Int32 orig,
                int arg0);
            
            """;

        [Fact]
        public async Task CanGenerateAllForNested() {
            const string source = """
                using MonoMod.HookGen;

                [assembly: GenerateHookHelpers(typeof(MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests.TestClass), Kind = DetourKind.Both)]
                """;
            const string helpers = """
                // <auto-generated />
                #nullable enable
                namespace On
                {
                    namespace MonoMod.Roslyn.UnitTests.HookGen.V2
                    {
                        internal static partial class HelperGeneratorTests
                        {
                            internal static partial class TestClass
                            {
                                public static global::MonoMod.RuntimeDetour.Hook Single(global::MonoMod.HookGen.HookSig_System_Void_0 hook, bool applyByDefault = true)
                                {
                                    var type = typeof(global::MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests.TestClass);
                                    var method = type.GetMethod("Single", (global::System.Reflection.BindingFlags)20, new global::System.Type[]
                                    {
                                    }
                                    );
                                    if (method is null) throw new global::System.MissingMethodException("MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests+TestClass", "Single");
                                    return new(method, hook, applyByDefault: applyByDefault);
                                }

                                public static global::MonoMod.RuntimeDetour.Hook Overloaded(global::MonoMod.HookGen.HookSig_System_Void_0 hook, bool applyByDefault = true)
                                {
                                    var type = typeof(global::MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests.TestClass);
                                    var method = type.GetMethod("Overloaded", (global::System.Reflection.BindingFlags)20, new global::System.Type[]
                                    {
                                    }
                                    );
                                    if (method is null) throw new global::System.MissingMethodException("MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests+TestClass", "Overloaded");
                                    return new(method, hook, applyByDefault: applyByDefault);
                                }

                                public static global::MonoMod.RuntimeDetour.Hook Overloaded(global::MonoMod.HookGen.HookSig_System_Void_1_System_Int32 hook, bool applyByDefault = true)
                                {
                                    var type = typeof(global::MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests.TestClass);
                                    var method = type.GetMethod("Overloaded", (global::System.Reflection.BindingFlags)20, new global::System.Type[]
                                    {
                                        typeof(int),
                                    }
                                    );
                                    if (method is null) throw new global::System.MissingMethodException("MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests+TestClass", "Overloaded");
                                    return new(method, hook, applyByDefault: applyByDefault);
                                }

                            }
                        }
                    }
                }

                namespace IL
                {
                    namespace MonoMod.Roslyn.UnitTests.HookGen.V2
                    {
                        internal static partial class HelperGeneratorTests
                        {
                            internal static partial class TestClass
                            {
                                public static global::MonoMod.RuntimeDetour.ILHook Single(global::MonoMod.Cil.ILContext.Manipulator hook, bool applyByDefault = true)
                                {
                                    var type = typeof(global::MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests.TestClass);
                                    var method = type.GetMethod("Single", (global::System.Reflection.BindingFlags)20, new global::System.Type[]
                                    {
                                    }
                                    );
                                    if (method is null) throw new global::System.MissingMethodException("MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests+TestClass", "Single");
                                    return new(method, hook, applyByDefault: applyByDefault);
                                }

                                public static global::MonoMod.RuntimeDetour.ILHook Overloaded_System_Void_0(global::MonoMod.Cil.ILContext.Manipulator hook, bool applyByDefault = true)
                                {
                                    var type = typeof(global::MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests.TestClass);
                                    var method = type.GetMethod("Overloaded", (global::System.Reflection.BindingFlags)20, new global::System.Type[]
                                    {
                                    }
                                    );
                                    if (method is null) throw new global::System.MissingMethodException("MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests+TestClass", "Overloaded");
                                    return new(method, hook, applyByDefault: applyByDefault);
                                }

                                public static global::MonoMod.RuntimeDetour.ILHook Overloaded_System_Void_1_System_Int32(global::MonoMod.Cil.ILContext.Manipulator hook, bool applyByDefault = true)
                                {
                                    var type = typeof(global::MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests.TestClass);
                                    var method = type.GetMethod("Overloaded", (global::System.Reflection.BindingFlags)20, new global::System.Type[]
                                    {
                                        typeof(int),
                                    }
                                    );
                                    if (method is null) throw new global::System.MissingMethodException("MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests+TestClass", "Overloaded");
                                    return new(method, hook, applyByDefault: applyByDefault);
                                }

                            }
                        }
                    }
                }

                
                """;
            await new Test {
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
                TestState = {
                    Sources = { source },
                    AdditionalReferences = { SelfMetadataReference, RuntimeDetourMetadataReference, UtilsMetadataReference },
                    GeneratedSources = {
                        attributesSource,
                        (generatorType, HookHelperGenerator.DelegateTypesFile, TestClassDelegates),
                        (generatorType, TestClassFile, helpers),
                    },
                },
                ExpectedDiagnostics = { }
            }.RunAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task CanDistinguishOverloadsOnly() {
            const string source = """
                using MonoMod.HookGen;

                [assembly: GenerateHookHelpers(typeof(MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests.TestClass), Members = ["Single", "Overloaded"], DistinguishOverloadsByName = true)]
                """;
            const string helpers = """
                // <auto-generated />
                #nullable enable
                namespace On
                {
                    namespace MonoMod.Roslyn.UnitTests.HookGen.V2
                    {
                        internal static partial class HelperGeneratorTests
                        {
                            internal static partial class TestClass
                            {
                                public static global::MonoMod.RuntimeDetour.Hook Single(global::MonoMod.HookGen.HookSig_System_Void_0 hook, bool applyByDefault = true)
                                {
                                    var type = typeof(global::MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests.TestClass);
                                    var method = type.GetMethod("Single", (global::System.Reflection.BindingFlags)20, new global::System.Type[]
                                    {
                                    }
                                    );
                                    if (method is null) throw new global::System.MissingMethodException("MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests+TestClass", "Single");
                                    return new(method, hook, applyByDefault: applyByDefault);
                                }

                                public static global::MonoMod.RuntimeDetour.Hook Overloaded_System_Void_0(global::MonoMod.HookGen.HookSig_System_Void_0 hook, bool applyByDefault = true)
                                {
                                    var type = typeof(global::MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests.TestClass);
                                    var method = type.GetMethod("Overloaded", (global::System.Reflection.BindingFlags)20, new global::System.Type[]
                                    {
                                    }
                                    );
                                    if (method is null) throw new global::System.MissingMethodException("MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests+TestClass", "Overloaded");
                                    return new(method, hook, applyByDefault: applyByDefault);
                                }

                                public static global::MonoMod.RuntimeDetour.Hook Overloaded_System_Void_1_System_Int32(global::MonoMod.HookGen.HookSig_System_Void_1_System_Int32 hook, bool applyByDefault = true)
                                {
                                    var type = typeof(global::MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests.TestClass);
                                    var method = type.GetMethod("Overloaded", (global::System.Reflection.BindingFlags)20, new global::System.Type[]
                                    {
                                        typeof(int),
                                    }
                                    );
                                    if (method is null) throw new global::System.MissingMethodException("MonoMod.Roslyn.UnitTests.HookGen.V2.HelperGeneratorTests+TestClass", "Overloaded");
                                    return new(method, hook, applyByDefault: applyByDefault);
                                }

                            }
                        }
                    }
                }

                
                """;
            await new Test {
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
                TestState = {
                    Sources = { source },
                    AdditionalReferences = { SelfMetadataReference, RuntimeDetourMetadataReference, UtilsMetadataReference },
                    GeneratedSources = {
                        attributesSource,
                        (generatorType, HookHelperGenerator.DelegateTypesFile, TestClassDelegates),
                        (generatorType, TestClassFile, helpers),
                    },
                },
                ExpectedDiagnostics = { }
            }.RunAsync().ConfigureAwait(false);
        }
    }
}
