//===--- SliceHelpers.cs --------------------------------------------------===//
//
// Copyright (c) 2015 Joe Duffy. All rights reserved.
//
// This file is distributed under the MIT License. See LICENSE.md for details.
//
//===----------------------------------------------------------------------===//

namespace System
{
    /// <summary>
    /// Internal helper methods used for implementing Slice.
    /// </summary>
    static class SliceHelpers
    {
        /// <summary>
        /// The offset, in bytes, to the first character in a string.
        /// </summary>
        internal static readonly int OffsetToStringData =
            System.Runtime.CompilerServices.RuntimeHelpers.OffsetToStringData;
    }

    /// <summary>
    /// Internal helper methods used for implementing Slice.
    /// </summary>
    class SliceHelpers<T>
    {
        /// <summary>
        /// The offset, in bytes, to the first element of an array of type T.
        /// </summary>
        internal static readonly int OffsetToArrayData =
            PtrUtils.ElemOffset<T>(new T[1]);
    }
}

