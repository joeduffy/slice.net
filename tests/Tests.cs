//===--- Tests.cs ---------------------------------------------------------===//
//
// Copyright (c) 2015 Joe Duffy. All rights reserved.
//
// This file is distributed under the MIT License. See LICENSE.md for details.
//
//===----------------------------------------------------------------------===//

using System;

class Tests
{
    public bool TestCreateOverArray(Tester t)
    {
        for (int i = 0; i < 2; i++) {
            var ints = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            // Try out two ways of creating a slice:
            Slice<int> slice;
            if (i == 0) {
                slice = new Slice<int>(ints);
            }
            else {
                slice = ints.Slice();
            }
            t.AssertEqual(ints.Length, slice.Length);

            // Now try out two ways of walking the slice's contents:
            for (int j = 0; j < ints.Length; j++) {
                t.AssertEqual(ints[j], slice[j]);
            }
            {
                int j = 0;
                foreach (var x in slice) {
                    t.AssertEqual(ints[j], x);
                    j++;
                }
            }
        }
        return true;
    }

    public bool TestCreateOverString(Tester t)
    {
        var str = "Hello, Slice!";
        Slice<char> slice = str.Slice();
        t.AssertEqual(str.Length, slice.Length);

        // Now try out two ways of walking the slice's contents:
        for (int j = 0; j < str.Length; j++) {
            t.AssertEqual(str[j], slice[j]);
        }
        {
            int j = 0;
            foreach (var x in slice) {
                t.AssertEqual(str[j], x);
                j++;
            }
        }
        return true;
    }

    public bool TestCreateOverPointer(Tester t)
    {
        unsafe {
            byte* buffer = stackalloc byte[256];
            for (int i = 0; i < 256; i++) { buffer[i] = (byte)i; }
            Slice<byte> slice = new Slice<byte>(buffer, 256);
            t.AssertEqual(256, slice.Length);

            // Now try out two ways of walking the slice's contents:
            for (int j = 0; j < slice.Length; j++) {
                t.AssertEqual(buffer[j], slice[j]);
            }
            {
                int j = 0;
                foreach (var x in slice) {
                    t.AssertEqual(buffer[j], x);
                    j++;
                }
            }
        }
        return true;
    }

    public bool TestSubslice(Tester t)
    {
        var slice = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }.Slice();

        // First a simple subslice over the whole array, using start.
        {
            var subslice1 = slice.Sub(0);
            t.AssertEqual(slice.Length, subslice1.Length);
            for (int i = 0; i < slice.Length; i++) {
                t.AssertEqual(slice[i], subslice1[i]);
            }
        }

        // Next a simple subslice over the whole array, using start and end.
        {
            var subslice2 = slice.Sub(0, slice.Length);
            t.AssertEqual(slice.Length, subslice2.Length);
            for (int i = 0; i < slice.Length; i++) {
                t.AssertEqual(slice[i], subslice2[i]);
            }
        }

        // Now do something more interesting; just take half the array.
        {
            int mid = slice.Length/2;
            var subslice3 = slice.Sub(mid);
            t.AssertEqual(mid, subslice3.Length);
            for (int i = mid, j = 0; i < slice.Length; i++, j++) {
                t.AssertEqual(slice[i], subslice3[j]);
            }
        }
 
        // Now take a hunk out of the middle.
        {
            int st = 3;
            int ed = 7;
            var subslice4 = slice.Sub(st, ed);
            t.AssertEqual(ed-st, subslice4.Length);
            for (int i = ed, j = 0; i < ed; i++, j++) {
                t.AssertEqual(slice[i], subslice4[j]);
            }
        }

        return true;
    }

    public bool TestCast(Tester t)
    {
        var ints = new int[100000];
        Random r = new Random(42324232);
        for (int i = 0; i < ints.Length; i++) { ints[i] = r.Next(); }
        var bytes = ints.Slice().Cast<int, byte>();
        t.Assert(bytes.Length == ints.Length * sizeof(int));
        for (int i = 0; i < ints.Length; i++) {
            t.AssertEqual(bytes[i*4], (ints[i]&0xff));
            t.AssertEqual(bytes[i*4+1], (ints[i]>>8&0xff));
            t.AssertEqual(bytes[i*4+2], (ints[i]>>16&0xff));
            t.AssertEqual(bytes[i*4+3], (ints[i]>>24&0xff));
        }
        return true;
    }

    public bool TestRangeCheckAlwaysThrowsArgumentOutOfRangeExceptionForNegativeValues(Tester tester)
    {
        var slice = new[] { 0, 1, 2, 3 }.Slice();

        unchecked {
            tester.Throws<ArgumentOutOfRangeException>(() => { int x = slice[-1]; });
        }
        checked {
            tester.Throws<ArgumentOutOfRangeException>(() => { int x = slice[-1]; });
        }

        return true;
    }
}

