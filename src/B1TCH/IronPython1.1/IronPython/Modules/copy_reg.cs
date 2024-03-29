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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

using IronMath;
using IronPython.Compiler.Ast;
using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

[assembly: PythonModule("copy_reg", typeof(IronPython.Modules.PythonCopyReg))]
namespace IronPython.Modules {
    [Documentation("Provides global reduction-function registration for pickling and copying objects.")]
    public static class PythonCopyReg {

        private static Dict dispatchTable = new Dict();
        private static Dict extensionCache = new Dict();
        private static Dict extensionRegistry = new Dict();
        private static Dict invertedRegistry = new Dict();

        private static BuiltinFunction pythonReduceComplex;
        private static BuiltinFunction pythonReconstructor;
        private static BuiltinFunction pythonCreateNewObject;

        public static Dict DispatchTable {
            [PythonName("dispatch_table")] get { return dispatchTable; }
            [PythonName("dispatch_table")] set { dispatchTable = value; }
        }

        public static Dict ExtensionCache {
            [PythonName("_extension_cache")] get { return extensionCache; }
            [PythonName("_extension_cache")] set { extensionCache = value; }
        }

        public static Dict ExtensionRegistry {
            [PythonName("_extension_registry")] get { return extensionRegistry; }
            [PythonName("_extension_registry")] set { extensionRegistry = value; }
        }

        public static Dict InvertedRegistry {
            [PythonName("_inverted_registry")] get { return invertedRegistry; }
            [PythonName("_inverted_registry")] set { invertedRegistry = value; }
        }

        public static BuiltinFunction PythonReduceComplex {
            [PythonName("pickle_complex")] get { return pythonReduceComplex; }
            [PythonName("pickle_complex")] set { pythonReduceComplex = value; }
        }

        public static BuiltinFunction PythonReconstructor {
            [PythonName("_reconstructor")] get { return pythonReconstructor; }
            [PythonName("_reconstructor")] set { pythonReconstructor = value; }
        }

        public static BuiltinFunction PythonNewObject {
            [PythonName("__newobj__")] get { return pythonCreateNewObject; }
            [PythonName("__newobj__")] set { pythonCreateNewObject = value; }
        }

        static PythonCopyReg() {
            pythonReduceComplex = BuiltinFunction.MakeMethod(
                "pickle_complex",
                typeof(PythonCopyReg).GetMethod("ReduceComplex"),
                FunctionType.Function | FunctionType.PythonVisible
            );

            pythonReconstructor = BuiltinFunction.MakeMethod(
                "_reconstructor",
                typeof(PythonCopyReg).GetMethod("Reconstructor"),
                FunctionType.Function | FunctionType.PythonVisible
            );

            pythonCreateNewObject = BuiltinFunction.MakeMethod(
                "__newobj__",
                typeof(PythonCopyReg).GetMethod("NewObject"),
                FunctionType.Function | FunctionType.PythonVisible
            );

            dispatchTable[TypeCache.Complex64] = pythonReduceComplex;
        }

        #region Public API

        [Documentation("pickle(type, function[, constructor]) -> None\n\n"
            + "Associate function with type, indicating that function should be used to\n"
            + "\"reduce\" objects of the given type when pickling. function should behave as\n"
            + "specified by the \"Extended __reduce__ API\" section of PEP 307.\n"
            + "\n"
            + "Reduction functions registered by calling pickle() can be retrieved later\n"
            + "through copy_reg.dispatch_table[type].\n"
            + "\n"
            + "Note that calling pickle() will overwrite any previous association for the\n"
            + "given type.\n"
            + "\n"
            + "The constructor argument is ignored, and exists only for backwards\n"
            + "compatibility."
            )]
        [PythonName("pickle")]
        public static void RegisterReduceFunction(object type, object function, [DefaultParameterValue(null)] object constructor) {
            EnsureCallable(function, "reduction functions must be callable");
            if (constructor != null) Constructor(constructor);
            dispatchTable[type] = function;
        }

        [Documentation("constructor(object) -> None\n\n"
            + "Raise TypeError if object isn't callable. This function exists only for\n"
            + "backwards compatibility; for details, see\n"
            + "http://mail.python.org/pipermail/python-dev/2006-June/066831.html."
            )]
        [PythonName("constructor")]
        public static void Constructor(object callable) {
            EnsureCallable(callable, "constructors must be callable");
        }

