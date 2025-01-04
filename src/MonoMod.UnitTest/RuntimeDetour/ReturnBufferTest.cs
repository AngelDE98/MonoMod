extern alias New;
using New::MonoMod.RuntimeDetour;
using System;
using System.Runtime.CompilerServices;
using Xunit;
using Xunit.Abstractions;

namespace MonoMod.UnitTest
{
    [Collection("RuntimeDetour")]
    public class ReturnBufferSysVTest : TestBase
    {
        public ReturnBufferSysVTest(ITestOutputHelper helper) : base(helper) { }

        [Fact]
        public void TestReturnBufferDetour()
        {
            Assert.True(Source(0, 0, 0, 0) is { f1: 1, f2: 2, f3: 3 });

            using var hook = new Hook(
                typeof(ReturnBufferSysVTest).GetMethod("Source", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!,
                typeof(ReturnBufferSysVTest).GetMethod("Target", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!,
                true
            );

            Assert.True(Source(0, 0, 0, 0) is { f1: 4, f2: 5, f3: 6 });
        }

        [Fact]
        public void TestReturnBufferNullableDetour()
        {
            Assert.True(Source(0, 0, 0, 0) is { f1: 1, f2: 2, f3: 3 });

            using var hook = new Hook(
                typeof(ReturnBufferSysVTest).GetMethod("SourceNullable", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!,
                typeof(ReturnBufferSysVTest).GetMethod("TargetNullable", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!,
                true
            );

            Assert.True(SourceNullable(0, 0, 0, 0) is null);
        }

        [Fact]
        public void TestNullableBoolDetour()
        {
            var instance = new TestClass();
            Assert.False(instance.Source());

            using var hook = new Hook(
                typeof(TestClass).GetMethod("Source", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!,
                typeof(TestClass).GetMethod("Target", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!,
                true
            );

            Assert.True(instance.Source());
        }

        internal struct TestStruct
        {
            public ulong f1, f2, f3; // 24 bytes
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal TestStruct Source(int a, int b, int c, int d)
        {
            return new TestStruct() { f1 = 1, f2 = 2, f3 = 3 };
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal TestStruct? SourceNullable(int a, int b, int c, int d)
        {
            return new TestStruct() { f1 = 1, f2 = 2, f3 = 3 };
        }

        internal static TestStruct Target(Func<ReturnBufferSysVTest, int, int, int, int, TestStruct> orig, ReturnBufferSysVTest self, int a, int b, int c, int d)
        {
            var s = orig(self, a, b, c, d);
            s.f1 += 3;
            s.f2 += 3;
            s.f3 += 3;
            return s;
        }

        internal static TestStruct? TargetNullable(Func<ReturnBufferSysVTest, int, int, int, int, TestStruct?> orig, ReturnBufferSysVTest self, int a, int b, int c, int d)
        {
            return null;
        }

        // keep the test in its own class because the existence of fields in the class has significance
        internal class TestClass
        {
            internal static bool? Target(TestClass instance)
            {
                _ = instance;
                return true;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            internal bool? Source()
            {
                return false;
            }

            // force error by having a field in the test class
            internal string s1 = "test";
        }
    }
}
