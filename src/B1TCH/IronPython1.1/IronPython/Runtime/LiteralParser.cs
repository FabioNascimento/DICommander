/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Text;
using System.Reflection;
using System.Diagnostics;

using IronPython.Runtime;
using IronPython.Compiler;
using IronPython.Runtime.Operations;

using IronMath;

namespace IronPython.Runtime {
    /// <summary>
    /// Summary description for ConstantValue.
    /// </summary>
    static class LiteralParser {
        public static string ParseString(string text, bool isRaw, bool isUni) {
            return ParseString(text, isRaw, isUni, true);
        }
        public static string ParseString(string text, bool isRaw, bool isUni, bool complete) {
            if (isRaw && !isUni) return text;

            //PERFORMANCE-ISSUE ??? maybe optimize for the 0-escapes case
            StringBuilder buf = new StringBuilder(text.Length);
            int i = 0;
            int l = text.Length;
            int val;
            while (i < l) {
                char ch = text[i++];
                if (ch == '\\') {
                    if (!complete && i >= text.Length) break;
                    if (i >= l) throw Ops.ValueError("Trailing \\ in string");
                    ch = text[i++];

                    if (ch == 'u' || ch == 'U') {
                        int len = (ch == 'u') ? 4 : 8;
                        int max = 16;
                        if (isUni) {
                            if (TryParseInt(text, i, len, max, out val)) {
                                buf.Append((char)val);
                                i += len;
                            } else {
                                throw Ops.UnicodeDecodeError(@"'unicodeescape' codec can't decode bytes in position {0}: truncated \uXXXX escape", i);
                            }
                        } else {
                            buf.Append('\\');
                            buf.Append(ch);
                        }
                    } else {
                        if (isRaw) {
                            buf.Append('\\');
                            buf.Append(ch);
                            continue;
                        }
                        switch (ch) {
                            case 'a': buf.Append('\a'); continue;
                            case 'b': buf.Append('\b'); continue;
                            case 'f': buf.Append('\f'); continue;
                            case 'n': buf.Append('\n'); continue;
                            case 'r': buf.Append('\r'); continue;
                            case 't': buf.Append('\t'); continue;
                            case 'v': buf.Append('\v'); continue;
                            case '\\': buf.Append('\\'); continue;
                            case '\'': buf.Append('\''); continue;
                            case '\"': buf.Append('\"'); continue;
                            case '\r': if (i < l && text[i] == '\n') i++; continue;
                            case '\n': continue;
                            case 'x': //hex
                                if (!TryParseInt(text, i, 2, 16, out val)) {
                                    goto default;
                                }
                                buf.Append((char)val);
                                i += 2;
                                continue;
                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7': {
                                    int onechar;
                                    val = ch - '0';
                                    if (i < l && HexValue(text[i], out onechar) && onechar < 8) {
                                        val = val * 8 + onechar;
                                        i++;
                                        if (i < l && HexValue(text[i], out onechar) && onechar < 8) {
                                            val = val * 8 + onechar;
                                            i++;
                                        }
                                    }
                                }

                                buf.Append((char)val);
                                continue;
                            default:
                                buf.Append("\\");
                                buf.Append(ch);
                                continue;
                        }
                    }
                } else {
                    buf.Append(ch);
                }
            }

            return buf.ToString();
        }

        private static bool HexValue(char ch, out int value) {
            switch (ch) {
                case '0':
                case '\x660': value = 0; break;
                case '1':
                case '\x661': value = 1; break;
                case '2':
                case '\x662': value = 2; break;
                case '3':
                case '\x663': value = 3; break;
                case '4':
                case '\x664': value = 4; break;
                case '5':
                case '\x665': value = 5; break;
                case '6':
                case '\x666': value = 6; break;
                case '7':
                case '\x667': value = 7; break;
                case '8':
                case '\x668': value = 8; break;
                case '9':
                case '\x669': value = 9; break;
                case 'a':
                case 'A': value = 10; break;
                case 'b':
                case 'B': value = 11; break;
                case 'c':
                case 'C': value = 12; break;
                case 'd':
                case 'D': value = 13; break;
                case 'e':
                case 'E': value = 14; break;
                case 'f':
                case 'F': value = 15; break;
                default:
                    value = -1;
                    return false;
            }
            return true;
        }

