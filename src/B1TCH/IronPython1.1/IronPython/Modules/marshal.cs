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
using System.Collections;
using System.Collections.Generic;
using System.Text;

using IronMath;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;

[assembly: PythonModule("marshal", typeof(IronPython.Modules.PythonMarshal))]
namespace IronPython.Modules {
    [PythonType("marshal")]
    public class PythonMarshal {
        #region Public marshal APIs
        [PythonName("dump")]
        public static void Dump(object value, object file) {
            Dump(value, file, null);
        }

        [PythonName("dump")]
        public static void Dump(object value, object file, object version) {
            PythonFile pf = file as PythonFile;
            if (pf == null) throw Ops.TypeErrorForBadInstance("expected file, found '{0}'", file);

            pf.Write(DumpString(value, version));
        }

        [PythonName("load")]
        public static object Load(object file) {
            PythonFile pf = file as PythonFile;
            if (pf == null) throw Ops.TypeErrorForBadInstance("expected file, found '{0}'", file);

            return LoadString(pf.Read());
        }

        [PythonName("dumps")]
        public static object DumpString(object value) {
            return DumpString(value, null);
        }

        [PythonName("dumps")]
        public static string DumpString(object value, object version) {
            byte[] bytes = ObjectToBytes(value);
            StringBuilder sb = new StringBuilder(bytes.Length);
            for (int i = 0; i < bytes.Length; i++) {
                sb.Append((char)bytes[i]);
            }
            return sb.ToString();
        }

        [PythonName("loads")]
        public static object LoadString(string @string) {
            string strParam = @string;

            byte[] bytes = new byte[strParam.Length];
            for (int i = 0; i < strParam.Length; i++) {
                bytes[i] = (byte)strParam[i];
            }
            return BytesToObject(bytes);
        }

        public static object version = "1";
        #endregion

        #region Implementation details

        static byte[] ObjectToBytes(object o) {
            MarshalWriter mw = new MarshalWriter();
            mw.WriteObject(o);
            return mw.GetBytes();
        }

        static object BytesToObject(byte[] bytes) {
            MarshalReader mr = new MarshalReader(bytes);
            return mr.ReadObject();
        }

        /*********************************************************
         * Format 
         *  
         * Tuple: '(',int cnt,tuple items
         * List:  '[',int cnt, list items
         * Dict:  '{',key item, value item, '0' terminator
         * Int :  'i',4 bytes
         * float: 'f', 1 byte len, float in string
         * BigInt:  'l', int encodingSize
         *      if the value is negative then the size is negative
         *      and needs to be subtracted from int.MaxValue
         * 
         *      the bytes are encoded in 15 bit multiples, a size of
         *      0 represents a value of zero.
         * 
         * True: 'T'
         * False: 'F'
         * Float: 'f', str len, float in str
         * string: 't', int len, bytes  (ascii)
         * string: 'u', int len, bytes (unicode)
         * StopIteration: 'S'   
         * None: 'N'
         * Long: 'I' followed by 8 bytes (little endian 64-bit value)
         * complex: 'x', byte len, real str, byte len, imag str
         * Buffer (array.array too, but they don't round trip): 's', int len, buffer bytes
         * code: 'c', more stuff...
         * 
         */
        class MarshalWriter {
            List<byte> bytes;

            public MarshalWriter() {
                bytes = new List<byte>();
            }

            public void WriteObject(object o) {
                ArrayList infinite = Ops.GetReprInfinite();

                if (infinite.Contains(o)) throw Ops.ValueError("Marshaled data contains infinite cycle");

                int index = infinite.Add(o);
                try {
                    if (o == null) bytes.Add((byte)'N');
                    else if (o == Ops.TRUE) bytes.Add((byte)'T');
                    else if (o == Ops.FALSE) bytes.Add((byte)'F');
                    else if (o is string) WriteString(o as string);
                    else if (o is int) WriteInt((int)o);
                    else if (o is float) WriteFloat((float)o);
                    else if (o is double) WriteFloat((double)o);
                    else if (o is long) WriteLong((long)o);
                    else if (o is List) WriteList(o);
                    else if (o is Dict) WriteDict(o);
                    else if (o is Tuple) WriteTuple(o);
                    else if (o is BigInteger) WriteInteger((BigInteger)o);
                    else if (o is Complex64) WriteComplex((Complex64)o);
                    else if (o is PythonBuffer) WriteBuffer((PythonBuffer)o);
                    else if (o == ExceptionConverter.GetPythonException("StopIteration")) WriteStopIteration();
                    else throw Ops.ValueError("unmarshallable object");
                } finally {
                    infinite.RemoveAt(index);
                }
            }

