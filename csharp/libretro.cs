using System;
using System.Runtime.InteropServices;

class Libretro {
	class Raw
	{
		//This is a very low-level class. It uses the C structures as much as possible, and is quite ugly to work with.
		
		static class Kernel32
		{
			[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
			public static extern IntPtr LoadLibrary(string dllToLoad);
			
			[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
			public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);
			
			[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
			[return: MarshalAs(UnmanagedType.U1)]
			public static extern bool FreeLibrary(IntPtr hModule);
		}
		IntPtr DllHandle;
		
		private Delegate LoadFromDLL(string name, Type type)
		{
			IntPtr func = Kernel32.GetProcAddress(DllHandle, name);
			if (func == IntPtr.Zero) throw new ArgumentException("The given DLL is not a libretro core");
			return Marshal.GetDelegateForFunctionPointer(func, type);
		}
		
		public Raw(string dll)
		{
			DllHandle = Kernel32.LoadLibrary(dll);
			if (DllHandle == IntPtr.Zero) Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			
			set_environment = (set_environment_t)LoadFromDLL("retro_set_environment", typeof(set_environment_t));
			set_audio_sample = (set_audio_sample_t)LoadFromDLL("retro_set_audio_sample", typeof(set_audio_sample_t));
			set_audio_sample_batch = (set_audio_sample_batch_t)LoadFromDLL("retro_set_audio_sample_batch", typeof(set_audio_sample_batch_t));
			set_input_poll = (set_input_poll_t)LoadFromDLL("retro_set_input_poll", typeof(set_input_poll_t));
			set_input_state = (set_input_state_t)LoadFromDLL("retro_set_input_state", typeof(set_input_state_t));
			init = (init_t)LoadFromDLL("retro_init", typeof(init_t));
			deinit = (deinit_t)LoadFromDLL("retro_deinit", typeof(deinit_t));
			api_version = (api_version_t)LoadFromDLL("retro_api_version", typeof(api_version_t));
			get_system_info = (get_system_info_t)LoadFromDLL("retro_get_system_info", typeof(get_system_info_t));
			get_system_av_info = (get_system_av_info_t)LoadFromDLL("retro_get_system_av_info", typeof(get_system_av_info_t));
			set_controller_port_device = (set_controller_port_device_t)LoadFromDLL("retro_set_controller_port_device", typeof(set_controller_port_device_t));
			reset = (reset_t)LoadFromDLL("retro_reset", typeof(reset_t));
			run = (run_t)LoadFromDLL("retro_run", typeof(run_t));
			serialize_size = (serialize_size_t)LoadFromDLL("retro_serialize_size", typeof(serialize_size_t));
			serialize = (serialize_t)LoadFromDLL("retro_serialize", typeof(serialize_t));
			unserialize = (unserialize_t)LoadFromDLL("retro_unserialize", typeof(unserialize_t));
			cheat_reset = (cheat_reset_t)LoadFromDLL("retro_cheat_reset", typeof(cheat_reset_t));
			cheat_set = (cheat_set_t)LoadFromDLL("retro_cheat_set", typeof(cheat_set_t));
			load_game = (load_game_t)LoadFromDLL("retro_load_game", typeof(load_game_t));
			load_game_special = (load_game_special_t)LoadFromDLL("retro_load_game_special", typeof(load_game_special_t));
			unload_game = (unload_game_t)LoadFromDLL("retro_unload_game", typeof(unload_game_t));
			get_region = (get_region_t)LoadFromDLL("retro_get_region", typeof(get_region_t));
			get_memory_data = (get_memory_data_t)LoadFromDLL("retro_get_memory_data", typeof(get_memory_data_t));
			get_memory_size = (get_memory_size_t)LoadFromDLL("retro_get_memory_size", typeof(get_memory_size_t));
			
			if (api_version() != ApiVersion)
			{
				throw new ArgumentException("The given DLL uses wrong version of libretro", "dll");
			}
		}
		
		public const uint ApiVersion = 1;
		
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct memory_descriptor
		{
			public ulong flags;
			public IntPtr ptr; // uint8_t[]
			public UIntPtr offset;
			public UIntPtr start;
			public UIntPtr select;
			public UIntPtr disconnect;
			public UIntPtr len;
			public string addrspace;
		};
		
		[StructLayout(LayoutKind.Sequential)]
		public struct memory_map {
			public IntPtr descriptors; // const memory_descriptor[]
			public uint num_descriptors;
		}
		
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct controller_description
		{
			public string desc;
			public uint id;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct controller_info
		{
			public IntPtr types; // controller_description[]
			public uint num_types;
		}
		
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct subsystem_memory_info
		{
			public string extension;
			public uint type;
		}
		
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct subsystem_rom_info
		{
			public string desc;
			public string valid_extensions;
			[MarshalAs(UnmanagedType.U1)] public bool need_fullpath;
			[MarshalAs(UnmanagedType.U1)] public bool block_extract;
			[MarshalAs(UnmanagedType.U1)] public bool required;
			public IntPtr memory; // subsystem_memory_info[]
			uint num_memory;
		};

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct subsystem_info
		{
			public string desc;
			public string ident;
			public IntPtr roms; // subsystem_rom_info[]
			uint num_roms;
			uint id;
		}
		
		//I have no idea how this is supposed to work. And it's never used either, so I can't test it.
		// typedef void (*retro_proc_address_t)(void);
		// typedef retro_proc_address_t (*retro_get_proc_address_t)(const char *sym);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public delegate IntPtr get_proc_address_t(string sym);
		
		[StructLayout(LayoutKind.Sequential)]
		public struct get_proc_address_interface
		{
			get_proc_address_t get_proc_address;
		}
		
		//disgusting stuff. since C# doesn't support varargs, I assume it's not
		// called with more than 8 arguments and forward all eight to sprintf.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public delegate void log_printf_t(int level, string fmt,
	            IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4,
	            IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8);
		
		[StructLayout(LayoutKind.Sequential)]
		public struct log_callback
		{
			public log_printf_t log;
		}
		
		// typedef uint64_t retro_perf_tick_t;
		// typedef int64_t retro_time_t;
		
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct perf_counter
		{
			public string ident;
			public ulong start;
			public ulong total;
			public ulong call_cnt;
			[MarshalAs(UnmanagedType.U1)] public bool registered;
		}
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate long perf_get_time_usec_t();
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate ulong perf_get_counter_t();
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate ulong get_cpu_features_t();
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void perf_log_t();
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void perf_register_t(out perf_counter counter);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void perf_start_t(out perf_counter counter);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void perf_stop_t(out perf_counter counter);
		
		[StructLayout(LayoutKind.Sequential)]
		public struct perf_callback
		{
			perf_get_time_usec_t    get_time_usec;
			get_cpu_features_t      get_cpu_features;
			
			perf_get_counter_t      get_perf_counter;
			perf_register_t         perf_register;
			perf_start_t            perf_start;
			perf_stop_t             perf_stop;
			perf_log_t              perf_log;
		}
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.U1)]
		public delegate bool set_sensor_state_t(uint port, sensor_action action, uint rate);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate float sensor_get_input_t(uint port, uint rate);
		
