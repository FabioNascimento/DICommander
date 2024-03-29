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
using System.Threading;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Types {

    [PythonType("ellipsis")]
    public class Ellipsis : ICodeFormattable {

        #region ICodeFormattable Members

        public string ToCodeString() {
            return "Ellipsis";
        }

        #endregion
    }

    public class NotImplemented : ICodeFormattable {
        #region ICodeFormattable Members

        public string ToCodeString() {
            return "NotImplemented";
        }

        #endregion
    }

    public class None : ICodeFormattable {
        #region ICodeFormattable Members

        public string ToCodeString() {
            return "None";
        }

        #endregion
    }

    class NoneTypeOps : EmptyTypeOps<NoneTypeOps> {
        public static ReflectedType MakeDynamicType() {
            return MakeDynamicType(0x1e1a1dd0,
                null,
                "None",
                "NoneType",
                typeof(None),
                typeof(NoneTypeOps));
        }
    }

    class EllipsisTypeOps : EmptyTypeOps<EllipsisTypeOps> {

        public static ReflectedType MakeDynamicType() {
            return MakeDynamicType(0x1e1a6208,
                new Ellipsis(),
                "Ellipsis",
                "ellipsis",
                typeof(Ellipsis),
                typeof(EllipsisTypeOps));
        }
    }

    class NotImplementedTypeOps : EmptyTypeOps<NotImplementedTypeOps> {

        public static ReflectedType MakeDynamicType() {
            return MakeDynamicType(0x1e1a1e98,
                new NotImplemented(),
                "NotImplemented",
                "NotImplementedType",
                typeof(NotImplemented),
                typeof(NotImplementedTypeOps));
        }
    }

    /// <summary>
    /// Provides default functionality for empty classes.  Empty classes consist of types
    /// such as None, Ellipsis, and NotImplemented.  These types all have the same members
    /// but differ by their names, hash codes, and singleton instances.  We use a single generic
    /// type that gets instantiated against the derived ops type (FooOps : EmptyTypeOps&lt;FooOps&gt;
    /// 
    /// The derived type provided the information such as name, concrete instance type, singleton
    /// instance value, etc...  All of this gets stored in the generic type's static fields which
    /// are bound per-type instantiation, leaving us with one set of the methods that need to be
    /// declared to ensure that all the types appear essentially the same.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    abstract class EmptyTypeOps<T> {
        private static ReflectedType typeInstance;
        private static string name;
        private static object instance;
        private static int hash;

        protected static ReflectedType MakeDynamicType(int hashCode,
            object singletonInstance,
            string instanceName,
            string typeName,
            Type baseType,
            Type opsType) {
            name = instanceName;
            instance = singletonInstance;
            hash = hashCode;

            if (typeInstance == null) {
                OpsReflectedType ret = new OpsReflectedType(typeName, baseType, opsType, null);
                if (Interlocked.CompareExchange<ReflectedType>(ref typeInstance, ret, null) == null)
                    return ret;
            }
            return typeInstance;
        }

        [StaticOpsMethod("__init__")]
        public static void InitMethod(params object[] prms) {
            // nop
        }

        internal static string Name {
            get {
                return name;
            }
        }

        internal static object Instance {
            get {
                return instance;
            }
        }

        internal static ReflectedType TypeInstance {
            get {
                return typeInstance;
            }
        }

        internal static int HashCode {
            get {
                return hash;
            }
        }

        [StaticOpsMethod("__hash__")]
        public static int HashMethod() {
            return hash;
        }

        [StaticOpsMethod("__repr__")]
        public static string ReprMethod() {
            return Name;
        }

        [StaticOpsMethod("__str__")]
        public static new string ToString() {
            return Name;
        }


        [PythonName("__new__")]
        public static object NewMethod(object type, params object[] prms) {
            if (type == typeInstance) {
                throw Ops.TypeError("cannot create instances of '{0}'", Name);
            }
            // someone is using  None.__new__ or type(None).__new__ to create
            // a new instance.  Call the type they want to create the instance for.
            return Ops.Call(type, prms);
        }

        [StaticOpsMethod("__delattr__")]
        public static void DelAttrMethod(string name) {
            typeInstance.DelAttr(DefaultContext.Default, instance, SymbolTable.StringToId(name));
        }

        [StaticOpsMethod("__getattribute__")]
        public static object GetAttributeMethod(string name) {
            return typeInstance.GetAttr(DefaultContext.Default, instance, SymbolTable.StringToId(name));
        }

        [StaticOpsMethod("__setattr__")]
        public static void SetAttrMethod(string name, object value) {
            typeInstance.SetAttr(DefaultContext.Default, instance, SymbolTable.StringToId(name), value);
        }

    }
}