            void WriteFloat(float f) {
                bytes.Add((byte)'f');
                WriteFloatString(f);
            }

            void WriteFloat(double f) {
                bytes.Add((byte)'f');
                WriteDoubleString(f);
            }

            void WriteFloatString(float f) {
                string s = f.ToString("G17");   // get maximum percision
                bytes.Add((byte)s.Length);
                for (int i = 0; i < s.Length; i++) {
                    bytes.Add((byte)s[i]);
                }
            }

            void WriteDoubleString(double d) {
                string s = d.ToString("G17");  // get maximum percision
                bytes.Add((byte)s.Length);
                for (int i = 0; i < s.Length; i++) {
                    bytes.Add((byte)s[i]);
                }
            }

            void WriteInteger(BigInteger val) {
                if (val == BigInteger.Zero) {
                    bytes.Add((byte)'l');
                    for(int i = 0; i<4; i++) bytes.Add(0);
                    return;
                }

                BigInteger mask = BigInteger.Create(short.MaxValue);
                uint startLen = (uint)val.Length;
                val = new BigInteger(val);

                bytes.Add((byte)'l');
                uint byteLen = ((startLen * 32) + 14) / 15; // len is in 32-bit multiples, we want 15-bit multiples
                bool fNeg = false;
                if (val < 0) {
                    fNeg = true;
                    val *= -1;
                }

                if (val <= short.MaxValue)
                    byteLen = 1;
                else if (val < (1 << 30)) {
                    byteLen = 2;
                }

                // write out length
                if (fNeg) {
                    WriteUInt32(uint.MaxValue - byteLen + 1);
                } else {
                    WriteUInt32(byteLen);
                }

                // write out value (15 bits at a time)
                while (val != 0) {
                    BigInteger res = (val & mask);
                    uint writeVal = res.ToUInt32();
                    bytes.Add((byte)((writeVal) & 0xff));
                    bytes.Add((byte)((writeVal >> 8) & 0xff));
                    val = val >> 15;
                }
            }

            void WriteBuffer(PythonBuffer b) {
                bytes.Add((byte)'s');
                List<byte> newBytes = new List<byte>();
                for (int i = 0; i < b.Size; i++) {
                    if (b[i] is string) {
                        string str = b[i] as string;
                        byte[] utfBytes = Encoding.UTF8.GetBytes(str);
                        if (utfBytes.Length != str.Length) {
                            newBytes.AddRange(utfBytes);
                        } else {
                            byte[] strBytes = Encoding.ASCII.GetBytes(str);
                            newBytes.AddRange(strBytes);
                        }
                    } else {
                        newBytes.Add((byte)b[i]);
                    }
                }
                WriteInt32(newBytes.Count);
                bytes.AddRange(newBytes);
            }

            void WriteLong(long l) {
                bytes.Add((byte)'I');

                for (int i = 0; i < 8; i++) {
                    bytes.Add((byte)(l & 0xff));
                    l = l >> 8;
                }
            }

            void WriteComplex(Complex64 val) {
                bytes.Add((byte)'x');
                WriteDoubleString(val.real);
                WriteDoubleString(val.imag);
            }

            void WriteStopIteration() {
                bytes.Add((byte)'S');
            }

            void WriteInt(int val) {
                bytes.Add((byte)'i');
                WriteInt32(val);
            }

            void WriteInt32(int val) {
                BitConverter.GetBytes(val);
                bytes.Add((byte)(val & 0xff));
                bytes.Add((byte)((val >> 8) & 0xff));
                bytes.Add((byte)((val >> 16) & 0xff));
                bytes.Add((byte)((val >> 24) & 0xff));
            }

