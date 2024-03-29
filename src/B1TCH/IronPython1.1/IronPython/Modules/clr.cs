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
using System.Reflection;
using System.IO;
using System.Diagnostics;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;

namespace IronPython.Modules {
    /// <summary>
    /// this class contains objecs and static methods used for
    /// .NET/CLS interop with Python.  
    /// </summary>
    //[PythonType(typeof(PythonModule))]
    public class ClrModule : IDisposable {
        SystemState state;
        public ClrModule(SystemState systemState) {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            state = systemState;
        }

        public override string ToString() {
            return "<module 'clr' (built-in)>";
        }

        public object References = new ReferencesTuple();

        #region Public methods

        public void AddReference(params object[] references) {
            if (references == null) throw Ops.TypeError("Expected string or Assembly, got NoneType");

            foreach (object reference in references) {
                AddReference(reference);
            }
        }

        public void AddReferenceToFile(params string[] files) {
            if (files == null) throw Ops.TypeError("Expected string, got NoneType");

            foreach (string file in files) {
                AddReferenceToFile(file);
            }
        }

        public void AddReferenceToFileAndPath(params string[] files) {
            if (files == null) throw Ops.TypeError("Expected string, got NoneType");

            foreach (string file in files) {
                AddReferenceToFileAndPath(file);
            }
        }

        public void AddReferenceByName(params string[] names) {
            if (names == null) throw Ops.TypeError("Expected string, got NoneType");

            foreach (string name in names) {
                AddReferenceByName(name);
            }
        }

        public void AddReferenceByPartialName(params string[] names) {
            if (names == null) throw Ops.TypeError("Expected string, got NoneType");

            foreach (string name in names) {
                AddReferenceByPartialName(name);
            }
        }

        public Assembly LoadAssemblyFromFileWithPath(string file) {
            if (file == null) throw Ops.TypeError("LoadAssemblyFromFileWithPath: arg 1 must be a string.");
            return Assembly.LoadFile(file);
        }

        public Assembly LoadAssemblyFromFile(string file) {
            if (file == null) throw Ops.TypeError("Expected string, got NoneType");

            if (file.IndexOf(System.IO.Path.DirectorySeparatorChar) != -1) {
                throw Ops.ValueError("filenames must not contain full paths, first add the path to sys.path");
            }

            // check all files in the path...
            IEnumerator ie = Ops.GetEnumerator(state.path);
            while (ie.MoveNext()) {
                string str;
                if (Converter.TryConvertToString(ie.Current, out str)) {
                    string fullName = System.IO.Path.Combine(str, file);
                    Assembly res;

                    if (TryLoadAssemblyFromFileWithPath(fullName, out res)) return res;
                    if (TryLoadAssemblyFromFileWithPath(fullName + ".EXE", out res)) return res;
                    if (TryLoadAssemblyFromFileWithPath(fullName + ".DLL", out res)) return res;
                }
            }

            return null;
        }

        public Assembly LoadAssemblyByName(string name) {
            if (name == null) throw Ops.TypeError("LoadAssemblyByName: arg 1 must be a string");
            return Assembly.Load(name);
        }

        public Assembly LoadAssemblyByPartialName(string name) {
            if (name == null) throw Ops.TypeError("LoadAssemblyByPartialName: arg 1 must be a string");
#pragma warning disable 618
            return Assembly.LoadWithPartialName(name);
#pragma warning restore 618
        }

        public Type GetClrType(Type type) {
            return type;
        }

        public DynamicType GetPythonType(Type t) {
            return Ops.GetDynamicTypeFromType(t);
        }

        #endregion

        public class Reference<T> : IReference {
            private T value;
            public Reference() {
                value = default(T);
            }

            public Reference(T value) {
                this.value = value;
            }

            public T Value {
                get { return value; }
                set { this.value = value; }
            }

            public override string ToString() {
                if ((object)Value == this)
                    return "Reference (...)";
                return string.Format("Reference({0})", Value);
            }

            /// <summary>
            /// Always throw an exception when trying to convert a Reference to
            /// a boolean.
            /// </summary>
            /// <returns></returns>
            [PythonName("__nonzero__")]
            public bool ToBoolean() {
                throw Ops.TypeError("Can't convert a Reference[{0}] instance to a bool", Ops.GetDynamicTypeFromType(typeof(T)).Name);
            }


            #region IReference Members

            object IReference.Value {
                get { return value; }
                set { this.value = (T)value; }
            }

