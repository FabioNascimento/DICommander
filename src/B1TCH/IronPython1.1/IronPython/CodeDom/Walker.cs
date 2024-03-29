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
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Diagnostics;

using System.CodeDom;
using System.CodeDom.Compiler;

using IronPython.Runtime;
using IronPython.Compiler.Ast;
using IronPython.Runtime.Operations;

namespace IronPython.CodeDom {
    class CodeWalker : IAstWalker {
        private static CodeTypeReference selfRef = new CodeTypeReference();

        private string file;
        private List<CodeTypeDeclaration> typeStack;
        private CodeTypeReference lastExpr;
        private CodeObject lastObject;
        private CodeMemberMethod curMethod;
        private Dictionary<string, CodeTypeReference> names;
        private List<string> importAs;
        private List<string> imports;
        private LocalReferences asmRefs;

        public CodeWalker(string filename, RemoteReferences references) {
            file = filename;
            typeStack = new List<CodeTypeDeclaration>();
            names = new Dictionary<string, CodeTypeReference>();
            importAs = new List<string>();
            imports = new List<string>();
            asmRefs = new LocalReferences(references);
        }

        public CodeObject LastObject {
            get {
                return lastObject;
            }
        }
        #region IAstWalker Members

        public bool Walk(FunctionDefinition node) {
            string nameStr = node.Name.GetString();
            if (nameStr == PythonGenerator.ctorFieldInit) {
                if (WalkFieldInitializations(node.Body)) {
                    // method is just a bunch of field member assignments,
                    // hide it...
                    return false;
                }
            }
            CodeMemberMethod method = new CodeMemberMethod();
            if (nameStr == "__init__") method = new CodeConstructor();

            if (nameStr.Length >= 2 && nameStr[0] == '_' && nameStr[1] != '_') {
                // private method (_ name), present to VS as non-mangled name.
                method.Name = nameStr.Substring(1);
                method.Attributes = MemberAttributes.Private;
            } else {
                // normal public name
                method.Name = nameStr;
                method.Attributes = MemberAttributes.Public;
            }

            // method starts at decorators, if they're available (still ends @ method)
            if (node.Decorators != null && node.Decorators.Start.Line != -1) {
                MarkForCodeDom(node.Decorators, method);
                MarkForCodeDomEndOnly(node, method);
            } else {
                MarkForCodeDom(node, method);
            }

            // process the method...
            try {
                string[] args = ProcessDecorators(node, method, node.Decorators as CallExpression);

                ProcessParameters(node, method, args);

                try {
                    CurrentMethod = method;

                    ProcessBody(node, method);
                } finally {
                    CurrentMethod = null;
                }
            } catch (System.ComponentModel.Design.Serialization.CodeDomSerializerException) {
                // we cannot deserialize this method, we will return
                // an empty method which we have marked for merge-only.  
                // When VS requests us to generate the file we will then perform
                // merges which preserve this code.
                method.UserData["MergeOnly"] = true;
                method.Statements.Clear();
            }

            // point is where the designer will try to focus if the
            // user wants to add event handler stuff.  We set it
            // to the line after the method & indented in 4 characters.
            method.UserData[typeof(System.Drawing.Point)] = new System.Drawing.Point(node.Start.Column + 4, node.Start.Line + 1);
            lastObject = method;

            return false;
        }

        public bool Walk(ClassDefinition node) {
            CodeTypeDeclaration ctd;
            CodeObject res = ctd = MarkForCodeDom(node, new CodeTypeDeclaration(node.Name.GetString()));

            // first scan all of the methods defined in the class...

            try {
                CodeTypeReference[] baseRefs = GetBaseCodeTypeReferences(node);

                ctd.BaseTypes.AddRange(baseRefs);

                using (PushType(ctd)) {
                    ctd.UserData["HasSlots"] = false;

                    if (!IsPassStmt(node.Body)) { // don't generate pass statements
                        if (node.Body is SuiteStatement) {
                            PreProcessSuite(node, ctd);

                            res = ProcessSuite(node, ctd);
                        } else {
                            CodeObject codebody = RecursiveWalk(node.Body);
                            if (codebody is CodeTypeMember) {
                                ctd.Members.Add((CodeTypeMember)codebody);
                            } else {
                                throw CodeDomSerializerError(node, "ClassDef for {0}", node.Body);
                            }
                        }
                    }
                }
            } catch (System.ComponentModel.Design.Serialization.CodeDomSerializerException) {
                // we cannot deserialize this class, we will return
                // an empty class which we have marked for merge-only.  
                // When VS requests us to generate the file we will then perform
                // merges which preserve this code.
                // Note if their are individual methods that we could not deserialize
                // from the class those are also appropriately flagged and due not
                res.UserData["MergeOnly"] = true;
                ctd.Members.Clear();
            }

            // we can now refere to this claass by name
            SaveType(ctd);
            lastObject = res;

            return false;
        }

        public bool Walk(CallExpression node) {
            CodeMethodReferenceExpression mref = MarkForCodeDom(node, new CodeMethodReferenceExpression());
            CodeExpression[] prms = new CodeExpression[node.Args.Count];
            CodeTypeReference[] paramTypes = new CodeTypeReference[node.Args.Count];

            for (int i = 0; i < node.Args.Count; i++) {
                // we allow tuples for array creation, but no where else.
                if (i == 0 && node.Args[i].Expression is TupleExpression) continue;

                prms[i] = (CodeExpression)RecursiveWalk(node.Args[i].Expression);

                // transform type references into type of references (they're technically
                // the same in Python).
                if (prms[i] is CodeTypeReferenceExpression) {
                    prms[i] = new CodeTypeOfExpression((prms[i] as CodeTypeReferenceExpression).Type);
                    paramTypes[i] = new CodeTypeReference(typeof(Type));
                } else {
                    paramTypes[i] = LastExpression;
                }
            }

            NameExpression ne = node.Target as NameExpression;
            if (ne == null) {
                // we only have a name or a target, not both.
                CodeObject targetExpr = RecursiveWalk(node.Target);
                CodeArrayCreateExpression arrCreate;
                CodeFieldReferenceExpression fieldRef;
                CodeTypeReferenceExpression typeRefExpr;

                if ((arrCreate = targetExpr as CodeArrayCreateExpression) != null) {
                    // System.Array[type]( (...) ), we fill in the args here...
                    Debug.Assert(node.Target is IndexExpression);
                    TupleExpression te = node.Args[0].Expression as TupleExpression;
                    if (te == null) throw CodeDomSerializerError(node, "expected tuple for object creation");

                    CodeExpression[] vals = new CodeExpression[te.Items.Count];
                    for (int i = 0; i < te.Items.Count; i++) {
                        vals[i] = (CodeExpression)RecursiveWalk(te.Items[i]);
                    }
                    arrCreate.Initializers.AddRange(vals);
                    lastObject = arrCreate;
                    return false;
                } else {
                    if (prms.Length > 0 && prms[0] == null) throw CodeDomSerializerError(node, "cannot deserialize a tuple except for array creation");

                    if (targetExpr is CodeMethodReferenceExpression) {
                        // ideal, we have a nice solid reference to the method
                        mref = (CodeMethodReferenceExpression)targetExpr;
                    } else if ((typeRefExpr = targetExpr as CodeTypeReferenceExpression) != null) {
                        // call to a constructor
                        lastObject = MarkForCodeDom(node, new CodeObjectCreateExpression(typeRefExpr.Type, prms));
                        return false;
                    } else if ((fieldRef = targetExpr as CodeFieldReferenceExpression) != null) {
                        // calling a method we failed to find, we'll
                        // promote the field ref to a method call.
                        mref.TargetObject = fieldRef.TargetObject;
                        mref.MethodName = fieldRef.FieldName;
                    } else {
                        // no name, our generator can handle this.
                        mref.TargetObject = (CodeExpression)targetExpr;
                    }
                }
            } else {
                if (prms.Length > 0 && prms[0] == null) throw CodeDomSerializerError(node, "cannot deserialize a tuple except for array creation");

                mref.MethodName = ne.Name.GetString();

                switch (mref.MethodName) {
                    case "super":
                        // base class call
                        LastExpression = CurrentType.BaseTypes[0];
                        lastObject = new CodeBaseReferenceExpression();
                        return false;
                    default:
                        // what is this, a built-in function?  We don't know
                        // what it's declared on, so we don't know the return type
                        LastExpression = new CodeTypeReference(typeof(object));
                        break;
                }
            }

            lastObject = MarkForCodeDom(node, new CodeMethodInvokeExpression(mref, prms));
            return false;
        }