            void WriteUInt32(uint val) {
                bytes.Add((byte)(val & 0xff));
                bytes.Add((byte)((val >> 8) & 0xff));
                bytes.Add((byte)((val >> 16) & 0xff));
                bytes.Add((byte)((val >> 24) & 0xff));
            }

            void WriteString(string s) {
                byte[] utfBytes = Encoding.UTF8.GetBytes(s);
                if (utfBytes.Length != s.Length) {
                    bytes.Add((byte)'u');
                    WriteInt32(utfBytes.Length);
                    for (int i = 0; i < utfBytes.Length; i++) {
                        bytes.Add(utfBytes[i]);
                    }
                } else {
                    byte[] strBytes = Encoding.ASCII.GetBytes(s);
                    bytes.Add((byte)'t');
                    WriteInt32(strBytes.Length);
                    for (int i = 0; i < strBytes.Length; i++) {
                        bytes.Add(strBytes[i]);
                    }
                }
            }

            void WriteList(object o) {
                List l = o as List;
                bytes.Add((byte)'[');
                WriteInt32(l.Count);
                for (int i = 0; i < l.Count; i++) {
                    WriteObject(l[i]);
                }
            }

            void WriteDict(object o) {
                Dict d = o as Dict;
                bytes.Add((byte)'{');
                IEnumerator<KeyValuePair<object, object>> ie = ((IEnumerable<KeyValuePair<object, object>>)d).GetEnumerator();
                while (ie.MoveNext()) {
                    WriteObject(ie.Current.Key);
                    WriteObject(ie.Current.Value);
                }
                bytes.Add((byte)'0');
            }

            void WriteTuple(object o) {
                Tuple t = o as Tuple;
                bytes.Add((byte)'(');
                WriteInt32(t.Count);
                for (int i = 0; i < t.Count; i++) {
                    WriteObject(t[i]);
                }
            }

            public byte[] GetBytes() {
                return bytes.ToArray();
            }
        }

        class MarshalReader {
            byte[] myBytes;
            int curIndex;
            Stack<ProcStack> stack;
            object result;

            public MarshalReader(byte[] bytes) {
                myBytes = bytes;
            }

            public object ReadObject() {
                while (curIndex < myBytes.Length) {
                    object res;
                    if (myBytes[curIndex] == '(') {
                        PushStack(StackType.Tuple);
                    } else if (myBytes[curIndex] == '[') {
                        PushStack(StackType.List);
                    } else if (myBytes[curIndex] == '{') {
                        PushStack(StackType.Dict);
                        /*} else if (myBytes[curIndex] == 'c') {*/
                    } else {
                        res = YieldSimple();
                        if (stack == null) {
                            return res;
                        }

                        do {
                            res = UpdateStack(res);
                        } while (res != null && stack.Count > 0);

                        if (stack.Count == 0) {
                            break;
                        }
                        continue;
                    }

                    // handle empty lists/tuples...
                    if (stack != null && stack.Count > 0 && stack.Peek().StackCount == 0) {
                        ProcStack ps = stack.Pop();
                        res = ps.StackObj;
                        List<object> listRes = res as List<object>;
                        if (listRes != null)
                            res = new Tuple(listRes);

                        if (stack.Count > 0) {
                            // empty list/tuple
                            do {
                                res = UpdateStack(res);
                            } while (res != null && stack.Count > 0);
                            if (stack.Count == 0) break;
                        } else {
                            result = res;
                            break;
                        }
                    }
                }

                return result;
            }

            void PushStack(StackType type) {
                ProcStack newStack = new ProcStack();
                newStack.StackType = type;
                curIndex++;

                switch (type) {
                    case StackType.Dict:
                        newStack.StackObj = new Dict();

                        if (curIndex == myBytes.Length) throw Ops.EofError("EOF read where object expected");

                        if (myBytes[curIndex] == '0')
                            newStack.StackCount = 0;
                        else 
                            newStack.StackCount = -1;

                        break;
                    case StackType.List:
                        newStack.StackObj = new List();
                        newStack.StackCount = ReadInt32();
                        break;
                    case StackType.Tuple:
                        newStack.StackObj = new List<object>();
                        newStack.StackCount = ReadInt32();
                        break;
                }

                if (stack == null) stack = new Stack<ProcStack>();

                stack.Push(newStack);
            }