		[StructLayout(LayoutKind.Sequential)]
		public struct sensor_interface
		{
			set_sensor_state_t set_sensor_state;
			sensor_get_input_t get_sensor_input;
		}
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.U1)]
		public delegate bool camera_start_t();
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.U1)]
		public delegate bool camera_stop_t();
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void camera_lifetime_status_t();
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void camera_frame_raw_framebuffer_t(IntPtr buffer, uint width, uint height, UIntPtr pitch);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void camera_frame_opengl_texture_t(uint width, uint height, IntPtr affine);
		
		[StructLayout(LayoutKind.Sequential)]
		public struct camera_callback
		{
			public ulong caps;
			public uint width;
			public uint height;
			camera_start_t start;
			camera_stop_t stop;
			camera_frame_raw_framebuffer_t frame_raw_framebuffer;
			camera_frame_opengl_texture_t frame_opengl_texture;
			camera_lifetime_status_t initialized;
			camera_lifetime_status_t deinitialized;
		}
		
// typedef void (*retro_location_set_interval_t)(unsigned interval_ms,
//       unsigned interval_distance);
// 
// typedef bool (*retro_location_start_t)(void);
// 
// typedef void (*retro_location_stop_t)(void);
// 
// typedef bool (*retro_location_get_position_t)(double *lat, double *lon,
//       double *horiz_accuracy, double *vert_accuracy);
// 
// typedef void (*retro_location_lifetime_status_t)(void);
		
		[StructLayout(LayoutKind.Sequential)]
		public struct location_callback
		{
//    retro_location_start_t         start;
//    retro_location_stop_t          stop;
//    retro_location_get_position_t  get_position;
//    retro_location_set_interval_t  set_interval;
// 
//    retro_location_lifetime_status_t initialized;
//    retro_location_lifetime_status_t deinitialized;
		}
		
