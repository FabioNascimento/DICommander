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
using System.Diagnostics;
using System.Threading;

using IronMath;
using IronPython.Runtime;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Operations {
    static partial class UInt16Ops {

        #region Generated UInt16Ops

        // *** BEGIN GENERATED CODE ***

        private static ReflectedType UInt16Type;
        public static DynamicType MakeDynamicType() {
            if (UInt16Type == null) {
                OpsReflectedType ort = new OpsReflectedType("UInt16", typeof(UInt16), typeof(UInt16Ops), null);
                if (Interlocked.CompareExchange<ReflectedType>(ref UInt16Type, ort, null) == null) {
                    return ort;
                }
            }
            return UInt16Type;
        }

        [PythonName("__new__")]
        public static object Make(DynamicType cls) {
            return Make(cls, default(UInt16));
        }

        [PythonName("__new__")]
        public static object Make(DynamicType cls, object value) {
            if (cls != UInt16Type) {
                throw Ops.TypeError("UInt16.__new__: first argument must be UInt16 type.");
            }
            IConvertible valueConvertible;
            if ((valueConvertible = value as IConvertible) != null) {
                switch (valueConvertible.GetTypeCode()) {
                    case TypeCode.Byte: return (UInt16)(Byte)value;
                    case TypeCode.SByte: return (UInt16)(SByte)value;
                    case TypeCode.Int16: return (UInt16)(Int16)value;
                    case TypeCode.UInt16: return (UInt16)(UInt16)value;
                    case TypeCode.Int32: return (UInt16)(Int32)value;
                    case TypeCode.UInt32: return (UInt16)(UInt32)value;
                    case TypeCode.Int64: return (UInt16)(Int64)value;
                    case TypeCode.UInt64: return (UInt16)(UInt64)value;
                    case TypeCode.Single: return (UInt16)(Single)value;
                    case TypeCode.Double: return (UInt16)(Double)value;
                }
            }
            if (value is String) {
                return UInt16.Parse((String)value);
            } else if (value is BigInteger) {
                return (UInt16)(BigInteger)value;
            } else if (value is ExtensibleInt) {
                return (UInt16)((ExtensibleInt)value).value;
            } else if (value is ExtensibleLong) {
                return (UInt16)((ExtensibleLong)value).Value;
            } else if (value is ExtensibleFloat) {
                return (UInt16)((ExtensibleFloat)value).value;
            } else if (value is Enum) {
                return Converter.CastEnumToUInt16(value);
            }
            throw Ops.ValueError("invalid value for UInt16.__new__");
        }

        [PythonName("__add__")]
        public static object Add(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__add__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            Int32 result = (Int32)(((Int32)leftUInt16) + ((Int32)((Boolean)right ? (UInt16)1 : (UInt16)0)));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Byte: {
                            Int32 result = (Int32)(((Int32)leftUInt16) + ((Int32)(Byte)right));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int32 result = (Int32)(((Int32)leftUInt16) + ((Int32)(SByte)right));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)leftUInt16) + ((Int32)(Int16)right));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int32 result = (Int32)(((Int32)leftUInt16) + ((Int32)(UInt16)right));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)leftUInt16) + ((Int64)(Int32)right));
                            if (Int32.MinValue <= result && result <= Int32.MaxValue) {
                                return (Int32)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)leftUInt16) + ((Int64)(UInt32)right));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Add(leftUInt16, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.AddImpl(leftUInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return SingleOps.AddImpl((Single)leftUInt16, (Single)(Single)right);
                        }
                    case TypeCode.Double: {
                            return FloatOps.Add((Double)leftUInt16, (Double)(Double)right);
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.Add(leftUInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.Add(leftUInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                Int64 result = (Int64)(((Int64)leftUInt16) + ((Int64)((ExtensibleInt)right).value));
                if (Int32.MinValue <= result && result <= Int32.MaxValue) {
                    return (Int32)result;
                } else return result;
            } else if (right is ExtensibleLong) {
                return LongOps.Add(leftUInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.Add((Double)leftUInt16, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Add(leftUInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__div__")]
        public static object Divide(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__div__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return UInt16Ops.DivideImpl((UInt16)leftUInt16, (UInt16)((Boolean)right ? (UInt16)1 : (UInt16)0));
                        }
                    case TypeCode.Byte: {
                            return UInt16Ops.DivideImpl((UInt16)leftUInt16, (UInt16)(Byte)right);
                        }
                    case TypeCode.SByte: {
                            return IntOps.Divide((Int32)leftUInt16, (Int32)(SByte)right);
                        }
                    case TypeCode.Int16: {
                            return IntOps.Divide((Int32)leftUInt16, (Int32)(Int16)right);
                        }
                    case TypeCode.UInt16: {
                            return UInt16Ops.DivideImpl((UInt16)leftUInt16, (UInt16)(UInt16)right);
                        }
                    case TypeCode.Int32: {
                            return IntOps.Divide((Int32)leftUInt16, (Int32)(Int32)right);
                        }
                    case TypeCode.UInt32: {
                            return UInt32Ops.DivideImpl((UInt32)leftUInt16, (UInt32)(UInt32)right);
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Divide((Int64)leftUInt16, (Int64)(Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.DivideImpl((UInt64)leftUInt16, (UInt64)(UInt64)right);
                        }
                    case TypeCode.Single: {
                            return SingleOps.DivideImpl((Single)leftUInt16, (Single)(Single)right);
                        }
                    case TypeCode.Double: {
                            return FloatOps.Divide((Double)leftUInt16, (Double)(Double)right);
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.Divide(leftUInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.Divide(leftUInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return IntOps.Divide((Int32)leftUInt16, (Int32)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.Divide(leftUInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.Divide((Double)leftUInt16, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Divide(leftUInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__floordiv__")]
        public static object FloorDivide(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__floordiv__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return UInt16Ops.FloorDivideImpl((UInt16)leftUInt16, (UInt16)((Boolean)right ? (UInt16)1 : (UInt16)0));
                        }
                    case TypeCode.Byte: {
                            return UInt16Ops.FloorDivideImpl((UInt16)leftUInt16, (UInt16)(Byte)right);
                        }
                    case TypeCode.SByte: {
                            return IntOps.FloorDivide((Int32)leftUInt16, (Int32)(SByte)right);
                        }
                    case TypeCode.Int16: {
                            return IntOps.FloorDivide((Int32)leftUInt16, (Int32)(Int16)right);
                        }
                    case TypeCode.UInt16: {
                            return UInt16Ops.FloorDivideImpl((UInt16)leftUInt16, (UInt16)(UInt16)right);
                        }
                    case TypeCode.Int32: {
                            return IntOps.FloorDivide((Int32)leftUInt16, (Int32)(Int32)right);
                        }
                    case TypeCode.UInt32: {
                            return UInt32Ops.FloorDivideImpl((UInt32)leftUInt16, (UInt32)(UInt32)right);
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.FloorDivide((Int64)leftUInt16, (Int64)(Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.FloorDivideImpl((UInt64)leftUInt16, (UInt64)(UInt64)right);
                        }
                    case TypeCode.Single: {
                            return SingleOps.FloorDivideImpl((Single)leftUInt16, (Single)(Single)right);
                        }
                    case TypeCode.Double: {
                            return FloatOps.FloorDivide((Double)leftUInt16, (Double)(Double)right);
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.FloorDivide(leftUInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.FloorDivide(leftUInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return IntOps.FloorDivide((Int32)leftUInt16, (Int32)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.FloorDivide(leftUInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.FloorDivide((Double)leftUInt16, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.FloorDivide(leftUInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__mod__")]
        public static object Mod(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__mod__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return UInt16Ops.ModImpl((UInt16)leftUInt16, (UInt16)((Boolean)right ? (UInt16)1 : (UInt16)0));
                        }
                    case TypeCode.Byte: {
                            return UInt16Ops.ModImpl((UInt16)leftUInt16, (UInt16)(Byte)right);
                        }
                    case TypeCode.SByte: {
                            return IntOps.Mod((Int32)leftUInt16, (Int32)(SByte)right);
                        }
                    case TypeCode.Int16: {
                            return IntOps.Mod((Int32)leftUInt16, (Int32)(Int16)right);
                        }
                    case TypeCode.UInt16: {
                            return UInt16Ops.ModImpl((UInt16)leftUInt16, (UInt16)(UInt16)right);
                        }
                    case TypeCode.Int32: {
                            return IntOps.Mod((Int32)leftUInt16, (Int32)(Int32)right);
                        }
                    case TypeCode.UInt32: {
                            return UInt32Ops.ModImpl((UInt32)leftUInt16, (UInt32)(UInt32)right);
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Mod((Int64)leftUInt16, (Int64)(Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ModImpl((UInt64)leftUInt16, (UInt64)(UInt64)right);
                        }
                    case TypeCode.Single: {
                            return SingleOps.ModImpl((Single)leftUInt16, (Single)(Single)right);
                        }
                    case TypeCode.Double: {
                            return FloatOps.Mod((Double)leftUInt16, (Double)(Double)right);
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.Mod(leftUInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.Mod(leftUInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return IntOps.Mod((Int32)leftUInt16, (Int32)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.Mod(leftUInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.Mod((Double)leftUInt16, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Mod(leftUInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__mul__")]
        public static object Multiply(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__mul__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            UInt16 result = (UInt16)(((UInt16)leftUInt16) * ((UInt16)((Boolean)right ? (UInt16)1 : (UInt16)0)));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Byte: {
                            Int32 result = (Int32)(((Int32)leftUInt16) * ((Int32)(Byte)right));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int32 result = (Int32)(((Int32)leftUInt16) * ((Int32)(SByte)right));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)leftUInt16) * ((Int32)(Int16)right));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            UInt32 result = (UInt32)(((UInt32)leftUInt16) * ((UInt32)(UInt16)right));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)leftUInt16) * ((Int64)(Int32)right));
                            if (Int32.MinValue <= result && result <= Int32.MaxValue) {
                                return (Int32)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)leftUInt16) * ((Int64)(UInt32)right));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Multiply(leftUInt16, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.MultiplyImpl(leftUInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return FloatOps.Multiply((Double)leftUInt16, (Double)(Single)right);
                        }
                    case TypeCode.Double: {
                            return FloatOps.Multiply(leftUInt16, (Double)right);
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.Multiply(leftUInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.Multiply(leftUInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                Int64 result = (Int64)(((Int64)leftUInt16) * ((Int64)((ExtensibleInt)right).value));
                if (Int32.MinValue <= result && result <= Int32.MaxValue) {
                    return (Int32)result;
                } else return result;
            } else if (right is ExtensibleLong) {
                return LongOps.Multiply(leftUInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.Multiply(leftUInt16, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Multiply(leftUInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__sub__")]
        public static object Subtract(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__sub__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            Int32 result = (Int32)(((Int32)leftUInt16) - ((Int32)((Boolean)right ? (UInt16)1 : (UInt16)0)));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Byte: {
                            Int32 result = (Int32)(((Int32)leftUInt16) - ((Int32)(Byte)right));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int32 result = (Int32)(((Int32)leftUInt16) - ((Int32)(SByte)right));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)leftUInt16) - ((Int32)(Int16)right));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int32 result = (Int32)(((Int32)leftUInt16) - ((Int32)(UInt16)right));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)leftUInt16) - ((Int64)(Int32)right));
                            if (Int32.MinValue <= result && result <= Int32.MaxValue) {
                                return (Int32)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)leftUInt16) - ((Int64)(UInt32)right));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.Subtract(leftUInt16, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.SubtractImpl(leftUInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return SingleOps.SubtractImpl((Single)leftUInt16, (Single)(Single)right);
                        }
                    case TypeCode.Double: {
                            return FloatOps.Subtract((Double)leftUInt16, (Double)(Double)right);
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.Subtract(leftUInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.Subtract(leftUInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                Int64 result = (Int64)(((Int64)leftUInt16) - ((Int64)((ExtensibleInt)right).value));
                if (Int32.MinValue <= result && result <= Int32.MaxValue) {
                    return (Int32)result;
                } else return result;
            } else if (right is ExtensibleLong) {
                return LongOps.Subtract(leftUInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.Subtract((Double)leftUInt16, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Subtract(leftUInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__radd__")]
        public static object ReverseAdd(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__radd__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            Int32 result = (Int32)(((Int32)((Boolean)right ? (UInt16)1 : (UInt16)0)) + ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Byte: {
                            Int32 result = (Int32)(((Int32)(Byte)right) + ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int32 result = (Int32)(((Int32)(SByte)right) + ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)(Int16)right) + ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int32 result = (Int32)(((Int32)(UInt16)right) + ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)(Int32)right) + ((Int64)leftUInt16));
                            if (Int32.MinValue <= result && result <= Int32.MaxValue) {
                                return (Int32)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)(UInt32)right) + ((Int64)leftUInt16));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseAdd(leftUInt16, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseAddImpl(leftUInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseAddImpl((Single)leftUInt16, (Single)(Single)right);
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseAdd((Double)leftUInt16, (Double)(Double)right);
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseAdd(leftUInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseAdd(leftUInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                Int64 result = (Int64)(((Int64)((ExtensibleInt)right).value) + ((Int64)leftUInt16));
                if (Int32.MinValue <= result && result <= Int32.MaxValue) {
                    return (Int32)result;
                } else return result;
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseAdd(leftUInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseAdd((Double)leftUInt16, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseAdd(leftUInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rdiv__")]
        public static object ReverseDivide(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__rdiv__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return UInt16Ops.ReverseDivideImpl((UInt16)leftUInt16, (UInt16)((Boolean)right ? (UInt16)1 : (UInt16)0));
                        }
                    case TypeCode.Byte: {
                            return UInt16Ops.ReverseDivideImpl((UInt16)leftUInt16, (UInt16)(Byte)right);
                        }
                    case TypeCode.SByte: {
                            return IntOps.ReverseDivide((Int32)leftUInt16, (Int32)(SByte)right);
                        }
                    case TypeCode.Int16: {
                            return IntOps.ReverseDivide((Int32)leftUInt16, (Int32)(Int16)right);
                        }
                    case TypeCode.UInt16: {
                            return UInt16Ops.ReverseDivideImpl((UInt16)leftUInt16, (UInt16)(UInt16)right);
                        }
                    case TypeCode.Int32: {
                            return IntOps.ReverseDivide((Int32)leftUInt16, (Int32)(Int32)right);
                        }
                    case TypeCode.UInt32: {
                            return UInt32Ops.ReverseDivideImpl((UInt32)leftUInt16, (UInt32)(UInt32)right);
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseDivide((Int64)leftUInt16, (Int64)(Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseDivideImpl((UInt64)leftUInt16, (UInt64)(UInt64)right);
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseDivideImpl((Single)leftUInt16, (Single)(Single)right);
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseDivide((Double)leftUInt16, (Double)(Double)right);
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseDivide(leftUInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseDivide(leftUInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return IntOps.ReverseDivide((Int32)leftUInt16, (Int32)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseDivide(leftUInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseDivide((Double)leftUInt16, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseDivide(leftUInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rfloordiv__")]
        public static object ReverseFloorDivide(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__rfloordiv__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return UInt16Ops.ReverseFloorDivideImpl((UInt16)leftUInt16, (UInt16)((Boolean)right ? (UInt16)1 : (UInt16)0));
                        }
                    case TypeCode.Byte: {
                            return UInt16Ops.ReverseFloorDivideImpl((UInt16)leftUInt16, (UInt16)(Byte)right);
                        }
                    case TypeCode.SByte: {
                            return IntOps.ReverseFloorDivide((Int32)leftUInt16, (Int32)(SByte)right);
                        }
                    case TypeCode.Int16: {
                            return IntOps.ReverseFloorDivide((Int32)leftUInt16, (Int32)(Int16)right);
                        }
                    case TypeCode.UInt16: {
                            return UInt16Ops.ReverseFloorDivideImpl((UInt16)leftUInt16, (UInt16)(UInt16)right);
                        }
                    case TypeCode.Int32: {
                            return IntOps.ReverseFloorDivide((Int32)leftUInt16, (Int32)(Int32)right);
                        }
                    case TypeCode.UInt32: {
                            return UInt32Ops.ReverseFloorDivideImpl((UInt32)leftUInt16, (UInt32)(UInt32)right);
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseFloorDivide((Int64)leftUInt16, (Int64)(Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseFloorDivideImpl((UInt64)leftUInt16, (UInt64)(UInt64)right);
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseFloorDivideImpl((Single)leftUInt16, (Single)(Single)right);
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseFloorDivide((Double)leftUInt16, (Double)(Double)right);
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseFloorDivide(leftUInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseFloorDivide(leftUInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return IntOps.ReverseFloorDivide((Int32)leftUInt16, (Int32)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseFloorDivide(leftUInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseFloorDivide((Double)leftUInt16, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseFloorDivide(leftUInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rmod__")]
        public static object ReverseMod(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__rmod__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            return UInt16Ops.ReverseModImpl((UInt16)leftUInt16, (UInt16)((Boolean)right ? (UInt16)1 : (UInt16)0));
                        }
                    case TypeCode.Byte: {
                            return UInt16Ops.ReverseModImpl((UInt16)leftUInt16, (UInt16)(Byte)right);
                        }
                    case TypeCode.SByte: {
                            return IntOps.ReverseMod((Int32)leftUInt16, (Int32)(SByte)right);
                        }
                    case TypeCode.Int16: {
                            return IntOps.ReverseMod((Int32)leftUInt16, (Int32)(Int16)right);
                        }
                    case TypeCode.UInt16: {
                            return UInt16Ops.ReverseModImpl((UInt16)leftUInt16, (UInt16)(UInt16)right);
                        }
                    case TypeCode.Int32: {
                            return IntOps.ReverseMod((Int32)leftUInt16, (Int32)(Int32)right);
                        }
                    case TypeCode.UInt32: {
                            return UInt32Ops.ReverseModImpl((UInt32)leftUInt16, (UInt32)(UInt32)right);
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseMod((Int64)leftUInt16, (Int64)(Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseModImpl((UInt64)leftUInt16, (UInt64)(UInt64)right);
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseModImpl((Single)leftUInt16, (Single)(Single)right);
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseMod((Double)leftUInt16, (Double)(Double)right);
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseMod(leftUInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseMod(leftUInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                return IntOps.ReverseMod((Int32)leftUInt16, (Int32)((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseMod(leftUInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseMod((Double)leftUInt16, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseMod(leftUInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rmul__")]
        public static object ReverseMultiply(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__rmul__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            UInt16 result = (UInt16)(((UInt16)((Boolean)right ? (UInt16)1 : (UInt16)0)) * ((UInt16)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Byte: {
                            Int32 result = (Int32)(((Int32)(Byte)right) * ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int32 result = (Int32)(((Int32)(SByte)right) * ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)(Int16)right) * ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            UInt32 result = (UInt32)(((UInt32)(UInt16)right) * ((UInt32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)(Int32)right) * ((Int64)leftUInt16));
                            if (Int32.MinValue <= result && result <= Int32.MaxValue) {
                                return (Int32)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)(UInt32)right) * ((Int64)leftUInt16));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseMultiply(leftUInt16, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseMultiplyImpl(leftUInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return FloatOps.ReverseMultiply((Double)leftUInt16, (Double)(Single)right);
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseMultiply(leftUInt16, (Double)right);
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseMultiply(leftUInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseMultiply(leftUInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                Int64 result = (Int64)(((Int64)((ExtensibleInt)right).value) * ((Int64)leftUInt16));
                if (Int32.MinValue <= result && result <= Int32.MaxValue) {
                    return (Int32)result;
                } else return result;
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseMultiply(leftUInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseMultiply(leftUInt16, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseMultiply(leftUInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rsub__")]
        public static object ReverseSubtract(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__rsub__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            Int32 result = (Int32)(((Int32)((Boolean)right ? (UInt16)1 : (UInt16)0)) - ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Byte: {
                            Int32 result = (Int32)(((Int32)(Byte)right) - ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.SByte: {
                            Int32 result = (Int32)(((Int32)(SByte)right) - ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int16: {
                            Int32 result = (Int32)(((Int32)(Int16)right) - ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.UInt16: {
                            Int32 result = (Int32)(((Int32)(UInt16)right) - ((Int32)leftUInt16));
                            if (UInt16.MinValue <= result && result <= UInt16.MaxValue) {
                                return (UInt16)result;
                            } else return result;
                        }
                    case TypeCode.Int32: {
                            Int64 result = (Int64)(((Int64)(Int32)right) - ((Int64)leftUInt16));
                            if (Int32.MinValue <= result && result <= Int32.MaxValue) {
                                return (Int32)result;
                            } else return result;
                        }
                    case TypeCode.UInt32: {
                            Int64 result = (Int64)(((Int64)(UInt32)right) - ((Int64)leftUInt16));
                            if (UInt32.MinValue <= result && result <= UInt32.MaxValue) {
                                return (UInt32)result;
                            } else return result;
                        }
                    case TypeCode.Int64: {
                            return Int64Ops.ReverseSubtract(leftUInt16, (Int64)right);
                        }
                    case TypeCode.UInt64: {
                            return UInt64Ops.ReverseSubtractImpl(leftUInt16, (UInt64)right);
                        }
                    case TypeCode.Single: {
                            return SingleOps.ReverseSubtractImpl((Single)leftUInt16, (Single)(Single)right);
                        }
                    case TypeCode.Double: {
                            return FloatOps.ReverseSubtract((Double)leftUInt16, (Double)(Double)right);
                        }
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseSubtract(leftUInt16, (BigInteger)right);
            } else if (right is Complex64) {
                return ComplexOps.ReverseSubtract(leftUInt16, (Complex64)right);
            } else if (right is ExtensibleInt) {
                Int64 result = (Int64)(((Int64)((ExtensibleInt)right).value) - ((Int64)leftUInt16));
                if (Int32.MinValue <= result && result <= Int32.MaxValue) {
                    return (Int32)result;
                } else return result;
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseSubtract(leftUInt16, ((ExtensibleLong)right).Value);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseSubtract((Double)leftUInt16, (Double)((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseSubtract(leftUInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__and__")]
        public static object BitwiseAnd(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__and__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            UInt16 rightUInt16 = (UInt16)((Boolean)right ? (UInt16)1 : (UInt16)0);
                            return leftUInt16 & rightUInt16;
                        }
                    case TypeCode.Byte: {
                            UInt16 rightUInt16 = (UInt16)(Byte)right;
                            return leftUInt16 & rightUInt16;
                        }
                    case TypeCode.SByte: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(SByte)right;
                            return leftInt32 & rightInt32;
                        }
                    case TypeCode.Int16: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(Int16)right;
                            return leftInt32 & rightInt32;
                        }
                    case TypeCode.UInt16: {
                            return leftUInt16 & (UInt16)right;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            return leftInt32 & (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            UInt32 leftUInt32 = (UInt32)leftUInt16;
                            return leftUInt32 & (UInt32)right;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftUInt16;
                            return leftInt64 & (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            UInt64 leftUInt64 = (UInt64)leftUInt16;
                            return leftUInt64 & (UInt64)right;
                        }
                }
            }
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftUInt16;
                return leftBigInteger & (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int32 leftInt32 = (Int32)leftUInt16;
                return leftInt32 & (Int32)((ExtensibleInt)right).value;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftUInt16;
                return leftBigInteger & (BigInteger)((ExtensibleLong)right).Value;
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rand__")]
        public static object ReverseBitwiseAnd(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__rand__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            UInt16 rightUInt16 = (UInt16)((Boolean)right ? (UInt16)1 : (UInt16)0);
                            return leftUInt16 & rightUInt16;
                        }
                    case TypeCode.Byte: {
                            UInt16 rightUInt16 = (UInt16)(Byte)right;
                            return leftUInt16 & rightUInt16;
                        }
                    case TypeCode.SByte: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(SByte)right;
                            return leftInt32 & rightInt32;
                        }
                    case TypeCode.Int16: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(Int16)right;
                            return leftInt32 & rightInt32;
                        }
                    case TypeCode.UInt16: {
                            return leftUInt16 & (UInt16)right;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            return leftInt32 & (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            UInt32 leftUInt32 = (UInt32)leftUInt16;
                            return leftUInt32 & (UInt32)right;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftUInt16;
                            return leftInt64 & (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            UInt64 leftUInt64 = (UInt64)leftUInt16;
                            return leftUInt64 & (UInt64)right;
                        }
                }
            }
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftUInt16;
                return leftBigInteger & (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int32 leftInt32 = (Int32)leftUInt16;
                return leftInt32 & (Int32)((ExtensibleInt)right).value;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftUInt16;
                return leftBigInteger & (BigInteger)((ExtensibleLong)right).Value;
            }
            return Ops.NotImplemented;
        }
        [PythonName("__or__")]
        public static object BitwiseOr(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__or__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            UInt16 rightUInt16 = (UInt16)((Boolean)right ? (UInt16)1 : (UInt16)0);
                            return leftUInt16 | rightUInt16;
                        }
                    case TypeCode.Byte: {
                            UInt16 rightUInt16 = (UInt16)(Byte)right;
                            return leftUInt16 | rightUInt16;
                        }
                    case TypeCode.SByte: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(SByte)right;
                            return leftInt32 | rightInt32;
                        }
                    case TypeCode.Int16: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(Int16)right;
                            return leftInt32 | rightInt32;
                        }
                    case TypeCode.UInt16: {
                            return leftUInt16 | (UInt16)right;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            return leftInt32 | (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            UInt32 leftUInt32 = (UInt32)leftUInt16;
                            return leftUInt32 | (UInt32)right;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftUInt16;
                            return leftInt64 | (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            UInt64 leftUInt64 = (UInt64)leftUInt16;
                            return leftUInt64 | (UInt64)right;
                        }
                }
            }
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftUInt16;
                return leftBigInteger | (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int32 leftInt32 = (Int32)leftUInt16;
                return leftInt32 | (Int32)((ExtensibleInt)right).value;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftUInt16;
                return leftBigInteger | (BigInteger)((ExtensibleLong)right).Value;
            }
            return Ops.NotImplemented;
        }
        [PythonName("__ror__")]
        public static object ReverseBitwiseOr(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__ror__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            UInt16 rightUInt16 = (UInt16)((Boolean)right ? (UInt16)1 : (UInt16)0);
                            return leftUInt16 | rightUInt16;
                        }
                    case TypeCode.Byte: {
                            UInt16 rightUInt16 = (UInt16)(Byte)right;
                            return leftUInt16 | rightUInt16;
                        }
                    case TypeCode.SByte: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(SByte)right;
                            return leftInt32 | rightInt32;
                        }
                    case TypeCode.Int16: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(Int16)right;
                            return leftInt32 | rightInt32;
                        }
                    case TypeCode.UInt16: {
                            return leftUInt16 | (UInt16)right;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            return leftInt32 | (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            UInt32 leftUInt32 = (UInt32)leftUInt16;
                            return leftUInt32 | (UInt32)right;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftUInt16;
                            return leftInt64 | (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            UInt64 leftUInt64 = (UInt64)leftUInt16;
                            return leftUInt64 | (UInt64)right;
                        }
                }
            }
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftUInt16;
                return leftBigInteger | (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int32 leftInt32 = (Int32)leftUInt16;
                return leftInt32 | (Int32)((ExtensibleInt)right).value;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftUInt16;
                return leftBigInteger | (BigInteger)((ExtensibleLong)right).Value;
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rxor__")]
        public static object BitwiseXor(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__rxor__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            UInt16 rightUInt16 = (UInt16)((Boolean)right ? (UInt16)1 : (UInt16)0);
                            return leftUInt16 ^ rightUInt16;
                        }
                    case TypeCode.Byte: {
                            UInt16 rightUInt16 = (UInt16)(Byte)right;
                            return leftUInt16 ^ rightUInt16;
                        }
                    case TypeCode.SByte: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(SByte)right;
                            return leftInt32 ^ rightInt32;
                        }
                    case TypeCode.Int16: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(Int16)right;
                            return leftInt32 ^ rightInt32;
                        }
                    case TypeCode.UInt16: {
                            return leftUInt16 ^ (UInt16)right;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            return leftInt32 ^ (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            UInt32 leftUInt32 = (UInt32)leftUInt16;
                            return leftUInt32 ^ (UInt32)right;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftUInt16;
                            return leftInt64 ^ (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            UInt64 leftUInt64 = (UInt64)leftUInt16;
                            return leftUInt64 ^ (UInt64)right;
                        }
                }
            }
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftUInt16;
                return leftBigInteger ^ (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int32 leftInt32 = (Int32)leftUInt16;
                return leftInt32 ^ (Int32)((ExtensibleInt)right).value;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftUInt16;
                return leftBigInteger ^ (BigInteger)((ExtensibleLong)right).Value;
            }
            return Ops.NotImplemented;
        }
        [PythonName("__xor__")]
        public static object ReverseBitwiseXor(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__xor__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean: {
                            UInt16 rightUInt16 = (UInt16)((Boolean)right ? (UInt16)1 : (UInt16)0);
                            return leftUInt16 ^ rightUInt16;
                        }
                    case TypeCode.Byte: {
                            UInt16 rightUInt16 = (UInt16)(Byte)right;
                            return leftUInt16 ^ rightUInt16;
                        }
                    case TypeCode.SByte: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(SByte)right;
                            return leftInt32 ^ rightInt32;
                        }
                    case TypeCode.Int16: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            Int32 rightInt32 = (Int32)(Int16)right;
                            return leftInt32 ^ rightInt32;
                        }
                    case TypeCode.UInt16: {
                            return leftUInt16 ^ (UInt16)right;
                        }
                    case TypeCode.Int32: {
                            Int32 leftInt32 = (Int32)leftUInt16;
                            return leftInt32 ^ (Int32)right;
                        }
                    case TypeCode.UInt32: {
                            UInt32 leftUInt32 = (UInt32)leftUInt16;
                            return leftUInt32 ^ (UInt32)right;
                        }
                    case TypeCode.Int64: {
                            Int64 leftInt64 = (Int64)leftUInt16;
                            return leftInt64 ^ (Int64)right;
                        }
                    case TypeCode.UInt64: {
                            UInt64 leftUInt64 = (UInt64)leftUInt16;
                            return leftUInt64 ^ (UInt64)right;
                        }
                }
            }
            if (right is BigInteger) {
                BigInteger leftBigInteger = (BigInteger)leftUInt16;
                return leftBigInteger ^ (BigInteger)right;
            } else if (right is ExtensibleInt) {
                Int32 leftInt32 = (Int32)leftUInt16;
                return leftInt32 ^ (Int32)((ExtensibleInt)right).value;
            } else if (right is ExtensibleLong) {
                BigInteger leftBigInteger = (BigInteger)leftUInt16;
                return leftBigInteger ^ (BigInteger)((ExtensibleLong)right).Value;
            }
            return Ops.NotImplemented;
        }
        [PythonName("__divmod__")]
        public static object DivMod(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__divmod__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return UInt16Ops.DivModImpl(leftUInt16, ((Boolean)right ? (UInt16)1 : (UInt16)0));
                    case TypeCode.Byte:
                        return UInt16Ops.DivModImpl(leftUInt16, (Byte)right);
                    case TypeCode.SByte:
                        return IntOps.DivMod(leftUInt16, (SByte)right);
                    case TypeCode.Int16:
                        return IntOps.DivMod(leftUInt16, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt16Ops.DivModImpl(leftUInt16, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.DivMod(leftUInt16, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt32Ops.DivModImpl(leftUInt16, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.DivMod(leftUInt16, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.DivModImpl(leftUInt16, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.DivModImpl(leftUInt16, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.DivMod(leftUInt16, (Double)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.DivMod(leftUInt16, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.DivMod(leftUInt16, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.DivMod(leftUInt16, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.DivMod(leftUInt16, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.DivMod(leftUInt16, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.DivMod(leftUInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rdivmod__")]
        public static object ReverseDivMod(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__rdivmod__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return UInt16Ops.ReverseDivModImpl(leftUInt16, ((Boolean)right ? (UInt16)1 : (UInt16)0));
                    case TypeCode.Byte:
                        return UInt16Ops.ReverseDivModImpl(leftUInt16, (Byte)right);
                    case TypeCode.SByte:
                        return IntOps.ReverseDivMod(leftUInt16, (SByte)right);
                    case TypeCode.Int16:
                        return IntOps.ReverseDivMod(leftUInt16, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt16Ops.ReverseDivModImpl(leftUInt16, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.ReverseDivMod(leftUInt16, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt32Ops.ReverseDivModImpl(leftUInt16, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.ReverseDivMod(leftUInt16, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.ReverseDivModImpl(leftUInt16, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.ReverseDivModImpl(leftUInt16, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.ReverseDivMod(leftUInt16, (Double)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseDivMod(leftUInt16, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.ReverseDivMod(leftUInt16, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseDivMod(leftUInt16, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.ReverseDivMod(leftUInt16, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseDivMod(leftUInt16, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReverseDivMod(leftUInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__lshift__")]
        public static object LeftShift(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__lshift__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return UInt16Ops.LeftShiftImpl(leftUInt16, ((Boolean)right ? (UInt16)1 : (UInt16)0));
                    case TypeCode.Byte:
                        return UInt16Ops.LeftShiftImpl(leftUInt16, (Byte)right);
                    case TypeCode.SByte:
                        return IntOps.LeftShift(leftUInt16, (SByte)right);
                    case TypeCode.Int16:
                        return IntOps.LeftShift(leftUInt16, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt16Ops.LeftShiftImpl(leftUInt16, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.LeftShift(leftUInt16, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt32Ops.LeftShiftImpl(leftUInt16, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.LeftShift(leftUInt16, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.LeftShiftImpl(leftUInt16, (UInt64)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.LeftShift(leftUInt16, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.LeftShift(leftUInt16, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.LeftShift(leftUInt16, ((ExtensibleLong)right).Value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rlshift__")]
        public static object ReverseLeftShift(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__rlshift__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return UInt16Ops.ReverseLeftShiftImpl(leftUInt16, ((Boolean)right ? (UInt16)1 : (UInt16)0));
                    case TypeCode.Byte:
                        return UInt16Ops.ReverseLeftShiftImpl(leftUInt16, (Byte)right);
                    case TypeCode.SByte:
                        return IntOps.ReverseLeftShift(leftUInt16, (SByte)right);
                    case TypeCode.Int16:
                        return IntOps.ReverseLeftShift(leftUInt16, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt16Ops.ReverseLeftShiftImpl(leftUInt16, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.ReverseLeftShift(leftUInt16, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt32Ops.ReverseLeftShiftImpl(leftUInt16, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.ReverseLeftShift(leftUInt16, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.ReverseLeftShiftImpl(leftUInt16, (UInt64)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseLeftShift(leftUInt16, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.ReverseLeftShift(leftUInt16, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseLeftShift(leftUInt16, ((ExtensibleLong)right).Value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__pow__")]
        public static object Power(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__pow__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return UInt16Ops.PowerImpl(leftUInt16, ((Boolean)right ? (UInt16)1 : (UInt16)0));
                    case TypeCode.Byte:
                        return UInt16Ops.PowerImpl(leftUInt16, (Byte)right);
                    case TypeCode.SByte:
                        return IntOps.Power(leftUInt16, (SByte)right);
                    case TypeCode.Int16:
                        return IntOps.Power(leftUInt16, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt16Ops.PowerImpl(leftUInt16, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.Power(leftUInt16, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt32Ops.PowerImpl(leftUInt16, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.Power(leftUInt16, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.PowerImpl(leftUInt16, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.PowerImpl(leftUInt16, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.Power(leftUInt16, (Double)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.Power(leftUInt16, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.Power(leftUInt16, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.Power(leftUInt16, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.Power(leftUInt16, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.Power(leftUInt16, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.Power(leftUInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rpow__")]
        public static object ReversePower(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__rpow__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return UInt16Ops.ReversePowerImpl(leftUInt16, ((Boolean)right ? (UInt16)1 : (UInt16)0));
                    case TypeCode.Byte:
                        return UInt16Ops.ReversePowerImpl(leftUInt16, (Byte)right);
                    case TypeCode.SByte:
                        return IntOps.ReversePower(leftUInt16, (SByte)right);
                    case TypeCode.Int16:
                        return IntOps.ReversePower(leftUInt16, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt16Ops.ReversePowerImpl(leftUInt16, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.ReversePower(leftUInt16, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt32Ops.ReversePowerImpl(leftUInt16, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.ReversePower(leftUInt16, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.ReversePowerImpl(leftUInt16, (UInt64)right);
                    case TypeCode.Single:
                        return SingleOps.ReversePowerImpl(leftUInt16, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.ReversePower(leftUInt16, (Double)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.ReversePower(leftUInt16, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.ReversePower(leftUInt16, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReversePower(leftUInt16, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return ComplexOps.ReversePower(leftUInt16, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReversePower(leftUInt16, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return ComplexOps.ReversePower(leftUInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rshift__")]
        public static object RightShift(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__rshift__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return UInt16Ops.RightShiftImpl(leftUInt16, ((Boolean)right ? (UInt16)1 : (UInt16)0));
                    case TypeCode.Byte:
                        return UInt16Ops.RightShiftImpl(leftUInt16, (Byte)right);
                    case TypeCode.SByte:
                        return IntOps.RightShift(leftUInt16, (SByte)right);
                    case TypeCode.Int16:
                        return IntOps.RightShift(leftUInt16, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt16Ops.RightShiftImpl(leftUInt16, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.RightShift(leftUInt16, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt32Ops.RightShiftImpl(leftUInt16, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.RightShift(leftUInt16, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.RightShiftImpl(leftUInt16, (UInt64)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.RightShift(leftUInt16, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.RightShift(leftUInt16, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.RightShift(leftUInt16, ((ExtensibleLong)right).Value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rrshift__")]
        public static object ReverseRightShift(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__rrshift__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return UInt16Ops.ReverseRightShiftImpl(leftUInt16, ((Boolean)right ? (UInt16)1 : (UInt16)0));
                    case TypeCode.Byte:
                        return UInt16Ops.ReverseRightShiftImpl(leftUInt16, (Byte)right);
                    case TypeCode.SByte:
                        return IntOps.ReverseRightShift(leftUInt16, (SByte)right);
                    case TypeCode.Int16:
                        return IntOps.ReverseRightShift(leftUInt16, (Int16)right);
                    case TypeCode.UInt16:
                        return UInt16Ops.ReverseRightShiftImpl(leftUInt16, (UInt16)right);
                    case TypeCode.Int32:
                        return IntOps.ReverseRightShift(leftUInt16, (Int32)right);
                    case TypeCode.UInt32:
                        return UInt32Ops.ReverseRightShiftImpl(leftUInt16, (UInt32)right);
                    case TypeCode.Int64:
                        return Int64Ops.ReverseRightShift(leftUInt16, (Int64)right);
                    case TypeCode.UInt64:
                        return UInt64Ops.ReverseRightShiftImpl(leftUInt16, (UInt64)right);
                }
            }
            if (right is BigInteger) {
                return LongOps.ReverseRightShift(leftUInt16, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return IntOps.ReverseRightShift(leftUInt16, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return LongOps.ReverseRightShift(leftUInt16, ((ExtensibleLong)right).Value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__truediv__")]
        public static object TrueDivide(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__truediv__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return FloatOps.TrueDivide(leftUInt16, ((Boolean)right ? (UInt16)1 : (UInt16)0));
                    case TypeCode.Byte:
                        return FloatOps.TrueDivide(leftUInt16, (Byte)right);
                    case TypeCode.SByte:
                        return FloatOps.TrueDivide(leftUInt16, (SByte)right);
                    case TypeCode.Int16:
                        return FloatOps.TrueDivide(leftUInt16, (Int16)right);
                    case TypeCode.UInt16:
                        return FloatOps.TrueDivide(leftUInt16, (UInt16)right);
                    case TypeCode.Int32:
                        return FloatOps.TrueDivide(leftUInt16, (Int32)right);
                    case TypeCode.UInt32:
                        return FloatOps.TrueDivide(leftUInt16, (UInt32)right);
                    case TypeCode.Int64:
                        return FloatOps.TrueDivide(leftUInt16, (Int64)right);
                    case TypeCode.UInt64:
                        return FloatOps.TrueDivide(leftUInt16, (UInt64)right);
                    case TypeCode.Single:
                        return FloatOps.TrueDivide(leftUInt16, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.TrueDivide(leftUInt16, (Double)right);
                }
            }
            if (right is BigInteger) {
                return FloatOps.TrueDivide(leftUInt16, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return FloatOps.TrueDivide(leftUInt16, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return FloatOps.TrueDivide(leftUInt16, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return FloatOps.TrueDivide(leftUInt16, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.TrueDivide(leftUInt16, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.TrueDivide(leftUInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        [PythonName("__rtruediv__")]
        public static object ReverseTrueDivide(object left, object right) {
            if (!(left is UInt16)) {
                throw Ops.TypeError("'__rtruediv__' requires UInt16, but received {0}", Ops.GetDynamicType(left).__name__);
            }
            UInt16 leftUInt16 = (UInt16)left;
            IConvertible rightConvertible;
            if ((rightConvertible = right as IConvertible) != null) {
                switch (rightConvertible.GetTypeCode()) {
                    case TypeCode.Boolean:
                        return FloatOps.ReverseTrueDivide(leftUInt16, ((Boolean)right ? (UInt16)1 : (UInt16)0));
                    case TypeCode.Byte:
                        return FloatOps.ReverseTrueDivide(leftUInt16, (Byte)right);
                    case TypeCode.SByte:
                        return FloatOps.ReverseTrueDivide(leftUInt16, (SByte)right);
                    case TypeCode.Int16:
                        return FloatOps.ReverseTrueDivide(leftUInt16, (Int16)right);
                    case TypeCode.UInt16:
                        return FloatOps.ReverseTrueDivide(leftUInt16, (UInt16)right);
                    case TypeCode.Int32:
                        return FloatOps.ReverseTrueDivide(leftUInt16, (Int32)right);
                    case TypeCode.UInt32:
                        return FloatOps.ReverseTrueDivide(leftUInt16, (UInt32)right);
                    case TypeCode.Int64:
                        return FloatOps.ReverseTrueDivide(leftUInt16, (Int64)right);
                    case TypeCode.UInt64:
                        return FloatOps.ReverseTrueDivide(leftUInt16, (UInt64)right);
                    case TypeCode.Single:
                        return FloatOps.ReverseTrueDivide(leftUInt16, (Single)right);
                    case TypeCode.Double:
                        return FloatOps.ReverseTrueDivide(leftUInt16, (Double)right);
                }
            }
            if (right is BigInteger) {
                return FloatOps.ReverseTrueDivide(leftUInt16, (BigInteger)right);
            } else if (right is ExtensibleInt) {
                return FloatOps.ReverseTrueDivide(leftUInt16, ((ExtensibleInt)right).value);
            } else if (right is ExtensibleLong) {
                return FloatOps.ReverseTrueDivide(leftUInt16, ((ExtensibleLong)right).Value);
            } else if (right is Complex64) {
                return FloatOps.ReverseTrueDivide(leftUInt16, (Complex64)right);
            } else if (right is ExtensibleFloat) {
                return FloatOps.ReverseTrueDivide(leftUInt16, ((ExtensibleFloat)right).value);
            } else if (right is ExtensibleComplex) {
                return FloatOps.ReverseTrueDivide(leftUInt16, ((ExtensibleComplex)right).value);
            }
            return Ops.NotImplemented;
        }
        internal static object DivideImpl(UInt16 x, UInt16 y) {
            return (UInt16)(x / y);
        }
        internal static object ModImpl(UInt16 x, UInt16 y) {
            return (UInt16)(x % y);
        }


        internal static object DivModImpl(UInt16 x, UInt16 y) {
            object div = DivideImpl(x, y);
            if (div == Ops.NotImplemented) return div;
            object mod = ModImpl(x, y);
            if (mod == Ops.NotImplemented) return mod;
            return Tuple.MakeTuple(div, mod);
        }
        internal static object ReverseDivideImpl(UInt16 x, UInt16 y) {
            return DivideImpl(y, x);
        }
        internal static object ReverseModImpl(UInt16 x, UInt16 y) {
            return ModImpl(y, x);
        }
        internal static object ReverseDivModImpl(UInt16 x, UInt16 y) {
            return DivModImpl(y, x);
        }
        internal static object FloorDivideImpl(UInt16 x, UInt16 y) {
            return DivideImpl(x, y);
        }
        internal static object ReverseFloorDivideImpl(UInt16 x, UInt16 y) {
            return DivideImpl(y, x);
        }
        internal static object ReverseLeftShiftImpl(UInt16 x, UInt16 y) {
            return LeftShiftImpl(y, x);
        }
        internal static object ReversePowerImpl(UInt16 x, UInt16 y) {
            return PowerImpl(y, x);
        }
        internal static object ReverseRightShiftImpl(UInt16 x, UInt16 y) {
            return RightShiftImpl(y, x);
        }


        // *** END GENERATED CODE ***

        #endregion

    }
}