            object UpdateStack(object res) {
                ProcStack curStack = stack.Peek();
                switch (curStack.StackType) {
                    case StackType.Dict:
                        Dict od = curStack.StackObj as Dict;
                        if (curStack.HaveKey) {
                            od.Add(curStack.Key, res);
                            curStack.HaveKey = false;
                        } else {
                            curStack.HaveKey = true;
                            curStack.Key = res;
                        }

                        if (curIndex == myBytes.Length) throw Ops.EofError("EOF read where object expected");
                        if (myBytes[curIndex] == '0') {
                            stack.Pop();
                            if (stack.Count == 0) {
                                result = od;
                            }
                            return od;
                        }
                        break;
                    case StackType.Tuple:
                        List<object> objs = curStack.StackObj as List<object>;
                        objs.Add(res);
                        curStack.StackCount--;
                        if (curStack.StackCount == 0) {
                            stack.Pop();
                            object tuple = Tuple.Make(objs);
                            if (stack.Count == 0) {
                                result = tuple;
                            }
                            return tuple;
                        }
                        break;
                    case StackType.List:
                        List ol = curStack.StackObj as List;
                        ol.AddNoLock(res);
                        curStack.StackCount--;
                        if (curStack.StackCount == 0) {
                            stack.Pop();
                            if (stack.Count == 0) {
                                result = ol;
                            }
                            return ol;
                        }
                        break;
                }
                return null;
            }

            enum StackType {
                Tuple,
                Dict,
                List
            }
            class ProcStack {
                public StackType StackType;
                public object StackObj;
                public int StackCount;
                public bool HaveKey;
                public object Key;
            }

            object YieldSimple() {
                object res;
                switch ((char)myBytes[curIndex++]) {
                    // simple ops to be read in
                    case 'i': res = ReadInt(); break;
                    case 'l': res = ReadBigInteger(); break;
                    case 'T': res = Ops.TRUE; break;
                    case 'F': res = Ops.FALSE; break;
                    case 'f': res = ReadFloat(); break;
                    case 't': res = ReadAsciiString(); break;
                    case 'u': res = ReadUnicodeString(); break;
                    case 'S': res = ExceptionConverter.GetPythonException("StopIteration"); break;
                    case 'N': res = null; break;
                    case 'x': res = ReadComplex(); break;
                    case 's': res = ReadBuffer(); break;
                    case 'I': res = ReadLong(); break;
                    default: throw Ops.ValueError("bad marshal data");
                }
                return res;
            }

            int ReadInt32() {
                if (curIndex + 3 >= myBytes.Length) throw Ops.ValueError("bad marshal data");

                int res = myBytes[curIndex] |
                    (myBytes[curIndex + 1] << 8) |
                    (myBytes[curIndex + 2] << 16) |
                    (myBytes[curIndex + 3] << 24);

                curIndex += 4;
                return res;
            }

            double ReadFloatStr() {
                if (curIndex >= myBytes.Length) throw Ops.EofError("EOF read where object expected");


                int len = myBytes[curIndex];
                curIndex++;
                if ((curIndex + len) > myBytes.Length) throw Ops.EofError("EOF read where object expected");

                string str = ASCIIEncoding.ASCII.GetString(myBytes, curIndex, len);

                curIndex += len;
                double res = 0;
                double.TryParse(str, out res);
                return res;
            }

            object ReadInt() {
                // bytes not present are treated as being -1
                byte b1, b2, b3, b4;

