.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 4:0:0:0
}

.assembly System.Slice.Core
{
}

.namespace System
{
    .class public auto ansi sealed beforefieldinit PtrUtils
        extends [mscorlib]System.Object
    {
        .method public hidebysig static !!T Get<T>(
            object obj, native uint offset) cil managed
        {
            .maxstack 2
            .locals ([0] uint8& addr)
            ldarg.0     // load the object
            stloc.0     // convert the object pointer to a byref
            ldloc.0     // load the object pointer as a byref
            ldarg.1     // load the offset
            add         // add the offset
            ldobj !!T   // load a T value from the computed address
            ret
        }

        .method public hidebysig static void Set<T>(
            object obj, native uint offset, !!T val) cil managed
        {
            .maxstack 2
            .locals ([0] uint8& addr)
            ldarg.0     // load the object
            stloc.0     // convert the object pointer to a byref
            ldloc.0     // load the object pointer as a byref
            ldarg.1     // load the offset
            add         // add the offset
            ldarg.2     // load the value to store
            stobj !!T   // store a T value to the computed address
            ret
        }

        .method public hidebysig static int32 ElemOffset<T>(!!T[] arr) cil managed
        {
            ldarg.0
            ldc.i4 0
            ldelema !!T
            ldarg.0
            sub
            ret
        }

        .method public hidebysig static int32 SizeOf<T>() cil managed
        {
            sizeof !!T
            ret
        }
    }
}

