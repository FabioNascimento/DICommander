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
using System.Diagnostics;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    class TwoObjects {
        internal object obj1;
        internal object obj2;

        public TwoObjects(object obj1, object obj2) {
            this.obj1 = obj1;
            this.obj2 = obj2;
        }
        public override int GetHashCode() {
            throw new NotSupportedException();
        }
        public override bool Equals(object other) {
            TwoObjects o = other as TwoObjects;
            if (o == null) return false;
            return o.obj1 == obj1 && o.obj2 == obj2;
        }
    }

    class CompareUtil {
        [ThreadStatic]
        private static Stack<object> cmpStack;

        internal static bool Check(object o) {
            if (cmpStack == null) {
                return false;
            }
            return cmpStack.Contains(o);
        }

        internal static void Push(object o) {
            Stack<object> infinite = GetInfiniteCmp();
            if (infinite.Contains(o)) {
                throw Ops.RuntimeError("maximum recursion depth exceeded in cmp");
            }
            cmpStack.Push(o);
        }

        internal static void Push(object o1, object o2) {
            Stack<object> infinite = GetInfiniteCmp();
            TwoObjects to = new TwoObjects(o1, o2);
            if (infinite.Contains(to)) {
                throw Ops.RuntimeError("maximum recursion depth exceeded in cmp");
            }
            cmpStack.Push(to);
        }

        internal static void Pop(object o) {
            Debug.Assert(cmpStack != null && cmpStack.Count > 0);
            Debug.Assert(cmpStack.Peek() == o);
            cmpStack.Pop();
        }

        internal static void Pop(object o1, object o2) {
            Debug.Assert(cmpStack != null && cmpStack.Count > 0);
            Debug.Assert(cmpStack.Peek() is TwoObjects);
            Debug.Assert(((TwoObjects)cmpStack.Peek()).obj1 == o1);
            Debug.Assert(((TwoObjects)cmpStack.Peek()).obj2 == o2);

            cmpStack.Pop();
        }

        private static Stack<object> GetInfiniteCmp() {
            if (cmpStack == null) {
                cmpStack = new Stack<object>();
            }
            return cmpStack;
        }
    }
}
