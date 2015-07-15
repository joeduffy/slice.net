# Slice.NET

A simple C# type, Slice, that provides a uniform API for working with

- Arrays and subarrays
- Strings and substrings
- Unmanaged memory buffers

Slice is a struct so it adds no allocations beyond the memory you're viewing.
It is fully type- and memory-safe.

In the first example, we'll just view an array of integers using a Slice:

    var ints = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    var slice = new Slice<int>(ints);
    slice = ints.Slice(); // Alternatively.
    PrintSlice(slice);

Printing the slice's contents is as easy as using Slice's Length and indexer:

    void PrintSlice<T>(Slice<T> slice) {
        for (int i = 0; i < slice.Length; i++) {
            Console.Write("{0} ", slice[i]);
        }
        Console.WriteLine();
    }

As you might expect, the first example prints all integers in the range [0, 10).

In the next example, we'll slice that view in half:

    subslice = slice.Sub(5);
    PrintSlice(subslice);

This prints out numbers in the range [5, 10).  Alternatively, we can slice out
just a few elements out of the middle:

    subslice = slice.Sub(3, 7);
    PrintSlice(subslice);

This prints out numbers in the range [3, 7).

In the following example, we will view a string as a Slice of characters:

    var str = "Hello, Slice!";
    var slice = str.Slice();
    PrintSlice(slice);

This prints out the sequence of chars 'H', 'e', ..., 'e', '!'.  As with any
slice, we can create a sub-slice using `slice.Sub`.  Conveniently, both arrays
and strings have extension methods making it easy to do this in one step:

    var subslice = str.Slice(7, 12);
    PrintSlice(slice); // Prints "Slice".

Finally, we show forming a slice over some unsafe bytes.  Imagine we wanted to
stack-allocate an array, and then pass it down to type-safe APIs without them
knowing they are dealing with unsafe memory.  Easy!

    var ints = stackalloc int[10];
    for (int i = 0; i < 10; i++) ints[i] = i;
    var slice = new Slice<int>(ints, 10);
    PrintSlice(slice);

Like the earlier ints example, this prints out all numbers in the range [0, 10).
Notice, however, that PrintSlice didn't need to do anything unsafe.