        /// <summary>
        /// Throw TypeError with a specified message if object isn't callable.
        /// </summary>
        private static void EnsureCallable(object @object, string message) {
            if (!Ops.IsCallable(@object)) {
                throw Ops.TypeError(message);
            }
        }

        [Documentation("pickle_complex(complex_number) -> (<type 'complex'>, (real, imag))\n\n"
            + "Reduction function for pickling complex numbers.")]
        [PythonName("pickle_complex")]
        public static Tuple ReduceComplex(object complex) {
            return Tuple.MakeTuple(
                Ops.GetDynamicTypeFromType(typeof(Complex64)),
                Tuple.MakeTuple(
                    Ops.GetAttr(DefaultContext.Default, complex, SymbolTable.RealPart),
                    Ops.GetAttr(DefaultContext.Default, complex, SymbolTable.ImaginaryPart)
                )
            );
        }

        [PythonName("clear_extension_cache")]
        public static void ClearExtensionCache() {
            extensionCache.Clear();
        }

        [Documentation("Register an extension code.")]
        [PythonName("add_extension")]
        public static void AddExtension(object moduleName, object objectName, object value) {
            Tuple key = Tuple.MakeTuple(moduleName, objectName);
            int code = GetCode(value);

            bool keyExists = extensionRegistry.ContainsKey(key);
            bool codeExists = invertedRegistry.ContainsKey(code);

            if (!keyExists && !codeExists) {
                extensionRegistry[key] = code;
                invertedRegistry[code] = key;
            } else if (keyExists && codeExists &&
                Ops.EqualRetBool(extensionRegistry[key], code) &&
                Ops.EqualRetBool(invertedRegistry[code], key)
            ) {
                // nop
            } else {
                if (keyExists) {
                    throw Ops.ValueError("key {0} is already registered with code {1}", Ops.Repr(key), Ops.Repr(extensionRegistry[key]));
                } else { // codeExists
                    throw Ops.ValueError("code {0} is already in use for key {1}", Ops.Repr(code), Ops.Repr(invertedRegistry[code]));
                }
            }
        }

        [Documentation("Unregister an extension code. (only for testing)")]
        [PythonName("remove_extension")]
        public static void RemoveExtension(object moduleName, object objectName, object value) {
            Tuple key = Tuple.MakeTuple(moduleName, objectName);
            int code = GetCode(value);

            object existingKey;
            object existingCode;

            if (extensionRegistry.TryGetValue(key, out existingCode) &&
                invertedRegistry.TryGetValue(code, out existingKey) &&
                Ops.EqualRetBool(existingCode, code) &&
                Ops.EqualRetBool(existingKey, key)
            ) {
                extensionRegistry.DeleteItem(key);
                invertedRegistry.DeleteItem(code);
            } else {
                throw Ops.ValueError("key {0} is not registered with code {1}", Ops.Repr(key), Ops.Repr(code));
            }
        }

        [Documentation("__newobj__(cls, *args) -> cls.__new__(cls, *args)\n\n"
            + "Helper function for unpickling. Creates a new object of a given class.\n"
            + "See PEP 307 section \"The __newobj__ unpickling function\" for details."
            )]
        [PythonName("__newobj__")]
        public static object NewObject(object cls, params object[] args) {
            object[] newArgs = new object[1 + args.Length];
            newArgs[0] = cls;
            for (int i = 0; i < args.Length; i++) newArgs[i + 1] = args[i];
            return Ops.Invoke(cls, SymbolTable.NewInst, newArgs);
        }

        [Documentation("_reconstructor(basetype, objtype, basestate) -> object\n\n"
            + "Helper function for unpickling. Creates and initializes a new object of a given\n"
            + "class. See PEP 307 section \"Case 2: pickling new-style class instances using\n"
            + "protocols 0 or 1\" for details."
            )]
        [PythonName("_reconstructor")]
        public static object Reconstructor(object objType, object baseType, object baseState) {
            object obj = Ops.Invoke(baseType, SymbolTable.NewInst, objType, baseState);
            Ops.Invoke(baseType, SymbolTable.Init, obj, baseState);
            return obj;
        }

        #endregion

        #region Private implementation

        /// <summary>
        /// Convert object to ushort, throwing ValueError on overflow.
        /// </summary>
        private static int GetCode(object value) {
            try {
                int intValue = Converter.ConvertToInt32(value);
                if (intValue > 0) return intValue;
                // fall through and throw below
            } catch (OverflowException) {
                // throw below
            }
            throw Ops.ValueError("code out of range");
        }

        #endregion

    }
}