        private static int HexValue(char ch) {
            int value;
            if (!HexValue(ch, out value)) {
                throw new ArgumentException("bad char for integer value: " + ch);
            }
            return value;
        }

        private static int CharValue(char ch, int b) {
            int val = HexValue(ch);
            if (val >= b) {
                throw new ArgumentException(String.Format("bad char for the integer value: '{0}' (base {1})", ch, b));
            }
            return val;
        }

        private static int ParseInt(string text, int b) {
            int ret = 0;
            int m = 1;
            for (int i = text.Length - 1; i >= 0; i--) {
                checked {
                    ret += m * CharValue(text[i], b);  // this is more generous than needed
                    m *= b;                            // is this optimized???
                }
            }
            return ret;
        }

        private static bool TryParseInt(string text, int start, int length, int b, out int value) {
            value = 0;
            if (start + length > text.Length) {
                return false;
            }
            for (int i = start, end = start + length; i < end; i++) {
                int onechar;
                if (HexValue(text[i], out onechar) && onechar < b) {
                    value = value * b + onechar;
                } else {
                    return false;
                }
            }
            return true;
        }

        public static object ParseInteger(string text, int b) {
            if (b == 0 && text.StartsWith("0x")) {
                int shift = 0;
                int ret = 0;
                for (int i = text.Length - 1; i >= 2; i--) {
                    ret |= HexValue(text[i]) << shift;
                    shift += 4;
                }
                return ret;
            }

            if (b == 0) b = DetectRadix(ref text);
            try {
                return ParseInt(text, b);
            } catch (OverflowException) {
                BigInteger ret = ParseBigInteger(text, b);
                int iret;
                if (ret.AsInt32(out iret)) return iret;
                return ret;
            }
        }

        public static object ParseIntegerSign(string text, int b) {
            int start = 0, end = text.Length, saveb = b;
            short sign = 1;

            if (b < 0 || b == 1 || b > 36) {
                throw new ArgumentException("base must be >= 2 and <= 36");
            }

            ParseIntegerStart(text, ref b, ref start, end, ref sign);

            int ret = 0;
            try {
                int saveStart = start;
                for (; ; ) {
                    int digit;
                    if (start >= end) {
                        if (saveStart == start) {
                            throw new ArgumentException("Invalid integer literal");
                        }
                        break;
                    }
                    if (!HexValue(text[start], out digit)) break;
                    if (!(digit < b)) {
                        throw new ArgumentException("Invalid integer literal");
                    }

                    checked {
                        // include sign here so that System.Int32.MinValue won't overflow
                        ret = ret * b + sign * digit;
                    }
                    start++;
                }
            } catch (OverflowException) {
                return ParseBigIntegerSign(text, saveb);
            }

            ParseIntegerEnd(text, start, end);

            return ret;
        }

        private static void ParseIntegerStart(string text, ref int b, ref int start, int end, ref short sign) {
            //  Skip whitespace
            while (start < end && Char.IsWhiteSpace(text, start)) start++;
            //  Sign?
            if (start < end) {
                switch (text[start]) {
                    case '-':
                        sign = -1;
                        goto case '+';
                    case '+':
                        start++;
                        break;
                }
            }
            //  Skip whitespace
            while (start < end && Char.IsWhiteSpace(text, start)) start++;

            //  Determine base
            if (b == 0) {
                if (start < end && text[start] == '0') {
                    start++;
                    // Hex or oct
                    if (start < end && (text[start] == 'x' || text[start] == 'X')) {
                        start++;
                        b = 16;
                    } else {
                        b = 8;
                    }
                } else {
                    b = 10;
                }
            }
        }

        private static void ParseIntegerEnd(string text, int start, int end) {
            //  Skip whitespace
            while (start < end && Char.IsWhiteSpace(text, start)) start++;

            if (start < end) {
                throw new ArgumentException("invalid integer number literal");
            }
        }

        private static int DetectRadix(ref string s) {
            if (s.StartsWith("0x") || s.StartsWith("0X")) {
                s = s.Substring(2);
                return 16;
            } else if (s.StartsWith("0")) {
                return 8;
            } else {
                return 10;
            }
        }

