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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.Serialization;

using System.Threading;
using System.Diagnostics;

using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {
    // OldClass represents the type of old-style Python classes (which could not inherit from 
    // built-in Python types). 
    // 
    // Object instances of old-style Python classes are represented by OldInstance.
    // 
    // UserType is the equivalent of OldClass for new-style Python classes (which can inherit 
    // from built-in types).

    [Flags]
    enum OldClassAttributes {
        None = 0x00,
        HasFinalizer = 0x01,
        HasSetAttr = 0x02,
    }

    [PythonType("classobj")]
    public sealed class OldClass : IPythonType, ICallableWithCallerContext, IFancyCallable, IDynamicObject, ICustomTypeDescriptor, ICodeFormattable, ICustomAttributes {
        public Tuple __bases__;
        public IAttributesDictionary __dict__;
        private int attrs;  // actually OldClassAttributes - losing type safety for thread safety
        public object __name__;

        public OldClass(string name, Tuple bases, IDictionary<object, object> dict) {
            __bases__ = bases; //!!! validate, maybe even sort
            __name__ = name;

            __dict__ = (IAttributesDictionary)dict;

            if (!__dict__.ContainsKey(SymbolTable.Doc)) {
                __dict__[SymbolTable.Doc] = null;
            }

            if (__dict__.ContainsKey(SymbolTable.Unassign)) {
                HasFinalizer = true;
            }
            if (__dict__.ContainsKey(SymbolTable.SetAttr)) {
                HasSetAttr = true;
            }
        }

        public string Name {
            get { return __name__.ToString(); }
        }

        public bool TryLookupSlot(SymbolId name, out object ret) {
            if (__dict__.TryGetValue(name, out ret)) return true;

            // bases only ever contains OldClasses (tuples are immutable, and when assigned
            // to we verify the types in the Tuple)
            foreach (OldClass c in __bases__) {
                if (c.TryLookupSlot(name, out ret)) return true;
            }
            ret = null;
            return false;
        }

        internal string FullName {
            get { return __dict__[SymbolTable.Module].ToString() + '.' + __name__; }
        }


        public Tuple BaseClasses {
            get {
                return __bases__;
            }
            set {
                __bases__ = ValidateBases(value);
            }
        }

        public bool HasFinalizer {
            get {
                return (attrs & (int)OldClassAttributes.HasFinalizer) != 0;
            }
            internal set {
                int oldAttrs, newAttrs;
                do {
                    oldAttrs = attrs;
                    newAttrs = value ? oldAttrs | ((int)OldClassAttributes.HasFinalizer) : oldAttrs & ((int)~OldClassAttributes.HasFinalizer);
                } while (Interlocked.CompareExchange(ref attrs, newAttrs, oldAttrs) != oldAttrs);
            }
        }

        internal bool HasSetAttr {
            get {
                return (attrs & (int)OldClassAttributes.HasSetAttr) != 0;
            }
            set {
                int oldAttrs, newAttrs;
                do {
                    oldAttrs = attrs;
                    newAttrs = value ? oldAttrs | ((int)OldClassAttributes.HasSetAttr) : oldAttrs & ((int)~OldClassAttributes.HasSetAttr);
                } while (Interlocked.CompareExchange(ref attrs, newAttrs, oldAttrs) != oldAttrs);
            }
        }

        public override string ToString() {
            return FullName;
        }

        #region ICallableWithCallerContext Members
        [PythonName("__call__")]
        public object Call(ICallerContext context, params object[] args) {
            OldInstance inst = new OldInstance(this);
            object value;
            // lookup the slot directly - we don't go through __getattr__
            // until after the instance is created.
            if (TryLookupSlot(SymbolTable.Init, out value)) {
                Ops.Call(Ops.GetDescriptor(value, inst, this), args);
            } else if (args.Length > 0) {
                throw Ops.TypeError("this constructor takes no arguments");
            }
            return inst;
        }

        #endregion

        #region IFancyCallable Members

        public object Call(ICallerContext context, object[] args, string[] names) {
            OldInstance inst = new OldInstance(this);
            object meth;
            if (Ops.TryGetAttr(inst, SymbolTable.Init, out meth)) {
                Ops.Call(context, meth, args, names);
            } else {
                Debug.Assert(names.Length != 0);
                throw Ops.TypeError("this constructor takes no arguments");
            }
            return inst;
        }

        #endregion

        #region IDynamicObject Members

        public DynamicType GetDynamicType() {
            return Ops.GetDynamicTypeFromType(typeof(OldClass));
        }

        #endregion

        #region ICustomAttributes Members

        public bool TryGetAttr(ICallerContext context, SymbolId name, out object value) {
            switch (name.Id) {
                case SymbolTable.BasesId: value = __bases__; return true;
                case SymbolTable.NameId: value = __name__; return true;
                case SymbolTable.DictId:
                    //!!! user code can modify __del__ property of __dict__ behind our back
                    HasSetAttr = true;  // pessimisticlly assume the user is setting __setattr__ in the dict
                    value = __dict__; return true;
            }

            if (TryLookupSlot(name, out value)) {
                value = Ops.GetDescriptor(value, null, this);
                return true;
            }
            return false;
        }

        private Tuple ValidateBases(object value) {
            Tuple t = value as Tuple;
            if (t == null) throw Ops.TypeError("__bases__ must be a tuple object");
            foreach (object o in t) {
                OldClass oc = o as OldClass;
                if (oc == null) throw Ops.TypeError("__bases__ items must be classes (got {0})", Ops.GetDynamicType(o).__name__);

                if (oc.IsSubclassOf(this)) {
                    throw Ops.TypeError("a __bases__ item causes an inheritance cycle");
                }
            }
            return t;
        }

        public void SetAttr(ICallerContext context, SymbolId name, object value) {
            switch (name.Id) {
                case SymbolTable.BasesId: __bases__ = ValidateBases(value); break;
                case SymbolTable.NameId:
                    string n = value as string;
                    if (n == null) throw Ops.TypeError("TypeError: __name__ must be a string object");
                    __name__ = n;
                    break;
                case SymbolTable.DictId:
                    IAttributesDictionary d = value as IAttributesDictionary;
                    if (d == null) throw Ops.TypeError("__dict__ must be set to dictionary");
                    __dict__ = d;
                    break;
                case SymbolTable.UnassignId:
                    HasFinalizer = true;
                    goto default;
                case SymbolTable.SetAttrId:
                    HasSetAttr = true;
                    goto default;
                default:
                    __dict__[name] = value;
                    break;
            }
        }

        public void DeleteAttr(ICallerContext context, SymbolId name) {
            if (!__dict__.Remove(name)) {
                throw Ops.AttributeError("{0} is not a valid attribute", name);
            }

            if (name == SymbolTable.Unassign) {
                HasFinalizer = false;
            }
            if (name == SymbolTable.SetAttr) {
                HasSetAttr = false;
            }
        }

        #endregion

        internal static void RecurseAttrHierarchy(OldClass oc, IDictionary<object, object> attrs) {
            foreach (object key in oc.__dict__.Keys) {
                if (!attrs.ContainsKey(key)) {
                    attrs.Add(key, key);
                }
            }

            //  recursively get attrs in parent hierarchy
            if (oc.__bases__.Count != 0) {
                foreach (OldClass parent in oc.__bases__) {
                    RecurseAttrHierarchy(parent, attrs);
                }
            }
        }

        #region ICustomAttributes Members

        public List GetAttrNames(ICallerContext context) {
            FieldIdDict attrs = new FieldIdDict(__dict__);
            RecurseAttrHierarchy(this, attrs);
            return List.Make(attrs);
        }

        public IDictionary<object, object> GetAttrDict(ICallerContext context) {
            return (IDictionary<object, object>)__dict__;
        }

        #endregion

        public bool IsSubclassOf(object other) {
            if (this == other) return true;

            IPythonType dt = other as IPythonType;
            if (dt == null) return false;

            Tuple bases = __bases__;
            foreach (object b in bases) {
                OldClass bc = b as OldClass;
                if (bc != null && bc.IsSubclassOf(other)) {
                    return true;
                }
            }
            return false;
        }

        #region ICustomTypeDescriptor Members

        AttributeCollection ICustomTypeDescriptor.GetAttributes() {
            return CustomTypeDescHelpers.GetAttributes(this);
        }

        string ICustomTypeDescriptor.GetClassName() {
            return CustomTypeDescHelpers.GetClassName(this);
        }

        string ICustomTypeDescriptor.GetComponentName() {
            return CustomTypeDescHelpers.GetComponentName(this);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter() {
            return CustomTypeDescHelpers.GetConverter(this);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() {
            return CustomTypeDescHelpers.GetDefaultEvent(this);
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() {
            return CustomTypeDescHelpers.GetDefaultProperty(this);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType) {
            return CustomTypeDescHelpers.GetEditor(this, editorBaseType);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes) {
            return CustomTypeDescHelpers.GetEvents(attributes);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents() {
            return CustomTypeDescHelpers.GetEvents(this);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes) {
            return CustomTypeDescHelpers.GetProperties(attributes);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties() {
            return CustomTypeDescHelpers.GetProperties(this);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) {
            return CustomTypeDescHelpers.GetPropertyOwner(this, pd);
        }

        #endregion

        #region ICodeFormattable Members

        string ICodeFormattable.ToCodeString() {
            return string.Format("<class {0} at {1}>", FullName, Ops.HexId(this));
        }

        #endregion


    }

    /// <summary>
    /// Custom dictionary use for old class instances so commonly used
    /// items can be accessed quickly w/o requiring dictionary access.
    /// 
    /// Keys are only added to the dictionary, once added they are never
    /// removed.
    /// </summary>
    [PythonType(typeof(Dict))]
    public class CustomOldClassDict : CustomSymbolDict, ICloneable {
        const int maxSize = 6;
        SymbolId[] extraKeys;
        new object[] values;

        public CustomOldClassDict() {
            extraKeys = new SymbolId[maxSize];
            for (int i = 0; i < maxSize; i++) extraKeys[i] = SymbolTable.Invalid;
            values = new object[maxSize];
        }

        public override SymbolId[] GetExtraKeys() {
            return extraKeys;
        }

        public override bool TrySetExtraValue(SymbolId key, object value) {
            for (int i = 0; i < extraKeys.Length; i++) {
                // see if we already have a key (once keys are assigned
                // they never change) that matches this ID.
                if (extraKeys[i].Id == key.Id) {
                    values[i] = value;
                    return true;
                }

                // no match, check for an unused slot, and replace it w/ ourselves in 
                // a thread safe manner.  

                // Callers that get our ExtraKeys will need to check for < 0 and stop processing 
                // when they see it. They will therefore see either a -1 (invalid) or -2 (object keys id)
                // If they get -2 then there was a race and logically it's as if the assignment 
                // happened before the read occured.
                if (extraKeys[i].Id < 0) {
                    int prevVal;
                    do {
                        prevVal = Interlocked.CompareExchange(ref extraKeys[i].Id,
                            SymbolTable.ObjectKeysId,
                            SymbolTable.InvalidId);

                        if (prevVal == SymbolTable.InvalidId) {
                            // we won the race - no one else will write to
                            // the key because it has a value of ObjectKeysId.  Additionaly
                            // anyone attempting to do an add right now will spin on the
                            // key until we do the exchange after setting our value.
                            values[i] = value;
                            Interlocked.Exchange(ref extraKeys[i].Id, key.Id);
                            return true;
                        } else if (prevVal == key.Id) {
                            // we lost a race w/ another thread setting our
                            // id, we just update the value now.
                            values[i] = value;
                            return true;
                        }
                        // otherwise id is ObjectKeysId (the slot is currently being updated)
                        //     spin & try again - it could be our ID or another ID
                        // or it's another ID (we lost the race to another thread), and need
                        // to continue to the next key.
                    } while (prevVal == SymbolTable.ObjectKeysId);
                }
            }

            return false;
        }

        public override bool TryGetExtraValue(SymbolId key, out object value) {
            for (int i = 0; i < extraKeys.Length; i++) {
                if (extraKeys[i].Id == key.Id) {
                    value = values[i];
                    return true;
                }
            }
            value = null;
            return false;
        }

        [PythonClassMethod("fromkeys")]
        public static object fromkeys(DynamicType cls, object seq) {
            return Dict.FromKeys(cls, seq, null);
        }

        [PythonClassMethod("fromkeys")]
        public static object fromkeys(DynamicType cls, object seq, object value) {
            return Dict.FromKeys(cls, seq, value);
        }
    }

    [PythonType("instance")]
    [Serializable]
    public sealed class OldInstance : IDynamicObject, IRichComparable, IRichEquality, ICodeFormattable, ICustomTypeDescriptor, IWeakReferenceable, ISerializable, ICustomAttributes {
        private IAttributesDictionary __dict__;
        internal OldClass __class__;
        private WeakRefTracker weakRef;       // initialized if user defines finalizer on class or instance

        public OldInstance(OldClass _class) {
            __class__ = _class;
            __dict__ = new CustomOldClassDict();
            if (__class__.HasFinalizer) {
                // class defines finalizer, we get it automatically.
                AddFinalizer();
            }
        }

        internal static void DoCoerce(ref object self, ref object other) {
            OldInstance ois = self as OldInstance;

            if (ois != null) {
                Tuple coerced = OldInstanceType.Instance.Coerce(ois, other) as Tuple;
                if (coerced != null) {
                    self = coerced[0];
                    other = coerced[1];
                    return;
                }
            }
        }

        #region Object overrides

        public override string ToString() {
            object ret;
            if (Ops.TryInvokeSpecialMethod(this, SymbolTable.String, out ret)) {
                string strRet;
                if (Converter.TryConvertToString(ret, out strRet) && strRet != null) {
                    return strRet;
                }
                throw Ops.TypeError("__str__ returned non-string type ({0})", Ops.GetDynamicType(ret).__name__);
            }

            return ToCodeString();
        }

        #endregion

        #region ICodeFormattable Members

        public string ToCodeString() {
            object ret;
            if (Ops.TryInvokeSpecialMethod(this, SymbolTable.Repr, out ret)) {
                string strRet;
                if (ret != null && Converter.TryConvertToString(ret, out strRet)) {
                    return strRet;
                }

                throw Ops.TypeError("__repr__ returned non-string type ({0})", Ops.GetDynamicType(ret).__name__);
            }

            return string.Format("<{0} instance at {1}>", __class__.FullName, Ops.HexId(this));
        }

        #endregion

        #region ICustomAttributes Members

        public bool TryGetAttr(ICallerContext context, SymbolId name, out object value) {
            if (name.Id == SymbolTable.DictId) {
                //!!! user code can modify __del__ property of __dict__ behind our back
                value = __dict__;
                return true;
            } else if (name.Id == SymbolTable.Class.Id) {
                value = __class__;
                return true;
            }

            if (TryRawGetAttr(name, out value)) return true;

            if (name.Id != SymbolTable.GetAttrId) {
                object getattr;
                if (TryRawGetAttr(SymbolTable.GetAttr, out getattr)) {
                    try {
                        value = Ops.Call(getattr, SymbolTable.IdToString(name));
                        return true;
                    } catch (MissingMemberException) {
                        // __getattr__ raised AttributeError, return false.
                    }
                }
            }

            return false;
        }

        public void SetAttr(ICallerContext context, SymbolId name, object value) {
            object setFunc;

            if (name.Id == SymbolTable.ClassId) {
                OldClass oc = value as OldClass;
                if (oc == null) {
                    throw Ops.TypeError("__class__ must be set to class");
                }
                __class__ = oc;
            } else if (name.Id == SymbolTable.DictId) {
                IAttributesDictionary dict = value as IAttributesDictionary;
                if (dict == null) {
                    throw Ops.TypeError("__dict__ must be set to a dictionary");
                }
                if (HasFinalizer() && !__class__.HasFinalizer) {
                    if (!dict.ContainsKey(SymbolTable.Unassign)) {
                        ClearFinalizer();
                    }
                } else if (dict.ContainsKey(SymbolTable.Unassign)) {
                    AddFinalizer();
                }

                __dict__ = dict;
            } else if (__class__.HasSetAttr && __class__.TryLookupSlot(SymbolTable.SetAttr, out setFunc)) {
                Ops.Call(Ops.GetDescriptor(setFunc, this, __class__), name.ToString(), value);
            } else if (name.Id == SymbolTable.UnassignId) {
                if (!HasFinalizer()) {
                    // user is defining __del__ late bound for the 1st time
                    AddFinalizer();
                }

                __dict__[name] = value;
            } else {
                __dict__[name] = value;
            }
        }

        public void DeleteAttr(ICallerContext context, SymbolId name) {
            switch (name.Id) {
                case SymbolTable.ClassId: throw Ops.TypeError("__class__ must be set to class");
                case SymbolTable.DictId: throw Ops.TypeError("__dict__ must be set to a dictionary");
                default:
                    if (name == SymbolTable.Unassign) {
                        // removing finalizer
                        if (HasFinalizer() && !__class__.HasFinalizer) {
                            ClearFinalizer();
                        }
                    }

                    if (!__dict__.Remove(name)) {
                        throw Ops.AttributeError("{0} is not a valid attribute", SymbolTable.IdToString(name));
                    }
                    break;
            }
        }

        #endregion

        #region ICustomAttributes Members

        public List GetAttrNames(ICallerContext context) {
            FieldIdDict attrs = new FieldIdDict(__dict__);
            OldClass.RecurseAttrHierarchy(this.__class__, attrs);
            return List.Make(attrs);
        }

        public IDictionary<object, object> GetAttrDict(ICallerContext context) {
            return (IDictionary<object, object>)__dict__;
        }

        #endregion

        #region IDynamicObject Members

        public DynamicType GetDynamicType() {
            return OldInstanceType.Instance;
        }

        #endregion

        #region IRichComparable Members

        public object CompareTo(object other) {
            OldInstance oiOther = other as OldInstance;
            IRichComparable irc;
            object res;
            // try __cmp__ first if we're comparing two old classes (even if they're different types)
            if (oiOther != null) {
                res = InternalCompare(SymbolTable.Cmp, other);
                if (res != Ops.NotImplemented) return res;

                res = oiOther.InternalCompare(SymbolTable.Cmp, this);
                if (res != Ops.NotImplemented) return ((int)res) * -1;

                irc = oiOther;
            } else {
                irc = other as IRichComparable;
            }

            // next try equals, return 0 if we match.
            res = RichEquals(other);
            if (res != Ops.NotImplemented) {
                if (Ops.IsTrue(res)) return 0;
            } else if (irc != null) {
                res = irc.RichEquals(this);
                if (res != Ops.NotImplemented && Ops.IsTrue(res)) return 0;
            }

            // next try less than
            res = LessThan(other);
            if (res != Ops.NotImplemented) {
                if (Ops.IsTrue(res)) return -1;
            } else if (irc != null) {
                res = irc.GreaterThan(this);
                if (res != Ops.NotImplemented && Ops.IsTrue(res)) return -1;
            }

            // finally try greater than
            res = GreaterThan(other);
            if (res != Ops.NotImplemented) {
                if (Ops.IsTrue(res)) return 1;
            } else if (irc != null) {
                res = irc.LessThan(this);
                if (res != Ops.NotImplemented && Ops.IsTrue(res)) return 1;
            }

            if (oiOther == null) {
                // finally try __cmp__ if our types are different
                res = InternalCompare(SymbolTable.Cmp, other);
                if (res != Ops.NotImplemented) return res;

                // try the other side...
                res = Ops.GetDynamicType(other).CompareTo(other, this);
                if (res != Ops.NotImplemented) return ((int)res) * -1;
            }

            return Ops.NotImplemented;
        }

        public object GreaterThan(object other) {
            return InternalCompare(SymbolTable.OpGreaterThan, other);
        }

        public object LessThan(object other) {
            return InternalCompare(SymbolTable.OpLessThan, other);
        }

        public object GreaterThanOrEqual(object other) {
            return InternalCompare(SymbolTable.OpGreaterThanOrEqual, other);
        }

        public object LessThanOrEqual(object other) {
            return InternalCompare(SymbolTable.OpLessThanOrEqual, other);
        }

        private object InternalCompare(SymbolId cmp, object other) {
            object meth;
            if (TryGetAttr(DefaultContext.Default, cmp, out meth)) return Ops.Call(meth, other);
            return Ops.NotImplemented;
        }

        #endregion

        #region ICustomTypeDescriptor Members

        AttributeCollection ICustomTypeDescriptor.GetAttributes() {
            return CustomTypeDescHelpers.GetAttributes(this);
        }

        string ICustomTypeDescriptor.GetClassName() {
            return CustomTypeDescHelpers.GetClassName(this);
        }

        string ICustomTypeDescriptor.GetComponentName() {
            return CustomTypeDescHelpers.GetComponentName(this);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter() {
            return CustomTypeDescHelpers.GetConverter(this);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() {
            return CustomTypeDescHelpers.GetDefaultEvent(this);
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() {
            return CustomTypeDescHelpers.GetDefaultProperty(this);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType) {
            return CustomTypeDescHelpers.GetEditor(this, editorBaseType);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes) {
            return CustomTypeDescHelpers.GetEvents(attributes);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents() {
            return CustomTypeDescHelpers.GetEvents(this);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes) {
            return CustomTypeDescHelpers.GetProperties(attributes);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties() {
            return CustomTypeDescHelpers.GetProperties(this);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) {
            return CustomTypeDescHelpers.GetPropertyOwner(this, pd);
        }

        #endregion

        #region IWeakReferenceable Members

        WeakRefTracker IWeakReferenceable.GetWeakRef() {
            return weakRef;
        }

        bool IWeakReferenceable.SetWeakRef(WeakRefTracker value) {
            weakRef = value;
            return true;
        }

        void IWeakReferenceable.SetFinalizer(WeakRefTracker value) {
            ((IWeakReferenceable)this).SetWeakRef(value);
        }

        #endregion

        #region IRichEquality Members

        public object RichGetHashCode() {
            object func;
            if (Ops.TryGetAttr(this, SymbolTable.Hash, out func)) {
                return Ops.ConvertTo(Ops.Call(func), typeof(int));
            }
            return Ops.NotImplemented;
        }

        public object RichEquals(object other) {
            object func;
            if (Ops.TryGetAttr(this, SymbolTable.OpEqual, out func)) {
                return Ops.Call(func, other);
            }

            if (Ops.TryGetAttr(this, SymbolTable.Cmp, out func)) {
                object ret = Ops.Call(func, other);
                if (ret is int) {
                    return ((int)ret) == 0;
                } else if (ret is ExtensibleInt) {
                    return ((ExtensibleInt)ret).value == 0;
                }
                throw Ops.TypeError("comparison did not return an int");
            }

            object coerce = OldInstanceType.Instance.Coerce(this, other);
            if (coerce != Ops.NotImplemented && !(coerce is OldInstance)) {
                return Ops.Equal(((Tuple)coerce)[0], ((Tuple)coerce)[1]);
            }

            return Ops.NotImplemented;
        }

        public object RichNotEquals(object other) {
            object func;
            if (Ops.TryGetAttr(this, SymbolTable.OpNotEqual, out func)) {
                return Ops.Call(func, other);
            }

            object res = RichEquals(other);
            if (res != Ops.NotImplemented) return Ops.Not(res);

            return Ops.NotImplemented;
        }

        #endregion

        #region ISerializable Members

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("__class__", __class__);
            info.AddValue("__dict__", __dict__);
        }

        #endregion

        #region Private Implementation Details

        private void RecurseAttrHierarchyInt(OldClass oc, IDictionary<SymbolId, object> attrs) {
            foreach (SymbolId key in oc.__dict__.Keys) {
                if (!attrs.ContainsKey(key)) {
                    attrs.Add(key, key);
                }
            }
            //  recursively get attrs in parent hierarchy
            if (oc.__bases__.Count != 0) {
                foreach (OldClass parent in oc.__bases__) {
                    RecurseAttrHierarchyInt(parent, attrs);
                }
            }
        }

        private void AddFinalizer() {
            InstanceFinalizer oif = new InstanceFinalizer(this);
            weakRef = new WeakRefTracker(oif, oif);
        }

        private void ClearFinalizer() {
            if (weakRef == null) return;

            WeakRefTracker wrt = weakRef;
            if (wrt != null) {
                // find our handler and remove it (other users could have created weak refs to us)
                for (int i = 0; i < wrt.HandlerCount; i++) {
                    if (wrt.GetHandlerCallback(i) is InstanceFinalizer) {
                        wrt.RemoveHandlerAt(i);
                        break;
                    }
                }

                // we removed the last handler
                if (wrt.HandlerCount == 0) {
                    GC.SuppressFinalize(wrt);
                    weakRef = null;
                }
            }
        }

        private bool HasFinalizer() {
            if (weakRef != null) {
                WeakRefTracker wrt = weakRef;
                if (wrt != null) {
                    for (int i = 0; i < wrt.HandlerCount; i++) {
                        if (wrt.GetHandlerCallback(i) is InstanceFinalizer) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool TryRawGetAttr(SymbolId name, out object ret) {
            if (__dict__.TryGetValue(name, out ret)) return true;

            if (__class__.TryLookupSlot(name, out ret)) {
                ret = Ops.GetDescriptor(ret, this, __class__);
                return true;
            }

            return false;
        }

        #endregion
    }

    partial class OldInstanceType : ReflectedType {
        internal static OldInstanceType Instance = new OldInstanceType();

        public OldInstanceType()
            : base(typeof(OldInstance)) {
        }

        public override bool IsSubclassOf(object other) {
            if (other == this || other == TypeCache.Object) return true;
            return false;
        }

        internal override object InvokeBinaryOperator(SymbolId op, object self, object other) {
            OldInstance.DoCoerce(ref self, ref other);

            return InvokeSpecialMethod(op, self, other);
        }

        internal override object InvokeSpecialMethod(SymbolId op, object self, object other) {
            object func;
            if (Ops.TryGetAttr(self, op, out func)) return Ops.Call(func, other);
            return Ops.NotImplemented;
        }

        internal override object InvokeSpecialMethod(SymbolId op, object self, object arg0, object arg1) {
            object func;
            if (Ops.TryGetAttr(self, op, out func)) return Ops.Call(func, arg0, arg1);
            return Ops.NotImplemented;
        }

        protected override object InvokeSpecialMethod(SymbolId op, object self) {
            object func;
            if (Ops.TryGetAttr(self, op, out func)) return Ops.Call(func);
            return Ops.NotImplemented;
        }

        internal override bool TryLookupBoundSlot(ICallerContext context, object inst, SymbolId name, out object ret) {
            return Ops.TryGetAttr(context, inst, name, out ret);
        }

        public override object CompareTo(object self, object other) {
            Debug.Assert(self is OldInstance);

            return ((OldInstance)self).CompareTo(other);
        }

        public override object Coerce(object self, object other) {
            object meth;
            if (Ops.TryGetAttr(DefaultContext.Default, self, SymbolTable.Coerce, out meth)) {
                return Ops.Call(meth, other);
            }
            return Ops.NotImplemented;
        }

        public override int GetInstanceLength(object self) {
            object ret;
            if (Ops.TryInvokeSpecialMethod(self, SymbolTable.Length, out ret)) {
                return Converter.ConvertToInt32(ret);
            }
            throw Ops.AttributeError("{0} instance has no attribute '__len__'", Name);
        }

        [PythonName("__getitem__")]
        public override object GetIndex(object self, object index) {
            // Use the DynamicType behavior, not ReflectedType one (expands tuples)
            return GetIndexHelper(self, index);
        }

        [PythonName("__setitem__")]
        public override void SetIndex(object self, object index, object value) {
            // Use the DynamicType behavior, not ReflectedType one (expands tuples)
            SetIndexHelper(self, index, value);
        }
    }
}
