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

using System.Diagnostics;
using System.Reflection;
using System.Text;

using System.Threading;

using IronPython.Compiler;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Hosting;

using ClrModule = IronPython.Modules.ClrModule;

namespace IronPython.Runtime {

    [PythonType(typeof(PythonModule))]
    public class SystemState : ICustomAttributes, IDisposable {
        [PythonHiddenField]
        public Encoding DefaultEncoding;

        private IAttributesDictionary __dict__;
        private object exception_type, exception_value, exception_traceback;
        private TopReflectedPackage topPackage;
        private ClrModule clrModule;
        private EngineOptions engineOptions;

        // See _socket.cs for details
        internal bool isSocketRefCountHookInstalled;

        [PythonName("__new__")]
        public static object MakeModule(ICallerContext context, DynamicType cls, params object[] args\u03c4) {
            return PythonModule.MakeModule(context, cls);
        }

        [PythonName("__new__")]
        public static object MakeModule(ICallerContext context, DynamicType cls, [ParamDict] Dict kwDict\u03c4, params object[] args\u03c4) {
            return MakeModule(context, cls, args\u03c4);
        }

        public SystemState()
            : this(new EngineOptions()) {
        }

        public SystemState(EngineOptions options) {
#if DEBUG
            // All fields should be initialized in Initialize(). So ensure that the fields have default values
            FieldInfo[] fields = typeof(SystemState).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (FieldInfo field in fields) {
                object value = field.GetValue(this);
                // If this throws an InvalidCastException, it must mean that a new field was added of a type 
                // that is not handled here. The assert will need to be relaxed to handle the new type.
                Debug.Assert(value == null ||
                    (value is int && ((int)value) == 0) ||
                    (value is bool && ((bool)value) == false));
            }
#endif
            engineOptions = options;
            Initialize();
        }

