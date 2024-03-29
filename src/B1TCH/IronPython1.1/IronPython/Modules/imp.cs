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
using System.IO;
using System.Diagnostics;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Compiler.Ast;
using IronPython.Compiler.Generation;
using IronPython.Compiler;
using IronPython.Runtime.Operations;

[assembly: PythonModule("imp", typeof(IronPython.Modules.PythonImport))]
namespace IronPython.Modules {
    [PythonType("imp")]
    public static class PythonImport {

        internal const int PythonSource = 1;
        internal const int PythonCompiled = 2;
        internal const int CExtension = 3;
        internal const int PythonResource = 4;
        internal const int PackageDirectory = 5;
        internal const int CBuiltin = 6;
        internal const int PythonFrozen = 7;
        internal const int PythonCodeResource = 8;
        internal const int SearchError = 0;

        [PythonName("get_magic")]
        public static string GetMagic() {
            return "";
        }

        [PythonName("get_suffixes")]
        public static List GetSuffixes() {
            return List.MakeList(Tuple.MakeTuple(".py", "U", PythonSource));
        }

        [PythonName("find_module")]
        public static Tuple FindModule(ICallerContext context, string name) {
            return FindBuiltinOrSysPath(context, name);
        }

        [PythonName("find_module")]
        public static Tuple FindModule(ICallerContext context, string name, List path) {
            if (path == null) {
                return FindBuiltinOrSysPath(context, name);
            } else {
                return FindModulePath(context, name, path);
            }
        }

        [PythonName("load_module")]
        public static object LoadModule(ICallerContext context, string name, PythonFile file, string filename, Tuple description) {
            if (description == null) {
                throw Ops.TypeError("load_module() argument 4 must be 3-item sequence, not None");
            }
            if (description.Count != 3) {
                throw Ops.TypeError("load_module() argument 4 must be sequence of length 3, not {0}", description.Count);
            }

            // already loaded? do reload()
            object mod;
            if (context.SystemState.modules.TryGetValue(name, out mod)) {
                PythonModule module = mod as PythonModule;
                if (module != null) {
                    return Builtin.Reload(module);
                }
            }
            int type = Converter.ConvertToInt32(description[2]);
            switch (type) {
                case PythonSource:
                    return LoadPythonSource(context, name, file, filename);
                case CBuiltin:
                    return LoadBuiltinModule(context, name);
                case PackageDirectory:
                    return LoadPackageDirectory(context, name, filename);
                default:
                    throw Ops.TypeError("don't know how to import {0}, (type code {1}", name, type);
            }
        }

        [Documentation("new_module(name) -> module\nCreates a new module without adding it to sys.modules.")]
        [PythonName("new_module")]
        public static object NewModule(ICallerContext context, string name) {
            return new PythonModule(name, new FieldIdDict(), context.SystemState);
        }

        private static long lock_count;

        [PythonName("lock_held")]
        public static bool IsLockHeld() {
            return lock_count != 0;
        }

        [PythonName("acquire_lock")]
        public static void AcquireLock() {
            lock_count++;
        }

        [PythonName("release_lock")]
        public static void ReleaseLock() {
            if (lock_count <= 0) {
                throw Ops.RuntimeError("not holding the import lock");
            }
            lock_count--;
        }

        public static object PY_SOURCE = PythonSource;
        public static object PY_COMPILED = PythonCompiled;
        public static object C_EXTENSION = CExtension;
        public static object PY_RESOURCE = PythonResource;
        public static object PKG_DIRECTORY = PackageDirectory;
        public static object C_BUILTIN = CBuiltin;
        public static object PY_FROZEN = PythonFrozen;
        public static object PY_CODERESOURCE = PythonCodeResource;
        public static object SEARCH_ERROR = SearchError;

        [PythonName("init_builtin")]
        public static object InitBuiltin(ICallerContext context, string name) {
            return LoadBuiltinModule(context, name);
        }

        [PythonName("init_frozen")]
        public static object InitFrozen(string name) {
            return null;
        }

        [PythonName("is_builtin")]
        public static int IsBuiltin(ICallerContext context, string name) {
            Type ty;
            if (context.SystemState.TopPackage.Builtins.TryGetValue(name, out ty)) {
                return 1;
            }
            return 0;
        }

        [PythonName("is_frozen")]
        public static bool IsFrozen(string name) {
            return false;
        }

        [PythonName("load_compiled")]
        public static object LoadCompiled(string name, string pathname) {
            return null;
        }

