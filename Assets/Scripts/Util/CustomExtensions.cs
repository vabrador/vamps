using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomExtensions {

    public static class FloatExtensions {
        public static float ZMap(this float f, float min, float max, float mapMin, float mapMax) {
            return mapMin + (Mathf.Max(min, Mathf.Min(max, f)) - min) / (max - min) * (mapMax - mapMin);
        }
    }

    public static class StringExtensions {
        
        public static bool ContainsAny(this String str, string[] matchers)
        {
            foreach (string matcher in matchers) {
                if (str.Contains(matcher)) {
                    return true;
                }
            }
            return false;
        }

        public static IEnumerable<int> ParseInts(this IEnumerable<string> strs) {
            List<int> ints = new List<int>();
            foreach (string str in strs) {
                ints.Add(int.Parse(str));
            }
            return ints;
        }

        public static string ArrayToString(this string[] strs) {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            int i = 0;
            int c = strs.Length;
            foreach (string str in strs) {
                sb.Append(str);
                i++;
                if (i < c) {
                    sb.Append(", ");
                }
            }
            sb.Append("]");
            return sb.ToString();
        }
        public static string ArrayToString(this int[] ints) {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            int i = 0;
            int c = ints.Length;
            foreach (int integer in ints) {
                sb.Append(integer);
                i++;
                if (i < c) {
                    sb.Append(", ");
                }
            }
            sb.Append("]");
            return sb.ToString();
        }
        public static string ArrayToString(this char[] chars) {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            int i = 0;
            int c = chars.Length;
            foreach (char ch in chars) {
                if (ch != '\0') {
                    sb.Append(ch);
                    i++;
                    if (i < c) {
                        sb.Append(", ");
                    }
                }
            }
            sb.Append("]");
            return sb.ToString();
        }
        public static string ArrayToString<T>(this T[] Ts) {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            int i = 0;
            int c = Ts.Length;
            foreach (T t in Ts) {
                sb.Append(t);
                i++;
                if (i < c) {
                    sb.Append(", ");
                }
            }
            sb.Append("]");
            return sb.ToString();
        }

    }

}
