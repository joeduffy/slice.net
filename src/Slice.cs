//===--- Slice.cs ---------------------------------------------------------===//
//
// Copyright (c) 2015 Joe Duffy. All rights reserved.
//
// This file is distributed under the MIT License. See LICENSE.md for details.
//
//===----------------------------------------------------------------------===//

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System
{
    /// <summary>
    /// Slice is a uniform API for dealing with arrays and subarrays, strings
    /// and substrings, and unmanaged memory buffers.  It adds minimal overhead
    /// to regular accesses and is a struct so that creation and subslicing do
    /// not require additional allocations.  It is type- and memory-safe.
    /// </summary>
    public partial struct Slice<T> : IEnumerable<T>
    {
        /// <summary>A managed array/string; or null for native ptrs.</summary>
        readonly object  m_object;
        /// <summary>An byte-offset into the array/string; or a native ptr.</summary>
        readonly UIntPtr m_offset;
        /// <summary>Fetches the number of elements this Slice contains.</summary>
        public readonly int Length;

        /// <summary>
        /// Creates a new slice over the entirety of the target array.
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the 'array' parameter is null.
        /// </exception>
        public Slice(T[] array)
        {
            Contract.Requires(array != null);
            m_object = array;
            m_offset = new UIntPtr((uint)SliceHelpers<T>.OffsetToArrayData);
            Length = array.Length;
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
        public Slice(T[] array, int start)
        {
            Contract.Requires(array != null);
            Contract.RequiresInInclusiveRange(start, array.Length);
            if (start < array.Length) {
                m_object = array;
                m_offset = new UIntPtr(
                    (uint)(SliceHelpers<T>.OffsetToArrayData + (start * PtrUtils.SizeOf<T>())));
                Length = array.Length - start;
            }
            else {
                m_object = null;
                m_offset = UIntPtr.Zero;
                Length = 0;
            }
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
        public Slice(T[] array, int start, int end)
        {
            Contract.Requires(array != null);
            Contract.RequiresInInclusiveRange(start, array.Length);
            if (start < array.Length) {
                m_object = array;
                m_offset = new UIntPtr(
                    (uint)(SliceHelpers<T>.OffsetToArrayData + (start * PtrUtils.SizeOf<T>())));
                Length = end - start;
            }
            else {
                m_object = null;
                m_offset = UIntPtr.Zero;
                Length = 0;
            }
        }

        /// <summary>
        /// Creates a new slice over the target unmanaged buffer.  Clearly this
        /// is quite dangerous, because we are creating arbitrarily typed T's
        /// out of a void*-typed block of memory.  And the length is not checked.
        /// But if this creation is correct, then all subsequent uses are correct.
        /// </summary>
        /// <param name="ptr">An unmanaged pointer to memory.</param>
        /// <param name="length">The number of T elements the memory contains.</param>
        public unsafe Slice(void* ptr, int length)
        {
            Contract.Requires(length >= 0);
            Contract.Requires(length == 0 || ptr != null);
            m_object = null;
            m_offset = new UIntPtr(ptr);
            Length = length;
        }

        /// <summary>
        /// An internal helper for creating slices.  Not for public use.
        /// </summary>
        internal Slice(object obj, UIntPtr offset, int length)
        {
            m_object = obj;
            m_offset = offset;
            Length = length;
        }

        /// <summary>
        /// Fetches the managed object (if any) that this Slice points at.
        /// </summary>
        internal object Object
        {
            get { return m_object; }
        }

        /// <summary>
        /// Fetches the offset -- or sometimes, raw pointer -- for this Slice.
        /// </summary>
        internal UIntPtr Offset
        {
            get { return m_offset; }
        }

        /// <summary>
        /// Fetches the element at the specified index.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified index is not in range (&lt;0 or &gt;&eq;length).
        /// </exception>
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] // it makes enumeration via IEnumerable faster
            get {
                Contract.RequiresInRange(index, Length);
                return PtrUtils.Get<T>(
                    m_object, m_offset + (index * PtrUtils.SizeOf<T>()));
            }
            set {
                Contract.RequiresInRange(index, Length);
                PtrUtils.Set<T>(
                    m_object, m_offset + (index * PtrUtils.SizeOf<T>()), value);
            }
        }

        /// <summary>
        /// Copies the contents of this Slice into a new array.  This heap
        /// allocates, so should generally be avoided, however is sometimes
        /// necessary to bridge the gap with APIs written in terms of arrays.
        /// </summary>
        public T[] Copy()
        {
            var dest = new T[Length];
            CopyTo(dest.Slice());
            return dest;
        }

        /// <summary>
        /// Copies the contents of this Slice into another.  The destination
        /// must be at least as big as the source, and may be bigger.
        /// </summary>
        /// <param name="dest">The Slice to copy items into.</param>
        public void CopyTo(Slice<T> dest)
        {
            Contract.Requires(dest.Length >= Length);
            if (Length == 0) {
                return;
            }

            // TODO(joe): specialize to use a fast memcpy if T is pointerless.
            for (int i = 0; i < Length; i++) {
                dest[i] = this[i];
            }
        }

        /// <summary>
        /// Forms a subslice out of the given slice, beginning at 'start'.
        /// </summary>
        /// <param name="start">The index at which to begin this subslice.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified start index is not in range (&lt;0 or &gt;&eq;length).
        /// </exception>
        public Slice<T> Sub(int start)
        {
            return Sub(start, Length);
        }

        /// <summary>
        /// Forms a subslice out of the given slice, beginning at 'start', and
        /// ending at 'end' (exclusive).
        /// </summary>
        /// <param name="start">The index at which to begin this subslice.</param>
        /// <param name="end">The index at which to end this subslice (exclusive).</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified start or end index is not in range (&lt;0 or &gt;&eq;length).
        /// </exception>
        public Slice<T> Sub(int start, int end)
        {
            Contract.RequiresInInclusiveRange(start, end, Length);
            return new Slice<T>(
                m_object, m_offset + (start * PtrUtils.SizeOf<T>()), end - start);
        }

        /// <summary>
        /// Checks to see if two slices point at the same memory.  Note that
        /// this does *not* check to see if the *contents* are equal.
        /// </summary>
        public bool ReferenceEquals(Slice<T> other)
        {
            return Object == other.Object &&
                Offset == other.Offset && Length == other.Length;
        }

        /// <summary>
        /// Returns item from given index without the boundaries check
        /// use only in places where moving outside the boundaries is impossible
        /// gain: performance: no boundaries check (single operation) 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal T GetItemWithoutBoundariesCheck(int index)
        {
            return PtrUtils.Get<T>(
                    m_object, m_offset + (index * PtrUtils.SizeOf<T>()));
        }
    }
}


