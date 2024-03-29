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
using System.Reflection.Emit;
using System.Diagnostics;
using System.Text;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Exceptions;
using IronPython.Modules;
using IronPython.Compiler.Generation;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler {
    [Flags]
    enum CallType {
        None = 0,
        ImplicitInstance,
    }

    class MethodBinder {
        string name;
        bool isBinaryOperator;
        private Dictionary<int, TargetSet> targetSets = new Dictionary<int, TargetSet>();
        private List<ParamsMethodMaker> paramsMakers = new List<ParamsMethodMaker>();

        private static bool IsUnsupported(MethodTracker method) {
            return (method.Method.CallingConvention & CallingConventions.VarArgs) != 0
                || method.Method.ContainsGenericParameters;
        }

        private static bool IsKwDictMethod(MethodTracker method) {
            ParameterInfo[] pis = method.GetParameters();
            for (int i = pis.Length - 1; i >= 0; i--) {
                if (pis[i].IsDefined(typeof(ParamDictAttribute), false))
                    return true;
            }
            return false;
        }

        public static FastCallable MakeFastCallable(string name, MethodInfo mi, bool isBinaryOperator) {
            //??? In the future add optimization for simple case of nothing tricky in mi
            return new MethodBinder(name, new MethodTracker[] { new MethodTracker(mi) }, isBinaryOperator).MakeFastCallable();
        }

        public static FastCallable MakeFastCallable(string name, MethodBase[] mis, bool isBinaryOperator) {
            return new MethodBinder(name, MethodTracker.GetTrackerArray(mis), isBinaryOperator).MakeFastCallable();
        }

        private MethodBinder(string name, MethodTracker[] methods, bool isBinaryOperator) {
            this.name = name;
            this.isBinaryOperator = isBinaryOperator;
            foreach (MethodTracker method in methods) {
                if (IsUnsupported(method)) continue;
                if (methods.Length > 1 && IsKwDictMethod(method)) continue;
                AddBasicMethodTargets(method);
            }

            foreach (ParamsMethodMaker maker in paramsMakers) {
                foreach (int count in targetSets.Keys) {
                    MethodTarget target = maker.MakeTarget(count);
                    if (target != null) AddTarget(target);
                }
            }
        }

        public string Name { get { return name; } }
        public int MaximumArgs {
            get {
                int minArgs, maxArgs;
                GetMinAndMaxArgs(out minArgs, out maxArgs);
                return maxArgs;
            }
        }
        public int MinimumArgs { get { int minArgs, maxArgs; GetMinAndMaxArgs(out minArgs, out maxArgs); return minArgs; } }

        private Delegate MakeFastCallable(bool needsContext, int nargs) {
            TargetSet ts = GetTargetSet(nargs);
            if (ts == null) return null;

            return ts.MakeCallTarget(needsContext);
        }

        public object CallWithContextN(ICallerContext context, object[] args) {
            return Call(context, CallType.None, args);
        }

        public object CallN(object[] args) {
            return Call(null, CallType.None, args);
        }

        private Delegate MakeFastCallableN(bool needsContext) {
            int minArgs, maxArgs;
            GetMinAndMaxArgs(out minArgs, out maxArgs);

            if (maxArgs <= 5 && paramsMakers.Count == 0) return null;
            if (needsContext) return new CallTargetWithContextN(this.CallWithContextN);
            else return new CallTargetN(this.CallN);
        }

        private void GetMinAndMaxArgs(out int minArgs, out int maxArgs) {
            List<int> argCounts = new List<int>(targetSets.Keys);
            argCounts.Sort();
            minArgs = argCounts[0];
            maxArgs = argCounts[argCounts.Count - 1];
        }

        public FastCallable MakeFastCallable() {
            bool needsContext = false;
            // If we have any instance/static conflicts then we'll use the slow path for everything
            foreach (TargetSet ts in targetSets.Values) {
                if (ts.HasConflict) return new FastCallableUgly(this);
                if (ts.NeedsContext) needsContext = true;
            }

            if (targetSets.Count == 0) return new FastCallableUgly(this);

            if (targetSets.Count == 1 && paramsMakers.Count == 0) {
                TargetSet ts = new List<TargetSet>(targetSets.Values)[0];
                if (ts.count <= Ops.MaximumCallArgs) return ts.MakeFastCallable();
            }

            int minArgs, maxArgs;
            GetMinAndMaxArgs(out minArgs, out maxArgs);
            if (needsContext) {
                FastCallableWithContextAny ret = new FastCallableWithContextAny(name, minArgs, maxArgs);
                ret.target0 = (CallTargetWithContext0)MakeFastCallable(needsContext, 0);
                ret.target1 = (CallTargetWithContext1)MakeFastCallable(needsContext, 1);
                ret.target2 = (CallTargetWithContext2)MakeFastCallable(needsContext, 2);
                ret.target3 = (CallTargetWithContext3)MakeFastCallable(needsContext, 3);
                ret.target4 = (CallTargetWithContext4)MakeFastCallable(needsContext, 4);
                ret.target5 = (CallTargetWithContext5)MakeFastCallable(needsContext, 5);
                ret.targetN = (CallTargetWithContextN)MakeFastCallableN(needsContext);
                return ret;
            } else {
                FastCallableAny ret = new FastCallableAny(name, minArgs, maxArgs);
                ret.target0 = (CallTarget0)MakeFastCallable(needsContext, 0);
                ret.target1 = (CallTarget1)MakeFastCallable(needsContext, 1);
                ret.target2 = (CallTarget2)MakeFastCallable(needsContext, 2);
                ret.target3 = (CallTarget3)MakeFastCallable(needsContext, 3);
                ret.target4 = (CallTarget4)MakeFastCallable(needsContext, 4);
                ret.target5 = (CallTarget5)MakeFastCallable(needsContext, 5);
                ret.targetN = (CallTargetN)MakeFastCallableN(needsContext);
                return ret;
            }
        }

        private Exception BadArgumentCount(CallType callType, int argCount) {
            if (targetSets.Count == 0) return Ops.TypeError("no callable targets, if this is a generic method make sure specify the type parameters");
            int minArgs, maxArgs;
            GetMinAndMaxArgs(out minArgs, out maxArgs);

            if (callType == CallType.ImplicitInstance) {
                argCount -= 1;
                minArgs -= 1;
                maxArgs -= 1;
            }

            // This generates Python style error messages assuming that all arg counts in between min and max are allowed
            //It's possible that discontinuous sets of arg counts will produce a weird error message
            return PythonFunction.TypeErrorForIncorrectArgumentCount(name, maxArgs, maxArgs - minArgs, argCount);
        }


        private TargetSet BuildTargetSet(int count) {
            TargetSet ts = new TargetSet(this, count);
            foreach (ParamsMethodMaker maker in paramsMakers) {
                MethodTarget target = maker.MakeTarget(count);
                if (target != null) ts.Add(target);
            }
            return ts;
        }

        private TargetSet GetTargetSet(int nargs) {
            TargetSet ts;
            if (targetSets.TryGetValue(nargs, out ts)) {
                return ts;
            } else if (paramsMakers.Count > 0) {
                ts = BuildTargetSet(nargs);
                if (ts.targets.Count > 0) {
                    return ts;
                }
            }
            return null;
        }

        public object Call(ICallerContext context, CallType callType, object[] args) {
            TargetSet ts = GetTargetSet(args.Length);
            if (ts != null) return ts.Call(context, callType, args);
            throw BadArgumentCount(callType, args.Length);
        }

        private void AddTarget(MethodTarget target) {
            int count = target.ParameterCount;
            TargetSet set;
            if (!targetSets.TryGetValue(count, out set)) {
                set = new TargetSet(this, count);
                targetSets[count] = set;
            }
            set.Add(target);
        }

        private void AddSimpleTarget(MethodTarget target) {
            AddTarget(target);
            if (target.method.IsParamsMethod) {
                ParamsMethodMaker maker = new ParamsMethodMaker(target);
                paramsMakers.Add(maker);
                //MethodTarget paramsTarget = maker.MakeTarget(target.ParameterCount - 1);
                //if (paramsTarget != null) AddTarget(paramsTarget);
            }
        }

        private void AddBasicMethodTargets(MethodTracker method) {
            List<Parameter> parameters = new List<Parameter>();
            int argIndex = 0;
            ArgBuilder instanceBuilder;
            if (!method.IsStatic) {
                parameters.Add(new Parameter(method.DeclaringType));
                instanceBuilder = new SimpleArgBuilder(argIndex++, parameters[0]);
            } else {
                instanceBuilder = new NullArgBuilder();
            }

            List<ArgBuilder> argBuilders = new List<ArgBuilder>();
            List<ArgBuilder> defaultBuilders = new List<ArgBuilder>();
            bool hasByRef = false;

            foreach (ParameterInfo pi in method.GetParameters()) {
                if (pi.ParameterType == typeof(ICallerContext)) {
                    argBuilders.Add(new ContextArgBuilder());
                    continue;
                }

                if (pi.DefaultValue != DBNull.Value) {
                    defaultBuilders.Add(new DefaultArgBuilder(pi.ParameterType, pi.DefaultValue));
                } else if (defaultBuilders.Count > 0) {
                    // If we get a bad method with non-contiguous default values, then just use the contiguous list
                    defaultBuilders.Clear();
                }

                if (pi.ParameterType.IsByRef) {
                    hasByRef = true;
                    Parameter param = new ByRefParameter(pi.ParameterType.GetElementType(), pi.IsOut && !pi.IsIn);
                    parameters.Add(param);
                    argBuilders.Add(new ReferenceArgBuilder(argIndex++, param));
                } else {
                    Parameter param = new Parameter(pi.ParameterType);
                    parameters.Add(param);
                    argBuilders.Add(new SimpleArgBuilder(argIndex++, param));
                }
            }

            ReturnBuilder returnBuilder = new ReturnBuilder(CompilerHelpers.GetReturnType(method.Method));

            for (int i = 1; i < defaultBuilders.Count + 1; i++) {
                List<ArgBuilder> defaultArgBuilders = argBuilders.GetRange(0, argBuilders.Count - i);
                defaultArgBuilders.AddRange(defaultBuilders.GetRange(defaultBuilders.Count - i, i));
                AddTarget(new MethodTarget(method, parameters.GetRange(0, parameters.Count - i),
                    instanceBuilder, defaultArgBuilders, returnBuilder));
            }

            if (hasByRef) AddSimpleTarget(MakeByRefReducedMethodTarget(method, parameters, instanceBuilder, argBuilders));
            AddSimpleTarget(new MethodTarget(method, parameters, instanceBuilder, argBuilders, returnBuilder));
        }

        private static MethodTarget MakeByRefReducedMethodTarget(MethodTracker method, List<Parameter> parameters, ArgBuilder instanceBuilder, List<ArgBuilder> argBuilders) {
            List<Parameter> newParameters = new List<Parameter>();
            foreach (Parameter param in parameters) {
                ByRefParameter p = param as ByRefParameter;
                if (p != null) {
                    if (!p.IsOutOnly) {
                        newParameters.Add(p.MakeInParameter());
                    }
                } else {
                    newParameters.Add(param);
                }
            }

            List<int> returnArgs = new List<int>();
            if (CompilerHelpers.GetReturnType(method.Method) != typeof(void)) {
                returnArgs.Add(-1);
            }
            int index = 0;
            int outParams = 0;
            List<ArgBuilder> newArgBuilders = new List<ArgBuilder>();
            foreach (ArgBuilder ab in argBuilders) {
                ReferenceArgBuilder rab = ab as ReferenceArgBuilder;
                if (rab != null) {
                    returnArgs.Add(index);
                    if (((ByRefParameter)rab.parameter).IsOutOnly) {
                        newArgBuilders.Add(new NullArgBuilder());
                        outParams++;
                    } else {
                        newArgBuilders.Add(new SimpleArgBuilder(rab.index - outParams, ((ByRefParameter)rab.parameter).MakeInParameter()));
                    }
                } else {
                    SimpleArgBuilder asb = ab as SimpleArgBuilder;
                    if (asb != null) {
                        newArgBuilders.Add(new SimpleArgBuilder(asb.index - outParams, asb.parameter));
                    } else {
                        newArgBuilders.Add(ab);
                    }
                }
                index++;
            }

            return new MethodTarget(method, newParameters, instanceBuilder, newArgBuilders,
                new ByRefReturnBuilder(CompilerHelpers.GetReturnType(method.Method), returnArgs));
        }

        class ParamsMethodMaker {
            private MethodTarget baseTarget;

            public ParamsMethodMaker(MethodTarget baseTarget) {
                this.baseTarget = baseTarget;
            }

            public MethodTarget MakeTarget(int count) {
                // The method with baseMethodTarget.ParameterCount-1 params has already been added
                if (count < baseTarget.ParameterCount - 1) return null;

                List<Parameter> newParameters = baseTarget.parameters.GetRange(0, baseTarget.parameters.Count - 1);
                List<ArgBuilder> newArgBuilders = baseTarget.argBuilders.GetRange(0, baseTarget.argBuilders.Count - 1);

                Type elementType = baseTarget.parameters[baseTarget.parameters.Count - 1].Type.GetElementType();

                int start = newParameters.Count;
                while (newParameters.Count < count) {
                    Parameter param = new Parameter(elementType);
                    newParameters.Add(param);
                }


                newArgBuilders.Add(new ParamsArgBuilder(start, count - start, new Parameter(elementType)));

                return new MethodTarget(baseTarget.method, newParameters, baseTarget.instanceBuilder, newArgBuilders, baseTarget.returnBuilder);
            }
        }


        public enum ParameterComparison {
            /// <summary>
            /// The two parameters are both better than each other.   This occurs when both
            /// parameters have a non-narrowing conversion to each others types.
            /// </summary>
            Conflict,
            /// <summary>
            /// The first parameter is better
            /// </summary>
            First,
            /// <summary>
            /// The second parameter is better.
            /// </summary>
            Second,
            /// <summary>
            /// The two parameters are of the same type
            /// </summary>
            Same,
            /// <summary>
            /// Neither parameter is better and there are no conflicts.
            /// </summary>
            Neither
        }

        public static ParameterComparison CompareParameterTypes(Type t1, Type t2) {
            if (t1 == t2) return ParameterComparison.Same;

            if (Converter.CanConvertFrom(t2, t1, NarrowingLevel.None)) {
                if (Converter.CanConvertFrom(t1, t2, NarrowingLevel.None)) {
                    return ParameterComparison.Conflict;
                } else {
                    return ParameterComparison.Second;
                }
            }
            if (Converter.CanConvertFrom(t1, t2, NarrowingLevel.None)) {
                return ParameterComparison.First;
            }

            // Special additional rules to order numeric value types
            if (Converter.PreferConvert(t1, t2)) return ParameterComparison.Second;
            else if (Converter.PreferConvert(t2, t1)) return ParameterComparison.First;

            return ParameterComparison.Neither;
        }
        class Parameter {
            protected Type type;
            public Parameter(Type type) {
                this.type = type;
            }

            public virtual Type Type {
                get { return type; }
            }

            public virtual bool HasConversionFrom(object o, NarrowingLevel allowNarrowing) {
                if (o == null) {
                    if (Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                        return true;
                    }
                    return !Type.IsValueType;
                } else {
                    return Converter.CanConvertFrom(o.GetType(), Type, allowNarrowing);
                }
            }

            public ParameterComparison CompareTo(Parameter other) {
                Type t1 = Type;
                Type t2 = other.Type;
                return CompareParameterTypes(t1, t2);
            }

            public virtual object ConvertFrom(object arg) {
                return Converter.Convert(arg, Type);
            }

            public string ToSignatureString() {
                return Ops.GetDynamicTypeFromType(Type).Name;
            }
        }


        class ByRefParameter : Parameter {
            private Type elementType;
            private bool isOutOnly;
            public ByRefParameter(Type elementType, bool isOutOnly)
                : base(typeof(ClrModule.Reference<>).MakeGenericType(elementType)) {
                this.elementType = elementType;
                this.isOutOnly = isOutOnly;
            }

            public Parameter MakeInParameter() {
                return new Parameter(elementType);
            }

            public bool IsOutOnly {
                get { return isOutOnly; }
            }

            public override object ConvertFrom(object arg) {
                if (arg == null) throw Ops.TypeErrorForTypeMismatch("clr.Reference", arg);
                Type argType = arg.GetType();
                if (!argType.IsGenericType || argType.GetGenericTypeDefinition() != typeof(ClrModule.Reference<>))
                    throw Ops.TypeErrorForTypeMismatch("clr.Reference", arg);

                object value = ((IReference)arg).Value;

                if (value == null) return null;
                return Converter.Convert(value, elementType);
            }

            public override bool HasConversionFrom(object o, NarrowingLevel allowNarrowing) {
                return o != null && o.GetType() == Type;
            }
        }

        private static ParameterComparison CompareParameters(IList<Parameter> parameters1, IList<Parameter> parameters2) {
            ParameterComparison ret = ParameterComparison.Same;
            for (int i = 0; i < parameters1.Count; i++) {
                Parameter p1 = parameters1[i];
                Parameter p2 = parameters2[i];
                ParameterComparison cmp = p1.CompareTo(p2);
                switch (ret) {
                    case ParameterComparison.Same:
                        ret = cmp; break;
                    case ParameterComparison.First:
                        if (cmp == ParameterComparison.Second) return ParameterComparison.Conflict;
                        break;
                    case ParameterComparison.Second:
                        if (cmp == ParameterComparison.First) return ParameterComparison.Conflict;
                        break;
                    case ParameterComparison.Neither:
                    case ParameterComparison.Conflict:
                        if (cmp != ParameterComparison.Same) ret = cmp;
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }

            return ret;
        }

        abstract class ArgBuilder {
            public virtual bool CanGenerate {
                get { return true; }
            }

            public abstract int Priority {
                get;
            }

            public virtual bool NeedsContext {
                get { return false; }
            }

            public abstract object Build(ICallerContext context, object[] args);
            public virtual void UpdateFromReturn(object callArg, object[] args) { }

            public abstract void Generate(CodeGen cg, Slot contextSlot, Slot[] argSlots);
        }

        class SimpleArgBuilder : ArgBuilder {
            internal int index;
            internal Parameter parameter;
            public SimpleArgBuilder(int index, Parameter parameter) {
                this.index = index;
                this.parameter = parameter;
            }

            public override int Priority {
                get { return 0; }
            }


            public override object Build(ICallerContext context, object[] args) {
                return parameter.ConvertFrom(args[index]);
            }

            public override void Generate(CodeGen cg, Slot contextSlot, Slot[] argSlots) {
                argSlots[index].EmitGet(cg);
                cg.EmitConvertFromObject(parameter.Type);
            }
        }

        class ReferenceArgBuilder : SimpleArgBuilder {
            public ReferenceArgBuilder(int index, Parameter parameter)
                : base(index, parameter) {
            }

            public override int Priority {
                get { return 5; }
            }

            public override void UpdateFromReturn(object callArg, object[] args) {
                ((IReference)args[index]).Value = callArg;
            }

            public override bool CanGenerate {
                get { return false; }
            }
        }


        class NullArgBuilder : ArgBuilder {
            public NullArgBuilder() { }

            public override int Priority {
                get { return 0; }
            }

            public override object Build(ICallerContext context, object[] args) {
                return null;
            }

            public override void Generate(CodeGen cg, Slot contextSlot, Slot[] argSlots) {
                cg.EmitRawConstant(null);
            }
        }

        class ContextArgBuilder : ArgBuilder {
            public ContextArgBuilder() { }
            public override object Build(ICallerContext context, object[] args) {
                return context;
            }

            public override bool NeedsContext {
                get { return true; }
            }

            public override int Priority {
                get { return -1; }
            }

            public override void Generate(CodeGen cg, Slot contextSlot, Slot[] argSlots) {
                contextSlot.EmitGet(cg);
            }
        }

        class DefaultArgBuilder : ArgBuilder {
            private Type argumentType;
            private object defaultValue;
            public DefaultArgBuilder(Type argumentType, object defaultValue) {
                this.argumentType = argumentType;
                this.defaultValue = defaultValue;
            }

            public override int Priority {
                get { return 3; }
            }

            public override object Build(ICallerContext context, object[] args) {
                return defaultValue;
            }

            public override void Generate(CodeGen cg, Slot contextSlot, Slot[] argSlots) {
                if (argumentType.IsByRef) {
                    Type baseType = argumentType.GetElementType();
                    Slot tmp = cg.GetLocalTmp(baseType);
                    // Emit the default value as the base type
                    EmitDefaultValue(cg, defaultValue, baseType);
                    tmp.EmitSet(cg);
                    // And pass the reference to the callee
                    tmp.EmitGetAddr(cg);
                } else {
                    // Emit the default value directly as the argument type
                    EmitDefaultValue(cg, defaultValue, argumentType);
                }
            }

            private static void EmitDefaultValue(CodeGen cg, object value, Type type) {
                if (value is Missing) {
                    EmitMissingValue(cg, type);
                } else {
                    cg.EmitRawConstant(value);
                    if (type.IsValueType) {
                        if (value == null) EmitException(cg, "Cannot cast None to {0}", type);
                        else if (value.GetType() != type) EmitException(cg, "Cannot cast {0} to {1}", value, type);
                    } else {
                        // null is any reference type
                        if (value != null) {
                            Type from = value.GetType();
                            if (!type.IsAssignableFrom(from)) {
                                EmitException(cg, "Cannot cast {0} to {1}", value, type);
                            } else {
                                if (from.IsValueType) {
                                    cg.Emit(OpCodes.Box, from);
                                }
                            }
                        }
                    }
                }
            }

            private static void EmitMissingValue(CodeGen cg, Type type) {
                switch (Type.GetTypeCode(type)) {
                    default:
                    case TypeCode.Object:
                        // struct
                        if (type.IsSealed && type.IsValueType && !type.IsEnum) {
                            Slot s = cg.GetLocalTmp(type);
                            s.EmitGetAddr(cg);
                            cg.Emit(OpCodes.Initobj, type);
                            s.EmitGet(cg);
                        } else if (type == typeof(object)) {
                            // parameter of type object receives the actual Missing value
                            cg.Emit(OpCodes.Ldsfld, typeof(Missing).GetField("Value"));
                        } else if (!type.IsValueType) {
                            cg.Emit(OpCodes.Ldnull);
                        } else {
                            EmitException(cg, "Cannot create default value for type {0}", type);
                        }
                        break;

                    case TypeCode.Empty:
                    case TypeCode.DBNull:
                        cg.Emit(OpCodes.Ldnull);
                        break;

                    case TypeCode.Boolean:
                    case TypeCode.Char:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                        cg.Emit(OpCodes.Ldc_I4_0); break;

                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        cg.Emit(OpCodes.Ldc_I4_0);
                        cg.Emit(OpCodes.Conv_I8);
                        break;

                    case TypeCode.Single:
                        cg.Emit(OpCodes.Ldc_R4, default(Single));
                        break;
                    case TypeCode.Double:
                        cg.Emit(OpCodes.Ldc_R8, default(Double));
                        break;
                    case TypeCode.Decimal:
                        cg.Emit(OpCodes.Ldc_I4_0);
                        cg.EmitNew(typeof(Decimal).GetConstructor(new Type[] { typeof(int) }));
                        break;
                    case TypeCode.DateTime:
                        Slot dt = cg.GetLocalTmp(typeof(DateTime));
                        dt.EmitGetAddr(cg);
                        cg.Emit(OpCodes.Initobj, typeof(DateTime));
                        dt.EmitGet(cg);
                        break;
                    case TypeCode.String:
                        cg.Emit(OpCodes.Ldnull); break;
                }
            }

            private static void EmitException(CodeGen cg, string format, params object[] args) {
                string message = String.Format(format, args);
                cg.Emit(OpCodes.Ldstr, message);
                cg.EmitCall(typeof(Ops), "TypeErrorForDefaultArgument");
                cg.Emit(OpCodes.Throw);
            }
        }

        class ParamsArgBuilder : ArgBuilder {
            private int start;
            private int count;
            private Parameter parameter;
            public ParamsArgBuilder(int start, int count, Parameter parameter) {
                this.start = start;
                this.count = count;
                this.parameter = parameter;
            }

            public override int Priority {
                get { return 4; }
            }

            public override object Build(ICallerContext context, object[] args) {
                Array paramsArray = Array.CreateInstance(parameter.Type, count);
                for (int i = 0; i < count; i++) {
                    paramsArray.SetValue(parameter.ConvertFrom(args[i + start]), i);
                }
                return paramsArray;
            }

            public override bool CanGenerate {
                get { return !parameter.Type.IsValueType; }
            }

            public override void Generate(CodeGen cg, Slot contextSlot, Slot[] argSlots) {
                cg.EmitInt(count);
                cg.Emit(OpCodes.Newarr, parameter.Type);
                for (int i = 0; i < count; i++) {
                    cg.Emit(OpCodes.Dup);
                    cg.EmitInt(i);
                    argSlots[start + i].EmitGet(cg);
                    cg.EmitConvertFromObject(parameter.Type);
                    cg.EmitStelem(parameter.Type);
                }
            }
        }

        class ReturnBuilder {
            protected Type returnType;
            public ReturnBuilder(Type returnType) { this.returnType = returnType; }

            public virtual object Build(ICallerContext context, object[] args, object ret) {
                return ret;
            }

            public virtual int CountOutParams {
                get { return 0; }
            }

            public virtual bool CanGenerate {
                get { return true; }
            }

            public virtual void Generate(CodeGen cg, Slot contextSlot, Slot[] argSlots) {
                cg.EmitConvertToObject(returnType);
                cg.EmitReturn();
            }
        }

        class ByRefReturnBuilder : ReturnBuilder {
            private IList<int> returnArgs;
            public ByRefReturnBuilder(Type returnType, IList<int> returnArgs)
                : base(returnType) {
                this.returnArgs = returnArgs;
            }

            protected static object GetValue(object[] args, object ret, int index) {
                if (index == -1) return ret;
                return args[index];
            }

            public override object Build(ICallerContext context, object[] args, object ret) {
                if (returnArgs.Count == 1) {
                    return GetValue(args, ret, returnArgs[0]);
                } else {
                    object[] retValues = new object[returnArgs.Count];
                    int rIndex = 0;
                    foreach (int index in returnArgs) {
                        retValues[rIndex++] = GetValue(args, ret, index);
                    }
                    return Tuple.MakeTuple(retValues);
                }
            }

            public override int CountOutParams {
                get { return returnArgs.Count; }
            }

            public override bool CanGenerate {
                get {
                    return false;
                }
            }

            public override void Generate(CodeGen cg, Slot contextSlot, Slot[] argSlots) {
                throw new NotImplementedException();
            }
        }

        class MethodTarget {
            public MethodTracker method;
            public List<Parameter> parameters;
            public List<ArgBuilder> argBuilders;
            public ArgBuilder instanceBuilder;
            public ReturnBuilder returnBuilder;


            private FastCallable fastCallable;

            public MethodTarget(MethodTracker method, List<Parameter> parameters, ArgBuilder instanceBuilder, List<ArgBuilder> argBuilders, ReturnBuilder returnBuilder) {
                this.method = method;
                this.parameters = parameters;
                this.instanceBuilder = instanceBuilder;
                this.argBuilders = argBuilders;
                this.returnBuilder = returnBuilder;

                parameters.TrimExcess();
                argBuilders.TrimExcess();
            }

            public bool NeedsContext {
                get { return argBuilders.Count > 0 && argBuilders[0].NeedsContext; }
            }

            public int ParameterCount {
                get { return parameters.Count; }
            }

            public bool IsApplicable(object[] args, NarrowingLevel allowNarrowing) {
                for (int i = 0; i < args.Length; i++) {
                    if (!parameters[i].HasConversionFrom(args[i], allowNarrowing)) return false;
                }
                return true;
            }

            public object Call(ICallerContext context, object[] args) {
                if (fastCallable == null) {
                    fastCallable = MakeFastCallable();
                }

                if (fastCallable == null) {
                    return CallReflected(context, args);
                } else {
                    return fastCallable.Call(context, args);
                }
            }

            public object CallReflected(ICallerContext context, object[] args) {
                if (IronPython.Compiler.Options.EngineDebug) {
                    PerfTrack.NoteEvent(PerfTrack.Categories.Methods, this);
                }

                object instance = instanceBuilder.Build(context, args);
                object[] callArgs = new object[argBuilders.Count];
                for (int i = 0; i < callArgs.Length; i++) {
                    callArgs[i] = argBuilders[i].Build(context, args);
                }

                object result;
                try {
                    if (method.Method is ConstructorInfo) {
                        result = ((ConstructorInfo)method.Method).Invoke(callArgs);
                    } else {
                        result = method.Method.Invoke(instance, callArgs);
                    }
                } catch (TargetInvocationException tie) {
                    throw ExceptionConverter.UpdateForRethrow(tie.InnerException);
                }

                //This is only used to support explicit Reference arguments
                for (int i = 0; i < callArgs.Length; i++) {
                    argBuilders[i].UpdateFromReturn(callArgs[i], args);
                }

                return returnBuilder.Build(context, callArgs, result);
            }

            private static bool CanOptimize(MethodBase mi) {
                //!!! All of the cases where we return false should probably never reach this point
                //!!! This matches current ReflectOptimizer behavior so this will be fixed in a second pass
                if (mi.ContainsGenericParameters) return false;
                if (mi.IsAbstract) return false;
                if (mi.IsFamily || mi.IsPrivate || mi.IsFamilyOrAssembly) return false;
                if (!mi.DeclaringType.IsVisible) {
                    return false;
                }

                return true;
            }

            public FastCallable MakeFastCallable() {
                Delegate target = MakeCallTarget(NeedsContext);
                if (target == null) return null;
                return FastCallable.Make(method.Name, NeedsContext, parameters.Count, target);
            }

            public bool CanMakeCallTarget() {
                if (!CanOptimize(method.Method)) return false;

                if (parameters.Count > Ops.MaximumCallArgs) {
                    return false;
                }

                if (!returnBuilder.CanGenerate) return false;

                foreach (ArgBuilder ab in argBuilders) {
                    if (!ab.CanGenerate) return false;
                }
                return true;
            }

            private bool IsDirectTarget(bool needsContext) {
                if (needsContext != NeedsContext) return false;
                if (!(method.IsStatic)) return false;
                if (!(method.Method is MethodInfo)) return false;
                if (returnBuilder.CountOutParams > 0) return false;
                int argCount = 0;
                if (NeedsContext) argCount++;
                while (argCount < argBuilders.Count) {
                    SimpleArgBuilder sab = argBuilders[argCount++] as SimpleArgBuilder;
                    if (sab == null || sab.parameter.Type != typeof(object)) return false;
                }
                return argCount <= Ops.MaximumCallArgs;
            }

            public Delegate MakeCallTarget(bool needsContext) {
                if (!CanMakeCallTarget()) return null;

                if (IsDirectTarget(needsContext)) {
                    Delegate ret = FastCallable.MakeDelegate((MethodInfo)method.Method);
                    if (ret != null) return ret;
                }

                if (IronPython.Compiler.Options.EngineDebug) {
                    PerfTrack.NoteEvent(PerfTrack.Categories.Compiler, "CT:" + this.ToString());
                }

                int contextOffset = needsContext ? 1 : 0;
                Type[] paramTypes = new Type[parameters.Count + contextOffset];
                if (needsContext) paramTypes[0] = typeof(ICallerContext);

                for (int i = 0; i < parameters.Count; i++) {
                    paramTypes[i + contextOffset] = typeof(object);
                }

                CodeGen cg = OutputGenerator.Snippets.DefineDynamicMethod(method.Name, typeof(object), paramTypes);

                Slot contextSlot = needsContext ? cg.argumentSlots[0] : null;
                Slot[] argSlots = new Slot[parameters.Count];
                for (int i = 0; i < argSlots.Length; i++) {
                    argSlots[i] = cg.argumentSlots[i + contextOffset];
                }


                if (!method.IsStatic) {
                    if (method.DeclaringType.IsValueType) {
                        SimpleArgBuilder sb = instanceBuilder as SimpleArgBuilder;
                        argSlots[sb.index].EmitGet(cg);
                        cg.Emit(OpCodes.Unbox, method.DeclaringType);
                    } else {
                        instanceBuilder.Generate(cg, contextSlot, argSlots);
                    }
                }

                for (int i = 0; i < argBuilders.Count; i++) {
                    argBuilders[i].Generate(cg, contextSlot, argSlots);
                }

                Type returnType = GenerateCall(cg);

                //!!! add support for reference arg remapping

                returnBuilder.Generate(cg, contextSlot, argSlots);

                cg.Finish();

                return cg.CreateDelegate(FastCallable.GetTargetType(needsContext, parameters.Count));
            }

            private Type GenerateCall(CodeGen cg) {
                MethodInfo mi = method.Method as MethodInfo;

                if (mi != null) {
                    cg.EmitCall(mi);
                    return mi.ReturnType;
                } else {
                    cg.EmitNew((ConstructorInfo)method.Method);
                    return ((ConstructorInfo)method.Method).DeclaringType;
                }
            }

            public int CompareTo(MethodTarget other, CallType callType) {
                ParameterComparison cmpParams = CompareParameters(this.parameters, other.parameters);
                if (cmpParams == ParameterComparison.First) return 1;
                if (cmpParams == ParameterComparison.Second) return -1;

                int ret = CompareEqualParameters(other);
                if (ret != 0) return ret;

                if (method.IsStatic && !other.method.IsStatic) {
                    return callType == CallType.ImplicitInstance ? -1 : +1;
                } else if (!method.IsStatic && other.method.IsStatic) {
                    return callType == CallType.ImplicitInstance ? +1 : -1;
                }

                return 0;
            }

            private static int FindMaxPriority(List<ArgBuilder> abs) {
                int max = -1;
                foreach (ArgBuilder ab in abs) {
                    max = Math.Max(max, ab.Priority);
                }
                return max;
            }

            public int CompareEqualParameters(MethodTarget other) {
                // Prefer normal methods over explicit interface implementations
                if (other.method.Method.IsPrivate && !this.method.Method.IsPrivate) return +1;
                if (this.method.Method.IsPrivate && !other.method.Method.IsPrivate) return -1;

                // Prefer non-generic methods over generic methods
                if (method.Method.IsGenericMethod) {
                    if (!other.method.Method.IsGenericMethod) {
                        return -1;
                    } else {
                        //!!! Need to support selecting least generic method here
                        return 0;
                    }
                } else if (other.method.Method.IsGenericMethod) {
                    return +1;
                }

                //prefer methods without out params over those with them
                switch (Compare(returnBuilder.CountOutParams, other.returnBuilder.CountOutParams)) {
                    case 1: return -1;
                    case -1: return 1;
                }

                //prefer methods using earlier conversions rules to later ones
                int maxPriorityThis = FindMaxPriority(this.argBuilders);
                int maxPriorityOther = FindMaxPriority(other.argBuilders);

                if (maxPriorityThis < maxPriorityOther) return +1;
                if (maxPriorityOther < maxPriorityThis) return -1;

                return 0;
            }

            protected static int Compare(int x, int y) {
                if (x < y) return -1;
                else if (x > y) return +1;
                else return 0;
            }

            public override string ToString() {
                return string.Format("MethodTarget({0} on {1}, optimized={2})", method.Method, method.DeclaringType.FullName, fastCallable != null);
            }

            public string ToSignatureString(string name, CallType callType) {
                StringBuilder buf = new StringBuilder(name);
                buf.Append("(");
                bool isFirstArg = true;
                int i = 0;
                if (callType == CallType.ImplicitInstance) i = 1;
                for (; i < parameters.Count; i++) {
                    if (isFirstArg) isFirstArg = false;
                    else buf.Append(", ");
                    buf.Append(parameters[i].ToSignatureString());
                }
                buf.Append(")");
                return buf.ToString(); //@todo add helper info for more interesting signatures
            }
        }


        class TargetSet {
            private MethodBinder binder;
            internal int count;
            internal List<MethodTarget> targets;
            private bool hasConflict = false;

            public TargetSet(MethodBinder binder, int count) {
                this.binder = binder;
                this.count = count;
                targets = new List<MethodTarget>();
            }


            public bool HasConflict {
                get { return hasConflict || binder.isBinaryOperator; }
            }

            public bool NeedsContext {
                get { return targets.Count != 1 || targets[0].NeedsContext || !targets[0].CanMakeCallTarget(); }
            }

            public FastCallable MakeFastCallable() {
                return FastCallable.Make(binder.name, NeedsContext, count, MakeCallTarget(NeedsContext));
            }

            public Delegate MakeCallTarget(bool needsContext) {
                if (targets.Count == 1) {
                    Delegate ret = targets[0].MakeCallTarget(needsContext);
                    if (ret != null) return ret;
                }

                switch (count) {
                    case 0:
                        return new CallTargetWithContext0(Call0);
                    case 1:
                        return new CallTargetWithContext1(Call1);
                    case 2:
                        return new CallTargetWithContext2(Call2);
                    case 3:
                        return new CallTargetWithContext3(Call3);
                    case 4:
                        return new CallTargetWithContext4(Call4);
                    case 5:
                        return new CallTargetWithContext5(Call5);
                    default:
                        return null;
                }
            }


            public object Call0(ICallerContext context) {
                return Call(context, CallType.None, new object[] { });
            }
            public object Call1(ICallerContext context, object arg0) {
                return Call(context, CallType.None, new object[] { arg0 });
            }
            public object Call2(ICallerContext context, object arg0, object arg1) {
                return Call(context, CallType.None, new object[] { arg0, arg1 });
            }
            public object Call3(ICallerContext context, object arg0, object arg1, object arg2) {
                return Call(context, CallType.None, new object[] { arg0, arg1, arg2 });
            }
            public object Call4(ICallerContext context, object arg0, object arg1, object arg2, object arg3) {
                return Call(context, CallType.None, new object[] { arg0, arg1, arg2, arg3 });
            }
            public object Call5(ICallerContext context, object arg0, object arg1, object arg2, object arg3, object arg4) {
                return Call(context, CallType.None, new object[] { arg0, arg1, arg2, arg3, arg4 });
            }
            public object CallN(ICallerContext context, object[] args) {
                return Call(context, CallType.None, args);
            }

            public object Call(ICallerContext context, CallType callType, object[] args) {
                if (targets.Count == 1 && !binder.isBinaryOperator) return targets[0].Call(context, args);

                List<MethodTarget> applicableTargets = new List<MethodTarget>();
                foreach (MethodTarget target in targets) {
                    if (target.IsApplicable(args, NarrowingLevel.None)) {
                        applicableTargets.Add(target);
                    }
                }

                if (applicableTargets.Count == 1) {
                    return applicableTargets[0].Call(context, args);
                }
                if (applicableTargets.Count > 1) {
                    MethodTarget target = FindBest(callType, applicableTargets);
                    if (target != null) {
                        return target.Call(context, args);
                    } else {
                        throw MultipleTargets(applicableTargets, context, callType, args);
                    }
                }

                if (binder.isBinaryOperator) return Ops.NotImplemented;

                //no targets are applicable without narrowing conversions, so try those

                foreach (MethodTarget target in targets) {
                    if (target.IsApplicable(args, NarrowingLevel.Preferred)) {
                        applicableTargets.Add(target);
                    }
                }

                if (applicableTargets.Count == 0) {
                    foreach (MethodTarget target in targets) {
                        if (target.IsApplicable(args, NarrowingLevel.All)) {
                            applicableTargets.Add(target);
                        }
                    }
                }

                if (applicableTargets.Count == 1) {
                    return applicableTargets[0].Call(context, args);
                }

                if (applicableTargets.Count == 0) {
                    throw NoApplicableTarget(context, callType, args);
                } else {
                    throw MultipleTargets(applicableTargets, context, callType, args);
                }
            }

            private static bool IsBest(MethodTarget candidate, List<MethodTarget> applicableTargets, CallType callType) {
                foreach (MethodTarget target in applicableTargets) {
                    if (candidate == target) continue;
                    if (candidate.CompareTo(target, callType) != +1) return false;
                }
                return true;
            }

            private static MethodTarget FindBest(CallType callType, List<MethodTarget> applicableTargets) {
                foreach (MethodTarget candidate in applicableTargets) {
                    if (IsBest(candidate, applicableTargets, callType)) return candidate;
                }
                return null;
            }

            private Exception NoApplicableTarget(ICallerContext context, CallType callType, object[] args) {
                return TypeErrorForOverloads("no overloads of {0} could match {1}", targets, callType, args);
            }

            private Exception MultipleTargets(List<MethodTarget> applicableTargets, ICallerContext context, CallType callType, object[] args) {
                return TypeErrorForOverloads("multiple overloads of {0} could match {1}", applicableTargets, callType, args);
            }

            private static string GetArgTypeNames(object[] args, CallType callType) {
                StringBuilder buf = new StringBuilder();
                buf.Append("(");
                bool isFirstArg = true;
                int i = 0;
                if (callType == CallType.ImplicitInstance) i = 1;
                for (; i < args.Length; i++) {
                    if (isFirstArg) isFirstArg = false;
                    else buf.Append(", ");
                    buf.Append(Ops.GetPythonTypeName(args[i]));
                }
                buf.Append(")");
                return buf.ToString();
            }

            private Exception TypeErrorForOverloads(string message, List<MethodTarget> targets, CallType callType, object[] args) {
                StringBuilder buf = new StringBuilder();
                buf.AppendFormat(message, binder.name, GetArgTypeNames(args, callType));
                buf.AppendLine();
                foreach (MethodTarget target in targets) {
                    buf.Append("  ");
                    buf.AppendLine(target.ToSignatureString(binder.name, callType));
                }
                return Ops.TypeError(buf.ToString());
            }

            public void Add(MethodTarget target) {
                for (int i = 0; i < targets.Count; i++) {
                    if (CompareParameters(targets[i].parameters, target.parameters) == ParameterComparison.Same) {
                        switch (targets[i].CompareEqualParameters(target)) {
                            case -1:
                                // the new method is strictly better than the existing one so remove the existing one
                                targets.RemoveAt(i);
                                i -= 1; // modify the index since we removed a target from the list
                                break;
                            case +1:
                                // the new method is strictly worse than the existing one so skip it
                                return;
                            case 0:
                                // the two methods are identical ignoring CallType so list a conflict
                                hasConflict = true;
                                break;
                        }
                    }
                }
                targets.Add(target);
            }

            public override string ToString() {
                return string.Format("TargetSet({0} on {1}, nargs={2})", targets[0].method.Name, targets[0].method.DeclaringType.FullName, count);
            }
        }
    }
}
