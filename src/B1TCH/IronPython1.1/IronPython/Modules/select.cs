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
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

using IronMath;
using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;

[assembly: PythonModule("select", typeof(IronPython.Modules.PythonSelect))]
namespace IronPython.Modules {
    [Documentation("Provides support for asynchronous socket operations.")]
    public static class PythonSelect {

        #region Public API

        public static IPythonType error = ExceptionConverter.CreatePythonException("error", "select");

        [Documentation("select(iwtd, owtd, ewtd[, timeout]) -> readlist, writelist, errlist\n\n"
            + "Block until sockets are available for reading or writing, until an error\n"
            + "occurs, or until a the timeout expires. The first three parameters are\n"
            + "sequences of socket objects (opened using the socket module). The last is a\n"
            + "timeout value, given in seconds as a float. If timeout is omitted, select()\n"
            + "blocks until at least one socket is ready. A timeout of zero never blocks, but\n"
            + "can be used for polling.\n"
            + "\n"
            + "The return value is a tuple of lists of sockets that are ready (subsets of\n"
            + "iwtd, owtd, and ewtd). If the timeout occurs before any sockets are ready, a\n"
            + "tuple of three empty lists is returned.\n"
            + "\n"
            + "Note that select() on IronPython works only with sockets; it will not work with\n"
            + "files or other objects."
            )]
        [PythonName("select")]
        public static Tuple Select(object iwtd, object owtd, object ewtd, [DefaultParameterValue(null)] object timeout) {
            List readerList, writerList, errorList;
            Dictionary<Socket, object> readerOriginals, writerOriginals, errorOriginals;
            ProcessSocketSequence(iwtd, out readerList, out readerOriginals);
            ProcessSocketSequence(owtd, out writerList, out writerOriginals);
            ProcessSocketSequence(ewtd, out errorList, out errorOriginals);

            int timeoutMicroseconds;

            if (timeout == null) {
                // -1 doesn't really work as infinite, but it appears that any other negative value does
                timeoutMicroseconds = -2;
            } else {
                double timeoutSeconds;
                if (!Converter.TryConvertToDouble(timeout, out timeoutSeconds)) {
                    throw Ops.TypeErrorForTypeMismatch("float or None", timeout);
                }
                timeoutMicroseconds = (int) (1000000 * timeoutSeconds);
            }

            try {
                Socket.Select(readerList, writerList, errorList, timeoutMicroseconds);
            } catch (ArgumentNullException) {
                throw MakeException(SocketExceptionToTuple(new SocketException((int)SocketError.InvalidArgument)));
            } catch (SocketException e) {
                throw MakeException(SocketExceptionToTuple(e));
            }

            // Convert back to what the user originally passed in
            for (int i = 0; i < readerList.Count; i++) readerList[i] = readerOriginals[(Socket)readerList[i]];
            for (int i = 0; i < writerList.Count; i++) writerList[i] = writerOriginals[(Socket)writerList[i]];
            for (int i = 0; i < errorList.Count; i++) errorList[i] = errorOriginals[(Socket)errorList[i]];

            return Tuple.MakeTuple(readerList, writerList, errorList);
        }

        private static Tuple SocketExceptionToTuple(SocketException e) {
            return Tuple.MakeTuple(e.ErrorCode, e.Message);
        }

        private static Exception MakeException(object value) {
            return ExceptionConverter.CreateThrowable(error, value);
        }

        /// <summary>
        /// Process a sequence of objects that are compatible with ObjectToSocket(). Return two
        /// things as out params: an in-order List of sockets that correspond to the original
        /// objects in the passed-in sequence, and a mapping of these socket objects to their
        /// original objects.
        /// 
        /// The socketToOriginal mapping is generated because the CPython select module supports
        /// passing to select either file descriptor numbers or an object with a fileno() method.
        /// We try to be faithful to what was originally requested when we return.
        /// </summary>
        private static void ProcessSocketSequence(object sequence, out List socketList, out Dictionary<Socket, object> socketToOriginal) {
            socketToOriginal = new Dictionary<Socket, object>();
            socketList = new List();

            IEnumerator cursor = Ops.GetEnumerator(sequence);
            while (cursor.MoveNext()) {
                object original = cursor.Current;
                Socket socket = ObjectToSocket(original);
                socketList.Add(socket);
                socketToOriginal[socket] = original;
            }
        }

        /// <summary>
        /// Return the System.Net.Sockets.Socket object that corresponds to the passed-in
        /// object. obj can be a System.Net.Sockets.Socket, a PythonSocket.SocketObj, a
        /// long integer (representing a socket handle), or a Python object with a fileno()
        /// method (whose result is used to look up an existing PythonSocket.SocketObj,
        /// which is in turn converted to a Socket.
        /// </summary>
        private static Socket ObjectToSocket(object obj) {
            Socket socket;
            PythonSocket.SocketObj pythonSocket = obj as PythonSocket.SocketObj;
            if (pythonSocket != null) {
                return pythonSocket.socket;
            }

            Int64 handle;
            if (!Converter.TryConvertToInt64(obj, out handle)) {
                object userSocket = obj;
                object filenoCallable = Ops.GetAttr(DefaultContext.Default, userSocket, SymbolTable.StringToId("fileno"));
                object fileno = Ops.Call(filenoCallable);
                handle = Converter.ConvertToInt64(fileno);
            }
            if (handle < 0) {
                throw Ops.ValueError("file descriptor cannot be a negative number ({0})", handle);
            }
            socket = PythonSocket.SocketObj.HandleToSocket(handle);
            if (socket == null) {
                SocketException e = new SocketException((int)SocketError.NotSocket);
                throw ExceptionConverter.CreateThrowable(error, Tuple.MakeTuple(e.ErrorCode, e.Message));
            }
            return socket;
        }

        #endregion

    }
}