        /// <summary>
        /// Performs sys's initialization
        /// It is in it's own function so we can do reload(sys). On reload(sys), most of the attributes need to be
        /// reset. The following are left as they are - argv, exc_type, modules, path, path_hooks, path_importer_cache, ps1, ps2.
        /// </summary>
        internal void Initialize() {
            if (__dict__ == null) {
                __dict__ = new FieldIdDict();

                // These fields do not get reset on "reload(sys)"
                argv = Ops.MakeList();
                modules = new Dict();
                modules["sys"] = this;
                modules["__builtin__"] = Importer.MakePythonModule(this, "__builtin__", TypeCache.Builtin);

                path = List.Make();
                ps1 = ">>> ";
                ps2 = "... ";
                isSocketRefCountHookInstalled = false;
                __stdin__ = new PythonFile(Console.OpenStandardInput(),
                                            Console.InputEncoding,
                                            "<stdin>",
                                            "r");
                __stdout__ = new PythonFile(Options.BufferedStandardOutAndError ? Console.OpenStandardOutput() : Console.OpenStandardOutput(0),
                                            Console.OutputEncoding,
                                            "<stdout>",
                                            "w");
                __stderr__ = new PythonFile(Options.BufferedStandardOutAndError ? Console.OpenStandardError() : Console.OpenStandardError(0),
                                            Console.OutputEncoding,
                                            "<stderr>",
                                            "w");
            }

            __dict__[SymbolTable.Name] = "sys";

            stdin = __stdin__;
            stdout = __stdout__;
            stderr = __stderr__;

            // removed from dictionary after the first call to set it.
            MethodInfo mi = typeof(SystemState).GetMethod("setdefaultencodingImpl",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            BuiltinMethodDescriptor descr = (BuiltinMethodDescriptor)BuiltinFunction.MakeMethod(
                "setdefaultencoding", mi, FunctionType.PythonVisible | FunctionType.Method).GetDescriptor();

            __dict__[SymbolTable.SetDefaultEncoding] = descr.GetAttribute(this, this);

            DefaultEncoding = Encoding.ASCII;
            byteorder = BitConverter.IsLittleEndian ? "little" : "big";
            copyright = "Copyright (c) Microsoft Corporation. All rights reserved.";
            maxint = Int32.MaxValue;
            maxunicode = (int)ushort.MaxValue;
            platform = "cli";
            version_info = Tuple.MakeTuple(2, 4, 0, "final", 0);    // report as being compatible w/ 2.4.0 final
            winver = "2.4";
            // !!! These fields do need to be reset on "reload(sys)". However, the initial value is specified by the 
            // engine elsewhere. For now, we initialize them just once to some default value
            if (version == null) {
                SetVersion(PythonEngine.VersionString);
                warnoptions = List.Make();
                executable = "";
            }
        }

        internal void SetVersion(string ironPythonVersion) {
            Version pythonVersion;
            string pythonVersionLevel;

            if (Options.Python25) {
                pythonVersion = new Version(2, 5, 0);
                pythonVersionLevel = "alpha";
            } else {
                pythonVersion = new Version(2, 4, 0);
                pythonVersionLevel = "release";
            }

            Debug.Assert(pythonVersion.Major <= 0xff, "version components must fit in two-hex-digit field");
            Debug.Assert(pythonVersion.Minor <= 0xff, "version components must fit in two-hex-digit field");
            Debug.Assert(pythonVersion.Build <= 0xff, "version components must fit in two-hex-digit field");

            // e.g. hexversion = 0x02040000;
            hexversion = (pythonVersion.Major << 24) + (pythonVersion.Minor << 16) + (pythonVersion.Build << 8);
            version_info = Tuple.MakeTuple(pythonVersion.Major, pythonVersion.Minor, pythonVersion.Build, pythonVersionLevel, 0);

            version = String.Format("{0}.{1}.{2} ({3})", pythonVersion.Major, pythonVersion.Minor, pythonVersion.Build, ironPythonVersion);
        }

        public override string ToString() {
            return "<module 'sys' (built-in)>";
        }

        public object api_version {
            [PythonName("api_version")]
            get { return "IronPython does not support the C APIs, the api_version is not supported"; }
            set { throw Ops.NotImplementedError("IronPython does not support the C APIs, the api_version is not supported"); }
        }

        [PythonName("argv")]
        public object argv;

        public string byteorder;

        [PythonName("builtin_module_names")]
        public Tuple builtin_module_names;

        [PythonName("copyright")]
        public string copyright;

        public object displayhook {
            [PythonName("displayhook")]
            get { return "IronPython does not support sys.displayhook"; }
            [PythonName("displayhook")]
            set { throw Ops.NotImplementedError("IronPython does not support sys.displayhook"); }
        }

        public object excepthook {
            [PythonName("excepthook")]
            get { return "IronPython does not support sys.excepthook"; }
            [PythonName("excepthook")]
            set { throw Ops.NotImplementedError("IronPython does not support sys.excepthook"); }
        }

        [PythonName("getcheckinterval")]
        public int GetCheckInterval() {
            throw Ops.NotImplementedError("IronPython does not support sys.getcheckinterval");
        }

        [PythonName("setcheckinterval")]
        public void SetCheckInterval(int value) {
            throw Ops.NotImplementedError("IronPython does not support sys.setcheckinterval");
        }

        [PythonName("warnoptions")]
        public List warnoptions;

        // as of 1.5 preferred access is exc_info, these may be null.
        public object exc_type {
            [PythonName("exc_type")]
            get {
                return exception_type;
            }
            set {
                exception_type = value;
            }
        }

        public object exc_value {
            [PythonName("exc_value")]
            get {
                return exception_value;
            }
            set {
                exception_value = value;
            }
        }

        public object exc_traceback {
            [PythonName("exc_traceback")]
            get {
                return exception_traceback;
            }
            set {
                exception_traceback = value;
            }
        }

        [ThreadStatic]
        internal static Exception RawException;

        [ThreadStatic]
        internal static TraceBack RawTraceBack;

        internal void SetRawException(Exception raw) {
            RawException = raw;

            // force update of non-thread static exception info...
            exc_info();
        }

        [PythonName("exc_clear")]
        public void ClearException() {
            ThreadAbortException tae = SystemState.RawException as ThreadAbortException;
            if (tae != null && tae.ExceptionState is PythonKeyboardInterruptException) {
                Thread.ResetAbort();
            }

            SystemState.RawException = null;
            SystemState.RawTraceBack = null;
        }

        [PythonName("exc_info")]
        public Tuple exc_info() {
            if (RawException == null) return Tuple.MakeTuple(null, null, null);
            object pyExcep = ExceptionConverter.ToPython(RawException);

            exc_traceback = RawTraceBack;

            if (pyExcep is StringException) {
                // string exceptions are special...  there tuple looks
                // like string, argument, traceback instead of
                //      type,   instance, traceback
                StringException se = RawException as StringException;
                Debug.Assert(se != null);

                exc_type = pyExcep;
                exc_value = se.Value;

                return Ops.MakeTuple(
                    pyExcep,
                    se.Value,
                    RawTraceBack);
            } else {
                object excType = Ops.GetAttr(DefaultContext.Default, pyExcep, SymbolTable.Class);
                exc_type = excType;
                exc_value = pyExcep;

                return Ops.MakeTuple(
                    excType,
                    pyExcep,
                    RawTraceBack);
            }
        }

        [PythonName("exec_prefix")]
        public string exec_prefix;
        [PythonName("executable")]
        public object executable;

        [PythonName("exit")]
        public void exit() {
            exit(null);
        }

        [PythonName("exit")]
        public void exit(object code) {
            if (code == null) {
                throw ExceptionConverter.CreateThrowable(ExceptionConverter.GetPythonException("SystemExit"));
            } else {
                // throw as a python exception here to get the args set.
                throw ExceptionConverter.CreateThrowable(ExceptionConverter.GetPythonException("SystemExit"), code);
            }
        }

        [PythonName("getdefaultencoding")]
        public string getdefaultencoding() {
            return DefaultEncoding.WebName.ToLower().Replace('-', '_');
        }

        [PythonName("getfilesystemencoding")]
        public object getfilesystemencoding() {
            return null;
        }

        [PythonName("_getframe")]
        public object _getframe() {
            throw Ops.ValueError("_getframe is not implemented");
        }

        [PythonName("_getframe")]
        public object _getframe(int depth) {
            throw Ops.ValueError("_getframe is not implemented");
        }

        [PythonName("hexversion")]
        public int hexversion;

        [PythonName("maxint")]
        public int maxint;
        [PythonName("maxunicode")]
        public object maxunicode;

        [PythonName("modules")]
        public IDictionary<object, object> modules;

        [PythonName("path")]
        public List path;

        [PythonName("platform")]
        public object platform;

        [PythonName("prefix")]
        public string prefix;

        [PythonName("ps1")]
        public object ps1;
        [PythonName("ps2")]
        public object ps2;

        public object setdefaultencodingImpl(object name) {
            if (name == null) throw Ops.TypeError("name cannot be None");
            string strName = name as string;
            if (strName == null) throw Ops.TypeError("name must be a string");

            Encoding enc;
            if (!StringOps.TryGetEncoding(this, strName, out enc)) {
                throw Ops.LookupError("'{0}' does not match any available encodings", strName);
            }

            DefaultEncoding = enc;
            __dict__.Remove(SymbolTable.SetDefaultEncoding);
            return null;
        }

        [PythonName("settrace")]
        public void SetTrace(object o) {
            throw Ops.NotImplementedError("sys.settrace is not yet supported by IronPython");
        }

        [PythonName("setrecursionlimit")]
        public void SetRecursionLimit(int limit) {
            if (limit < 0) throw Ops.ValueError("recursion limit must be positive");
            PythonFunction.EnforceRecursion = (limit != Int32.MaxValue);
            PythonFunction.MaximumDepth = limit;
        }

        [PythonName("getrecursionlimit")]
        public object GetRecursionLimit() {
            return Ops.Int2Object(PythonFunction.MaximumDepth);
        }

        [PythonName("getrefcount")]
        public int GetReferenceCount(object o) {
            throw Ops.NotImplementedError("IronPython uses mark and sweep garbage collector, getrefcount is not supported");
        }

        [PythonName("stdin")]
        public object stdin;
        [PythonName("stdout")]
        public object stdout;
        [PythonName("stderr")]
        public object stderr;

        [PythonName("__stdin__")]
        public object __stdin__;
        [PythonName("__stdout__")]
        public object __stdout__;
        [PythonName("__stderr__")]
        public object __stderr__;

        [PythonName("version")]
        public string version;

        [PythonName("version_info")]
        public object version_info;

        [PythonName("winver")]
        public object winver;

        #region ICustomAttributes Members

        public bool TryGetAttr(ICallerContext context, SymbolId name, out object value) {
            if (name == SymbolTable.Dict) {
                value = __dict__;
                return true;
            }

            if (__dict__.TryGetValue(name, out value)) {
                return value != Uninitialized.instance;
            }

            if (TypeCache.SystemState.TryGetAttr(context, this, name, out value)) return true;

            return false;
        }

        public void SetAttr(ICallerContext context, SymbolId name, object value) {
            TypeCache.SystemState.SetAttrWithCustomDict(context, this, __dict__, name, value);
        }

        public void DeleteAttr(ICallerContext context, SymbolId name) {
            if (!TypeCache.SystemState.DeleteAttrWithCustomDict(context, this, __dict__, name)) {
                throw Ops.AttributeErrorForMissingAttribute("sys", name);
            }
        }

        public List GetAttrNames(ICallerContext context) {
            List ret = new List(((IDictionary<object, object>)__dict__).Keys);
            ret.Extend(TypeCache.SystemState.GetAttrNames(context, this));
            return ret;
        }

        public IDictionary<object, object> GetAttrDict(ICallerContext context) {
            return TypeCache.SystemState.GetAttrDictWithCustomDict(context, this, __dict__);
        }

        #endregion

        internal TopReflectedPackage TopPackage {
            get {
                if (topPackage == null)
                    Interlocked.CompareExchange<TopReflectedPackage>(ref topPackage, new TopReflectedPackage(), null);

                return topPackage;
            }
        }

        internal ClrModule ClrModule {
            get {
                if (clrModule == null) {
                    ClrModule mod = new ClrModule(this);
                    if (Interlocked.CompareExchange<ClrModule>(ref clrModule, mod, null) != null) {
                        mod.Dispose();
                    }
                }
                return clrModule;
            }
        }

        internal EngineOptions EngineOptions { get { return engineOptions; } }

        ~SystemState() {
            Dispose(true);
        }
        #region IDisposable Members

        public void Dispose() {
            Dispose(false);
        }

        #endregion

        private void Dispose(bool finalizing) {
            if (!finalizing) {
                if (clrModule != null) {
                    clrModule.Dispose();
                }
            }
        }
    }
}