// typedef bool (*retro_set_rumble_state_t)(unsigned port, 
//       enum retro_rumble_effect effect, uint16_t strength);
		
		[StructLayout(LayoutKind.Sequential)]
		public struct rumble_interface
		{
//    retro_set_rumble_state_t set_rumble_state;
		}
		
// typedef void (*retro_audio_callback_t)(void);
// 
// typedef void (*retro_audio_set_state_callback_t)(bool enabled);

		[StructLayout(LayoutKind.Sequential)]
		public struct audio_callback
		{
//    retro_audio_callback_t callback;
//    retro_audio_set_state_callback_t set_state;
		}
		
// typedef int64_t retro_usec_t;
// typedef void (*retro_frame_time_callback_t)(retro_usec_t usec);
		[StructLayout(LayoutKind.Sequential)]
		public struct frame_time_callback
		{
//    retro_frame_time_callback_t callback;
			public long reference;
		}
		
		public const long HW_FRAME_BUFFER_VALID = -1;
		
// typedef void (*retro_hw_context_reset_t)(void);
// 
// typedef uintptr_t (*retro_hw_get_current_framebuffer_t)(void);
// 
// typedef retro_proc_address_t (*retro_hw_get_proc_address_t)(const char *sym);
		
		[StructLayout(LayoutKind.Sequential)]
		public struct hw_render_callback
		{
//    enum retro_hw_context_type context_type;
//    retro_hw_context_reset_t context_reset;
//    retro_hw_get_current_framebuffer_t get_current_framebuffer;
//    retro_hw_get_proc_address_t get_proc_address;
			[MarshalAs(UnmanagedType.U1)] public bool depth;
			[MarshalAs(UnmanagedType.U1)] public bool stencil;
			[MarshalAs(UnmanagedType.U1)] public bool bottom_left_origin;
			uint version_major;
			uint version_minor;
			[MarshalAs(UnmanagedType.U1)] public bool cache_context;
//    retro_hw_context_reset_t context_destroy;
			[MarshalAs(UnmanagedType.U1)] public bool debug_context;
		}
		
// typedef void (*retro_keyboard_event_t)(bool down, unsigned keycode, 
//       uint32_t character, uint16_t key_modifiers);
		
		[StructLayout(LayoutKind.Sequential)]
		public struct keyboard_callback
		{
//    retro_keyboard_event_t callback;
		}

