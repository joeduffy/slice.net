//===--- PerformanceTests.cs ---------------------------------------------------------===//
//
// Copyright (c) 2015 Joe Duffy. All rights reserved.
//
// This file is distributed under the MIT License. See LICENSE.md for details.
//
//===----------------------------------------------------------------------===//

using System;
using System.Collections.Generic;
using System.Linq;

class PerformanceTests
{
    /// <summary>
    /// the JIT compiler eliminates bounds check for some standard loops i.e. (int j = 0; j &lt; array.Length; j++) { sum += array[j] }
    /// the purpose of this test is to show how big the difference is because of that
    /// and hopefully in the future prove that we gained similar performance
    /// </summary>
    public bool TestPerfOfStandardBoundariesForLoop(Tester tester)
    {
        const int elementsCount = 10000;
        var array = GenerateRandomNumbers(elementsCount);

        tester.CleanUpMemory();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        int arraySum = 0;
        for (int i = 0; i < elementsCount; i++) {
            for (int j = 0; j < array.Length; j++) {
                arraySum += array[j];
            }
        }
        sw.Stop();
        Console.WriteLine("    - array of ints: {0}", sw.Elapsed);

        tester.CleanUpMemory();

        var slice = array.Slice();
        sw.Restart();
        int sliceSum = 0;
        for (int i = 0; i < elementsCount; i++) {
            for (int j = 0; j < slice.Length; j++) {
                sliceSum += slice[j];
            }
        }
        sw.Stop();
        Console.WriteLine("    - slice of ints: {0}", sw.Elapsed);

        tester.CleanUpMemory();

        var list = array.ToList();
        sw.Restart();
        int listSum = 0;
        for (int i = 0; i < elementsCount; i++) {
            for (int j = 0; j < list.Count; j++) {
                listSum += list[j];
            }
        }
        sw.Stop();
        Console.WriteLine("    - list of ints:  {0}", sw.Elapsed);

        tester.AssertEqual(arraySum, sliceSum);
        tester.AssertEqual(listSum, sliceSum);
        return true;
    }

    /// <summary>
    /// the JIT compiler does not eliminate bounds check for nonstandard loops i.e. (int j = 0; j &lt; array.Length / 2; j++) { sum += array[j + 1] }
    /// the purpose of this test is to show that Slice is as fast as Array in this case
    /// </summary>
    public bool TestPerfOfNonStandardBoundariesForLoop(Tester tester)
    {
        const int elementsCount = 10000;
        var array = GenerateRandomNumbers(elementsCount);

        tester.CleanUpMemory();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        int arraySum = 0;
        for (int i = 0; i < elementsCount; i++) {
            for (int j = 0; j < array.Length / 2; j++) {
                arraySum += array[j + 1];
            }
        }
        sw.Stop();
        Console.WriteLine("    - array of ints: {0}", sw.Elapsed);

        tester.CleanUpMemory();

        var slice = array.Slice();
        sw.Restart();
        int sliceSum = 0;
        for (int i = 0; i < elementsCount; i++) {
            for (int j = 0; j < slice.Length / 2; j++) {
                sliceSum += slice[j + 1];
            }
        }
        sw.Stop();
        Console.WriteLine("    - slice of ints: {0}", sw.Elapsed);

        tester.CleanUpMemory();

        var list = array.ToList();
        sw.Restart();
        int listSum = 0;
        for (int i = 0; i < elementsCount; i++) {
            for (int j = 0; j < list.Count / 2; j++) {
                listSum += list[j + 1];
            }
        }
        sw.Stop();
        Console.WriteLine("    - list of ints:  {0}", sw.Elapsed);

        tester.AssertEqual(arraySum, sliceSum);
        tester.AssertEqual(listSum, sliceSum);
        return true;
    }

