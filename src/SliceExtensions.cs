namespace System
{
    public static class SliceExtensions
    {
        // TODO(joe): constrain generics to pointerless data.
        public static Slice<U> Cast<[Primitive]T, [Primitive]U>(this Slice<T> span)
        {
            int countOfU =
                span.Length * PtrUtils.SizeOf<T>() / PtrUtils.SizeOf<U>();
            if (countOfU == 0) {
                return default(Slice<U>);
            }
            return new Slice<U>(span.Object, span.Offset, countOfU);
        }

        public static Slice<T> Slice<T>(this T[] arr)
        {
            return new Slice<T>(arr);
        }

        public static Slice<T> Slice<T>(this T[] arr, int start)
        {
            return new Slice<T>(arr, start);
        }

        public static Slice<T> Slice<T>(this T[] arr, int start, int end)
        {
           return new Slice<T>(arr, start, end - start);
        }

        public static Slice<char> Slice(this string str)
        {
            Contract.Requires(str != null);
            return new Slice<char>(
                str,
                new UIntPtr((uint)SliceHelpers.OffsetToStringData),
                str.Length
            );
        }

        public static Slice<char> Slice(this string str, int start)
        {
            Contract.Requires(str != null);
            Contract.RequiresInInclusiveRange(start, str.Length);
            return new Slice<char>(
                str,
                new UIntPtr((uint)(SliceHelpers.OffsetToStringData + (start * sizeof(char)))),
                str.Length - start
            );
        }

        public static Slice<char> Slice(this string str, int start, int end)
        {
            Contract.Requires(str != null);
            Contract.RequiresInInclusiveRange(start, end, str.Length);
            return new Slice<char>(
                str,
                new UIntPtr((uint)(SliceHelpers.OffsetToStringData + (start * sizeof(char)))),
                end - start
            );
        }
    }
}

