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

using IronPython.Compiler;
using IronPython.Runtime;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using IronPython.Modules;

namespace IronPython.Compiler {
    public class NameEnvironment {
        private PythonModule globals;
        private object locals;
        private Dictionary<string, bool> globalNames = new Dictionary<string, bool>();
        private ReflectedType builtin;


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#globals")]
        public NameEnvironment(PythonModule globals, object locals) {
            this.globals = globals;
            if (locals == null) locals = globals.__dict__;
            this.locals = locals;
            this.builtin = TypeCache.Builtin;
        }

        public PythonModule Globals {
            get { return globals; }
            set { globals = value; }
        }

        public void MarkGlobal(string name) {
            globalNames[name] = true;
        }

        public void Set(string name, object value) {
            if (globalNames.ContainsKey(name)) {
                globals.SetAttr(DefaultContext.Default, SymbolTable.StringToId(name), value);
            } else {
                Ops.SetIndex(locals, name, value);
            }
        }

        public object Get(string name) {
            try {
                return Ops.GetIndex(locals, name);
            } catch (KeyNotFoundException) {
            }

            SymbolId fieldId = SymbolTable.StringToId(name);
            object value;
            if (Ops.TryGetAttr(globals, fieldId, out value)) {
                return value;
            } else if (Ops.TryGetAttr(builtin, fieldId, out value)) {
                return value;
            } else return null;
        }
    }
}
