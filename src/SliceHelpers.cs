namespace System
{
    static class SliceHelpers
    {
        internal static readonly int OffsetToStringData =
            System.Runtime.CompilerServices.RuntimeHelpers.OffsetToStringData;
    }

    class SliceHelpers<T>
    {
        internal static readonly int OffsetToArrayData =
            PtrUtils.ElemOffset<T>(new T[1]);
    }
}

