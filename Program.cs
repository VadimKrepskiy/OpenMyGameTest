using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TaskExam
{
    public static class MultiSet
    {
        public static void Add(this Dictionary<char, int> dict, string word)
        {
            for (int i = 0; i < word.Length; ++i)
            {
                dict.TryGetValue(word[i], out int value);
                dict[word[i]] = value + 1;
            }
        }
        public static bool IsSubset(this Dictionary<char, int> dict, Dictionary<char, int> subset)
        {
            foreach (var item in subset)
                if (!dict.ContainsKey(item.Key) || dict[item.Key] < item.Value)
                    return false;
            return true;
        }
    }
    public static class TwoDimensionalArray
    {
        public static KeyValuePair<int,int> Find<T>(this T[][] array, T value) where T: IEqualityOperators<T,T,bool>
        {
            for (int i = 0; i < array.Length; ++i)
                for (int j = 0; j < array[i].Length; ++j)
                    if (array[i][j] == value)
                        return new KeyValuePair<int, int>(i, j);
            return new KeyValuePair<int, int>(-1, -1);
        }
    }
    internal class TaskSolver
    {
        public static void Main(string[] args)
        {
            TestGenerateWordsFromWord();
            TestMaxLengthTwoChar();
            TestGetPreviousMaxDigital();
            TestSearchQueenOrHorse();
            TestCalculateMaxCoins();
            Console.WriteLine("All Test completed!");
        }
        public static List<string> GenerateWordsFromWord(string word, List<string> wordDictionary)
        {
            Dictionary<char, int> keyWordLetters = new(){ word };
            List<string> result = new();
            foreach (var item in wordDictionary)
            {
                Dictionary<char, int> itemLetters = new() { item };
                if (keyWordLetters.IsSubset(itemLetters))
                    result.Add(item);
            }
            result.Sort();
            return result;
        }
        /// задание 2) Два уникальных символа
        public static int MaxLengthTwoChar(string word)
        {
            HashSet<char> letterSet = new(word);
            if (letterSet.Count == 1)
                return 0;
            char[] letterArray = new char[letterSet.Count];
            letterSet.CopyTo(letterArray);
            int result = 0;
            for (int i = 0; i < letterArray.Length-1; ++i) {//be bea
                for (int j = 1; j < letterArray.Length - i; ++j)
                {
                    var correctWord = word;
                    bool isCorrect = true;
                    for (int k = 0; k < i; ++k)
                        correctWord = correctWord.Replace(letterArray[k].ToString(),"");
                    for (int k = i + 1; k < j; ++k)
                        correctWord = correctWord.Replace(letterArray[k].ToString(), "");
                    for (int k = j+1; k < letterArray.Length; ++k)
                        correctWord = correctWord.Replace(letterArray[k].ToString(), "");
                    for(int k = 1; k < correctWord.Length; ++k)
                        if (correctWord[k-1] == correctWord[k])
                        {
                            isCorrect = false;
                            break;
                        }
                    if (isCorrect && result < correctWord.Length)
                        result = correctWord.Length;
                }
            }
            return result;
        }
        /// задание 3) Предыдущее число
        public static long GetPreviousMaxDigital(long value)
        {
            int[] digitals = new int[(int)(Math.Log10(value)+1)];
            digitals[^1] = (int)value % 10;
            value /= 10;
            bool isLeast = true;
            int i = digitals.Length;
            int j;
            for (int k = digitals.Length-2; k > -1; --k)
            {
                digitals[k] = (int)value % 10;
                value /= 10;
                if (isLeast && digitals[k + 1] < digitals[k])
                {
                    isLeast= false;
                    i = k+1;
                    j = digitals.Length;
                    while (i < j && digitals[i-1] <= digitals[--j]) ;
                    Swap(digitals, i - 1, j);
                }
            }
            if (isLeast || digitals[0] == 0)
                return -1;
            j = digitals.Length;
            while (i < --j)
                Swap(digitals, i++, j);
            long result = digitals[^1];
            for (int k = digitals.Length - 2; k > -1; --k)
                result += (long)Math.Pow(10, digitals.Length - k - 1)* digitals[k];
            return result;
        }
        private static void Swap(int[] arr, int i, int j)
        {
            arr[i] += arr[j];
            arr[j] = arr[i] - arr[j];
            arr[i] = arr[i] - arr[j];
        }
        private class Cage
        {
            public int X { get; private set; }
            public int Y { get; private set; }
            public int Dist { get; private set; }
            public Cage(int x, int y, int dist = 0)
            {
                X = x;
                Y = y;
                Dist = dist;
            }
            public Cage(KeyValuePair<int,int> pos) : this(pos.Key, pos.Value) { }
            public override bool Equals(object? obj)
            {
                return obj is Cage node && X == node.X &&
                        Y == node.Y;//&&Dist == node.Dist;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(X, Y);
            }
        }
        private static bool IsValid(int x, int y, char[][] array)
        {
            return x >= 0 && x < array.Length && y >= 0 && y < array[0].Length && array[x][y] != 'x';
        }
        private static int BFS(char[][] gridMap, int[,] steps, Cage start, Cage end, int maxStepLength = 1)
        {
            HashSet<Cage> visited = new();
            Queue<Cage> queue = new();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var cage = queue.Dequeue();
                if (cage.X == end.X && cage.Y == end.Y)
                    return cage.Dist;
                if (!visited.Contains(cage))
                {
                    visited.Add(cage);
                    for (int i = 0; i < steps.GetUpperBound(0) + 1; ++i)
                        for (int l = 1; l < maxStepLength+1; ++l)
                        {
                            int x = cage.X + steps[i, 0]*l, y = cage.Y + steps[i, 1]*l;
                            if (IsValid(x, y, gridMap))
                                queue.Enqueue(new Cage(x, y, cage.Dist + 1));
                            else break;
                        }
                }
            }
            return -1;
        }
        /// задание 4) Конь и Королева
        public static List<int> SearchQueenOrHorse(char[][] gridMap)
        {
            int[,] horseSteps = new int[,] { { 2, 1 },{ 2, -1 },{ -2, 1 },{ -2, -1 },{ 1, 2},{ 1, -2 },{ -1, 2 },{ -1, -2 } };
            int[,] queenSteps = new int[,] { { 1, 0}, { -1, 0 }, { 0, 1 }, { 0, -1 }, { 1, 1}, { 1, -1 }, { -1, 1}, { -1, -1 } };
            Cage start = new(gridMap.Find('s')), end = new(gridMap.Find('e'));
            int horseCount = BFS(gridMap, horseSteps, start, end);
            int queenCount = BFS(gridMap, queenSteps, start, end, Math.Max(gridMap.Length, gridMap[0].Length)-1);
            return new List<int> { horseCount, queenCount };
        }

        /// задание 5) Жадина
        public static long CalculateMaxCoins(int[][] mapData, int idStart, int idFinish)
        {
            int n = 0;
            for (int i = 0; i < mapData.Length; ++i)
            {
                if (n < mapData[i][1])
                    n = mapData[i][1];
                if (n < mapData[i][0])
                    n = mapData[i][0];
            }
            ++n;
            List<KeyValuePair<int,int>>[] graph = new List<KeyValuePair<int, int>>[n];
            for (int i = 0; i < n; ++i)
                graph[i] = new List<KeyValuePair<int, int>>();
            for(int i = 0; i < mapData.Length; ++i)
            {
                if (mapData[i][0] != idFinish && mapData[i][1] != idStart)
                    graph[mapData[i][0]].Add(new KeyValuePair<int, int>(mapData[i][1], mapData[i][2]));
                if (mapData[i][1] != idFinish && mapData[i][0] != idStart)
                    graph[mapData[i][1]].Add(new KeyValuePair<int, int>(mapData[i][0], mapData[i][2]));
            }
            long[] weights = new long[n];
            weights[idStart] = 1;
            bool[] visited = new bool[n];
            for (int i = 0; i < graph[idStart].Count; ++i)
                weights[graph[idStart][i].Key] = graph[idStart][i].Value;
            visited[idStart] = true;
            for (int i = 1; i < n; ++i)
            {
                int vertex = -1;
                for (int j = 0; j < n; ++j)
                    if (!visited[j] && (vertex == -1 || weights[j] > weights[vertex]))
                        vertex = j;
                if (weights[vertex] == 0)
                    break;
                visited[vertex] = true;

                foreach(var item in graph[vertex])
                {
                    if (weights[vertex] + item.Value > weights[item.Key])
                        weights[item.Key] = weights[vertex] + item.Value;
                }
            }
            var result = weights[idFinish];
            return (result == 0) ? -1 : result;
        }

        /// Тесты (можно/нужно добавлять свои тесты) 

        private static void TestGenerateWordsFromWord()
        {
            var wordsList = new List<string>
            {
                "кот", "ток", "око", "мимо", "гром", "ром", "мама",
                "рог", "морг", "огр", "мор", "порог", "бра", "раб", "зубр"
            };
            AssertSequenceEqual(GenerateWordsFromWord("арбуз", wordsList), new[] { "бра", "зубр", "раб" });
            AssertSequenceEqual(GenerateWordsFromWord("лист", wordsList), new List<string>());
            AssertSequenceEqual(GenerateWordsFromWord("маг", wordsList), new List<string>());
            AssertSequenceEqual(GenerateWordsFromWord("погром", wordsList), new List<string> { "гром", "мор", "морг", "огр", "порог", "рог", "ром" });
        }

        private static void TestMaxLengthTwoChar()
        {
            AssertEqual(MaxLengthTwoChar("beabeeab"), 5);
            AssertEqual(MaxLengthTwoChar("а"), 0);
            AssertEqual(MaxLengthTwoChar("ab"), 2);
        }

        private static void TestGetPreviousMaxDigital()
        {
            AssertEqual(GetPreviousMaxDigital(21), 12l);
            AssertEqual(GetPreviousMaxDigital(531), 513l);
            AssertEqual(GetPreviousMaxDigital(1027), -1l);
            AssertEqual(GetPreviousMaxDigital(2071), 2017l);
            AssertEqual(GetPreviousMaxDigital(207034), 204730l);
            AssertEqual(GetPreviousMaxDigital(135), -1l);
        }

        private static void TestSearchQueenOrHorse()
        {
            char[][] gridA =
            {
                new[] {'s', '#', '#', '#', '#', '#'},
                new[] {'#', 'x', 'x', 'x', 'x', '#'},
                new[] {'#', '#', '#', '#', 'x', '#'},
                new[] {'#', '#', '#', '#', 'x', '#'},
                new[] {'#', '#', '#', '#', '#', 'e'},
            };

            AssertSequenceEqual(SearchQueenOrHorse(gridA), new[] { 3, 2 });

            char[][] gridB =
            {
                new[] {'s', '#', '#', '#', '#', 'x'},
                new[] {'#', 'x', 'x', 'x', 'x', '#'},
                new[] {'#', 'x', '#', '#', 'x', '#'},
                new[] {'#', '#', '#', '#', 'x', '#'},
                new[] {'x', '#', '#', '#', '#', 'e'},
            };

            AssertSequenceEqual(SearchQueenOrHorse(gridB), new[] { -1, 3 });

            char[][] gridC =
            {
                new[] {'s', '#', '#', '#', '#', 'x'},
                new[] {'x', 'x', 'x', 'x', 'x', 'x'},
                new[] {'#', '#', '#', '#', 'x', '#'},
                new[] {'#', '#', '#', 'e', 'x', '#'},
                new[] {'x', '#', '#', '#', '#', '#'},
            };

            AssertSequenceEqual(SearchQueenOrHorse(gridC), new[] { 2, -1 });


            char[][] gridD =
            {
                new[] {'e', '#'},
                new[] {'x', 's'},
            };

            AssertSequenceEqual(SearchQueenOrHorse(gridD), new[] { -1, 1 });

            char[][] gridE =
            {
                new[] {'e', '#'},
                new[] {'x', 'x'},
                new[] {'#', 's'},
            };

            AssertSequenceEqual(SearchQueenOrHorse(gridE), new[] { 1, -1 });

            char[][] gridF =
            {
                new[] {'x', '#', '#', 'x'},
                new[] {'#', 'x', 'x', '#'},
                new[] {'#', 'x', '#', 'x'},
                new[] {'e', 'x', 'x', 's'},
                new[] {'#', 'x', 'x', '#'},
                new[] {'x', '#', '#', 'x'},
            };

            AssertSequenceEqual(SearchQueenOrHorse(gridF), new[] { -1, 5 });
        }

        private static void TestCalculateMaxCoins()
        {
            var mapA = new[]
            {
                new []{0, 1, 1},
                new []{0, 2, 4},
                new []{0, 3, 3},
                new []{1, 3, 10},
                new []{2, 3, 6},
            };

            AssertEqual(CalculateMaxCoins(mapA, 0, 3), 11l);

            var mapB = new[]
            {
                new []{0, 1, 1},
                new []{1, 2, 53},
                new []{2, 3, 5},
                new []{5, 4, 10}
            };

            AssertEqual(CalculateMaxCoins(mapB, 0, 5), -1l);

            var mapC = new[]
            {
                new []{0, 1, 1},
                new []{0, 3, 2},
                new []{0, 5, 10},
                new []{1, 2, 3},
                new []{2, 3, 2},
                new []{2, 4, 7},
                new []{3, 5, 3},
                new []{4, 5, 8}
            };

            AssertEqual(CalculateMaxCoins(mapC, 0, 5), 19l);
        }

        /// Тестирующая система, лучше не трогать этот код

        private static void Assert(bool value)
        {
            if (value)
            {
                return;
            }

            throw new Exception("Assertion failed");
        }

        private static void AssertEqual(object value, object expectedValue)
        {
            if (value.Equals(expectedValue))
            {
                return;
            }

            throw new Exception($"Assertion failed expected = {expectedValue} actual = {value}");
        }

        private static void AssertSequenceEqual<T>(IEnumerable<T> value, IEnumerable<T> expectedValue)
        {
            if (ReferenceEquals(value, expectedValue))
            {
                return;
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (expectedValue is null)
            {
                throw new ArgumentNullException(nameof(expectedValue));
            }

            var valueList = value.ToList();
            var expectedValueList = expectedValue.ToList();

            if (valueList.Count != expectedValueList.Count)
            {
                throw new Exception($"Assertion failed expected count = {expectedValueList.Count} actual count = {valueList.Count}");
            }

            for (var i = 0; i < valueList.Count; i++)
            {
                if (!valueList[i].Equals(expectedValueList[i]))
                {
                    throw new Exception($"Assertion failed expected value at {i} = {expectedValueList[i]} actual = {valueList[i]}");
                }
            }
        }

    }

}