                switch (myBytes.Length - curIndex) {
                    case 0: b1 = 255; b2 = 255; b3 = 255; b4 = 255; break;
                    case 1: b1 = myBytes[curIndex]; b2 = 255; b3 = 255; b4 = 255; break;
                    case 2: b1 = myBytes[curIndex]; b2 = myBytes[curIndex + 1]; b3 = 255; b4 = 255; break;
                    case 3: b1 = myBytes[curIndex]; b2 = myBytes[curIndex + 1]; b3 = myBytes[curIndex + 2]; b4 = 255; break;
                    default:
                        b1 = myBytes[curIndex];
                        b2 = myBytes[curIndex + 1];
                        b3 = myBytes[curIndex + 2];
                        b4 = myBytes[curIndex + 3];
                        break;
                }

                curIndex += 4;
                byte[] bytes = new byte[] { b1, b2, b3, b4 };
                return Ops.Int2Object(BitConverter.ToInt32(bytes, 0));
                //return Ops.int2object(b1 | (b2 << 8) | (b3 << 16) | (b4 << 24));
            }

            object ReadFloat() {
                return ReadFloatStr();
            }

            object ReadAsciiString() {
                int len = ReadInt32();
                if (len + curIndex > myBytes.Length) throw Ops.EofError("EOF read where object expected");

                string res = ASCIIEncoding.ASCII.GetString(myBytes, curIndex, len);

                curIndex += len;
                return res;
            }
            object ReadUnicodeString() {
                int len = ReadInt32();
                if (len + curIndex > myBytes.Length) throw Ops.EofError("EOF read where object expected");

                string res = Encoding.UTF8.GetString(myBytes, curIndex, len);

                curIndex += len;
                return res;
            }
            object ReadComplex() {
                double real = ReadFloatStr();
                double imag = ReadFloatStr();

                return new Complex64(real, imag);
            }
            object ReadBuffer() {
                int len = ReadInt32();

                if (len + curIndex > myBytes.Length) throw Ops.ValueError("bad marshal data");

                string res = Encoding.UTF8.GetString(myBytes, curIndex, len);

                curIndex += len;

                return res;
            }

            object ReadLong() {
                if (curIndex + 8 > myBytes.Length) throw Ops.ValueError("bad marshal data");

                long res = 0;
                for (int i = 0; i < 8; i++) {
                    res |= (((long)myBytes[curIndex++]) << (i * 8));
                }

                return res;
            }

            object ReadBigInteger() {
                int encodingSize = ReadInt32();
                int sign = 1;
                if (encodingSize < 0) {
                    sign = -1;
                    encodingSize *= -1;
                }
                int len = encodingSize * 2;

                if (len + curIndex > myBytes.Length) throw Ops.ValueError("bad marshal data");

                // first read the values in shorts so we can work
                // with them as 15-bit bytes easier...
                short[] shData = new short[encodingSize];
                for (int i = 0; i < shData.Length; i++) {
                    shData[i] = (short)(myBytes[curIndex + i * 2] | (myBytes[curIndex + 1 + i * 2] << 8));
                }

                // then convert the short's into BigInteger's 32-bit 
                // format.
                uint[] numData = new uint[(shData.Length + 1) / 2];
                int bitWriteIndex = 0, shortIndex = 0, bitReadIndex = 0;
                while (shortIndex < shData.Length) {
                    short val = shData[shortIndex];
                    int shift = bitWriteIndex % 32;

                    if (bitReadIndex != 0) {
                        // we're read some bits, mask them off
                        // and adjust the shift.
                        int maskOff = ~((1 << bitReadIndex) - 1);
                        val = (short)(val & maskOff);
                        shift -= bitReadIndex;
                    }

                    // write the value into numData
                    if (shift < 0) {
                        numData[bitWriteIndex / 32] |= (uint)(val >> (shift * -1));
                    } else {
                        numData[bitWriteIndex / 32] |= (uint)(val << shift);
                    }

                    // and advance our indices
                    if ((bitWriteIndex % 32) <= 16) {
                        bitWriteIndex += (15 - bitReadIndex);
                        bitReadIndex = 0;
                        shortIndex++;
                    } else {
                        bitReadIndex = (32 - (bitWriteIndex % 32));
                        bitWriteIndex += bitReadIndex;
                    }
                }
                curIndex += len;

                // and finally pass the data onto the big integer.
                return new BigInteger(sign, numData);
            }
        }

        #endregion
    }
}
