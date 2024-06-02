# Notes on issues in various runtime versions

## `fixed` on strings in old Mono

Some old versions of Mono have broken `conv.u` instruction handling.

The following code will crash those old versions with an assert in the JIT's local propagation routine:

```cs
fixed (char* pStr = "some string")
{
    // ...
}
```

This is because the sequence that Roslyn emits for `fixed` over a string is this:

```
  .locals (
    string pinned stringLocalMarkedPinned,
    char* ptrLocal
  )

  // ...

  // load string object onto the stack...
  stloc stringLocalMarkedPinned
  ldloc stringLocalMarkedPinned
  conv.u
  stloc ptrLocal
  ldloc ptrLocal
  brfalse.s PTR_NULL

  ldloc ptrLocal
  call int32 [mscorlib]System.Runtime.CompilerServices.RuntimeHelpers::get_OffsetToStringData()
  add
  stloc ptrLocal

PTR_NULL:
  // code using the pointer in ptrLocal
```

Importantly, this sequence uses `conv.u` on a string object to convert a value of type `O` to a value of type `U`
(a.k.a. `nuint` or `UIntPtr`), giving the address of the object, then uses `RuntimeHelpers.OffsetToStringData` to
offset that to the start of the string data.

New runtimes expose `GetPinnableReference()` on `string`, and if it's present, Roslyn will use that instead. However,
the versions of Mono that this is a problem with (such as the version Unity 5.x uses) expose the .NET 3.5 API surface
which does *not* include `GetPinnableReference()`.

This fails because the JIT's importer has an incomplete implementation of `conv.u`.

The relevant code is in `mini/method-to-ir.c`, in the `type_from_op` function:

```c
// ...
switch (ins->opcode) {
// ...
case CEE_CONV_U:
    ins->type = STACK_PTR;
    switch (src1->type) {
    case STACK_I4:
        ins->opcode = OP_ICONV_TO_U;
        break;
    case STACK_PTR:
    case STACK_MP:
#if SIZEOF_REGISTER == 8
        ins->opcode = OP_LCONV_TO_U;
#else
        ins->opcode = OP_MOVE;
#endif
    break;
    case STACK_I8:
        ins->opcode = OP_LCONV_TO_U;
        break;
    case STACK_R8:
        ins->opcode = OP_FCONV_TO_U;
        break;
    }
    break;
// ...
}
// ...
```

In the problematic case, `src1->type` is `STACK_OBJ`, which is not handled, and so `ins->opcode` remains the IL
opcode for `conv.u`, as opposed to a Mono IR opcode. This then causes an assertion failure in `mini/local-propagation.c`
in `mono_local_cprop`:

```c
g_assert (ins->opcode > MONO_CEE_LAST);
```

Which, of course, fails, because `ins->opcode` is still `CEE_CONV_U`, which is less than `MONO_CEE_LAST`.

### Workarounds

1. Use an array, like `new char[] { /* ... */ }`.
2. Convert the string to a `ReadOnlySpan<char>` first, like `str.AsSpan()`.
