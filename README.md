# Slice.NET

A simple C# type, Slice, that provides a uniform API for working with

- Arrays and subarrays
- Strings and substrings
- Unmanaged memory buffers

Slice is a struct so it adds no allocations beyond the memory you're viewing.
It is fully type- and memory-safe.

## Unform access to many kinds of contiguous memory

.NET gives you IEnumerable<T>; today as an abstraction that works across a wide
variety of collections, and IList<T> for indexable things.  There's no standard
way to access the notion of a "contiguous buffer," with zero overhead, however,
which is a common need in low-level systems programs.

Slice fills this need.  For example, to create one:

   // Over an array:
   Slice<int> ints = new Slice<int>(int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

   // Over a string (of chars):
   Slice<char> chars = new Slice<char>("Hello, Slice!");

   // Over an unmanaged memory buffer:
   byte\* bb = stackalloc byte[256];
   Slice<byte> bytes = new Slice<byte>(bb);

Now given a Slice, we can write APIs that work across all of these memory types.
For example, to print the characters we can use the Length plus indexer:

    void PrintSlice<T>(Slice<T> slice) {
        for (int i = 0; i < slice.Length; i++) {
            Console.Write("{0} ", slice[i]);
        }
        Console.WriteLine();
    }

Or even leverage the fact that Slice implements IEnumerable efficiently:

    void PrintSlice<T>(Slice<T> slice) {
        foreach (T t in slice) {
            Console.Write("{0} ", t);
        }
        Console.WriteLine();
    }

## Make subslices without allocations

Slice is a struct, so creating new ones is cheap.  Underneath the hood, it's
little more than a pointer plus offset.  As a result it's common for programs to
create subslices for a variety of tasks.

Maybe your calling an API and only want it to see a subset of the data:

    var ints = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    PrintSlice(ints.Sub(5)); // Only print the 2nd half of the array.

Or maybe you're parsing data out of a string and want to avoid tricky index
arithmetic, while at the same time not wanting to use String.Substring which
allocates memory each time you use it:

    var name = "Joe Duffy".Slice();
    int space = name.IndexOf(' ');
    var firstName = name.Sub(0, space);
    var lastName = name.Sub(space+1);

## Interpret untyped data w/out copying or allocating

It's relatively common in low-level systems programming to encounter untyped
data -- like byte buffers coming in off the network -- that a program wants to
interpret as a higher-level data structure.  This often requires copying or
allocating, a definite no-no in areas like the low-level networking stack.

To see how, let's say we're parsing a TCP header:

    [StructLayout(...)]
    struct TcpHeader {
        ushort SourcePort;
        ushort DestinationPort;
        ...
        ushort Checksum;
        ushort UrgentPointer;
    }

Our of a byte\* that came from the networking stack.  Easy enough:

    void HandleRequest(byte* payload, int length)
    {
        var slice = new Slice<byte>(payload, length);
        var header = slice.Read<TcpHeader>(); // Parses out the header.
        slice = slice.Sub(sizeof(TcpHeader));  // Advance to beyond the header.
        // Keep parsing ...
    }

## Establish safety boundaries for unsafe code

As we saw above, Slice can bridge the boundary between unsafe code using pointers,
and safe code written in terms of Slices.  HandleRequest above, for example, can
do a one time translation of a pointer/length pair into a Slice and -- provided
that code and its caller is correct about the length and lifetime of the memory --
all subsequent code is verified to be type- and memory-safe.  This reduces the
task of auditing unsafe code for potential security vulnerabilities.

A best practice is to make this transition explicit.  HandleRequest above might
be better off written more like this:

    unsafe void HandleRequestUnsafe(byte* payload, int length)
    {
        HandleRequest(new Slice<byte>(payload, length));
    }

    void HandleRequest(Slice<byte> payload)
    {
        // The meat goes here.  This is the (preferred) / advertised API.
    }

