using System;

class Program
{
    public static void Main()
    {
        {
            var ints = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var slice = new Slice<int>(ints);
            PrintSlice(slice);
            var subslice1 = slice.Sub(5);
            PrintSlice(subslice1);
            var subslice2 = slice.Sub(3, 7);
            PrintSlice(subslice2);
            var bytes = ints.Slice().Cast<int, byte>();
            PrintSlice(bytes);
        }
        {
            var str = "Hello, world";
            var slice = str.Slice();
            PrintSlice(slice);
            slice = slice.Sub(7);
            PrintSlice(slice);
        }
        unsafe {
            int* ints = stackalloc int[10];
            for (int i = 0; i < 10; i++) {
                ints[i] = i;
            }
            var slice = new Slice<int>(ints, 10);
            PrintSlice(slice);
        }
    }

    static void PrintSlice<T>(Slice<T> slice)
    {
#if GC_STRESS
        for (int i = 0; i < 100; i++) {
            for (int j = 0; j < 1000000; j++) {
                new Object();
            }
            GC.Collect(2);
#endif
            Console.Write("#{0}\t", slice.Length);
            for (int k = 0; k < slice.Length; k++) {
                Console.Write("{0} ", slice[k]);
            }
            Console.WriteLine();
#if GC_STRESS
        }
#endif
    }
}

