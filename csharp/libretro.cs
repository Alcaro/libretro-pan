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
			set_video_refresh = (set_video_refresh_t)LoadFromDLL("retro_set_video_refresh", typeof(set_video_refresh_t));
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
			
			if (api_version() != API_VERSION)
			{
				throw new ArgumentException("The given DLL uses wrong version of libretro", "dll");
			}
		}
		
		public const uint API_VERSION = 1;
		
		
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
		
		[StructLayout(LayoutKind.Sequential)]
		public struct controller_info
		{
			public IntPtr types; // controller_description[]
			public uint num_types;
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
		
		//disgusting stuff. since C# doesn't support varargs, I'll assume it's not
		// called with more than 8 arguments and forward all eight to sprintf.
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void log_printf_t(int level, IntPtr fmt,
		                    IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4,
		                    IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8);
		
		[StructLayout(LayoutKind.Sequential)]
		public struct log_callback
		{
			public log_printf_t log;
		}
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.U1)]
		public delegate bool replace_image_index_t(uint index, IntPtr info);//retro_game_info
		
		[StructLayout(LayoutKind.Sequential)]
		public struct disk_control_callback
		{
			public set_eject_state_t set_eject_state;
			public get_eject_state_t get_eject_state;
			
			public get_image_index_t get_image_index;
			public set_image_index_t set_image_index;
			public get_num_images_t  get_num_images;
			
			public replace_image_index_t replace_image_index;
			public add_image_index_t add_image_index;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct system_av_info {
			public IntPtr geometry;//game_geometry*
			public IntPtr timing;//system_timing*
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
		public delegate void set_video_refresh_t([MarshalAs(UnmanagedType.FunctionPtr)]video_refresh_t video);
		public set_video_refresh_t set_video_refresh;
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void set_audio_sample_t([MarshalAs(UnmanagedType.FunctionPtr)]audio_sample_t audio);
		public set_audio_sample_t set_audio_sample;
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void set_audio_sample_batch_t([MarshalAs(UnmanagedType.FunctionPtr)]audio_sample_batch_t audio_batch);
		public set_audio_sample_batch_t set_audio_sample_batch;
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void set_input_poll_t([MarshalAs(UnmanagedType.FunctionPtr)]input_poll_t poll);
		public set_input_poll_t set_input_poll;
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void set_input_state_t([MarshalAs(UnmanagedType.FunctionPtr)]input_state_t state);
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
		public delegate bool load_game_t(ref game_info game);
		public load_game_t load_game;
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.U1)]
		public delegate bool load_game_special_t(uint game_type, IntPtr info, UIntPtr num_info);//retro_game_info[]
		public load_game_special_t load_game_special;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void unload_game_t();
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
		Unknown        = 0,
		First          = 0,
		Backspace      = 8,
		Tab            = 9,
		Clear          = 12,
		Return         = 13,
		Pause          = 19,
		Escape         = 27,
		Space          = 32,
		Exclaim        = 33,
		QuoteDbl       = 34,
		Hash           = 35,
		Dollar         = 36,
		Ampersand      = 38,
		Quote          = 39,
		LeftParen      = 40,
		RightParen     = 41,
		Asterisk       = 42,
		Plus           = 43,
		Comma          = 44,
		Minus          = 45,
		Period         = 46,
		Slash          = 47,
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
		Colon          = 58,
		Semicolon      = 59,
		Less           = 60,
		Equals         = 61,
		Greater        = 62,
		Question       = 63,
		At             = 64,
		LeftBracket    = 91,
		Backslash      = 92,
		RightBracket   = 93,
		Caret          = 94,
		Underscore     = 95,
		Backquote      = 96,
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
		Delete         = 127,
		
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
		KP_Period      = 266,
		KP_Divide      = 267,
		KP_Multiply    = 268,
		KP_Minus       = 269,
		KP_Plus        = 270,
		KP_Enter       = 271,
		KP_Equals      = 272,
		
		Up             = 273,
		Down           = 274,
		Right          = 275,
		Left           = 276,
		Insert         = 277,
		Home           = 278,
		End            = 279,
		PageUp         = 280,
		PageDown       = 281,
		
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
		
		NumLock        = 300,
		CapsLock       = 301,
		ScrolLock      = 302,
		RShift         = 303,
		LShift         = 304,
		RCtrl          = 305,
		LCtrl          = 306,
		RAlt           = 307,
		LAlt           = 308,
		RMeta          = 309,
		LMeta          = 310,
		LSuper         = 311,
		RSuper         = 312,
		Mode           = 313,
		Compose        = 314,
		
		Help           = 315,
		Print          = 316,
		SysReq         = 317,
		Break          = 318,
		Menu           = 319,
		Power          = 320,
		Euro           = 321,
		Undo           = 322,
		
		Last
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
		
		SetRotation = 1,
		GetOverscan = 2,
		GetCanDupe = 3,
		SetMessage = 6,
		Shutdown = 7,
		SetPerformanceLevel = 8,
		GetSystemDirectory = 9,
		SetPixelFormat = 10,
		SetInputDescriptors = 11,
		SetKeyboardCallback = 12,
		SetDiskControlInterface = 13,
		SetHWRender = 14,
		GetVariable = 15,
		SetVariables = 16,
		GetVariableUpdate = 17,
		SetSupportNoGame = 18,
		GetLibretroPath = 19,
		SetAudioCallback = 22,
		SetFrameTimeCallback = 21,
		GetRumbleInterface = 23,
		GetInputDeviceCapabilities = 24,
		GetSensorInterface = (25 | Experimental),
		GetCameraInterface = (26 | Experimental),
		GetLogInterface = 27,
		GetPerfInterface = 28,
		GetLocationInterface = 29,
		GetContentDirectory = 30,
		GetSaveDirectory = 31,
		SetSystemAVInfo = 32,
		SetProcAddressCallback = 33,
		SetSubsystemInfo = 34,
		SetControllerInfo = 35,
		SetMemoryMaps = (36 | Experimental),
		SetGeometry = 37,
		GetUsername = 38,
		GetLanguage = 39
	}
	
	public enum MemDesc {
		Const     = (1 << 0),
		BigEndian = (1 << 1),
		Align2    = (1 << 16),
		Align4    = (2 << 16),
		Align8    = (3 << 16),
		Minsize2  = (1 << 24),
		Minsize4  = (2 << 24),
		Minsize8  = (3 << 24)
	}
	
	unsafe public struct memory_descriptor
	{
		public ulong flags;
		public byte* ptr;
		public ulong offset;
		public ulong start;
		public ulong select;
		public ulong disconnect;
		public ulong len;
		public string addrspace;
	};
	
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct controller_description
	{
		public string desc;
		public uint id;
	}
	
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct subsystem_memory_info
	{
		public string extension;
		public uint type;
	}
	
	public struct subsystem_rom_info
	{
		public string desc;
		public string valid_extensions;
		public bool need_fullpath;
		public bool block_extract;
		public bool required;
		public subsystem_memory_info[] memory;
		public uint num_memory;
	};
	
	public struct subsystem_info
	{
		public string desc;
		public string ident;
		public subsystem_rom_info[] roms;
		public uint num_roms;
		public uint id;
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
	
	public enum LogLevel {
		Debug,
		Info,
		Warn,
		Error
	}
	
	public enum CpuFeat {
		SimdSSE    = (1 << 0),
		SimdSSE2   = (1 << 1),
		SimdVMX    = (1 << 2),
		SimdVMX128 = (1 << 3),
		SimdAVX    = (1 << 4),
		SimdNEON   = (1 << 5),
		SimdSSE3   = (1 << 6),
		SimdSSSE3  = (1 << 7),
		SimdMMX    = (1 << 8),
		SimdMMXEXT = (1 << 9),
		SimdSSE4   = (1 << 10),
		SimdSSE42  = (1 << 11),
		SimdAVX2   = (1 << 12),
		SimdVFPU   = (1 << 13),
		SimdPS     = (1 << 14),
		SimdAES    = (1 << 15)
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
	
	public enum sensor_action {
		AccelerometerEnable,
		AccelerometerDisable
	}
	
	public enum sensor_id {
		AccelerometerX,
		AccelerometerY,
		AccelerometerZ
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
	
	public enum camera_buffer
	{
		OpenGLTexture,
		RawFramebuffer
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
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void location_set_interval_t(uint interval_ms, uint interval_distance);
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public delegate bool location_start_t();
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void location_stop_t();
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public delegate bool location_get_position_t(out double lat, out double lon, out double horiz_accuracy, out double vert_accuracy);
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void location_lifetime_status_t();
	
	[StructLayout(LayoutKind.Sequential)]
	public struct location_callback
	{
		public location_start_t         start;
		public location_stop_t          stop;
		public location_get_position_t  get_position;
		public location_set_interval_t  set_interval;
		
		public location_lifetime_status_t initialized;
		public location_lifetime_status_t deinitialized;
	}
	
	public enum rumble_effect
	{
		Strong,
		Weak
	};
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public delegate bool set_rumble_state_t(uint port, int effect, ushort strength);
	
	[StructLayout(LayoutKind.Sequential)]
	public struct rumble_interface
	{
		public set_rumble_state_t set_rumble_state;
	}
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void audio_callback_t();
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void audio_set_state_callback_t([MarshalAs(UnmanagedType.U1)] bool enabled);
	
	[StructLayout(LayoutKind.Sequential)]
	public struct audio_callback
	{
		public audio_callback_t callback;
		public audio_set_state_callback_t set_state;
	}
	
	// typedef int64_t retro_usec_t;
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void frame_time_callback_t(long usec);
	
	[StructLayout(LayoutKind.Sequential)]
	public struct frame_time_callback
	{
		frame_time_callback_t callback;
		public long reference;
	}
	
	public const long HWFrameBufferValid = -1;
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void hw_context_reset_t();
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate UIntPtr hw_get_current_framebuffer_t();
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public delegate IntPtr hw_get_proc_address_t(string sym);
	
	public enum hw_context_type
	{
		None,
		OpenGL,
		OpenGLES2,
		OpenGLCore,
		OpenGLES3,
		OpenGLESVersion
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public struct hw_render_callback
	{
		int context_type;
		public hw_context_reset_t context_reset;
		public hw_get_current_framebuffer_t get_current_framebuffer;
		public hw_get_proc_address_t get_proc_address;
		[MarshalAs(UnmanagedType.U1)] public bool depth;
		[MarshalAs(UnmanagedType.U1)] public bool stencil;
		[MarshalAs(UnmanagedType.U1)] public bool bottom_left_origin;
		uint version_major;
		uint version_minor;
		[MarshalAs(UnmanagedType.U1)] public bool cache_context;
		public hw_context_reset_t context_destroy;
		[MarshalAs(UnmanagedType.U1)] public bool debug_context;
	}
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void keyboard_event_t([MarshalAs(UnmanagedType.U1)] bool down, uint keycode, uint character, ushort key_modifier);
	
	[StructLayout(LayoutKind.Sequential)]
	public struct keyboard_callback
	{
		public keyboard_event_t callback;
	}
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public delegate bool set_eject_state_t([MarshalAs(UnmanagedType.U1)] bool ejected);
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public delegate bool get_eject_state_t();
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate uint get_image_index_t();
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void set_image_index_t(uint index);
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate uint get_num_images_t();
	
	public delegate bool replace_image_index_t(uint index, game_info info);
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.U1)]
	public delegate bool add_image_index_t();
	
	public struct disk_control_callback
	{
		public set_eject_state_t set_eject_state;
		public get_eject_state_t get_eject_state;
		
		public get_image_index_t get_image_index;
		public set_image_index_t set_image_index;
		public get_num_images_t  get_num_images;
		
		public replace_image_index_t replace_image_index;
		public add_image_index_t add_image_index;
	}
	
	public enum pixel_format {
		XRGB1555,
		XRGB8888,
		RGB565
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
		public game_geometry geometry;
		public system_timing timing;
	};
	
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct variable {
		public string key;
		public string value;
	};
	
	public struct game_info {
		public string path;
		public byte[] data;
		public string meta;
	};
	
	
	
	Raw raw;
	
	public Libretro(string dll)
	{
		raw = new Raw(dll);
	}
	
	
	public void init()
	{
		raw.set_environment(i_env);
		raw.set_video_refresh(i_video);
		raw.set_audio_sample(i_audio);
		raw.set_audio_sample_batch(i_audio_batch);
		raw.set_input_poll(i_input_poll);
		raw.set_input_state(i_input_state);
		raw.init();
	}
	
	public void deinit()
	{
		raw.deinit();
	}
	
	public system_info get_system_info()
	{
		system_info info = new system_info();
		raw.get_system_info(out info);
		return info;
	}
	public system_av_info get_system_av_info()
	{
		Raw.system_av_info rawinfo = new Raw.system_av_info();
		raw.get_system_av_info(out rawinfo);
		system_av_info info = new system_av_info();
		//TODO: copy
		return info;
	}
	
	public void set_controller_port_device_t(uint port, uint device)
	{
		raw.set_controller_port_device(port, device);
	}
	
	public void reset()
	{
		raw.reset();
	}
	
	public void run()
	{
		raw.run();
	}
	
	public ulong serialize_size()
	{
		return raw.serialize_size().ToUInt64();
	}
	
	public void serialize(byte[] data)
	{
		//ulong size = serialize_size();
		//if (size==0) throw new InvalidOperationException("Core does not support serialization");
		//
		//if (data.Length < serialize_size()) throw new ArgumentException("Too small buffer", "data");
		
		unsafe {
			fixed (byte* p = data)
			{
				if (!raw.serialize((IntPtr)p, new UIntPtr((uint)data.Length)))
				{
					throw new InvalidOperationException("Serialization failed");
				}
			}
		}
	}
	
	public byte[] serialize()
	{
		byte[] data = new byte[serialize_size()];
		serialize(data);
		return data;
	}
	
	public void unserialize(byte[] data)
	{
		//if (data.Length < serialize_size()) throw new ArgumentException("Too small buffer", "data");
		
		unsafe {
			fixed (byte* p = data)
			{
				if (!raw.unserialize((IntPtr)p, new UIntPtr((uint)data.Length)))
				{
					throw new InvalidOperationException("Unserialization failed");
				}
			}
		}
	}
	
	public void cheat_reset()
	{
		raw.cheat_reset();
	}
	public void cheat_set(uint index, bool enabled, string code)
	{
		raw.cheat_set(index, enabled, code);
	}
	
	public void load_game(game_info game)
	{
		unsafe
		{
			fixed(byte* bytes = game.data)
			{
				Raw.game_info rawgame;
				rawgame.path = game.path;
				rawgame.data = new IntPtr(bytes);
				rawgame.size = new UIntPtr((uint)game.data.Length);
				rawgame.meta = game.meta;
				raw.load_game(ref rawgame);
			}
		}
	}
	public void load_game_special(uint game_type, game_info[] info)
	{
		//TODO
		throw new InvalidOperationException("Not implemented");
	}
	
	public void unload_game()
	{
		raw.unload_game();
	}
	
	public Region get_region()
	{
		return (Region)raw.get_region();
	}
	
	public unsafe byte* get_memory_data(uint id)
	{
		return (byte*)raw.get_memory_data(id).ToPointer();
	}
	public ulong get_memory_size(uint id)
	{
		return raw.get_memory_size(id).ToUInt64();
	}
	
	
	
	bool i_env(uint cmd, IntPtr data)
	{
		System.Console.WriteLine("Env "+cmd);
		switch ((Environment)cmd)
		{
			//case Environment.SetRotation: // const unsigned *
			//case Environment.GetOverscan: // bool *
			//case Environment.GetCanDupe: // bool *
			//case Environment.SetMessage: // const struct retro_message *
			//case Environment.Shutdown: // N/A (NULL)
			//case Environment.SetPerformanceLevel: // const unsigned *
			//case Environment.GetSystemDirectory: // const char **
			//case Environment.SetPixelFormat: // const enum retro_pixel_format *
			//case Environment.SetInputDescriptors: // const struct retro_input_descriptor *
			//case Environment.SetKeyboardCallback: // const struct retro_keyboard_callback *
			//case Environment.SetDiskControlInterface: // const struct retro_disk_control_callback *
			//case Environment.SetHWRender: // struct retro_hw_render_callback *
			//case Environment.GetVariable: // struct retro_variable *
			//case Environment.SetVariables: // const struct retro_variable *
			//case Environment.GetVariableUpdate: // bool *
			//case Environment.SetSupportNoGame: // const bool *
			//case Environment.GetLibretroPath: // const char **
			//case Environment.SetAudioCallback: // const struct retro_audio_callback *
			//case Environment.SetFrameTimeCallback: // const struct retro_frame_time_callback *
			//case Environment.GetRumbleInterface: // struct retro_rumble_interface *
			//case Environment.GetInputDeviceCapabilities: // uint64_t *
			//case Environment.GetSensorInterface: // struct retro_sensor_interface *
			//case Environment.GetCameraInterface: // struct retro_camera_callback *
			case Environment.GetLogInterface: // struct retro_log_callback *
			{
				Raw.log_callback log = new Raw.log_callback();
				log.log = log_printf_cb;
				Marshal.StructureToPtr(log, data, false);
				return true;
			}
			//case Environment.GetPerfInterface: // struct retro_perf_callback *
			//case Environment.GetLocationInterface: // struct retro_location_callback *
			//case Environment.GetContentDirectory: // const char **
			//case Environment.GetSaveDirectory: // const char **
			//case Environment.SetSystemAVInfo: // const struct retro_system_av_info *
			//case Environment.SetProcAddressCallback: // const struct retro_get_proc_address_interface *
			//case Environment.SetSubsystemInfo: // const struct retro_subsystem_info *
			//case Environment.SetControllerInfo: // const struct retro_controller_info *
			//case Environment.SetMemoryMaps: // const struct retro_memory_map *
			//case Environment.SetGeometry: // const struct retro_game_geometry *
			//case Environment.GetUsername: // const char **
			//case Environment.GetLanguage: // unsigned *
			default:
				return false;
		}
	}
	
	[DllImport("msvcrt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
	private static extern int _snprintf([MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder str, IntPtr length, IntPtr format,
	                                    IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4,
	                                    IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8);
	
	void log_printf_cb(int level, IntPtr fmt,
	            IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4,
	            IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8)
	{
		if (log_cb == null) return;
		
		System.Text.StringBuilder sb = new System.Text.StringBuilder(256);
		while (true) { // somewhat weird, but that's the best I could do without duplicating anything
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
	public log_cb_t log_cb;
	
	
	void i_video(IntPtr data, uint width, uint height, UIntPtr pitch)
	{
		unsafe {
			video_cb(data.ToPointer(), width, height, pitch.ToUInt64());
		}
	}
	
	
		//[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		//[return: MarshalAs(UnmanagedType.U1)]
		//public delegate bool environment_t(uint cmd, IntPtr data);//void*
		//[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		//public delegate void video_refresh_t(IntPtr data, uint width, uint height, UIntPtr pitch);//void[]
		//[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		//public delegate void audio_sample_t(short left, short right);
		//[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		//public delegate void audio_sample_batch_t(IntPtr data, UIntPtr frames);//int16_t[]
		//[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		//public delegate void input_poll_t();
		//[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		//public delegate short input_state_t(uint port, uint device, uint index, uint id);
		//raw.set_video(i_video);
		//raw.set_audio(i_audio);
		//raw.set_audio_batch(i_audio_batch);
		//raw.set_input_poll(i_input_poll);
		//raw.set_input_state(i_input_state);
	
	public unsafe delegate void video_cb_t(void* data, uint width, uint height, ulong pitch);
	public video_cb_t video_cb;
	
	void i_audio(short left, short right)
	{
		if (audio_cb != null) audio_cb(left, right);
		else
		{
			short[] data={left, right};
			unsafe
			{
				fixed(short* dataptr = data)
				{
					audio_batch_cb(dataptr, 1);
				}
			}
		}
	}
	
	public delegate void audio_cb_t(short left, short right);
	public audio_cb_t audio_cb;
	
	void i_audio_batch(IntPtr wdata, UIntPtr wframes) // w for wrapped
	{
		unsafe {
			short* data = (short*)wdata.ToPointer();
			ulong frames = wframes.ToUInt64();
			
			if (audio_batch_cb != null) audio_batch_cb(data, frames);
			else
			{
				for (ulong i=0;i<frames;i++)
				{
					audio_cb(data[i*2], data[i*2+1]);
				}
			}
		}
	}
	
	public unsafe delegate void audio_batch_cb_t(short* data, ulong frames);
	public audio_batch_cb_t audio_batch_cb;
	
	void i_input_poll()
	{
		if (input_poll_cb != null) input_poll_cb();
	}
	
	public delegate void input_poll_cb_t();
	public input_poll_cb_t input_poll_cb;
	
	short i_input_state(uint port, uint device, uint index, uint id)
	{
		return input_state_cb(port, device, index, id);
	}
	
	public delegate short input_state_cb_t(uint port, uint device, uint index, uint id);
	public input_state_cb_t input_state_cb;
}