        public static BigInteger ParseBigInteger(string text, int b) {
            if (b == 0) b = DetectRadix(ref text);
            BigInteger ret = BigInteger.Zero;
            BigInteger m = BigInteger.One;

            int i = text.Length - 1;
            if (text[i] == 'l' || text[i] == 'L') i -= 1;

            int groupMax = 7;
            if (b <= 10) groupMax = 9;// 2 147 483 647

            while (i >= 0) {
                // extract digits in a batch
                int smallMultiplier = 1;
                uint uval = 0;

                for (int j = 0; j < groupMax && i >= 0; j++) {
                    uval = (uint)(CharValue(text[i--], b) * smallMultiplier + uval);
                    smallMultiplier *= b;
                }

                // this is more generous than needed
                ret += m * BigInteger.Create(uval);
                if (i >= 0) m = m * (smallMultiplier);
            }

            return ret;
        }

        public static BigInteger ParseBigIntegerSign(string text, int b) {
            int start = 0, end = text.Length;
            short sign = 1;

            if (b < 0 || b == 1 || b > 36) {
                throw new ArgumentException("base must be >= 2 and <= 36");
            }

            ParseIntegerStart(text, ref b, ref start, end, ref sign);

            BigInteger ret = BigInteger.Zero;
            for (; ; ) {
                int digit;
                if (start >= end) break;
                if (!HexValue(text[start], out digit)) break;
                if (!(digit < b)) {
                    throw new ArgumentException("Invalid integer literal");
                }
                ret = ret * b + digit;
                start++;
            }

            if (start < end && (text[start] == 'l' || text[start] == 'L')) {
                start++;
            }

            ParseIntegerEnd(text, start, end);

            return sign < 0 ? -ret : ret;
        }


        public static double ParseFloat(string text) {
            try {
                //
                // Strings that end with '\0' is the specific case that CLR libraries allow,
                // however Python doesn't. Since we use CLR floating point number parser,
                // we must check explicitly for the strings that end with '\0'
                //
                if (text != null && text.Length > 0 && text[text.Length - 1] == '\0') {
                    throw Ops.ValueError("null byte in float literal");
                }
                return ParseFloatNoCatch(text);
            } catch (OverflowException) {
                return Double.PositiveInfinity;
            }
        }

        private static double ParseFloatNoCatch(string text) {
            return double.Parse(text, System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //??? is this fully Python compatible
        }

        public static Complex64 ParseComplex64(string text) {
            // remove no-meaning spaces
            text = text.Trim();
            if (text == string.Empty) throw Ops.ValueError("complex() arg is an empty string");
            if (text.IndexOf(' ') != -1) throw Ops.ValueError("complex() arg is a malformed string");

            try {
                int len = text.Length;

                char lastChar = text[len - 1];
                if (lastChar != 'j' && lastChar != 'J')
                    return Complex64.MakeReal(ParseFloatNoCatch(text));

                // search for sign char for the imaginary part
                int signPos = text.LastIndexOfAny(new char[] { '+', '-' });

                // it is possible the sign belongs to 'e'
                if (signPos > 1) {
                    char prevChar = text[signPos - 1];
                    if (prevChar == 'e' || prevChar == 'E') {
                        signPos = text.Substring(0, signPos - 1).LastIndexOfAny(new char[] { '+', '-' });
                    }
                }

                if (signPos == -1) {
                    // special: "j"
                    return Complex64.MakeImaginary(len == 1 ? 1 : ParseFloatNoCatch(text.Substring(0, len - 1)));
                } else {
                    // special: "+j", "-j"
                    return new Complex64(
                        signPos == 0 ? 0 : ParseFloatNoCatch(text.Substring(0, signPos)),
                        (len == signPos + 2) ? (text[signPos] == '+' ? 1 : -1) : ParseFloatNoCatch(text.Substring(signPos, len - signPos - 1)));
                }

            } catch (OverflowException) {
                throw Ops.ValueError("complex() literal too large to convert");
            } catch {
                throw Ops.ValueError("complex() arg is a malformed string");
            }
        }

        public static Complex64 ParseImaginary(string text) {
            return Complex64.MakeImaginary(double.Parse(
                text.Substring(0, text.Length - 1),
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat
                ));
        }
    }
}
