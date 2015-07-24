//===--- SliceEnumerators.cs ---------------------------------------------------------===//
//
// Copyright (c) 2015 Joe Duffy. All rights reserved.
//
// This file is distributed under the MIT License. See LICENSE.md for details.
//
//===----------------------------------------------------------------------===//

using System.Collections;
using System.Collections.Generic;

namespace System
{
    /// <summary>
    /// this file contains all logic responsible for IEnumerable implementation
    /// these are mostly some tricks so they are kept is separate file
    /// </summary>
    public partial struct Slice<T>
    {
        /// <summary>
        /// compiler is using this method in every occurence of foreach
        /// CONDITIONS:
        ///     1. Slice is not used via the IEnumerable interface
        ///     2. Slice type has public method  
        ///         "GetEnumerator that takes no parameters and returns a type that has two members:
        ///         a) a method MoveNext that takes no parameters and return a Boolean, and 
        ///         b) a property Current with a getter that returns an Object"
        ///         source: http://blogs.msdn.com/b/kcwalina/archive/2007/07/18/ducknotation.aspx
        /// GAINS:
        ///     1. does not call interface members on structures which is expensive
        ///     2. does not put it into try/finally block with Dispose in the finally
        /// </summary>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// please use the generic version if possible
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new EnumeratorObject(this);
        }

        /// <summary>
        /// compiler is using this method in every occurence of foreach
        /// where Slice IS accessed via the <see cref="IEnumerable{T}"/> interface
        /// </summary>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new EnumeratorObject(this);
        }

        /// <summary>
        /// A struct-based enumerator, to make fast enumerations possible.
        /// This isn't designed for direct use, instead see GetEnumerator.
        /// Using structure avoids expensive heap allocations
        /// </summary>
        public struct Enumerator
        {
            Slice<T> m_slice;    // The slice being enumerated.
            int      m_position; // The current position.

            internal Enumerator(Slice<T> slice)
            {
                m_slice = slice;
                m_position = -1;
            }

            public T Current
            {
                get { return m_slice[m_position]; }
            }

            public bool MoveNext()
            {
                return ++m_position < m_slice.Length;
            }
        }

        /// <summary>
        /// enumerator that implements <see cref="IEnumerator{T}"/> pattern (including <see cref="IDisposable"/>).
        /// it is used by LINQ and foreach when Slice is accessed via <see cref="IEnumerable{T}"/>
        /// it is reference type to avoid boxing when calling interface methods on stuctures
        /// </summary>
        private class EnumeratorObject : IEnumerator<T>
        {
            Slice<T> m_slice;    // The slice being enumerated.
            int      m_position; // The current position.

            public EnumeratorObject(Slice<T> slice)
            {
                m_slice = slice;
                m_position = -1;
            }

            public T Current
            {
                get { return m_slice[m_position]; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public void Dispose()
            {
                // we don't have any managed resources here so we do not Dispose anything here
                // we also don't need to set m_slice to null because when this instance becames garbage
                // the reference stored in this field is also going to became GC-reachable (garbage)
            }

            public bool MoveNext()
            {
                return ++m_position < m_slice.Length;
            }

            public void Reset()
            {
                m_position = -1;
            }
        }
    }
}


