#!/usr/bin/env python3
from cffi import FFI
import re
import os

#https://cffi.readthedocs.org/en/release-0.8/

if 'LD_LIBRARY_PATH' not in os.environ:
	os.environ['LD_LIBRARY_PATH'] = os.getcwd()+"/.."
	import sys
	os.execv(__file__, sys.argv)

def read_core(name):
	ffi=FFI()
	
	with open("../libretro.h", "r") as file:
		iface=file.read()
	
	startpos=iface.find('#define RETRO_API_VERSION')
	iface=iface[startpos : iface.find('#ifdef __cplusplus', startpos)]
	iface=iface.replace("INT_MAX", "...") # Force it to ask the C compiler for this one.
	iface=iface.replace("/*", "\n/*") # Fix bugs if a comment starts on a define.
	iface=re.sub(r"#define +[A-Z0-9_]+\(.*", "", iface) # Delete RETRO_DEVICE_SUBCLASS.
	iface=re.sub(r"(#define +[A-Z0-9_]+) .*", r"\1 ...", iface) # Nuke the rest of the defines. Most of them make the parser barf.
	ffi.cdef(iface)
	core=ffi.verify("""#include "libretro.h" """, include_dirs=[".."], library_dirs=[".."], libraries=[name])
	if core.retro_api_version() != core.RETRO_API_VERSION: return (None, None)
	return (ffi, core)
ffi, core = read_core("snes9x_libretro")

#envs used by snes9x:
#16 SET_VARIABLES: Ignored. It just gives us some data we don't need.
#35 SET_CONTROLLER_INFO: Ignored. It just gives us some data we don't need.
#2  GET_OVERSCAN: Ignored. Do whatever.
#27 GET_LOG_INTERFACE: Ignored. CFFI does not support variadic callbacks.
#8  SET_PERFORMANCE_LEVEL: Ignored. It just gives us some data we don't need.
@ffi.callback("retro_environment_t")
def env(id, data):
	print("Env "+str(id))
	return False
core.retro_set_environment(env)

@ffi.callback("retro_video_refresh_t")
def video(data, width, height, pitch):
	print(width)
core.retro_set_video_refresh(video)

@ffi.callback("retro_audio_sample_t")
def audio_single(left, right):
	pass
core.retro_set_audio_sample(audio_single)

@ffi.callback("retro_audio_sample_batch_t")
def audio(data, frames):
	pass
	return frames
core.retro_set_audio_sample_batch(audio)

@ffi.callback("retro_input_poll_t")
def poll():
	pass
core.retro_set_input_poll(poll)

@ffi.callback("retro_input_state_t")
def input(port, device, index, id):
	return 0
core.retro_set_input_state(input)

core.retro_init()

game=ffi.new("struct retro_game_info*", [ ffi.new("char[]", b"../smw.smc"), ffi.NULL, 0, ffi.NULL ])
if core.retro_load_game(game)==False: exit(1)
game=None