        [PythonName("load_compiled")]
        public static object LoadCompiled(string name, string pathname, PythonFile file) {
            return null;
        }

        [PythonName("load_dynamic")]
        public static object LoadDynamic(string name, string pathname) {
            return null;
        }

        [PythonName("load_dynamic")]
        public static object LoadDynamic(string name, string pathname, PythonFile file) {
            return null;
        }

        [PythonName("load_source")]
        public static object LoadSource(ICallerContext context, string name, string pathname) {
            CompilerContext cc = context.CreateCompilerContext().CopyWithNewSourceFile(pathname);
            Parser parser = Parser.FromFile(context.SystemState, cc);
            return GenerateAndInitializeModule(context, cc, parser, name, pathname);
        }

        [PythonName("load_source")]
        public static object LoadSource(ICallerContext context, string name, string pathname, PythonFile file) {
            return LoadPythonSource(context, name, file, pathname);
        }

        #region Implementation

        private static Tuple FindBuiltinOrSysPath(ICallerContext context, string name) {
            List sysPath = context.SystemState.path;
            if (sysPath == null) {
                throw Ops.ImportError("sys.path must be a list of directory names");
            }
            return FindModuleBuiltinOrPath(context, name, sysPath);
        }

        private static Tuple FindModulePath(ICallerContext context, string name, List path) {
            Debug.Assert(path != null);

            if (name == null) {
                throw Ops.TypeError("find_module() argument 1 must be string, not None");
            }

            foreach (object d in path) {
                string dir = d as string;
                if (dir == null) continue;  // skip invalid entries

                string pathName = Path.Combine(dir, name);
                if (Directory.Exists(pathName)) {
                    if (File.Exists(Path.Combine(pathName, "__init__.py"))) {
                        return Tuple.MakeTuple(null, pathName, Tuple.MakeTuple("", "", PackageDirectory));
                    }
                }

                string fileName = pathName + ".py";
                if (File.Exists(fileName)) {
                    FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    PythonFile pf = new PythonFile(fs, context.SystemState.DefaultEncoding, fileName, "U");
                    return Tuple.MakeTuple(pf, fileName, Tuple.MakeTuple(".py", "U", PythonSource));
                }
            }

            throw Ops.ImportError("No module named {0}", name);
        }

        private static Tuple FindModuleBuiltinOrPath(ICallerContext context, string name, List path) {
            if (name.Equals("sys")) return BuiltinModuleTuple(name);
            if (name.Equals("clr")) {
                context.ContextFlags |= CallerContextAttributes.ShowCls;
                return BuiltinModuleTuple(name);
            }
            Type ty;
            if (context.SystemState.TopPackage.Builtins.TryGetValue(name, out ty)) {
                return BuiltinModuleTuple(name);
            }

            return FindModulePath(context, name, path);
        }

        private static Tuple BuiltinModuleTuple(string name) {
            return Tuple.MakeTuple(null, name, Tuple.MakeTuple("", "", CBuiltin));
        }

        private static PythonModule LoadPythonSource(ICallerContext context, string name, PythonFile file, string filename) {
            CompilerContext cc = context.CreateCompilerContext().CopyWithNewSourceFile(filename);
            Parser parser = Parser.FromString(context.SystemState, cc, file.Read());
            return GenerateAndInitializeModule(context, cc, parser, name, filename);
        }

        private static PythonModule GenerateAndInitializeModule(ICallerContext context, CompilerContext cc, Parser parser, string name, string filename) {
            Statement stmt = parser.ParseFileInput();

            PythonModule module = OutputGenerator.GenerateModule(context.SystemState, cc, stmt, name);
            module.Filename = filename;
            module.ModuleName = name;

            return Importer.InitializeModule(filename, module);
        }

        private static PythonModule LoadPackageDirectory(ICallerContext context, string name, string filename) {
            string init = Path.Combine(filename, "__init__.py");
            CompilerContext cc = context.CreateCompilerContext().CopyWithNewSourceFile(init);
            Parser parser = Parser.FromFile(context.SystemState, cc);
            Statement stmt = parser.ParseFileInput();

            PythonModule module = OutputGenerator.GenerateModule(context.SystemState, cc, stmt, name);
            module.Filename = init;
            module.ModuleName = name;

            return Importer.InitializeModule(init, module);
        }

        private static object LoadBuiltinModule(ICallerContext context, string name) {
            return Importer.ImportBuiltin(context.Module, name);
        }

        #endregion
    }
}
