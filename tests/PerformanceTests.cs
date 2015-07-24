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
    private const int ElementsCount = 10000;

    /// <summary>
    /// the JIT compiler eliminates bounds check for some standard loops i.e. (int j = 0; j &lt; array.Length; j++) { sum += array[j] }
    /// the purpose of this test is to show how big the difference is because of that
    /// and hopefully in the future prove that we gained similar performance
    /// </summary>
    public bool TestPerfOfStandardBoundariesForLoop(Tester tester)
    {
        ExecuteAndMeasure(
            GenerateRandomNumbers(ElementsCount),
            array => {
                int sum = 0;
                for (int j = 0; j < array.Length; j++) {
                    sum += array[j];
                }
                return sum;
            });

        ExecuteAndMeasure(
           GenerateRandomNumbers(ElementsCount).Slice(),
           slice => {
                int sum = 0;
                for (int j = 0; j < slice.Length; j++) {
                    sum += slice[j];
               }
               return sum;
           });

        ExecuteAndMeasure(
           GenerateRandomNumbers(ElementsCount).ToList(),
           list => {
                int sum = 0;
                for (int j = 0; j < list.Count; j++) {
                    sum += list[j];
                }
               return sum;
           });

        return true;
    }

    /// <summary>
    /// the JIT compiler does not eliminate bounds check for nonstandard loops 
    /// i.e. (int j = 0; j &lt; array.Length / 2; j++) { sum += array[j + 1] }
    /// the purpose of this test is to show how fast Slice is when compared 
    /// to array and list that do not get any extra support from CLR
    /// </summary>
    public bool TestPerfOfNonStandardBoundariesForLoop(Tester tester)
    {
        ExecuteAndMeasure(
            GenerateRandomNumbers(ElementsCount),
            array => {
                int sum = 0;
                for (int j = 0; j < array.Length / 2; j++) {
                    sum += array[j * 2];
                }
                return sum;
            });

        ExecuteAndMeasure(
           GenerateRandomNumbers(ElementsCount).Slice(),
           slice => {
                int sum = 0;
                for (int j = 0; j < slice.Length / 2; j++) {
                    sum += slice[j * 2];
                }
                return sum;
           });

        ExecuteAndMeasure(
           GenerateRandomNumbers(ElementsCount).ToList(),
           list => {
                int sum = 0;
                for (int j = 0; j < list.Count / 2; j++) {
                    sum += list[j * 2];
                }
                return sum;
           });

        return true;
    }

    /// <summary>
    /// the compiler optimizes foreach loops for:
    ///  1) built in collections like arrays
    ///  2) these collections that do have public method "GetEnumerator that takes no parameters and returns a type that has two members:
    ///     a) a method MoveNext that takes no parameters and return a Boolean, and 
    ///     b) a property Current with a getter that returns an Object"
    ///     source: http://blogs.msdn.com/b/kcwalina/archive/2007/07/18/ducknotation.aspx
    /// the gain is that is does not call interface members on structures which is expensive
    /// and does not put it into try/finally block with Dispose in the finally
    /// the purpose of this test is to show how big the difference is because of that
    /// and hopefully in the future prove that we gained similar performance
    /// </summary>
    public bool TestPerfOfForEachLoop(Tester tester)
    {
        ExecuteAndMeasure(
            GenerateRandomNumbers(ElementsCount),
            array => {
                int sum = 0;
                foreach (var number in array) {
                    sum += number;
                }
                return sum;
            });

        ExecuteAndMeasure(
           GenerateRandomNumbers(ElementsCount).Slice(),
            slice => {
                int sum = 0;
                foreach (var number in slice) {
                    sum += number;
                }
                return sum;
           });

        ExecuteAndMeasure(
           GenerateRandomNumbers(ElementsCount).ToList(),
            list => {
                int sum = 0;
                foreach (var number in list) {
                    sum += number;
                }
                return sum;
           });

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
        ExecuteAndMeasure(
            GenerateRandomNumbers(ElementsCount),
            Sum);

        ExecuteAndMeasure(
            GenerateRandomNumbers(ElementsCount).Slice(),
            slice => Sum(slice));

        ExecuteAndMeasure(
           GenerateRandomNumbers(ElementsCount).ToList(),
           Sum);

        return true;
    }

    private static int ExecuteAndMeasure<T>(T data, Func<T, int> test)
    {
        Tester.CleanUpMemory();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        int sum = 0;

        for (int i = 0; i < ElementsCount; i++) {
            sum += test.Invoke(data);;
        }

        stopwatch.Stop();
        Console.WriteLine("    - {0}:  {1}", new string(typeof(T).Name.Take(5).ToArray()), stopwatch.Elapsed);
        return sum;
    }

    /// <summary>
    /// we generate new set of data every time just to make sure that each of them is cached in CPU cached in similar way
    /// </summary>
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
    private static int Sum(IEnumerable<int> numbers)
    {
        int sum = 0;
        foreach (var number in numbers) {
            sum += number;
        }
        return sum;
    }
}

