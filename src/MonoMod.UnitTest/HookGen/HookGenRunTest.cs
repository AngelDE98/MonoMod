﻿#pragma warning disable CS1720 // Expression will always cause a System.NullReferenceException because the type's default value is null
#pragma warning disable xUnit1013 // Public method should be marked as test

using Xunit;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using System.IO;
using MonoMod.RuntimeDetour.HookGen;
using System.Collections.Generic;

namespace MonoMod.UnitTest {
    [Collection("HookGen")]
    public class HookGenRunTest {
        [Fact]
        public void TestHookGenRun() {
            string outputPath = Path.Combine(Environment.CurrentDirectory, "testdump", "MonoMod.UnitTest.Hooks.dll");
            try {
                string dir = Path.GetDirectoryName(outputPath);
                if (!Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }
                if (File.Exists(outputPath)) {
                    File.SetAttributes(outputPath, FileAttributes.Normal);
                    File.Delete(outputPath);
                }
            } catch (Exception e) {
                Console.WriteLine("Couldn't create testdump.");
                Console.WriteLine(e);
            }

            using (MonoModder mm = new MonoModder {
                InputPath = typeof(HookGenRunTest).Assembly.Location,
                ReadingMode = ReadingMode.Deferred,

                MissingDependencyThrow = false,
            }) {
                mm.Read();
                mm.MapDependencies();

                HookGenerator gen = new HookGenerator(mm, "MonoMod.UnitTest.Hooks") {
                    HookPrivate = true,
                };
                using (ModuleDefinition mOut = gen.OutputModule) {
                    gen.Generate();

                    if (outputPath != null) {
                        mOut.Write(outputPath);
                    } else {
                        using (MemoryStream ms = new MemoryStream())
                            mOut.Write(ms);
                    }
                }
            }
        }

        // The test above needs to deal with the entire assembly, including the following code.
    }
}

namespace MonoMod.UnitTest.HookGenTrash.Other {
    class Dummy {
        public List<int> A() => default;
        public List<Dummy> B() => default;
        public int C() => default;
        public Dummy D() => default;
        public T E<T>() => default;
    }
}

// Taken from tModLoader. This just needs to not crash.
namespace MonoMod.UnitTest.HookGenTrash.tModLoader {
    public class ItemDefinition {
    }
    class DefinitionOptionElement<T> where T : class {
    }
    public abstract class ConfigElement<T> {
    }
    abstract class DefinitionElement<T> : ConfigElement<T> where T : class {
        protected abstract DefinitionOptionElement<T> CreateDefinitionOptionElement();
    }
    class ItemDefinitionElement : DefinitionElement<ItemDefinition> {
        protected override DefinitionOptionElement<ItemDefinition> CreateDefinitionOptionElement() => null;
    }
}