    /// <summary>
    /// the compiler optimizes foreach loops for collections that do have public method 
    /// "GetEnumerator that takes no parameters and returns a type that has two members: 
    ///     a) a method MoveMext that takes no parameters and return a Boolean, and 
    ///     b) a property Current with a getter that returns an Object"
    /// source: http://blogs.msdn.com/b/kcwalina/archive/2007/07/18/ducknotation.aspx
    /// the gain is that is does not call interface members on structures which is expensive
    /// and does not put it into try/finally block with Dispose in the finally
    /// the purpose of this test is to show how big the difference is because of that
    /// and hopefully in the future prove that we gained similar performance
    /// </summary>
    public bool TestPerfOfForEachLoop(Tester tester)
    {
        const int elementsCount = 10000;
        var array = GenerateRandomNumbers(elementsCount);

        tester.CleanUpMemory();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        int arraySum = 0;
        for (int i = 0; i < elementsCount; i++) {
            foreach (int number in array) {
                arraySum += number;
            }
        }
        sw.Stop();
        Console.WriteLine("    - array of ints: {0}", sw.Elapsed);

        tester.CleanUpMemory();

        var slice = array.Slice();
        sw.Restart();
        int sliceSum = 0;
        for (int i = 0; i < elementsCount; i++) {
            foreach (int number in slice) {
                sliceSum += number;
            }
        }
        sw.Stop();
        Console.WriteLine("    - slice of ints: {0}", sw.Elapsed);

        tester.CleanUpMemory();

        var list = array.ToList();
        sw.Restart();
        int listSum = 0;
        for (int i = 0; i < elementsCount; i++) {
            foreach (int number in list) {
                listSum += number;
            }
        }
        sw.Stop();
        Console.WriteLine("    - list of ints:  {0}", sw.Elapsed);

        tester.AssertEqual(arraySum, sliceSum);
        tester.AssertEqual(listSum, sliceSum);
        return true;
    }

    /// <summary>
    /// the compiler does not optimize foreach loops for collections when they are used via interfaces
    /// it might call interface members on structures which is expensive
    /// and it does put it into try/finally block with Dispose in the finally
    /// the purpose of this test is to show how big the difference is because of that
    /// and hopefully in the future prove that we gained similar performance
    /// </summary>
    public bool TestPerfOfForEachLoopWhenUsedViaInterface(Tester tester)
    {
        const int elementsCount = 10000;
        var array = GenerateRandomNumbers(elementsCount);

        tester.CleanUpMemory();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        int arraySum = 0;
        for (int i = 0; i < elementsCount; i++) {
            arraySum += Sum(array);
        }
        sw.Stop();
        Console.WriteLine("    - array of ints: {0}", sw.Elapsed);

        tester.CleanUpMemory();

        var slice = array.Slice();
        sw.Restart();
        int sliceSum = 0;
        for (int i = 0; i < elementsCount; i++) {
            sliceSum += Sum(slice);
        }
        sw.Stop();
        Console.WriteLine("    - slice of ints: {0}", sw.Elapsed);

        tester.CleanUpMemory();

        var list = array.ToList();
        sw.Restart();
        int listSum = 0;
        for (int i = 0; i < elementsCount; i++) {
            listSum += Sum(list);
        }
        sw.Stop();
        Console.WriteLine("    - list of ints:  {0}", sw.Elapsed);

        tester.AssertEqual(arraySum, sliceSum);
        tester.AssertEqual(listSum, sliceSum);
        return true;
    }

    private static int[] GenerateRandomNumbers(int count)
    {
        var ints = new int[count];
        var random = new Random(1234);
        for (int i = 0; i < ints.Length; i++) {
            ints[i] = random.Next();
        }
        return ints;
    }

    /// <summary>
    /// iterates explicit on IEnumerable instead of calling .Sum() extension method, 
    /// which implementation might try to cast it to array etc. or use other tricks
    /// </summary>
    /// <param name="numbers"></param>
    /// <returns></returns>
    private static int Sum(IEnumerable<int> numbers)
    {
        int sum = 0;
        foreach (var number in numbers) {
            sum += number;
        }
        return sum;
    }
}