        public bool Walk(FieldExpression node) {
            CodeObject targetObj = RecursiveWalk(node.Target);

            CodeTypeReferenceExpression treParent = targetObj as CodeTypeReferenceExpression;
            if (treParent != null) {
                CodeTypeReference trParent = treParent.Type;
                // last name was a namespace or type, let's see if we have
                // a type in the namespace or a continuing namespace.
                TypeReference refType = GetTypeByName(trParent.BaseType + "." + node.Name.GetString());
                if (refType != null) {
                    LastExpression = new CodeTypeReference(refType.FullName);
                    lastObject = MarkForCodeDom(node, new CodeTypeReferenceExpression(LastExpression));
                    return false;
                }

                CodeTypeReference ns = GetNamespaceByName(trParent.BaseType + "." + node.Name.GetString());
                if (ns != null) {
                    LastExpression = ns;
                    lastObject = MarkForCodeDom(node, new CodeTypeReferenceExpression(ns));
                    return false;
                }
            }

            CodeExpression targetExpr = (CodeExpression)targetObj;

            CodeTypeReference targetType = LastExpression;
            string strName = node.Name.GetString();

            // check if we're looking for a private member, if so, look it
            // up w/o the _
            if (targetExpr is CodeThisReferenceExpression &&
                strName.Length >= 2 && strName[0] == '_' && strName[1] != '_')
                strName = strName.Substring(1);

            switch (GetMemberType(targetType, strName)) {
                case MemberTypes.Field:
                    lastObject = MarkForCodeDom(node, new CodeFieldReferenceExpression(targetExpr, strName));
                    break;
                case MemberTypes.Property:
                    lastObject = MarkForCodeDom(node, new CodePropertyReferenceExpression(targetExpr, strName));
                    break;
                case MemberTypes.Method:
                    lastObject = MarkForCodeDom(node, new CodeMethodReferenceExpression(targetExpr, strName));
                    break;
                case MemberTypes.Event:
                    lastObject = MarkForCodeDom(node, new CodeEventReferenceExpression(targetExpr, strName));
                    break;
                case MemberTypes.TypeInfo:
                    // only used for self.__class__ currently.
                    lastObject = MarkForCodeDom(node, new CodeTypeOfExpression(CurrentType.Name));
                    break;
                default:
                    throw CodeDomSerializerError(node, "unknown field property: {0}", strName);
            }
            return false;
        }

        public bool Walk(IndexExpression node) {
            CodeExpression targetExpr = (CodeExpression)RecursiveWalk(node.Target);
            CodeExpression indexExpr = (CodeExpression)RecursiveWalk(node.Index);

            CodeTypeReferenceExpression targetType = targetExpr as CodeTypeReferenceExpression;
            CodeTypeReferenceExpression indexType = indexExpr as CodeTypeReferenceExpression;

            if (targetType != null && indexType != null && targetType.Type.BaseType == "System.Array") {
                // array creation expression, not an Index expression. 
                // we fill in the type here, return this to our caller (we should be
                // the target of a CallExpr) and CallExpr will fill in the initializers.
                CodeArrayCreateExpression create = new CodeArrayCreateExpression();
                create.CreateType = indexType.Type;
                lastObject = create;
            } else {
                lastObject = new CodeIndexerExpression(targetExpr, indexExpr);
            }
            return false;
        }

        public bool Walk(ParenthesisExpression node) {
            lastObject = RecursiveWalk(node.Expression);
            return false;
        }

        public bool Walk(ConstantExpression node) {
            lastObject = MarkForCodeDom(node, new CodePrimitiveExpression(node.Value));
            return false;
        }

        public bool Walk(NameExpression node) {
            CodeObject res;

            string name = node.Name.GetString();
            if (name == "False") {
                res = new CodePrimitiveExpression(false);
                LastExpression = new CodeTypeReference(typeof(bool));
            } else if (name == "True") {
                res = new CodePrimitiveExpression(true);
                LastExpression = new CodeTypeReference(typeof(bool));
            } else if (name == "None") {
                res = new CodePrimitiveExpression(null);
                LastExpression = new CodeTypeReference();
            } else {
                CodeTypeReference type = GetNamespaceByName(name);
                if (type != null) {
                    res = new CodeTypeReferenceExpression(type);
                    LastExpression = type;
                } else {
                    type = GetTypeFromVariableName(node);
                    if (type != SelfReference) {
                        res = new CodeVariableReferenceExpression(name);
                    } else {
                        res = new CodeThisReferenceExpression();
                    }
                    LastExpression = type;
                }
            }

            lastObject = MarkForCodeDom(node, res);
            return false;
        }

        public bool Walk(AndExpression node) {
            lastObject = MarkForCodeDom(node, new CodeBinaryOperatorExpression(
                (CodeExpression)RecursiveWalk(node.Left),
                CodeBinaryOperatorType.BooleanAnd,
                (CodeExpression)RecursiveWalk(node.Right)));

            return false;

        }

        public bool Walk(OrExpression node) {
            lastObject = MarkForCodeDom(node, new CodeBinaryOperatorExpression(
                (CodeExpression)RecursiveWalk(node.Left),
                CodeBinaryOperatorType.BooleanOr,
                (CodeExpression)RecursiveWalk(node.Right)));
            return false;
        }