// typedef bool (*retro_set_eject_state_t)(bool ejected);
// 
// typedef bool (*retro_get_eject_state_t)(void);
// 
// typedef unsigned (*retro_get_image_index_t)(void);
// 
// typedef bool (*retro_set_image_index_t)(unsigned index);
// 
// typedef unsigned (*retro_get_num_images_t)(void);
// 
// struct retro_game_info;
// 
// typedef bool (*retro_replace_image_index_t)(unsigned index,
//       const struct retro_game_info *info);
// 
// typedef bool (*retro_add_image_index_t)(void);

		[StructLayout(LayoutKind.Sequential)]
		public struct disk_control_callback
		{
//    retro_set_eject_state_t set_eject_state;
//    retro_get_eject_state_t get_eject_state;
// 
//    retro_get_image_index_t get_image_index;
//    retro_set_image_index_t set_image_index;
//    retro_get_num_images_t  get_num_images;
// 
//    retro_replace_image_index_t replace_image_index;
//    retro_add_image_index_t add_image_index;
		}
		
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct message
		{
			public string msg;
			public uint frames;
		}
		
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct input_descriptor
		{
			public uint port;
			public uint device;
			public uint index;
			public uint id;
			public string description;
		}
		
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct system_info {
			public string library_name;
			public string library_version;
			public string valid_extensions;
			[MarshalAs(UnmanagedType.U1)] public bool need_fullpath;
			[MarshalAs(UnmanagedType.U1)] public bool block_extract;
		};
		
		[StructLayout(LayoutKind.Sequential)]
		public struct game_geometry {
			public uint base_width;
			public uint base_height;
			public uint max_width;
			public uint max_height;
			
			public float aspect_ratio;
		};
		
		[StructLayout(LayoutKind.Sequential)]
		public struct system_timing {
			public double fps;
			public double sample_rate;
		};
		
		[StructLayout(LayoutKind.Sequential)]
		public struct system_av_info {
			public IntPtr geometry;//game_geometry*
			public IntPtr timing;//system_timing*
		};
		
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct variable {
			public string key;
			public string value;
		};
		
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct game_info {
			public string path;
			public IntPtr data;//uint8_t[]
			public UIntPtr size;
			public string meta;
		};
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.U1)]
		public delegate bool environment_t(uint cmd, IntPtr data);//void*
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void video_refresh_t(IntPtr data, uint width, uint height, UIntPtr pitch);//void[]
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void audio_sample_t(short left, short right);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void audio_sample_batch_t(IntPtr data, UIntPtr frames);//int16_t[]
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void input_poll_t();
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate short input_state_t(uint port, uint device, uint index, uint id);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void set_environment_t([MarshalAs(UnmanagedType.FunctionPtr)]environment_t env);
		public set_environment_t set_environment;
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void set_audio_sample_t([MarshalAs(UnmanagedType.FunctionPtr)]audio_sample_t env);
		public set_audio_sample_t set_audio_sample;
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void set_audio_sample_batch_t([MarshalAs(UnmanagedType.FunctionPtr)]audio_sample_batch_t env);
		public set_audio_sample_batch_t set_audio_sample_batch;
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void set_input_poll_t([MarshalAs(UnmanagedType.FunctionPtr)]input_poll_t env);
		public set_input_poll_t set_input_poll;
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void set_input_state_t([MarshalAs(UnmanagedType.FunctionPtr)]input_state_t env);
		public set_input_state_t set_input_state;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void init_t();
		public init_t init;
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void deinit_t();
		public deinit_t deinit;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate uint api_version_t();
		public api_version_t api_version;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void get_system_info_t(out system_info info);
		public get_system_info_t get_system_info;
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void get_system_av_info_t(out system_av_info info);
		public get_system_av_info_t get_system_av_info;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void set_controller_port_device_t(uint port, uint device);
		public set_controller_port_device_t set_controller_port_device;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void reset_t();
		public reset_t reset;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void run_t();
		public run_t run;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate UIntPtr serialize_size_t();
		public serialize_size_t serialize_size;
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.U1)]
		public delegate bool serialize_t(IntPtr data, UIntPtr size);//uint8_t[]
		public serialize_t serialize;
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.U1)]
		public delegate bool unserialize_t(IntPtr data, UIntPtr size);//uint8_t[]
		public unserialize_t unserialize;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void cheat_reset_t();
		public cheat_reset_t cheat_reset;
		[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public delegate void cheat_set_t(uint index, [MarshalAs(UnmanagedType.U1)] bool enabled, string code);
		public cheat_set_t cheat_set;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.U1)]
		public delegate bool load_game_t(out game_info game);
		public load_game_t load_game;
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.U1)]
		public delegate bool load_game_special_t(uint game_type, IntPtr info, UIntPtr num_info);//retro_game_info[]
		public load_game_special_t load_game_special;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void unload_game_t(out system_info info);
		public unload_game_t unload_game;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate uint get_region_t();
		public get_region_t get_region;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate IntPtr get_memory_data_t(uint id);
		public get_memory_data_t get_memory_data;
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate UIntPtr get_memory_size_t(uint id);
		public get_memory_size_t get_memory_size;
	}
	
	
	
	public const uint DeviceTypeShift = 8;
	public const uint DeviceMask = ((1 << (int)DeviceTypeShift) - 1);
	public static uint DeviceSubclass(uint base_, uint id)
	{
		return (((id + 1) << (int)DeviceTypeShift) | base_);
	}
	
	public enum Device {
		None,
		Joypad,
		Mouse,
		Keyboard,
		Lightgun,
		Analog,
		Pointer
	}
	
	public enum DevJoypad {
		B,
		Y,
		Select,
		Start,
		Up,
		Down,
		Left,
		Right,
		A,
		X,
		L,
		R,
		L2,
		R2,
		L3,
		R3
	}
	
	public enum DevAnalogIndex {
		Left,
		Right
	}
	public enum DevAnalog {
		X,
		Y
	}
	
	public enum DevMouse {
		X,
		Y,
		Left,
		Right,
		WheelUp,
		WheelDown,
		Middle
	}
	
	public enum DevLightgun {
		X,
		Y,
		Trigger,
		Cursor,
		Turbo,
		Pause,
		Start
	}
	
	public enum DevPointer {
		X,
		Y,
		Pressed
	}
	
	public enum Region {
		NTSC,
		PAL
	}
	
	public enum Language {
		English,
		Japanese,
		French,
		Spanish,
		German,
		Italian,
		Dutch,
		Portugese,
		Russian,
		Korean,
		ChineseTraditional,
		ChineseSimplified,
		Last
	}
	
	public enum MemType {
		SaveRAM,
		RTC,
		SystemRAM,
		VideoRAM,
		
		Mask = 0xFF
	}
	
	public enum Key
	{
		UNKNOWN        = 0,
		FIRST          = 0,
		BACKSPACE      = 8,
		TAB            = 9,
		CLEAR          = 12,
		RETURN         = 13,
		PAUSE          = 19,
		ESCAPE         = 27,
		SPACE          = 32,
		EXCLAIM        = 33,
		QUOTEDBL       = 34,
		HASH           = 35,
		DOLLAR         = 36,
		AMPERSAND      = 38,
		QUOTE          = 39,
		LEFTPAREN      = 40,
		RIGHTPAREN     = 41,
		ASTERISK       = 42,
		PLUS           = 43,
		COMMA          = 44,
		MINUS          = 45,
		PERIOD         = 46,
		SLASH          = 47,
		_0             = 48,
		_1             = 49,
		_2             = 50,
		_3             = 51,
		_4             = 52,
		_5             = 53,
		_6             = 54,
		_7             = 55,
		_8             = 56,
		_9             = 57,
		COLON          = 58,
		SEMICOLON      = 59,
		LESS           = 60,
		EQUALS         = 61,
		GREATER        = 62,
		QUESTION       = 63,
		AT             = 64,
		LEFTBRACKET    = 91,
		BACKSLASH      = 92,
		RIGHTBRACKET   = 93,
		CARET          = 94,
		UNDERSCORE     = 95,
		BACKQUOTE      = 96,
		a              = 97,
		b              = 98,
		c              = 99,
		d              = 100,
		e              = 101,
		f              = 102,
		g              = 103,
		h              = 104,
		i              = 105,
		j              = 106,
		k              = 107,
		l              = 108,
		m              = 109,
		n              = 110,
		o              = 111,
		p              = 112,
		q              = 113,
		r              = 114,
		s              = 115,
		t              = 116,
		u              = 117,
		v              = 118,
		w              = 119,
		x              = 120,
		y              = 121,
		z              = 122,
		DELETE         = 127,
		
		KP0            = 256,
		KP1            = 257,
		KP2            = 258,
		KP3            = 259,
		KP4            = 260,
		KP5            = 261,
		KP6            = 262,
		KP7            = 263,
		KP8            = 264,
		KP9            = 265,
		KP_PERIOD      = 266,
		KP_DIVIDE      = 267,
		KP_MULTIPLY    = 268,
		KP_MINUS       = 269,
		KP_PLUS        = 270,
		KP_ENTER       = 271,
		KP_EQUALS      = 272,
		
		UP             = 273,
		DOWN           = 274,
		RIGHT          = 275,
		LEFT           = 276,
		INSERT         = 277,
		HOME           = 278,
		END            = 279,
		PAGEUP         = 280,
		PAGEDOWN       = 281,
		
		F1             = 282,
		F2             = 283,
		F3             = 284,
		F4             = 285,
		F5             = 286,
		F6             = 287,
		F7             = 288,
		F8             = 289,
		F9             = 290,
		F10            = 291,
		F11            = 292,
		F12            = 293,
		F13            = 294,
		F14            = 295,
		F15            = 296,
		
		NUMLOCK        = 300,
		CAPSLOCK       = 301,
		SCROLLOCK      = 302,
		RSHIFT         = 303,
		LSHIFT         = 304,
		RCTRL          = 305,
		LCTRL          = 306,
		RALT           = 307,
		LALT           = 308,
		RMETA          = 309,
		LMETA          = 310,
		LSUPER         = 311,
		RSUPER         = 312,
		MODE           = 313,
		COMPOSE        = 314,
		
		HELP           = 315,
		PRINT          = 316,
		SYSREQ         = 317,
		BREAK          = 318,
		MENU           = 319,
		POWER          = 320,
		EURO           = 321,
		UNDO           = 322,
		
		LAST
	}
	
	public enum KeyMod {
		None       = 0x0000,
		
		Shift      = 0x01,
		Ctrl       = 0x02,
		Alt        = 0x04,
		Meta       = 0x08,
		
		NumLock    = 0x10,
		CapsLock   = 0x20,
		ScrolLock  = 0x40,
	}
	
	public enum Environment {
		Experimental = 0x10000,
		Private = 0x20000,
		
		SET_ROTATION = 1,
		GET_OVERSCAN = 2,
		GET_CAN_DUPE = 3,
		SET_MESSAGE  = 6,
		SHUTDOWN     = 7,
		SET_PERFORMANCE_LEVEL = 8,
		GET_SYSTEM_DIRECTORY = 9,
		SET_PIXEL_FORMAT = 10,
		SET_INPUT_DESCRIPTORS = 11,
		SET_KEYBOARD_CALLBACK = 12,
		SET_DISK_CONTROL_INTERFACE = 13,
		SET_HW_RENDER = 14,
		GET_VARIABLE = 15,
		SET_VARIABLES = 16,
		GET_VARIABLE_UPDATE = 17,
		SET_SUPPORT_NO_GAME = 18,
		GET_LIBRETRO_PATH = 19,
		SET_AUDIO_CALLBACK = 22,
		SET_FRAME_TIME_CALLBACK = 21,
		GET_RUMBLE_INTERFACE = 23,
		GET_INPUT_DEVICE_CAPABILITIES = 24,
		GET_SENSOR_INTERFACE = (25 | Experimental),
		GET_CAMERA_INTERFACE = (26 | Experimental),
		GET_LOG_INTERFACE = 27,
		GET_PERF_INTERFACE = 28,
		GET_LOCATION_INTERFACE = 29,
		GET_CONTENT_DIRECTORY = 30,
		GET_SAVE_DIRECTORY = 31,
		SET_SYSTEM_AV_INFO = 32,
		SET_PROC_ADDRESS_CALLBACK = 33,
		SET_SUBSYSTEM_INFO = 34,
		SET_CONTROLLER_INFO = 35,
		SET_MEMORY_MAPS = (36 | Experimental),
		SET_GEOMETRY = 37,
		GET_USERNAME = 38,
		GET_LANGUAGE = 39
	}
	
