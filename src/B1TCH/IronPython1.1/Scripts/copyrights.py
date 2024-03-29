#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
#  This source code is subject to terms and conditions of the Shared Source License
#  for IronPython. A copy of the license can be found in the License.html file
#  at the root of this distribution. If you can not locate the Shared Source License
#  for IronPython, please send an email to ironpy@microsoft.com.
#  By using this source code in any fashion, you are agreeing to be bound by
#  the terms of the Shared Source License for IronPython.
#
#  You must not remove this notice, or any other, from this software.
#
######################################################################################

cs_header = """/* **********************************************************************************
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
"""

py_header = """#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
#  This source code is subject to terms and conditions of the Shared Source License
#  for IronPython. A copy of the license can be found in the License.html file
#  at the root of this distribution. If you can not locate the Shared Source License
#  for IronPython, please send an email to ironpy@microsoft.com.
#  By using this source code in any fashion, you are agreeing to be bound by
#  the terms of the Shared Source License for IronPython.
#
#  You must not remove this notice, or any other, from this software.
#
######################################################################################
"""

old_cs_header = cs_header

old_py_header = py_header

def add_header(filename, old_header, new_header):
    text = open(filename, 'r').read()
    if not text.startswith(old_header):
        print 'no old header', filename
        text = new_header + "\n" + text
        open(filename, 'w').write(text)

def do_dir(dirname):
    import os
    for file in os.listdir(dirname):
        print "Processing:", file
        if file == "ExternalCode": continue
        filename = os.path.join(dirname, file)
        if os.path.isdir(filename):
            do_dir(filename)
        elif filename.endswith(".cs"):
            add_header(filename, old_cs_header, cs_header)
        elif filename.endswith(".py"):
            add_header(filename, old_py_header, py_header)


if __name__ == "__main__":
    do_dir(".")