        public bool Walk(BinaryExpression node) {
            CodeBinaryOperatorType theOp;

            if (node.Operator == BinaryOperator.Add) theOp = CodeBinaryOperatorType.Add;
            else if (node.Operator == BinaryOperator.BitwiseAnd) theOp = CodeBinaryOperatorType.BitwiseAnd;
            else if (node.Operator == BinaryOperator.BitwiseOr) theOp = CodeBinaryOperatorType.BitwiseOr;
            else if (node.Operator == BinaryOperator.Divide) theOp = CodeBinaryOperatorType.Divide;
            else if (node.Operator == BinaryOperator.Equal) theOp = CodeBinaryOperatorType.ValueEquality;
            else if (node.Operator == BinaryOperator.GreaterThan) theOp = CodeBinaryOperatorType.GreaterThan;
            else if (node.Operator == BinaryOperator.GreaterThanOrEqual) theOp = CodeBinaryOperatorType.GreaterThanOrEqual;
            else if (node.Operator == BinaryOperator.Is) theOp = CodeBinaryOperatorType.IdentityEquality;
            else if (node.Operator == BinaryOperator.IsNot) theOp = CodeBinaryOperatorType.IdentityInequality;
            else if (node.Operator == BinaryOperator.LessThan) theOp = CodeBinaryOperatorType.LessThan;
            else if (node.Operator == BinaryOperator.LessThanOrEqual) theOp = CodeBinaryOperatorType.LessThanOrEqual;
            else if (node.Operator == BinaryOperator.Mod) theOp = CodeBinaryOperatorType.Modulus;
            else if (node.Operator == BinaryOperator.Multiply) theOp = CodeBinaryOperatorType.Multiply;
            else if (node.Operator == BinaryOperator.Subtract) theOp = CodeBinaryOperatorType.Subtract;
            else if (node.Operator == BinaryOperator.NotEqual) theOp = CodeBinaryOperatorType.IdentityInequality;

            else throw CodeDomSerializerError(node, "can't generate CodeDom tree for: {0}", node.Operator.Symbol);

            lastObject = MarkForCodeDom(node, new CodeBinaryOperatorExpression(
                (CodeExpression)RecursiveWalk(node.Left),
                theOp,
                (CodeExpression)RecursiveWalk(node.Right)));
            return false;
        }

        private CodeMemberMethod GetMethod(string name) {
            for (int i = 0; i < CurrentType.Members.Count; i++) {
                CodeMemberMethod method = CurrentType.Members[i] as CodeMemberMethod;
                if (method != null && method.Name == name) {
                    return method;
                }
            }
            return null;
        }
        public bool Walk(AssignStatement node) {
            if (node.Left.Count != 1)
                throw CodeDomSerializerError(node, "Can only generate CodeDom trees w/ one left-hand side");

            NameExpression lhname = node.Left[0] as NameExpression;
            if (lhname != null) {
                string name = lhname.Name.GetString();
                // assignment to a local, return w/ an init expression.
                CodeTypeReference localType = GetLocalType(name);
                if (localType == null) {
                    // local isn't defined yet, this is a CodeVariableDeclarationStatement
                    CodeExpression initStmt = (CodeExpression)RecursiveWalk(node.Right);
                    if (CurrentMethod != null) DeclareLocal(name, LastExpression);
                    else if (CurrentType != null) {
                        CallExpression ce = node.Right as CallExpression;
                        if (ce != null) {
                            NameExpression callTargetName = ce.Target as NameExpression;
                            if (callTargetName != null && callTargetName.Name.GetString() == "property") {
                                return AddProperty(node, name, ce);
                            }
                        } else {
                            DeclareField(name, LastExpression, node);
                        }
                    } else throw CodeDomSerializerError(node, "Assignment in unexpected location");


                    lastObject = new CodeVariableDeclarationStatement(LastExpression, name, initStmt);
                    return false;
                }
            }
            lastObject = MarkForCodeDom(node,
                new CodeAssignStatement(
                    (CodeExpression)RecursiveWalk(node.Left[0]),
                    (CodeExpression)RecursiveWalk(node.Right)));

            return false;
        }

        private bool AddProperty(AssignStatement node, string name, CallExpression ce) {
            CodeMemberProperty prop = new CodeMemberProperty();
            prop.Name = name;
            for (int i = 0; i < ce.Args.Count; i++) {
                Arg arg = ce.Args[i];
                if (arg.Name != SymbolTable.Empty) {
                    switch (arg.Name.GetString()) {
                        case "fget": AddGetter(node, prop, arg.Expression); break;
                        case "fset": AddSetter(node, prop, arg.Expression); break;
                        default: throw CodeDomSerializerError(node, "cannot deserialized del properties");
                    }
                } else {
                    switch (i) {
                        case 0: AddGetter(node, prop, arg.Expression); break;
                        case 1: AddSetter(node, prop, arg.Expression); break;
                        default: throw CodeDomSerializerError(node, "cannot deserialized del properties");
                    }
                }
            }
            MarkForCodeDom(node, prop);
            lastObject = prop;
            return false;
        }

        private CodeMemberMethod AddSetter(AssignStatement node, CodeMemberProperty prop, Expression propExpr) {
            NameExpression ne = propExpr as NameExpression;
            if (ne == null) throw CodeDomSerializerError(node, "setter method is not a name");

            prop.HasSet = true;

            CodeMemberMethod method = GetMethod(ne.Name.GetString());
            CurrentType.Members.Remove(method);
            if (method == null) throw CodeDomSerializerError(node, "cannot find setter method");

            prop.Attributes = method.Attributes;
            prop.Type = method.Parameters[0].Type;
            prop.SetStatements.AddRange(method.Statements);
            prop.UserData["GetName"] = ne.Name.GetString();
            prop.UserData["HasAccepts"] = method.UserData["HasAccepts"];
            prop.UserData["HasReturns"] = method.UserData["HasReturns"];
            return method;
        }

        private CodeMemberMethod AddGetter(AssignStatement node, CodeMemberProperty prop, Expression propExpr) {
            NameExpression ne = propExpr as NameExpression;
            if (ne == null) throw CodeDomSerializerError(node, "setter method is not a name");

            prop.HasGet = true;
            CodeMemberMethod method = GetMethod(ne.Name.GetString());
            CurrentType.Members.Remove(method);
            if (method == null) throw CodeDomSerializerError(node, "cannot find getter method");

            prop.Attributes = method.Attributes;
            prop.Type = method.ReturnType;
            prop.GetStatements.AddRange(method.Statements);
            prop.UserData["SetName"] = ne.Name.GetString();
            prop.UserData["HasAccepts"] = method.UserData["HasAccepts"];
            prop.UserData["HasReturns"] = method.UserData["HasReturns"];
            return method;
        }

