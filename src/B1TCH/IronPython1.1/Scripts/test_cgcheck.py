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


def test_main(level='full'):
    import sys
    old_args = sys.argv
    sys.argv = ['checkonly']

    generators = [
        'generate_alltypes',
        'generate_binops', 
        'generate_calls', 
        'generate_convert', 
        'generate_environment', 
        'generate_exceptions', 
        'generate_math', 
        'generate_ops',
        'generate_walker',
        'generate_typecache',
        ]

    for gen in generators:
            print "Running", gen
            __import__(gen)
            
    sys.argv = old_args
    
if __name__=="__main__":
    test_main()    
