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
using System.Collections.Generic;
using System.Text;

using IronMath;
using IronPython.Runtime;

namespace IronPython.Runtime.Operations {
    public partial class ExtensibleComplex {
        #region Generated Extensible ComplexOps

        #endregion
    }
    public static partial class ComplexOps {
        #region Generated ComplexOps

        // *** BEGIN GENERATED CODE ***


        [PythonName("__add__")]
        public static object Add(Complex64 x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                return x + (int)other;
            } else if (other is Complex64) {
                return x + (Complex64)other;
            } else if (other is double) {
                return x + (double)other;
            } else if ((object)(bi = other as BigInteger) != null) {
                return x + bi;
            } else if (other is long) {
                return x + (long)other;
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return x + xc.value;
            } else if ((object)(num = other as INumber) != null) {
                return num.ReverseAdd(x);
            } else if (other is string) {
                return Ops.NotImplemented;
            } else if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return x + y;
            }
            return Ops.NotImplemented;
        }


        [PythonName("__radd__")]
        public static object ReverseAdd(Complex64 x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                return (int)other + x;
            } else if (other is Complex64) {
                return (Complex64)other + x;
            } else if (other is double) {
                return (double)other + x;
            } else if ((object)(bi = other as BigInteger) != null) {
                return bi + x;
            } else if (other is long) {
                return (long)other + x;
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return xc.value + x;
            } else if ((object)(num = other as INumber) != null) {
                return num.Add(x);
            } else if (other is string) {
                return Ops.NotImplemented;
            } else if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return y + x;
            }
            return Ops.NotImplemented;
        }


        [PythonName("__sub__")]
        public static object Subtract(Complex64 x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                return x - (int)other;
            } else if (other is Complex64) {
                return x - (Complex64)other;
            } else if (other is double) {
                return x - (double)other;
            } else if ((object)(bi = other as BigInteger) != null) {
                return x - bi;
            } else if (other is long) {
                return x - (long)other;
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return x - xc.value;
            } else if ((object)(num = other as INumber) != null) {
                return num.ReverseSubtract(x);
            } else if (other is string) {
                return Ops.NotImplemented;
            } else if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return x - y;
            }
            return Ops.NotImplemented;
        }


        [PythonName("__rsub__")]
        public static object ReverseSubtract(Complex64 x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                return (int)other - x;
            } else if (other is Complex64) {
                return (Complex64)other - x;
            } else if (other is double) {
                return (double)other - x;
            } else if ((object)(bi = other as BigInteger) != null) {
                return bi - x;
            } else if (other is long) {
                return (long)other - x;
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return xc.value - x;
            } else if ((object)(num = other as INumber) != null) {
                return num.Subtract(x);
            } else if (other is string) {
                return Ops.NotImplemented;
            } else if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return y - x;
            }
            return Ops.NotImplemented;
        }


        [PythonName("__pow__")]
        public static object Power(Complex64 x, object other) {
            BigInteger bi;
            INumber num;
            ExtensibleComplex xc;

            if (other is int) return Power(x, (Complex64)((int)other));
            if (other is Complex64) return Power(x, (Complex64)other);
            if (other is double) return Power(x, (Complex64)((double)other));
            if ((object)(bi = other as BigInteger) != null) return Power(x, (Complex64)bi);
            if (other is bool) return Power(x, (Complex64)((bool)other ? 1 : 0));
            if (other is long) return Power(x, (Complex64)((long)other));
            if ((object)(xc = other as ExtensibleComplex) != null) return Power(x, xc.value);
            if ((object)(num = other as INumber) != null) return num.ReversePower(x);
            if (other is byte) return Power(x, (Complex64)(int)((byte)other));
            return Ops.NotImplemented;
        }


        [PythonName("__rpow__")]
        public static object ReversePower(Complex64 x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) return ReversePower(x, (Complex64)((int)other));
            if (other is Complex64) return ReversePower(x, (Complex64)other);
            if (other is double) return ReversePower(x, (Complex64)((double)other));
            if ((object)(bi = other as BigInteger) != null) return ReversePower(x, (Complex64)bi);
            if (other is bool) return ReversePower(x, (Complex64)((bool)other ? 1 : 0));
            if (other is long) return ReversePower(x, (Complex64)((long)other));
            if ((object)(xc = other as ExtensibleComplex) != null) return ReversePower(x, xc.value);
            if ((object)(num = other as INumber) != null) return num.Power(x);
            if (other is byte) return ReversePower(x, (Complex64)(int)((byte)other));
            return Ops.NotImplemented;
        }


        [PythonName("__mul__")]
        public static object Multiply(Complex64 x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                return x * (int)other;
            } else if (other is Complex64) {
                return x * (Complex64)other;
            } else if (other is double) {
                return x * (double)other;
            } else if ((object)(bi = other as BigInteger) != null) {
                return x * bi;
            } else if (other is long) {
                return x * (long)other;
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return x * xc.value;
            } else if ((object)(num = other as INumber) != null) {
                return num.ReverseMultiply(x);
            } else if (other is string) {
                return Ops.NotImplemented;
            } else if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return x * y;
            }
            return Ops.NotImplemented;
        }


        [PythonName("__rmul__")]
        public static object ReverseMultiply(Complex64 x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) {
                return (int)other * x;
            } else if (other is Complex64) {
                return (Complex64)other * x;
            } else if (other is double) {
                return (double)other * x;
            } else if ((object)(bi = other as BigInteger) != null) {
                return bi * x;
            } else if (other is long) {
                return (long)other * x;
            } else if ((object)(xc = other as ExtensibleComplex) != null) {
                return xc.value * x;
            } else if ((object)(num = other as INumber) != null) {
                return num.Multiply(x);
            } else if (other is string) {
                return Ops.NotImplemented;
            } else if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return y * x;
            }
            return Ops.NotImplemented;
        }


        [PythonName("__truediv__")]
        public static object TrueDivide(Complex64 x, object other) {
            BigInteger bi;
            INumber num;
            ExtensibleComplex xc;

            if (other is int) return TrueDivide(x, (Complex64)((int)other));
            if (other is Complex64) return TrueDivide(x, (Complex64)other);
            if (other is double) return TrueDivide(x, (Complex64)((double)other));
            if ((object)(bi = other as BigInteger) != null) return TrueDivide(x, (Complex64)bi);
            if (other is bool) return TrueDivide(x, (Complex64)((bool)other ? 1 : 0));
            if (other is long) return TrueDivide(x, (Complex64)((long)other));
            if ((object)(xc = other as ExtensibleComplex) != null) return TrueDivide(x, xc.value);
            if ((object)(num = other as INumber) != null) return num.ReverseTrueDivide(x);
            if (other is byte) return TrueDivide(x, (Complex64)(int)((byte)other));
            return Ops.NotImplemented;
        }


        [PythonName("__rtruediv__")]
        public static object ReverseTrueDivide(Complex64 x, object other) {
            BigInteger bi;
            ExtensibleComplex xc;
            INumber num;

            if (other is int) return ReverseTrueDivide(x, (Complex64)((int)other));
            if (other is Complex64) return ReverseTrueDivide(x, (Complex64)other);
            if (other is double) return ReverseTrueDivide(x, (Complex64)((double)other));
            if ((object)(bi = other as BigInteger) != null) return ReverseTrueDivide(x, (Complex64)bi);
            if (other is bool) return ReverseTrueDivide(x, (Complex64)((bool)other ? 1 : 0));
            if (other is long) return ReverseTrueDivide(x, (Complex64)((long)other));
            if ((object)(xc = other as ExtensibleComplex) != null) return ReverseTrueDivide(x, xc.value);
            if ((object)(num = other as INumber) != null) return num.TrueDivide(x);
            if (other is byte) return ReverseTrueDivide(x, (Complex64)(int)((byte)other));
            return Ops.NotImplemented;
        }


        // *** END GENERATED CODE ***

        #endregion
    }
}
