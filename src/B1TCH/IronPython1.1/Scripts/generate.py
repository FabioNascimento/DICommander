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

import re
import sys

src_dir = sys.prefix

START = "#region Generated %s"
END =   "#endregion"
PREFIX = r"^([ \t]+)"

try:
    import os
    listdir = os.listdir
    pathjoin = os.path.join
    isdir = os.path.isdir
except Exception, e:
    import nt
    import System.IO        
        
    def pathjoin(dir, file):
        if(dir[-1] == '\\'):
            return dir+file
        return dir + "\\" + file
    
    def isdir(dir):
        return System.IO.Directory.Exists(dir)
        
    listdir = nt.listdir        

class CodeWriter:
    def __init__(self, indent=0):
        self.lines = []
        self.__indent = indent

    def begin_generated(self):
        self.writeline()
        self.writeline("// *** BEGIN GENERATED CODE ***")
        self.writeline()

    def end_generated(self):
        self.writeline()
        self.writeline("// *** END GENERATED CODE ***")
        self.writeline()

    def indent(self): self.__indent += 1
    def dedent(self): self.__indent -= 1

    def writeline(self, text=None):
        if text is None or text.strip() == "":
            self.lines.append("")
        else:
            self.lines.append("    "*self.__indent + text)

    def write(self, template, **kw):
        if kw:
            template = template % kw
        for l in template.split('\n'):
            self.writeline(l)

    def enter_block(self, text=None, **kw):
        if text is not None:
            self.writeline((text % kw) + " {")
        self.indent()

    def else_block(self, text=None, **kw):
        self.dedent()
        if text:
            self.writeline("} else " + (text % kw) + " {")
        else:
            self.writeline("} else {")
        self.indent()
        
    def case_block(self, text=None, **kw):
        self.enter_block(text, **kw)
        self.indent()
        
    def case_label(self, text=None, **kw):
        self.write(text, **kw)
        self.indent()
        
    def exit_case_block(self):
        self.exit_block()
        self.dedent()

    def catch_block(self, text=None, **kw):
        self.dedent()
        if text:
            self.writeline("} catch " + (text % kw) + " {")
        else:
            self.writeline("} catch {")
        self.indent()

    def finally_block(self):
        self.dedent()
        self.writeline("} finally {")
        self.indent()

    def exit_block(self):
        self.dedent()
        self.writeline('}')

    def text(self):
        return '\n'.join(self.lines)


class CodeGenerator:
    def __init__(self, name, generator):
        self.generator = generator
        self.generators = []
        self.replacer = BlockReplacer(name)

    def do_file(self, filename):
        g = FileGenerator(filename, self.generator, self.replacer)
        if g.has_match:
            self.generators.append(g)

    def do_generate(self):
        if not self.generators:
            raise "didn't find a match for %s" % self.replacer.name
        for g in self.generators:
            g.generate()

    def do_dir(self, dirname):
        for file in listdir(dirname):            
            filename = pathjoin(dirname, file)
            if isdir(filename):
                self.do_dir(filename)
            elif filename.endswith(".cs"):
                self.do_file(filename)

    def doit(self):
        self.do_dir(src_dir)
        for g in self.generators:
            g.collect_info()
        self.do_generate()

class BlockReplacer:
    def __init__(self, name):
        self.start = START % name
        self.end = END# % name
        self.block_pat = re.compile(PREFIX+self.start+".*?"+self.end,
                                    re.DOTALL|re.MULTILINE)
        self.name = name

    def match(self, text):
        m = self.block_pat.search(text)
        if m is None: return None
        indent = m.group(1)
        return indent
        
        startIndex = text.find(self.start)
        if startIndex != -1:
            origStart = startIndex
            # search backwards and fine the new line...
            while startIndex > 0 and text[startIndex] != '\n':
                startIndex = startIndex - 1
            startIndex = startIndex + 1
            
            endIndex = text.find(self.end, startIndex)
            if endIndex != -1:
                indent = text[startIndex:origStart]
                return (indent, startIndex, endIndex+len(self.end))
        
        return None
    
    def replace(self, cw, text, indent):
        code = cw.lines
        code.insert(0, self.start)
        code.append(self.end)
        
       #code_text = '\n' + indent
        code_text = indent
        delim = False
        for line in code:
            if delim:
                code_text += "\n"
                if line:
                    code_text += indent
            code_text += line
            delim = True
        
        return self.block_pat.sub(code_text, text)
        #indicies = self.match(text)
        
        #res = text[0:indicies[1]] + code_text + text[indicies[2]:len(text)]
        #return res

class FileGenerator:
    def __init__(self, filename, generator, replacer):
        self.filename = filename
        self.generator = generator
        self.replacer = replacer

        thefile = open(filename)
        self.text = thefile.read()
        thefile.close()
        self.indent = self.replacer.match(self.text)
        self.has_match = self.indent is not None
    
    
    def collect_info(self):
        pass
    
    def generate(self):
        if sys.argv.count('checkonly') > 0:
            print "generate (check-only)", self.filename
        else:
            print "generate", self.filename
        cw = CodeWriter()
        cw.text = self.replacer.replace(CodeWriter(), self.text, self.indent)
        cw.begin_generated()
        self.generator(cw)
        cw.end_generated()
        new_text = self.replacer.replace(cw, self.text, self.indent)
        if self.text != new_text:
            if sys.argv.count('checkonly') > 0:
                print "different!"
                sys.exit(1)
            else:
                open(self.filename, 'w').write(new_text)
            