// #define RETRO_ENVIRONMENT_SET_ROTATION  1  /* const unsigned * --
// #define RETRO_ENVIRONMENT_GET_OVERSCAN  2  /* bool * --
// #define RETRO_ENVIRONMENT_GET_CAN_DUPE  3  /* bool * --
// #define RETRO_ENVIRONMENT_SET_MESSAGE   6  /* const struct retro_message * --
// #define RETRO_ENVIRONMENT_SHUTDOWN      7  /* N/A (NULL) --
// #define RETRO_ENVIRONMENT_SET_PERFORMANCE_LEVEL 8 /* const unsigned * --
// #define RETRO_ENVIRONMENT_GET_SYSTEM_DIRECTORY 9 /* const char ** --
// #define RETRO_ENVIRONMENT_SET_PIXEL_FORMAT 10 /* const enum retro_pixel_format * --
// #define RETRO_ENVIRONMENT_SET_INPUT_DESCRIPTORS 11 /* const struct retro_input_descriptor * --
// #define RETRO_ENVIRONMENT_SET_KEYBOARD_CALLBACK 12 /* const struct retro_keyboard_callback * --
// #define RETRO_ENVIRONMENT_SET_DISK_CONTROL_INTERFACE 13 /* const struct retro_disk_control_callback * --
// #define RETRO_ENVIRONMENT_SET_HW_RENDER 14 /* struct retro_hw_render_callback * --
// #define RETRO_ENVIRONMENT_GET_VARIABLE 15 /* struct retro_variable * --
// #define RETRO_ENVIRONMENT_SET_VARIABLES 16 /* const struct retro_variable * --
// #define RETRO_ENVIRONMENT_GET_VARIABLE_UPDATE 17 /* bool * --
// #define RETRO_ENVIRONMENT_SET_SUPPORT_NO_GAME 18 /* const bool * --
// #define RETRO_ENVIRONMENT_GET_LIBRETRO_PATH 19 /* const char ** --
// #define RETRO_ENVIRONMENT_SET_AUDIO_CALLBACK 22 /* const struct retro_audio_callback * --
// #define RETRO_ENVIRONMENT_SET_FRAME_TIME_CALLBACK 21 /* const struct retro_frame_time_callback * --
// #define RETRO_ENVIRONMENT_GET_RUMBLE_INTERFACE 23 /* struct retro_rumble_interface * --
// #define RETRO_ENVIRONMENT_GET_INPUT_DEVICE_CAPABILITIES 24 /* uint64_t * --
// #define RETRO_ENVIRONMENT_GET_SENSOR_INTERFACE 25 /* struct retro_sensor_interface * --
// #define RETRO_ENVIRONMENT_GET_CAMERA_INTERFACE 26 /* struct retro_camera_callback * --
// #define RETRO_ENVIRONMENT_GET_LOG_INTERFACE 27 /* struct retro_log_callback * --
// #define RETRO_ENVIRONMENT_GET_PERF_INTERFACE 28 /* struct retro_perf_callback * --
// #define RETRO_ENVIRONMENT_GET_LOCATION_INTERFACE 29 /* struct retro_location_callback * --
// #define RETRO_ENVIRONMENT_GET_CONTENT_DIRECTORY 30 /* const char ** --
// #define RETRO_ENVIRONMENT_GET_SAVE_DIRECTORY 31 /* const char ** --
// #define RETRO_ENVIRONMENT_SET_SYSTEM_AV_INFO 32 /* const struct retro_system_av_info * --
// #define RETRO_ENVIRONMENT_SET_PROC_ADDRESS_CALLBACK 33 /* const struct retro_get_proc_address_interface * --
// #define RETRO_ENVIRONMENT_SET_SUBSYSTEM_INFO 34 /* const struct retro_subsystem_info * --
// #define RETRO_ENVIRONMENT_SET_CONTROLLER_INFO 35 /* const struct retro_controller_info * --
// #define RETRO_ENVIRONMENT_SET_MEMORY_MAPS 36 /* const struct retro_memory_map * --
// #define RETRO_ENVIRONMENT_SET_GEOMETRY 37 /* const struct retro_game_geometry * --
// #define RETRO_ENVIRONMENT_GET_USERNAME 38  /* const char **
// #define RETRO_ENVIRONMENT_GET_LANGUAGE 39 /* unsigned * --
	
	public enum MemDesc {
		CONST     = (1 << 0),
		BIGENDIAN = (1 << 1),
		ALIGN_2   = (1 << 16),
		ALIGN_4   = (2 << 16),
		ALIGN_8   = (3 << 16),
		MINSIZE_2 = (1 << 24),
		MINSIZE_4 = (2 << 24),
		MINSIZE_8 = (3 << 24)
	}
	
	public enum LogLevel {
		DEBUG,
		INFO,
		WARN,
		ERROR
	}
	
	public enum CpuFeat {
		SIMD_SSE    = (1 << 0),
		SIMD_SSE2   = (1 << 1),
		SIMD_VMX    = (1 << 2),
		SIMD_VMX128 = (1 << 3),
		SIMD_AVX    = (1 << 4),
		SIMD_NEON   = (1 << 5),
		SIMD_SSE3   = (1 << 6),
		SIMD_SSSE3  = (1 << 7),
		SIMD_MMX    = (1 << 8),
		SIMD_MMXEXT = (1 << 9),
		SIMD_SSE4   = (1 << 10),
		SIMD_SSE42  = (1 << 11),
		SIMD_AVX2   = (1 << 12),
		SIMD_VFPU   = (1 << 13),
		SIMD_PS     = (1 << 14),
		SIMD_AES    = (1 << 15)
	}
	
	public enum sensor_action {
		ACCELEROMETER_ENABLE,
		ACCELEROMETER_DISABLE
	}
	
	public enum sensor_id {
		ACCELEROMETER_X,
		ACCELEROMETER_Y,
		ACCELEROMETER_Z
	}
	
	public enum camera_buffer
	{
		OPENGL_TEXTURE,
		RAW_FRAMEBUFFER
	}
	
	public enum rumble_effect
	{
		STRONG,
		WEAK
	};
	
	public enum hw_context_type
	{
		NONE,
		OPENGL,
		OPENGLES2,
		OPENGL_CORE,
		OPENGLES3,
		OPENGLES_VERSION
	}
	
	public enum pixel_format {
		XRGB1555,
		XRGB8888,
		RGB565
	}
	
	
	
	Raw raw;
	
	public Libretro(string dll, log_cb_t log)
	{
		raw = new Raw(dll);
		log_cb = log;
	}
	
	public void init()
	{
		raw.set_environment(env);
		raw.init();
	}
	
	bool env(uint cmd, IntPtr data)
	{
		System.Console.WriteLine("Env "+cmd);
		switch ((Environment)cmd)
		{
			case Environment.GET_LOG_INTERFACE:
			{
				Raw.log_callback log = new Raw.log_callback();
				log.log = log_printf_cb;
				Marshal.StructureToPtr(log, data, false);
				return true;
			}
			default:
				return false;
		}
	}
	
	[DllImport("msvcrt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
	private static extern int _snprintf([MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder str, IntPtr length, String format,
	                                    IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4,
	                                    IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8);
	
	void log_printf_cb(int level, string fmt,
	            IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4,
	            IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8)
	{
		if (log_cb==null) return;
		
		System.Text.StringBuilder sb = new System.Text.StringBuilder(256);
		while (true) {
			int len = _snprintf(sb, new IntPtr(sb.Capacity), fmt, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
			//stupid _snprintf, returning negative values on overflow instead of the length. do it properly.
			if (len <= 0 || len >= sb.Capacity)
			{
				sb.Capacity *= 2;
				continue;
			}
			sb.Length = len;
			break;
		} while (sb.Length >= sb.Capacity);
		
		log_cb((LogLevel)level, sb.ToString());
	}
	
	public delegate void log_cb_t(LogLevel level, string text);
	log_cb_t log_cb;
}

class TinyGUI
{
	static void Main()
	{
		//Libretro core = new Libretro("snes9x_libretro.dll", log);
		Libretro core = new Libretro("minir_testcore_libretro.dll", log);
		core.init();
	}
	
	static void log(Libretro.LogLevel level, string text)
	{
		System.Console.WriteLine("Log: "+text);
	}
}
