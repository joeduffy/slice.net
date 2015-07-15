namespace System
{
    /// <summary>
    /// A collection of convenient Slice helpers, exposed as extension methods.
    /// </summary>
    public static class SliceExtensions
    {
        /// <summary>
        /// Casts a Slice of one primitive type (T) to another primitive type (U).
        /// These types may not contain managed objects, in order to preserve type
        /// safety.  This is checked statically by a Roslyn analyzer.
        /// </summary>
        /// <param name="slice">The source slice, of type T.</param>
        public static Slice<U> Cast<[Primitive]T, [Primitive]U>(this Slice<T> slice)
            where T : struct
            where U : struct
        {
            int countOfU =
                slice.Length * PtrUtils.SizeOf<T>() / PtrUtils.SizeOf<U>();
            if (countOfU == 0) {
                return default(Slice<U>);
            }
            return new Slice<U>(slice.Object, slice.Offset, countOfU);
        }

        /// <summary>
        /// Reads a structure of type T out of a slice of bytes.
        /// </summary>
        public static T Read<[Primitive]T>(this Slice<byte> slice)
            where T : struct
        {
            Contract.Requires(slice.Length >= PtrUtils.SizeOf<T>());
            return slice.Cast<byte, T>()[0];
        }

        /// <summary>
        /// Writes a structure of type T into a slice of bytes.
        /// </summary>
        public static void Write<[Primitive]T>(this Slice<byte> slice, T value)
            where T : struct
        {
            Contract.Requires(slice.Length >= PtrUtils.SizeOf<T>());
            var cast = slice.Cast<byte, T>();
            cast[0] = value;
        }

        /// <summary>
        /// Creates a new slice over the portion of the target array.
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the 'array' parameter is null.
        /// </exception>
        public static Slice<T> Slice<T>(this T[] array)
        {
            return new Slice<T>(array);
        }

        /// <summary>
        /// Creates a new slice over the portion of the target array beginning
        /// at 'start' index.
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <param name="start">The index at which to begin the slice.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the 'array' parameter is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified start index is not in range (&lt;0 or &gt;&eq;length).
        /// </exception>
        public static Slice<T> Slice<T>(this T[] array, int start)
        {
            return new Slice<T>(array, start);
        }

        /// <summary>
        /// Creates a new slice over the portion of the target array beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <param name="start">The index at which to begin the slice.</param>
        /// <param name="end">The index at which to end the slice (exclusive).</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the 'array' parameter is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified start or end index is not in range (&lt;0 or &gt;&eq;length).
        /// </exception>
        public static Slice<T> Slice<T>(this T[] array, int start, int end)
        {
           return new Slice<T>(array, start, end - start);
        }

        /// <summary>
        /// Creates a new slice over the portion of the target string.
        /// </summary>
        /// <param name="str">The target string.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the 'str' parameter is null.
        /// </exception>
        public static Slice<char> Slice(this string str)
        {
            Contract.Requires(str != null);
            return new Slice<char>(
                str,
                new UIntPtr((uint)SliceHelpers.OffsetToStringData),
                str.Length
            );
        }

        /// <summary>
        /// Creates a new slice over the portion of the target string beginning
        /// at 'start' index.
        /// </summary>
        /// <param name="str">The target string.</param>
        /// <param name="start">The index at which to begin the slice.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the 'str' parameter is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified start index is not in range (&lt;0 or &gt;&eq;length).
        /// </exception>
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

        /// <summary>
        /// Creates a new slice over the portion of the target string beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="str">The target string.</param>
        /// <param name="start">The index at which to begin the slice.</param>
        /// <param name="end">The index at which to end the slice (exclusive).</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the 'start' parameter is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified start or end index is not in range (&lt;0 or &gt;&eq;length).
        /// </exception>
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