            #endregion
        }


        #region Private implementation methods

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            AssemblyName an = new AssemblyName(args.Name);
            return LoadAssemblyFromFile(an.Name);
        }

        private void AddReference(object reference) {
            Assembly asmRef = reference as Assembly;
            if (asmRef != null) {
                AddReference(asmRef);
                return;
            }

            string strRef = reference as string;
            if (strRef != null) {
                AddReference(strRef);
                return;
            }

            throw Ops.TypeError("invalid assembly type. expected string or Assembly, got {0}", Ops.GetPythonTypeName(reference));
        }

        private void AddReference(Assembly assembly) {
            Tuple referencesTuple = References as Tuple;
            if (referencesTuple == null) {
                throw Ops.TypeError("cannot add reference");
            }

            // Load the assembly into IronPython
            if (state.TopPackage.LoadAssembly(state, assembly)) {
                // Add it to the references tuple if we
                // loaded a new assembly.
                References = referencesTuple.AddSequence(Tuple.MakeTuple(assembly));
            }
        }

        private void AddReference(string name) {
            if (name == null) throw Ops.TypeError("Expected string, got NoneType");

            Assembly asm = null;

            try {
                asm = LoadAssemblyByName(name);
            } catch { }

            // note we don't explicit call to get the file version
            // here because the assembly resolve event will do it for us.

            if (asm == null) {
                asm = LoadAssemblyByPartialName(name);
            }

            if (asm == null) {
                throw Ops.IOError("Could not add reference to assembly {0}", name);
            }
            AddReference(asm);
        }

        private void AddReferenceToFileAndPath(string file) {
            if (file == null) throw Ops.TypeError("Expected string, got NoneType");

            // update our path w/ the path of this file...
            string path = System.IO.Path.GetDirectoryName(file);
            List list = state.path;
            if (list == null) throw Ops.TypeError("cannot update path, it is not a list");

            list.Add(path);

            // then fall through to the normal loading process
            AddReferenceToFile(System.IO.Path.GetFileName(file));
        }

        private void AddReferenceToFile(string file) {
            if (file == null) throw Ops.TypeError("Expected string, got NoneType");

            Assembly asm = LoadAssemblyFromFile(file);
            if (asm == null) {
                throw Ops.IOError("Could not add reference to assembly {0}", file);
            }

            AddReference(asm);
        }

        private void AddReferenceByName(string name) {
            if (name == null) throw Ops.TypeError("Expected string, got NoneType");

            Assembly asm = LoadAssemblyByName(name);

            if (asm == null) {
                throw Ops.IOError("Could not add reference to assembly {0}", name);
            }

            AddReference(asm);
        }

        private void AddReferenceByPartialName(string name) {
            if (name == null) throw Ops.TypeError("Expected string, got NoneType");

            Assembly asm = LoadAssemblyByPartialName(name);
            if (asm == null) {
                throw Ops.IOError("Could not add reference to assembly {0}", name);
            }

            AddReference(asm);
        }

        private bool TryLoadAssemblyFromFileWithPath(string path, out Assembly res) {
            if (File.Exists(path)) {
                try {
                    res = LoadAssemblyFromFileWithPath(path);
                    if (res != null) return true;
                } catch { }
            }
            res = null;
            return false;
        }

        #endregion


        #region Runtime Type Checking support

        // Supports runtime type checking.  These decorators are currently primarily
        // used by the codedom implementation to carry
        // extra information regarding types at runtime.

        [PythonName("accepts")]
        public object Accepts(params object[] types) {
            return new ArgChecker(types);
        }

        [PythonName("returns")]
        public object Returns(object type) {
            return new ReturnChecker(type);
        }

        [PythonName("Self")]
        public object Self() {
            return null;
        }

        [PythonType("accepts_checker")]
        public class ArgChecker : ICallable {
            private object[] expected;

            public ArgChecker(object[] prms) {
                expected = prms;
            }

            #region ICallable Members

            public object Call(params object[] args) {
                // expect only to receive the function we'll call here.
                if (args.Length != 1) throw Ops.TypeError("bad arg count");

                return new RuntimeChecker(args[0], expected);
            }

            #endregion


            [PythonType("checker")]
            public class RuntimeChecker : ICallable, IDescriptor {
                private object[] expected;
                private object func;
                private object inst;

                public RuntimeChecker(object function, object[] expectedArgs) {
                    expected = expectedArgs;
                    func = function;
                }