        public bool Walk(AugAssignStatement node) {
            CodeMethodReferenceExpression cm = RecursiveWalk(node.Right) as CodeMethodReferenceExpression;
            if (cm == null) throw CodeDomSerializerError(node, "+= must be followed by method for event");


            CodeEventReferenceExpression codeEvent = RecursiveWalk(node.Left) as CodeEventReferenceExpression;
            if (codeEvent == null) throw CodeDomSerializerError(node, "left hand side must be event");
            CodeTypeReference eventType = LastExpression;

            lastObject = MarkForCodeDom(node,
                new CodeAttachEventStatement(
                    new CodeEventReferenceExpression(codeEvent.TargetObject, codeEvent.EventName),
                    new CodeDelegateCreateExpression(eventType, cm.TargetObject, cm.MethodName)
                    ));
            return false;
        }

        public bool Walk(ExpressionStatement node) {
            lastObject = MarkForCodeDom(node,
                new CodeExpressionStatement((CodeExpression)RecursiveWalk(node.Expression)));
            return false;
        }

        public bool Walk(FromImportStatement node) {
            StringBuilder name = new StringBuilder();
            for (int i = 0; i < node.Names.Count; i++) {
                if (i != 0) name.Append(".");
                name.Append(node.Names[i]);
            }
            CodeNamespaceImport cni = MarkForCodeDom(node, new CodeNamespaceImport(node.Root.MakeString()));
            if (node.Names == FromImportStatement.Star) {
                cni.UserData["FromImport"] = "*";
                AddImportAs(node.Root.MakeString());
            } else {
                cni.UserData["FromImport"] = name.ToString();
                //!!! need to support this.
            }

            lastObject = cni;
            return false;
        }

        public bool Walk(IfStatement node) {
            CodeConditionStatement startCcs;

            CodeConditionStatement ccs = startCcs = MarkForCodeDom(node, new CodeConditionStatement(
                 (CodeExpression)RecursiveWalk(node.Tests[0].Test),
                 IsPassStmt(node.Tests[0].Body) ? new CodeStatement[] { } : GetStatements(RecursiveWalk(node.Tests[0].Body))));

            for (int i = 1; i < node.Tests.Count; i++) {
                CodeConditionStatement newCcs = MarkForCodeDom(node, new CodeConditionStatement(
                    (CodeExpression)RecursiveWalk(node.Tests[i].Test),
                    IsPassStmt(node.Tests[i].Body) ? new CodeStatement[] { } : GetStatements(RecursiveWalk(node.Tests[i].Body))));

                ccs.FalseStatements.Add(newCcs);
                ccs = newCcs;
            }

            if (node.ElseStatement != null && !IsPassStmt(node.ElseStatement)) { //!!! Should store the fact that there was an empty else statement in UserData so that we can restore it in generation
                CodeObject co = RecursiveWalk(node.ElseStatement);
                if (co is CodeStatement)
                    ccs.FalseStatements.Add((CodeStatement)co);
                else {
                    if (co is CodeObjectSuite) {
                        CodeObjectSuite cos = co as CodeObjectSuite;
                        for (int i = 0; i < cos.Count; i++) {
                            ccs.FalseStatements.Add((CodeStatement)cos[i]);
                        }
                    } else {
                        throw CodeDomSerializerError(node, "Non-SuiteStatement for conditional body: {0}", co.GetType());
                    }
                }
            }

            lastObject = startCcs;
            return false;
        }

        public bool Walk(ImportStatement node) {
            StringBuilder name = new StringBuilder();
            for (int i = 0; i < node.Names.Count; i++) {
                if (i != 0) name.Append(".");
                name.Append(node.Names[i].MakeString());
            }

            string ns = name.ToString();
            AddImport(ns);

            lastObject = MarkForCodeDom(node, new CodeNamespaceImport(ns));
            return false;
        }

        public bool Walk(PassStatement node) {
            lastObject = MarkForCodeDom(node, new CodeSnippetStatement("pass"));
            return false;
        }

        public bool Walk(ReturnStatement node) {
            lastObject = MarkForCodeDom(node,
                new CodeMethodReturnStatement((CodeExpression)RecursiveWalk(node.Expression)));
            return false;
        }

        public bool Walk(SuiteStatement node) {
            CodeObject[] cos = new CodeObject[node.Statements.Count];

            int i = 0;
            foreach (Statement s in node.Statements) {
                cos[i++] = RecursiveWalk(s);
            }

            lastObject = MarkForCodeDom(node, new CodeObjectSuite(cos));
            return false;
        }

        public bool Walk(WhileStatement node) {
            if (node.ElseStatement != null) {
                throw CodeDomSerializerError(node, "Cannot create CodeDom trees w/ while's w/ elses");
            }

            CodeStatement[] statements;
            if (IsPassStmt(node.Body)) {
                statements = new CodeStatement[] { };
            } else {
                SuiteStatement ss = node.Body as SuiteStatement;
                if (ss != null) {
                    statements = new CodeStatement[ss.Statements.Count];
                    for (int i = 0; i < ss.Statements.Count; i++) {
                        statements[i] = (CodeStatement)RecursiveWalk(ss.Statements[i]);
                    }
                } else {
                    statements = new CodeStatement[] { (CodeStatement)RecursiveWalk(node.Body) };
                }
            }

            lastObject = MarkForCodeDom(node,
                new CodeIterationStatement(
                    null,
                    (CodeExpression)RecursiveWalk(node.Test),
                    null,
                    statements)
                );
            return false;
        }

