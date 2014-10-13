#!/usr/bin/env python3
from cffi import FFI
import re
import os

if 'LD_LIBRARY_PATH' not in os.environ:
	os.environ['LD_LIBRARY_PATH'] = os.getcwd()+"/.."
	import sys
	os.execv(__file__, sys.argv)

def read_core(name):
	ffi=FFI()
	
	with open("../libretro.h", "r") as myfile:
		iface=myfile.read()
	
	startpos=iface.find('#define RETRO_API_VERSION')
	iface=iface[startpos : iface.find('#ifdef __cplusplus', startpos)]
	iface=iface.replace("INT_MAX", "...") # Force it to ask the C compiler for this one.
	iface=iface.replace("/*", "\n/*") # Fix bugs if a comment starts on a define.
	iface=re.sub(r"#define +[A-Z0-9_]+\(.*", "", iface) # Delete RETRO_DEVICE_SUBCLASS.
	iface=re.sub(r"(#define +[A-Z0-9_]+) .*", r"\1 ...", iface) # Nuke the rest of the defines. Most of them make the parser barf.
	ffi.cdef(iface)
	return (ffi, ffi.verify("""#include "libretro.h" """, include_dirs=[".."], library_dirs=[".."], libraries=[name]))

def env(id, data):
	print(id)
	print(data)
	return False

ffi, core = read_core("snes9x_libretro")
core.retro_set_environment(ffi.callback("retro_environment_t", env))