                public RuntimeChecker(object instance, object function, object[] expectedArgs)
                    : this(function, expectedArgs) {
                    inst = instance;
                }

                private void ValidateArgs(object[] args) {
                    int start = 0;

                    if (inst != null) {
                        start = 1;
                    }


                    // no need to validate self... the method should handle it.
                    for (int i = start; i < args.Length + start; i++) {
                        DynamicType dt = Ops.GetDynamicType(args[i - start]);
                        if (dt != expected[i] && !dt.IsSubclassOf(expected[i])) {
                            throw Ops.AssertionError("argument {0} has bad value (got {1}, expected {2})", i, dt, expected[i]);
                        }
                    }
                }

                #region ICallable Members

                public object Call(params object[] args) {
                    ValidateArgs(args);

                    if (inst != null) {
                        object[] realArgs = new object[args.Length + 1];
                        realArgs[0] = inst;
                        Array.Copy(args, 0, realArgs, 1, args.Length);
                        return Ops.Call(func, realArgs);
                    } else {
                        return Ops.Call(func, args);
                    }
                }

                #endregion

                #region IDescriptor Members

                public object GetAttribute(object instance, object owner) {
                    return new RuntimeChecker(instance, func, expected);
                }

                #endregion
            }
        }

        [PythonType("return_checker")]
        public class ReturnChecker : ICallable {
            public object retType;

            public ReturnChecker(object returnType) {
                retType = returnType;
            }

            #region ICallable Members

            public object Call(params object[] args) {
                // expect only to receive the function we'll call here.
                if (args.Length != 1) throw Ops.TypeError("bad arg count");

                return new RuntimeChecker(args[0], retType);
            }

            #endregion

            [PythonType("checker")]
            public class RuntimeChecker : ICallable, IDescriptor {
                private object retType;
                private object func;
                private object inst;

                public RuntimeChecker(object function, object expectedReturn) {
                    retType = expectedReturn;
                    func = function;
                }

                public RuntimeChecker(object instance, object function, object expectedReturn)
                    : this(function, expectedReturn) {
                    inst = instance;
                }

                private void ValidateReturn(object ret) {
                    // we return void...
                    if (ret == null && retType == null) return;

                    DynamicType dt = Ops.GetDynamicType(ret);
                    if (dt != retType) {
                        if (!dt.IsSubclassOf(retType))
                            throw Ops.AssertionError("bad return value returned (expected {0}, got {1})", retType, dt);
                    }
                }

                #region ICallable Members

                public object Call(params object[] args) {
                    object ret;
                    if (inst != null) {
                        object[] realArgs = new object[args.Length + 1];
                        realArgs[0] = inst;
                        Array.Copy(args, 0, realArgs, 1, args.Length);
                        ret = Ops.Call(func, realArgs);
                    } else {
                        ret = Ops.Call(func, args);
                    }
                    ValidateReturn(ret);
                    return ret;
                }

                #endregion

                #region IDescriptor Members

                public object GetAttribute(object instance, object owner) {
                    return new RuntimeChecker(instance, func, retType);
                }

                #endregion
            }

        }
        #endregion

        #region IDisposable Members

        public void Dispose() {
            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        #endregion
    }


    /// <summary>
    /// Special subclass of Tuple to provide improved formatting when outputting the Tuple.
    /// </summary>
    [PythonType("references_tuple")]
    class ReferencesTuple : Tuple {

        public ReferencesTuple()
            : base(Tuple.MakeTuple()) {
        }

        public ReferencesTuple(object data)
            : base(data) {
        }

        [PythonName("__add__")]
        public override object AddSequence(object other) {
            Tuple o = other as Tuple;
            if (o == null) {
                throw Ops.TypeErrorForBadInstance("can only concatenate tuple (not \"{0}\") to tuple", other);
            }

            List newData = new List(this);
            foreach (object item in o) {
                newData.AddNoLock(item);
            }
            return new ReferencesTuple(newData);
        }

        [PythonName("__str__")]
        public override string ToString() {
            StringBuilder buf = new StringBuilder();
            buf.Append("(");
            for (int i = 0; i < this.GetLength(); i++) {
                if (i != 0) buf.AppendLine(",");
                buf.Append('<');
                buf.Append(this[i].ToString());
                buf.Append('>');
            }
            buf.AppendLine(")");
            return buf.ToString();
        }
    }
}