        public bool Walk(RaiseStatement node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(TryFinallyStatement node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(TryStatement node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(BreakStatement node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }
        public bool Walk(ContinueStatement node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(DelStatement node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(ExecStatement node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(GlobalStatement node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(GlobalSuite node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(ForStatement node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(PrintStatement node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(AssertStatement node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(YieldStatement node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(BackQuoteExpression node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(DictionaryExpression node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(ErrorExpression node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(GeneratorExpression node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(LambdaExpression node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(ListComprehension node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(ListExpression node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(SliceExpression node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(TupleExpression node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(UnaryExpression node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(ConditionalExpression node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(Arg node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(DottedName node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(IfStatementTest node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(ListComprehensionFor node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(ListComprehensionIf node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(TryStatementHandler node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        public bool Walk(WithStatement node) {
            throw CodeDomSerializerError(node, "cannot generate {0} for {1}", node, node.GetType());
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // post-walkers, we do nothing w/ these.

        public void PostWalk(AndExpression node) {
        }

        public void PostWalk(BackQuoteExpression node) {
        }

        public void PostWalk(BinaryExpression node) {
        }

        public void PostWalk(CallExpression node) {
        }


        public void PostWalk(ConstantExpression node) {
        }

        public void PostWalk(DictionaryExpression node) {
        }


        public void PostWalk(ErrorExpression node) {
        }
        public void PostWalk(FieldExpression node) {
        }


        public void PostWalk(GeneratorExpression node) {
        }

        public void PostWalk(IndexExpression node) {
        }

        public void PostWalk(LambdaExpression node) {
        }


        public void PostWalk(ListComprehension node) {
        }

        public void PostWalk(ListExpression node) {
        }

        public void PostWalk(NameExpression node) {
        }


        public void PostWalk(OrExpression node) {
        }

        public void PostWalk(ParenthesisExpression node) {
        }
        public void PostWalk(SliceExpression node) {
        }

        public void PostWalk(TupleExpression node) {
        }
        public void PostWalk(UnaryExpression node) {
        }

        public void PostWalk(ConditionalExpression node) {
        }


        public void PostWalk(AssertStatement node) {
        }

        public void PostWalk(AssignStatement node) {
        }


        public void PostWalk(AugAssignStatement node) {
        }


        public void PostWalk(BreakStatement node) {
        }

        public void PostWalk(ClassDefinition node) {
        }

        public void PostWalk(ContinueStatement node) {
        }

        public void PostWalk(DelStatement node) {
        }

        public void PostWalk(ExecStatement node) {
        }

        public void PostWalk(ExpressionStatement node) {
        }

        public void PostWalk(ForStatement node) {
        }

        public void PostWalk(FromImportStatement node) {
        }

        public void PostWalk(GlobalStatement node) {
        }

        public void PostWalk(GlobalSuite node) {
        }

        public void PostWalk(IfStatement node) {
        }

        public void PostWalk(ImportStatement node) {
        }

        public void PostWalk(PassStatement node) {
        }

        public void PostWalk(PrintStatement node) {
        }

        public void PostWalk(RaiseStatement node) {
        }

        public void PostWalk(ReturnStatement node) {
        }

        public void PostWalk(SuiteStatement node) {
        }

        public void PostWalk(TryFinallyStatement node) {
        }

        public void PostWalk(TryStatement node) {
        }

        public void PostWalk(WhileStatement node) {
        }

        public void PostWalk(YieldStatement node) {
        }

        public void PostWalk(Arg node) {
        }

        public void PostWalk(DottedName node) {
        }

        public void PostWalk(IfStatementTest node) {
        }

        public void PostWalk(ListComprehensionFor node) {
        }

        public void PostWalk(ListComprehensionIf node) {
        }

        public void PostWalk(TryStatementHandler node) {
        }
        public void PostWalk(FunctionDefinition node) {
        }

        public void PostWalk(WithStatement node) {
        }

        #endregion

        #region Parser state
        public TypeReference GetTypeByReference(CodeTypeReference type) {
            return GetTypeByName(type.BaseType);
        }

        public CodeTypeReference GetNamespaceByName(string name) {
            for (int i = 0; i < importAs.Count; i++) {
                if (importAs[i] == name) {
                    // refs to a namespace
                    return new CodeTypeReference(importAs[i]);
                }
            }

            for (int i = 0; i < imports.Count; i++) {
                if (imports[i] == name) {
                    // refs to a namespace
                    return new CodeTypeReference(imports[i]);
                }
            }

            // see if we're a partial name space...
            for (int i = 0; i < importAs.Count; i++) {
                if (importAs[i].Length > name.Length &&
                    String.Compare(name, 0, importAs[i], 0, name.Length) == 0) {
                    // refs to a namespace
                    return new CodeTypeReference(name);
                }
            }

            for (int i = 0; i < imports.Count; i++) {
                if (imports[i].Length > name.Length &&
                    String.Compare(name, 0, imports[i], 0, name.Length) == 0) {
                    // refs to a namespace
                    return new CodeTypeReference(name);
                }
            }
            return null;
        }

        public TypeReference GetTypeByName(string name) {
            // try the raw type name first
            TypeReference t = asmRefs.GetType(name);
            if (t != null) return t;

            // then try looking up a fully qualified
            // type based upon our Import *
            for (int i = 0; i < importAs.Count; i++) {
                t = asmRefs.GetType(importAs[i] + "." + name);
                if (t != null) return t;
            }

            if (name == "object") return new TypeReference(typeof(object));
            return null;
        }

        /// <summary> Given a CodeTypeDeclaration try and find the member associated
        /// with the specifid name </summary>
        private static CodeTypeMember FindMember(CodeTypeDeclaration ctd, string name) {
            for (int i = 0; i < ctd.Members.Count; i++) {
                if (ctd.Members[i].Name == name) {
                    return (ctd.Members[i]);
                }
            }
            return null;
        }

        /// <summary> Given a NameExpr try to determines the type.  We handle self references,
        /// arguments, locals, and other CLR types. </summary>
        public CodeTypeReference GetTypeFromVariableName(NameExpression ne) {
            string name = ne.Name.GetString();

            if (name == "self" &&
                curMethod != null &&
                (curMethod.Attributes & MemberAttributes.Static) == 0) {
                return SelfReference;
            }

            if (name == "__name__") return new CodeTypeReference(typeof(string));

            CodeTypeReference res;
            // check for a local variable
            res = GetLocalType(name);
            if (res != null) return res;

            // check for a reference to a class we know about
            if (names.TryGetValue(name, out res)) {
                return res;
            }

            // check for a type in an imported namespace
            TypeReference typ = GetTypeByName(name);
            if (typ == null) return new CodeTypeReference(typeof(object));      // we don't know the type, just treat it as object
            return new CodeTypeReference(typ.FullName);
        }

        /// <summary> Saves a user declared type so we can look it up by name </summary>
        public void SaveType(CodeTypeDeclaration ctd) {
            names[ctd.Name] = new CodeTypeReference(ctd.Name);
        }

        /// <summary> Returns a single CodeTypeReference that represents the current type. </summary>
        public static CodeTypeReference SelfReference {
            get {
                return selfRef;
            }
        }
        /// <summary> Gets the filename that we are currently parsing </summary>
        public string Filename {
            get {
                return file;
            }
        }

        /// <summary> Adds an import that is imported from foo import * <summary>
        public void AddImportAs(string nameSpace) {
            importAs.Add(nameSpace);
        }

        /// <summary> Adds an import that is imported as import foo </summary>
        public void AddImport(string nameSpace) {
            imports.Add(nameSpace);
        }

        /// <summary>
        /// Pushes a class definition onto the stack.  returns an IDisposable
        /// that when disposed will pop the stack off, allowing for using(xyz.PushType()).
        /// </summary>
        public IDisposable PushType(CodeTypeDeclaration type) {
            typeStack.Add(type);
            return new TypePopper(this);
        }

        /// <summary>
        /// Pops the current type off the type stack
        /// </summary>
        public CodeTypeDeclaration PopType() {
            CodeTypeDeclaration res = typeStack[typeStack.Count - 1];
            typeStack.RemoveAt(typeStack.Count - 1);
            return res;
        }

        /// <summary>
        /// Gets the current class for which we're emitting code for
        /// </summary>
        public CodeTypeDeclaration CurrentType {
            get {
                return typeStack[typeStack.Count - 1];
            }
        }

        /// <summary>
        /// Gets the current method that is being generated.  Setting
        /// the current method updates our environment variables.
        /// </summary>
        public CodeMemberMethod CurrentMethod {
            get {
                return curMethod;
            }
            set {
                // update our currently known types w/ the parameter types.
                if (value == null) {
                    for (int i = 0; i < curMethod.Parameters.Count; i++) {
                        names.Remove(curMethod.Parameters[i].Name);
                    }
                } else {
                    for (int i = 0; i < value.Parameters.Count; i++) {
                        names[value.Parameters[i].Name] = value.Parameters[i].Type;
                    }
                }
                curMethod = value;
            }
        }

        /// <summary> Gets a CodeTypeReference that represents the type of a local variable. </summary>
        public CodeTypeReference GetLocalType(string name) {
            if (CurrentMethod != null) {

                Dictionary<string, CodeTypeReference> localDict = CurrentMethod.UserData["Locals"] as Dictionary<string, CodeTypeReference>;
                if (localDict == null) CurrentMethod.UserData["Locals"] = localDict = new Dictionary<string, CodeTypeReference>();

                CodeTypeReference res;
                if (localDict.TryGetValue(name, out res)) {
                    return res;
                }
            }
            return null;
        }

        /// <summary> Declares a local variable and specifies it's name & type.  This allows accurate
        /// field, method, property, etc... resolution because of the type information </summary>
        public void DeclareLocal(string name, CodeTypeReference type) {
            Debug.Assert(CurrentMethod != null);

            Dictionary<string, CodeTypeReference> localDict = CurrentMethod.UserData["Locals"] as Dictionary<string, CodeTypeReference>;
            if (localDict == null) CurrentMethod.UserData["Locals"] = localDict = new Dictionary<string, CodeTypeReference>();

            localDict[name] = type;
        }

        public void DeclareField(string name, CodeTypeReference type, Node node) {
            Debug.Assert(CurrentType != null);

            CurrentType.Members.Add(MarkForCodeDom(node, new CodeMemberField(type, name)));
        }

        /// <summary> Gets or sets the type of the last expression</summary>
        public CodeTypeReference LastExpression {
            get {
                return lastExpr;
            }
            set {
                lastExpr = value;
            }
        }

        private class TypePopper : IDisposable {
            private readonly CodeWalker state;

            public TypePopper(CodeWalker walker) {
                state = walker;
            }

            public void Dispose() {
                state.PopType();
            }
        }
        #endregion

        #region Private implementation details

        private CodeObject RecursiveWalk(Node n) {
            n.Walk(this);
            return lastObject;
        }

        private Exception CodeDomSerializerError(Node node, string format, params object[] args) {
            return new System.ComponentModel.Design.Serialization.CodeDomSerializerException(
                String.Format(format, args),
                new System.CodeDom.CodeLinePragma(Filename, node.Start.Line));

        }


        #region FuncDef helpers
        /// <summary>
        /// Walks the body of the function turning it into code statements.
        /// </summary>
        private void ProcessBody(FunctionDefinition node, CodeMemberMethod method) {
            if (IsPassStmt(node.Body)) {
                // don't generate pass statements.
            } else {
                //body
                CodeObject co = RecursiveWalk(node.Body);

                if (co is CodeStatement) {
                    method.Statements.Add(co as CodeStatement);
                } else if (co is CodeObjectSuite) {
                    CodeObjectSuite cos = co as CodeObjectSuite;
                    for (int i = 0; i < cos.Count; i++) {
                        method.Statements.Add((CodeStatement)cos[i]);
                    }
                } else {
                    throw CodeDomSerializerError(node, "Non-SuiteStatement for method body: {0}", co.GetType());
                }
            }
        }

        private void ProcessParameters(FunctionDefinition node, CodeMemberMethod method, string[] args) {
            int argIndex = 0;
            foreach (Expression e in node.Parameters) {
                CodeParameterDeclarationExpression cpde = MarkForCodeDom(e, new CodeParameterDeclarationExpression());

                if (e is NameExpression) {
                    if (args == null || argIndex >= args.Length) cpde.Type = MarkForCodeDom(e, new CodeTypeReference(typeof(object)));
                    else cpde.Type = MarkForCodeDom(e, new CodeTypeReference(args[argIndex]));

                    cpde.Name = ((NameExpression)e).Name.GetString();
                } else throw CodeDomSerializerError(e, "non-Name expression: {0}", e);

                argIndex++;

                if (argIndex == 1 && (method.Attributes & MemberAttributes.Static) == 0) {
                    // instance function, we want to remember the name for
                    // round tripping....
                    method.UserData["ThisArg"] = cpde.Name;
                    if (args != null) method.UserData["ThisType"] = args[0];
                } else {
                    method.Parameters.Add(cpde);
                }
            }
        }

        /// <summary>
        /// Walks through the decorators on a function (if any) and gets
        /// the argument types, return type, and determines if the method
        /// is static or not.  Returns the argument types as strings.
        /// </summary>
        private string[] ProcessDecorators(FunctionDefinition node, CodeMemberMethod method, CallExpression decs) {
            method.UserData["HasReturns"] = false;
            method.UserData["HasAccepts"] = false;
            string[] args = null;
            while (decs != null) {
                CallExpression ce = decs.Target as CallExpression;
                if (ce != null) {
                    switch (((NameExpression)ce.Target).Name.GetString()) {
                        case "returns":
                            method.ReturnType = MarkForCodeDom(ce, new CodeTypeReference(ExtractArgument(node, ce, 0)));
                            method.UserData["HasReturns"] = true;
                            break;
                        case "accepts":
                            method.UserData["HasAccepts"] = true;
                            args = new string[ce.Args.Count];

                            ExtractArgumentTypes(node, args, ce);
                            break;
                    }
                }

                NameExpression ne = decs.Target as NameExpression;
                if (ne != null) {
                    if (ne.Name.GetString() != "staticmethod") throw Ops.ValueError("bad value for decorator name");
                    method.Attributes |= MemberAttributes.Static;
                }

                decs = decs.Args[0].Expression as CallExpression;
            }
            return args;
        }

        private void ExtractArgumentTypes(FunctionDefinition node, string[] args, CallExpression ce) {
            for (int i = 0; i < ce.Args.Count; i++) {
                args[i] = ExtractArgument(node, ce, i);
            }
        }

        private string ExtractArgument(FunctionDefinition node, CallExpression ce, int arg) {
            NameExpression typeName = ce.Args[arg].Expression as NameExpression;
            string res;
            if (typeName != null) {
                res = typeName.Name.GetString();
            } else if (ce.Args[arg].Expression is ConstantExpression) {
                ConstantExpression cne = ce.Args[arg].Expression as ConstantExpression;
                if (cne.Value == null) {
                    res = "System.Void";
                } else {
                    throw CodeDomSerializerError(node, "unexpected constant " + cne.Value.ToString());
                }
            } else if (arg == 0 && ce.Args[arg].Expression is CallExpression) {
                // Self() ???  - we can't refer our own typename inside of a classdef
                CallExpression ice = ce.Args[arg].Expression as CallExpression;
                NameExpression selfName = ice.Target as NameExpression;
                if (selfName == null || (selfName.Name.GetString() != "Self"))
                    throw CodeDomSerializerError(node, "unexpected call in @accepts: {0}", ice.Target);
                return CurrentType.Name;
            } else {

                FieldExpression fe = ce.Args[arg].Expression as FieldExpression;
                if (fe != null) return GetFieldString(fe);
                else throw CodeDomSerializerError(node, "Extracting argument w/ non-field & non-name");
            }
            return res;
        }

        private bool WalkFieldInitializations(Statement body) {
            SuiteStatement suite = body as SuiteStatement;
            if (suite != null) {
                // first time through make sure we have a valid initialization func def
                for (int i = 0; i < suite.Statements.Count; i++) {
                    AssignStatement assign = suite.Statements[i] as AssignStatement;
                    if (assign == null) return false;

                    if (assign.Left.Count != 1) return false;

                    FieldExpression fe = assign.Left[0] as FieldExpression;
                    if (fe == null) return false;

                    NameExpression ne = fe.Target as NameExpression;
                    if (ne == null || ne.Name.GetString() != "self") return false;
                }

                // second time through set the member init expressions
                for (int i = 0; i < suite.Statements.Count; i++) {
                    AssignStatement assign = suite.Statements[i] as AssignStatement;
                    FieldExpression fe = assign.Left[0] as FieldExpression;
                    SymbolId name = fe.Name;

                    for (int j = 0; j < this.CurrentType.Members.Count; j++) {
                        if (name.GetString() == CurrentType.Members[j].Name) {
                            CodeMemberField field = CurrentType.Members[j] as CodeMemberField;
                            Debug.Assert(field != null);
                            Debug.Assert(field.InitExpression == null);

                            CodeExpression ce = RecursiveWalk(assign.Right) as CodeExpression;
                            Debug.Assert(ce != null);
                            field.InitExpression = ce;
                        }
                    }
                }
                return true;
            }
            return false;

        }

        private static bool IsPassStmt(Statement statement) {
            //!!! Need to store the difference between these two in UserData
            if (statement is PassStatement)
                return true;

            if (statement is SuiteStatement
                && ((SuiteStatement)statement).Statements.Count == 1
                && ((SuiteStatement)statement).Statements[0] is PassStatement)
                return true;

            return false;
        }

        #endregion

        #region ClassDef helpers
        /// <summary> Scans a suite for method declarations and pre-adds them to
        /// the class (so we can properly look them up later) </summary>
        private static void PreProcessSuite(ClassDefinition node, CodeTypeDeclaration ctd) {
            foreach (Statement s in ((SuiteStatement)node.Body).Statements) {
                FunctionDefinition fd = s as FunctionDefinition;
                if (fd != null) {
                    CodeMemberMethod method = new CodeMemberMethod();
                    method.Name = fd.Name.GetString();
                    if (method.Name != PythonGenerator.ctorFieldInit) {
                        if (method.Name.Length >= 2 && method.Name[0] == '_' && method.Name[1] != '_')
                            method.Name = method.Name.Substring(1);

                        ctd.Members.Add(method);
                    }
                }
            }
        }

        private CodeObject ProcessSuite(ClassDefinition node, CodeTypeDeclaration ctd) {
            int docIndex = -1;
            bool fFirst = true;
            CodeNamespace cn = null;
            CodeObject res = ctd;

            foreach (Statement s in ((SuiteStatement)node.Body).Statements) {
                // pull out doc statement
                if (fFirst) {
                    fFirst = false;
                    if (node.Body.Documentation != null) {
                        docIndex = ctd.Members.Add(MarkForCodeDom(node, new CodeSnippetTypeMember("\"\"\"" + node.Body.Documentation + "\"\"\"")));
                        continue;
                    }
                }

                if (ProcessSlotsAssign(node, ctd, docIndex, s as AssignStatement)) {
                    // assignment to slots, we intercept this.
                    continue;
                }

                CodeObject co = RecursiveWalk(s);
                if (!(s is FunctionDefinition && ((FunctionDefinition)s).Name.GetString() == PythonGenerator.ctorFieldInit)) {
                    CodeTypeMember ctm;
                    if (co is CodeNamespaceImport) {
                        // this would appear to be a top-level namespace, not a type declaration
                        if (cn == null) {
                            cn = MarkForCodeDom(node, new CodeNamespace(node.Name.GetString()));
                            MarkForCodeDom(s, cn);
                        }
                        //if (ctd != null && ctd.Members.Count > 0) throw CodeDomSerializerError(state, "Mixing types & namespaces");

                        ctd = null;
                        res = cn;

                        cn.Imports.Add((CodeNamespaceImport)co);                        
                    } else if ((ctm = co as CodeTypeMember) != null) {
                        SaveCodeTypeMember(node, ctd, cn, ctm);
                    } else {
                        throw CodeDomSerializerError(node, "bad code object for class member: {0}", co);
                    }
                }
            }
            return res;
        }

        private CodeTypeReference[] GetBaseCodeTypeReferences(ClassDefinition node) {
            CodeTypeReference[] baseRefs = new CodeTypeReference[node.Bases.Count];
            for (int i = 0; i < node.Bases.Count; i++) {
                Expression o = node.Bases[i];

                if (o is NameExpression) {
                    baseRefs[i] = MarkForCodeDom(node, new CodeTypeReference(((NameExpression)o).Name.GetString()));
                } else if (o is FieldExpression) {
                    FieldExpression fe = o as FieldExpression;
                    baseRefs[i] = MarkForCodeDom(node, new CodeTypeReference(GetFieldString(fe)));
                } else throw CodeDomSerializerError(node, "bad type for base: {0}", o);
            }
            return baseRefs;
        }

        private void SaveCodeTypeMember(ClassDefinition node, CodeTypeDeclaration ctd, CodeNamespace cn, CodeTypeMember co) {
            if (cn != null && co is CodeTypeDeclaration) {
                cn.Types.Add((CodeTypeDeclaration)co);
            } else {
                CodeMemberMethod cmm = co as CodeMemberMethod;
                if (cmm != null &&
                    cmm.Name == "RealEntryPoint" &&
                    (cmm.Attributes & MemberAttributes.Static) != 0) {

                    // entry point method
                    CodeEntryPointMethod entry = new CodeEntryPointMethod();

                    entry.UserData["IPCreated"] = cmm.UserData["IPCreated"];
                    entry.UserData["EndLine"] = cmm.UserData["EndLine"];
                    entry.UserData["EndColumn"] = cmm.UserData["EndColumn"];
                    entry.UserData["Column"] = cmm.UserData["Column"];
                    entry.UserData["Line"] = cmm.UserData["Line"];

                    foreach (CodeStatement cs in cmm.Statements) {
                        entry.Statements.Add(cs);
                    }
                    CodeTypeDeclaration entryPointClass = MarkForCodeDom(node, new CodeTypeDeclaration("EntryPoint"));
                    entryPointClass.Members.Add(entry);
                    entryPointClass.UserData["NoEmit"] = true;
                    cn.Types.Add(entryPointClass);

                } else {
                    if (ctd == null) throw CodeDomSerializerError(node, "type member declared in namespace");

                    if (cmm == null) {
                        ctd.Members.Add(co);
                    } else {
                        // replace the pre-processed method holder
#if DEBUG
                        bool fReplaced = false;
#endif
                        for (int i = 0; i < ctd.Members.Count; i++) {
                            CodeMemberMethod replacing = ctd.Members[i] as CodeMemberMethod;
                            if (replacing == null || replacing.Name != cmm.Name) continue;
#if DEBUG
                            fReplaced = true;
#endif
                            ctd.Members[i] = cmm;
                            break;
                        }
#if DEBUG
                        Debug.Assert(fReplaced, String.Format("failed to find member: {0}", cmm.Name));
#endif
                    }
                }
            }
        }

        private bool ProcessSlotsAssign(ClassDefinition node, CodeTypeDeclaration ctd, int docIndex, AssignStatement assign) {
            if (assign != null &&
                assign.Left.Count == 1 &&
                assign.Left[0] is NameExpression &&
                ((NameExpression)assign.Left[0]).Name.GetString() == "__slots__") {

                ctd.UserData["HasSlots"] = true;
                ListExpression le = assign.Right as ListExpression;
                if (le == null) throw CodeDomSerializerError(node, "assignment to __slots__ other than lists");

                List<string> slots = new List<string>();
                foreach (Expression expr in le.Items) {
                    ConstantExpression ce = expr as ConstantExpression;
                    if (ce == null) throw CodeDomSerializerError(node, "non-constant assignment to __slots__");

                    slots.Add((string)ce.Value);
                }

                string docStr = node.Body.Documentation;
                if (docStr != null) {
                    // pull out the types from the string.
                    foreach (string slot in slots) {
                        string pat = @"type\s*\(\s*" + slot + @"\s*\)\s*==\s*([A-Za-z0-9_\.]+)";
                        Regex re = new Regex(pat);
                        Match m = re.Match(docStr);
                        if (m.Success) {
                            string type = m.Groups[1].Value;
                            TypeReference fieldType = GetTypeByName(type);
                            CodeMemberField field;

                            if (fieldType != null) {
                                field = MarkForCodeDom(node.Body, new CodeMemberField(new CodeTypeReference(fieldType.FullName), slot));
                            } else {
                                field = MarkForCodeDom(node.Body, new CodeMemberField(new CodeTypeReference(type), slot));
                            }

                            // check accessibility
                            if (slot.Length >= 2 && slot[0] == '_' && slot[1] != '_') {
                                field.Attributes = MemberAttributes.Private | (field.Attributes & (~MemberAttributes.AccessMask));
                                field.Name = slot.Substring(1);
                            } else {
                                field.Attributes = MemberAttributes.Public | (field.Attributes & (~MemberAttributes.AccessMask));
                            }

                            ctd.Members.Add(field);
                        } else {
                            throw CodeDomSerializerError(node, "failed to find type for slot " + slot);
                        }
                    }

                    ctd.Members.RemoveAt(docIndex);
                }
                return true;
            }
            return false;
        }
        #endregion

        #region FieldExpr impl
        MemberTypes GetMemberType(CodeTypeReference parent, string name) {

            if (parent == SelfReference) {
                MemberTypes res = GetTypeFromSelf(name);
                if (res != MemberTypes.All) return res;
            } else {
                MemberTypes res = GetTypeFromAssembly(parent, name);
                if (res != MemberTypes.All) return res;
            }

            // we know nothing about this type...
            LastExpression = new CodeTypeReference(typeof(object));
            return MemberTypes.Field;
        }

        private MemberTypes GetTypeFromAssembly(CodeTypeReference parent, string name) {
            TypeReference refType = GetTypeByReference(parent);

            if (refType != null) {
                return GetMemberFromType(name, refType);
            }
            return MemberTypes.All;
        }

        private MemberTypes GetMemberFromType(string name, TypeReference refType) {
            MemberTypes mt;
            TypeReference type = refType.GetMemberType(name, out mt);

            if (type != null) LastExpression = new CodeTypeReference(type.FullName);
            else LastExpression = new CodeTypeReference(typeof(object));

            return mt;
        }

        private MemberTypes GetTypeFromSelf(string name) {
            if (name == "__class__") {
                LastExpression = new CodeTypeReference(typeof(System.Type));
                return MemberTypes.TypeInfo; // requesting our TypeInfo
            }

            CodeTypeMember mem = FindMember(CurrentType, name);

            if (mem is CodeMemberMethod) {
                LastExpression = ((CodeMemberMethod)mem).ReturnType;
                return MemberTypes.Method;
            } else if (mem is CodeMemberProperty) {
                LastExpression = ((CodeMemberProperty)mem).Type;
                return MemberTypes.Property;
            } else if (mem is CodeMemberField) {
                LastExpression = ((CodeMemberField)mem).Type;
                return MemberTypes.Field;
            } else if (mem is CodeMemberEvent) {
                LastExpression = ((CodeMemberEvent)mem).Type;
                return MemberTypes.Event;
            }

            for (int i = 0; i < CurrentType.BaseTypes.Count; i++) {
                TypeReference baseType = GetTypeByReference(CurrentType.BaseTypes[i]);
                if (baseType != null) {
                    MemberTypes res = GetMemberFromType(name, baseType);
                    if (res != MemberTypes.All) return res;
                }
            }

            return MemberTypes.All;
        }

        public static string GetFieldString(FieldExpression fe) {
            StringBuilder sb = new StringBuilder();
            while (fe != null) {
                if (sb.Length != 0) sb.Insert(0, '.');
                sb.Insert(0, fe.Name.GetString());

                NameExpression typeName = fe.Target as NameExpression;
                if (typeName != null) {
                    sb.Insert(0, typeName.Name.GetString() + ".");
                }
                fe = fe.Target as FieldExpression;
            }
            return sb.ToString();
        }
        #endregion

        private static CodeStatement[] GetStatements(CodeObject co) {
            if (co is CodeObjectSuite) {
                CodeObjectSuite cos = co as CodeObjectSuite;
                CodeStatement[] res = new CodeStatement[cos.Count];
                for (int i = 0; i < cos.Count; i++) {
                    res[i] = cos[i] as CodeStatement;
                }
                return res;
            } else {
                return new CodeStatement[] { (CodeStatement)co };
            }
        }

        /// <summary>
        /// Updates user data w/ common information on the node so that
        /// we can correctly round-trip and identity that we created the node.
        /// </summary>
        public static T MarkForCodeDom<T>(Node node, T codeObject) where T : CodeObject {
            codeObject.UserData["IPCreated"] = true;
            codeObject.UserData["Line"] = node.Start.Line;
            codeObject.UserData["Column"] = node.Start.Column;
            return MarkForCodeDomEndOnly(node, codeObject);
        }
        public static T MarkForCodeDomEndOnly<T>(Node node, T codeObject) where T : CodeObject {
            codeObject.UserData["EndLine"] = node.End.Line;
            codeObject.UserData["EndColumn"] = node.End.Column;
            return codeObject;
        }
        #endregion
    }
}
