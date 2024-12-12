using MonoMod.Utils;
using System;
using Xunit;
using Xunit.Abstractions;

namespace MonoMod.UnitTest
{
    public unsafe class SizeofByRefLikeTest : TestBase
    {
        public SizeofByRefLikeTest(ITestOutputHelper helper) : base(helper)
        {
        }

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(string))]
        [InlineData(typeof(SizeofByRefLikeTest))]
        [InlineData(typeof(ReadOnlySpan<char>))]
        [InlineData(typeof(ReadOnlyMemory<char>))]
        [InlineData(typeof(Span<byte>))]
        [InlineData(typeof(void*))]
        [InlineData(typeof(int*))]
        [InlineData(typeof(int), true)]
        [InlineData(typeof(string), true)]
        [InlineData(typeof(object), true)]
        public void SizeofDoesNotThrow(Type t, bool byref = false)
        {
            if (byref)
            {
                t = t.MakeByRefType();
            }
            _ = t.GetManagedSize();
        }
    }
}
