//dynamic P/Invoke is ugly, requires LoadLibrary
//see http://blogs.msdn.com/b/jonathanswift/archive/2006/10/03/dynamically-calling-an-unmanaged-dll-from-.net-_2800_c_23002900_.aspx

using System;
using System.Runtime.InteropServices;
class LibretroRaw
{

// #define RETRO_API_VERSION         1
// 
// #define RETRO_DEVICE_TYPE_SHIFT         8
// #define RETRO_DEVICE_MASK               ((1 << RETRO_DEVICE_TYPE_SHIFT) - 1)
// #define RETRO_DEVICE_SUBCLASS(base, id) (((id + 1) << RETRO_DEVICE_TYPE_SHIFT) | base)
// 
// #define RETRO_DEVICE_NONE         0
// #define RETRO_DEVICE_JOYPAD       1
// #define RETRO_DEVICE_MOUSE        2
// #define RETRO_DEVICE_KEYBOARD     3
// #define RETRO_DEVICE_LIGHTGUN     4
// #define RETRO_DEVICE_ANALOG       5
// #define RETRO_DEVICE_POINTER      6
// 
// #define RETRO_DEVICE_ID_JOYPAD_B        0
// #define RETRO_DEVICE_ID_JOYPAD_Y        1
// #define RETRO_DEVICE_ID_JOYPAD_SELECT   2
// #define RETRO_DEVICE_ID_JOYPAD_START    3
// #define RETRO_DEVICE_ID_JOYPAD_UP       4
// #define RETRO_DEVICE_ID_JOYPAD_DOWN     5
// #define RETRO_DEVICE_ID_JOYPAD_LEFT     6
// #define RETRO_DEVICE_ID_JOYPAD_RIGHT    7
// #define RETRO_DEVICE_ID_JOYPAD_A        8
// #define RETRO_DEVICE_ID_JOYPAD_X        9
// #define RETRO_DEVICE_ID_JOYPAD_L       10
// #define RETRO_DEVICE_ID_JOYPAD_R       11
// #define RETRO_DEVICE_ID_JOYPAD_L2      12
// #define RETRO_DEVICE_ID_JOYPAD_R2      13
// #define RETRO_DEVICE_ID_JOYPAD_L3      14
// #define RETRO_DEVICE_ID_JOYPAD_R3      15
// 
// #define RETRO_DEVICE_INDEX_ANALOG_LEFT   0
// #define RETRO_DEVICE_INDEX_ANALOG_RIGHT  1
// #define RETRO_DEVICE_ID_ANALOG_X         0
// #define RETRO_DEVICE_ID_ANALOG_Y         1
// 
// #define RETRO_DEVICE_ID_MOUSE_X          0
// #define RETRO_DEVICE_ID_MOUSE_Y          1
// #define RETRO_DEVICE_ID_MOUSE_LEFT       2
// #define RETRO_DEVICE_ID_MOUSE_RIGHT      3
// #define RETRO_DEVICE_ID_MOUSE_WHEELUP    4
// #define RETRO_DEVICE_ID_MOUSE_WHEELDOWN  5
// #define RETRO_DEVICE_ID_MOUSE_MIDDLE     6
// 
// #define RETRO_DEVICE_ID_LIGHTGUN_X        0
// #define RETRO_DEVICE_ID_LIGHTGUN_Y        1
// #define RETRO_DEVICE_ID_LIGHTGUN_TRIGGER  2
// #define RETRO_DEVICE_ID_LIGHTGUN_CURSOR   3
// #define RETRO_DEVICE_ID_LIGHTGUN_TURBO    4
// #define RETRO_DEVICE_ID_LIGHTGUN_PAUSE    5
// #define RETRO_DEVICE_ID_LIGHTGUN_START    6
// 
// #define RETRO_DEVICE_ID_POINTER_X         0
// #define RETRO_DEVICE_ID_POINTER_Y         1
// #define RETRO_DEVICE_ID_POINTER_PRESSED   2
// 
// #define RETRO_REGION_NTSC  0
// #define RETRO_REGION_PAL   1
// 
// enum retro_language
// {
//    RETRO_LANGUAGE_ENGLISH             =  0,
//    RETRO_LANGUAGE_JAPANESE            =  1,
//    RETRO_LANGUAGE_FRENCH              =  2,
//    RETRO_LANGUAGE_SPANISH             =  3,
//    RETRO_LANGUAGE_GERMAN              =  4,
//    RETRO_LANGUAGE_ITALIAN             =  5,
//    RETRO_LANGUAGE_DUTCH               =  6,
//    RETRO_LANGUAGE_PORTUGUESE          =  7,
//    RETRO_LANGUAGE_RUSSIAN             =  8,
//    RETRO_LANGUAGE_KOREAN              =  9,
//    RETRO_LANGUAGE_CHINESE_TRADITIONAL = 10,
//    RETRO_LANGUAGE_CHINESE_SIMPLIFIED  = 11,
//    RETRO_LANGUAGE_LAST,
// 
//    RETRO_LANGUAGE_DUMMY          = INT_MAX 
// };
// 
// #define RETRO_MEMORY_MASK        0xff
// #define RETRO_MEMORY_SAVE_RAM    0
// #define RETRO_MEMORY_RTC         1
// #define RETRO_MEMORY_SYSTEM_RAM  2
// #define RETRO_MEMORY_VIDEO_RAM   3
// 
// enum retro_key
// {
//    RETROK_UNKNOWN        = 0,
//    RETROK_FIRST          = 0,
//    RETROK_BACKSPACE      = 8,
//    RETROK_TAB            = 9,
//    RETROK_CLEAR          = 12,
//    RETROK_RETURN         = 13,
//    RETROK_PAUSE          = 19,
//    RETROK_ESCAPE         = 27,
//    RETROK_SPACE          = 32,
//    RETROK_EXCLAIM        = 33,
//    RETROK_QUOTEDBL       = 34,
//    RETROK_HASH           = 35,
//    RETROK_DOLLAR         = 36,
//    RETROK_AMPERSAND      = 38,
//    RETROK_QUOTE          = 39,
//    RETROK_LEFTPAREN      = 40,
//    RETROK_RIGHTPAREN     = 41,
//    RETROK_ASTERISK       = 42,
//    RETROK_PLUS           = 43,
//    RETROK_COMMA          = 44,
//    RETROK_MINUS          = 45,
//    RETROK_PERIOD         = 46,
//    RETROK_SLASH          = 47,
//    RETROK_0              = 48,
//    RETROK_1              = 49,
//    RETROK_2              = 50,
//    RETROK_3              = 51,
//    RETROK_4              = 52,
//    RETROK_5              = 53,
//    RETROK_6              = 54,
//    RETROK_7              = 55,
//    RETROK_8              = 56,
//    RETROK_9              = 57,
//    RETROK_COLON          = 58,
//    RETROK_SEMICOLON      = 59,
//    RETROK_LESS           = 60,
//    RETROK_EQUALS         = 61,
//    RETROK_GREATER        = 62,
//    RETROK_QUESTION       = 63,
//    RETROK_AT             = 64,
//    RETROK_LEFTBRACKET    = 91,
//    RETROK_BACKSLASH      = 92,
//    RETROK_RIGHTBRACKET   = 93,
//    RETROK_CARET          = 94,
//    RETROK_UNDERSCORE     = 95,
//    RETROK_BACKQUOTE      = 96,
//    RETROK_a              = 97,
//    RETROK_b              = 98,
//    RETROK_c              = 99,
//    RETROK_d              = 100,
//    RETROK_e              = 101,
//    RETROK_f              = 102,
//    RETROK_g              = 103,
//    RETROK_h              = 104,
//    RETROK_i              = 105,
//    RETROK_j              = 106,
//    RETROK_k              = 107,
//    RETROK_l              = 108,
//    RETROK_m              = 109,
//    RETROK_n              = 110,
//    RETROK_o              = 111,
//    RETROK_p              = 112,
//    RETROK_q              = 113,
//    RETROK_r              = 114,
//    RETROK_s              = 115,
//    RETROK_t              = 116,
//    RETROK_u              = 117,
//    RETROK_v              = 118,
//    RETROK_w              = 119,
//    RETROK_x              = 120,
//    RETROK_y              = 121,
//    RETROK_z              = 122,
//    RETROK_DELETE         = 127,
// 
//    RETROK_KP0            = 256,
//    RETROK_KP1            = 257,
//    RETROK_KP2            = 258,
//    RETROK_KP3            = 259,
//    RETROK_KP4            = 260,
//    RETROK_KP5            = 261,
//    RETROK_KP6            = 262,
//    RETROK_KP7            = 263,
//    RETROK_KP8            = 264,
//    RETROK_KP9            = 265,
//    RETROK_KP_PERIOD      = 266,
//    RETROK_KP_DIVIDE      = 267,
//    RETROK_KP_MULTIPLY    = 268,
//    RETROK_KP_MINUS       = 269,
//    RETROK_KP_PLUS        = 270,
//    RETROK_KP_ENTER       = 271,
//    RETROK_KP_EQUALS      = 272,
// 
//    RETROK_UP             = 273,
//    RETROK_DOWN           = 274,
//    RETROK_RIGHT          = 275,
//    RETROK_LEFT           = 276,
//    RETROK_INSERT         = 277,
//    RETROK_HOME           = 278,
//    RETROK_END            = 279,
//    RETROK_PAGEUP         = 280,
//    RETROK_PAGEDOWN       = 281,
// 
//    RETROK_F1             = 282,
//    RETROK_F2             = 283,
//    RETROK_F3             = 284,
//    RETROK_F4             = 285,
//    RETROK_F5             = 286,
//    RETROK_F6             = 287,
//    RETROK_F7             = 288,
//    RETROK_F8             = 289,
//    RETROK_F9             = 290,
//    RETROK_F10            = 291,
//    RETROK_F11            = 292,
//    RETROK_F12            = 293,
//    RETROK_F13            = 294,
//    RETROK_F14            = 295,
//    RETROK_F15            = 296,
// 
//    RETROK_NUMLOCK        = 300,
//    RETROK_CAPSLOCK       = 301,
//    RETROK_SCROLLOCK      = 302,
//    RETROK_RSHIFT         = 303,
//    RETROK_LSHIFT         = 304,
//    RETROK_RCTRL          = 305,
//    RETROK_LCTRL          = 306,
//    RETROK_RALT           = 307,
//    RETROK_LALT           = 308,
//    RETROK_RMETA          = 309,
//    RETROK_LMETA          = 310,
//    RETROK_LSUPER         = 311,
//    RETROK_RSUPER         = 312,
//    RETROK_MODE           = 313,
//    RETROK_COMPOSE        = 314,
// 
//    RETROK_HELP           = 315,
//    RETROK_PRINT          = 316,
//    RETROK_SYSREQ         = 317,
//    RETROK_BREAK          = 318,
//    RETROK_MENU           = 319,
//    RETROK_POWER          = 320,
//    RETROK_EURO           = 321,
//    RETROK_UNDO           = 322,
// 
//    RETROK_LAST,
// 
//    RETROK_DUMMY          = INT_MAX /* Ensure sizeof(enum) == sizeof(int) */
// };
// 
// enum retro_mod
// {
//    RETROKMOD_NONE       = 0x0000,
// 
//    RETROKMOD_SHIFT      = 0x01,
//    RETROKMOD_CTRL       = 0x02,
//    RETROKMOD_ALT        = 0x04,
//    RETROKMOD_META       = 0x08,
// 
//    RETROKMOD_NUMLOCK    = 0x10,
//    RETROKMOD_CAPSLOCK   = 0x20,
//    RETROKMOD_SCROLLOCK  = 0x40,
// 
//    RETROKMOD_DUMMY = INT_MAX /* Ensure sizeof(enum) == sizeof(int) */
// };
// 
// #define RETRO_ENVIRONMENT_EXPERIMENTAL 0x10000
// #define RETRO_ENVIRONMENT_PRIVATE 0x20000
// 
// #define RETRO_ENVIRONMENT_SET_ROTATION  1  /* const unsigned * --
// #define RETRO_ENVIRONMENT_GET_OVERSCAN  2  /* bool * --
// #define RETRO_ENVIRONMENT_GET_CAN_DUPE  3  /* bool * --
// #define RETRO_ENVIRONMENT_SET_MESSAGE   6  /* const struct retro_message * --
// #define RETRO_ENVIRONMENT_SHUTDOWN      7  /* N/A (NULL) --
// #define RETRO_ENVIRONMENT_SET_PERFORMANCE_LEVEL 8
//                                            /* const unsigned * --
// #define RETRO_ENVIRONMENT_GET_SYSTEM_DIRECTORY 9
//                                            /* const char ** --
// #define RETRO_ENVIRONMENT_SET_PIXEL_FORMAT 10
//                                            /* const enum retro_pixel_format * --
// #define RETRO_ENVIRONMENT_SET_INPUT_DESCRIPTORS 11
//                                            /* const struct retro_input_descriptor * --
// #define RETRO_ENVIRONMENT_SET_KEYBOARD_CALLBACK 12
//                                            /* const struct retro_keyboard_callback * --
// #define RETRO_ENVIRONMENT_SET_DISK_CONTROL_INTERFACE 13
//                                            /* const struct retro_disk_control_callback * --
// #define RETRO_ENVIRONMENT_SET_HW_RENDER 14
//                                            /* struct retro_hw_render_callback * --
// #define RETRO_ENVIRONMENT_GET_VARIABLE 15
//                                            /* struct retro_variable * --
// #define RETRO_ENVIRONMENT_SET_VARIABLES 16
//                                            /* const struct retro_variable * --
// #define RETRO_ENVIRONMENT_GET_VARIABLE_UPDATE 17
//                                            /* bool * --
// #define RETRO_ENVIRONMENT_SET_SUPPORT_NO_GAME 18
//                                            /* const bool * --
// #define RETRO_ENVIRONMENT_GET_LIBRETRO_PATH 19
//                                            /* const char ** --
// #define RETRO_ENVIRONMENT_SET_AUDIO_CALLBACK 22
//                                            /* const struct retro_audio_callback * --
// #define RETRO_ENVIRONMENT_SET_FRAME_TIME_CALLBACK 21
//                                            /* const struct retro_frame_time_callback * --
// #define RETRO_ENVIRONMENT_GET_RUMBLE_INTERFACE 23
//                                            /* struct retro_rumble_interface * --
// #define RETRO_ENVIRONMENT_GET_INPUT_DEVICE_CAPABILITIES 24
//                                            /* uint64_t * --
// #define RETRO_ENVIRONMENT_GET_SENSOR_INTERFACE (25 | RETRO_ENVIRONMENT_EXPERIMENTAL)
//                                            /* struct retro_sensor_interface * --
// #define RETRO_ENVIRONMENT_GET_CAMERA_INTERFACE (26 | RETRO_ENVIRONMENT_EXPERIMENTAL)
//                                            /* struct retro_camera_callback * --
// #define RETRO_ENVIRONMENT_GET_LOG_INTERFACE 27
//                                            /* struct retro_log_callback * --
// #define RETRO_ENVIRONMENT_GET_PERF_INTERFACE 28
//                                            /* struct retro_perf_callback * --
// #define RETRO_ENVIRONMENT_GET_LOCATION_INTERFACE 29
//                                            /* struct retro_location_callback * --
// #define RETRO_ENVIRONMENT_GET_CONTENT_DIRECTORY 30
//                                            /* const char ** --
// #define RETRO_ENVIRONMENT_GET_SAVE_DIRECTORY 31
//                                            /* const char ** --
// #define RETRO_ENVIRONMENT_SET_SYSTEM_AV_INFO 32
//                                            /* const struct retro_system_av_info * --
// #define RETRO_ENVIRONMENT_SET_PROC_ADDRESS_CALLBACK 33
//                                            /* const struct retro_get_proc_address_interface * --
// #define RETRO_ENVIRONMENT_SET_SUBSYSTEM_INFO 34
//                                            /* const struct retro_subsystem_info * --
// #define RETRO_ENVIRONMENT_SET_CONTROLLER_INFO 35
//                                            /* const struct retro_controller_info * --
// #define RETRO_ENVIRONMENT_SET_MEMORY_MAPS (36 | RETRO_ENVIRONMENT_EXPERIMENTAL)
//                                            /* const struct retro_memory_map * --
// #define RETRO_ENVIRONMENT_SET_GEOMETRY 37
//                                            /* const struct retro_game_geometry * --
// #define RETRO_ENVIRONMENT_GET_USERNAME 38 
//                                            /* const char **
// #define RETRO_ENVIRONMENT_GET_LANGUAGE 39
//                                            /* unsigned * --
// 
// #define RETRO_MEMDESC_CONST     (1 << 0)
// #define RETRO_MEMDESC_BIGENDIAN (1 << 1)
// #define RETRO_MEMDESC_ALIGN_2   (1 << 16)
// #define RETRO_MEMDESC_ALIGN_4   (2 << 16)
// #define RETRO_MEMDESC_ALIGN_8   (3 << 16)
// #define RETRO_MEMDESC_MINSIZE_2 (1 << 24)
// #define RETRO_MEMDESC_MINSIZE_4 (2 << 24)
// #define RETRO_MEMDESC_MINSIZE_8 (3 << 24)
// struct retro_memory_descriptor
// {
//    uint64_t flags;
//    void *ptr;
//    size_t offset;
//    size_t start;
//    size_t select;
//    size_t disconnect;
//    size_t len;
//    const char *addrspace;
// };
// 
// struct retro_memory_map
// {
//    const struct retro_memory_descriptor *descriptors;
//    unsigned num_descriptors;
// };
// 
// struct retro_controller_description
// {
//    const char *desc;
//    unsigned id;
// };
// 
// struct retro_controller_info
// {
//    const struct retro_controller_description *types;
//    unsigned num_types;
// };
// 
// struct retro_subsystem_memory_info
// {
//    const char *extension;
//    unsigned type;
// };
// 
// struct retro_subsystem_rom_info
// {
//    const char *desc;
//    const char *valid_extensions;
//    bool need_fullpath;
//    bool block_extract;
//    bool required;
//    const struct retro_subsystem_memory_info *memory;
//    unsigned num_memory;
// };
// 
// struct retro_subsystem_info
// {
//    const char *desc;
//    const char *ident;
//    const struct retro_subsystem_rom_info *roms;
//    unsigned num_roms;
//    unsigned id;
// };
// 
// typedef void (*retro_proc_address_t)(void);
// 
// typedef retro_proc_address_t (*retro_get_proc_address_t)(const char *sym);
// 
// struct retro_get_proc_address_interface
// {
//    retro_get_proc_address_t get_proc_address;
// };
// 
// enum retro_log_level
// {
//    RETRO_LOG_DEBUG = 0,
//    RETRO_LOG_INFO,
//    RETRO_LOG_WARN,
//    RETRO_LOG_ERROR,
// 
//    RETRO_LOG_DUMMY = INT_MAX
// };
// 
// typedef void (*retro_log_printf_t)(enum retro_log_level level,
//       const char *fmt, ...);
// 
// struct retro_log_callback
// {
//    retro_log_printf_t log;
// };
// 
// #define RETRO_SIMD_SSE      (1 << 0)
// #define RETRO_SIMD_SSE2     (1 << 1)
// #define RETRO_SIMD_VMX      (1 << 2)
// #define RETRO_SIMD_VMX128   (1 << 3)
// #define RETRO_SIMD_AVX      (1 << 4)
// #define RETRO_SIMD_NEON     (1 << 5)
// #define RETRO_SIMD_SSE3     (1 << 6)
// #define RETRO_SIMD_SSSE3    (1 << 7)
// #define RETRO_SIMD_MMX      (1 << 8)
// #define RETRO_SIMD_MMXEXT   (1 << 9)
// #define RETRO_SIMD_SSE4     (1 << 10)
// #define RETRO_SIMD_SSE42    (1 << 11)
// #define RETRO_SIMD_AVX2     (1 << 12)
// #define RETRO_SIMD_VFPU     (1 << 13)
// #define RETRO_SIMD_PS       (1 << 14)
// #define RETRO_SIMD_AES      (1 << 15)
// 
// typedef uint64_t retro_perf_tick_t;
// typedef int64_t retro_time_t;
// 
// struct retro_perf_counter
// {
//    const char *ident;
//    retro_perf_tick_t start;
//    retro_perf_tick_t total;
//    retro_perf_tick_t call_cnt;
//    bool registered;
// };
// 
// typedef retro_time_t (*retro_perf_get_time_usec_t)(void);
// 
// typedef retro_perf_tick_t (*retro_perf_get_counter_t)(void);
// 
// typedef uint64_t (*retro_get_cpu_features_t)(void);
// 
// typedef void (*retro_perf_log_t)(void);
// typedef void (*retro_perf_register_t)(struct retro_perf_counter *counter);
// typedef void (*retro_perf_start_t)(struct retro_perf_counter *counter);
// typedef void (*retro_perf_stop_t)(struct retro_perf_counter *counter);
// 
// struct retro_perf_callback
// {
//    retro_perf_get_time_usec_t    get_time_usec;
//    retro_get_cpu_features_t      get_cpu_features;
// 
//    retro_perf_get_counter_t      get_perf_counter;
//    retro_perf_register_t         perf_register;
//    retro_perf_start_t            perf_start;
//    retro_perf_stop_t             perf_stop;
//    retro_perf_log_t              perf_log;
// };
// 
// enum retro_sensor_action
// {
//    RETRO_SENSOR_ACCELEROMETER_ENABLE = 0,
//    RETRO_SENSOR_ACCELEROMETER_DISABLE,
// 
//    RETRO_SENSOR_DUMMY = INT_MAX
// };
// 
// #define RETRO_SENSOR_ACCELEROMETER_X 0
// #define RETRO_SENSOR_ACCELEROMETER_Y 1
// #define RETRO_SENSOR_ACCELEROMETER_Z 2
// 
// typedef bool (*retro_set_sensor_state_t)(unsigned port, 
//       enum retro_sensor_action action, unsigned rate);
// 
// typedef float (*retro_sensor_get_input_t)(unsigned port, unsigned id);
// 
// struct retro_sensor_interface
// {
//    retro_set_sensor_state_t set_sensor_state;
//    retro_sensor_get_input_t get_sensor_input;
// };
// 
// enum retro_camera_buffer
// {
//    RETRO_CAMERA_BUFFER_OPENGL_TEXTURE = 0,
//    RETRO_CAMERA_BUFFER_RAW_FRAMEBUFFER,
// 
//    RETRO_CAMERA_BUFFER_DUMMY = INT_MAX
// };
// 
// typedef bool (*retro_camera_start_t)(void);
// 
// typedef void (*retro_camera_stop_t)(void);
// 
// typedef void (*retro_camera_lifetime_status_t)(void);
// 
// typedef void (*retro_camera_frame_raw_framebuffer_t)(const uint32_t *buffer, 
//       unsigned width, unsigned height, size_t pitch);
// 
// typedef void (*retro_camera_frame_opengl_texture_t)(unsigned texture_id, 
//       unsigned texture_target, const float *affine);
// 
// struct retro_camera_callback
// {
//    uint64_t caps;
//    unsigned width;
//    unsigned height;
//    retro_camera_start_t start;
//    retro_camera_stop_t stop;
//    retro_camera_frame_raw_framebuffer_t frame_raw_framebuffer;
//    retro_camera_frame_opengl_texture_t frame_opengl_texture;
//    retro_camera_lifetime_status_t initialized;
//    retro_camera_lifetime_status_t deinitialized;
// };
// 
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
// 
// struct retro_location_callback
// {
//    retro_location_start_t         start;
//    retro_location_stop_t          stop;
//    retro_location_get_position_t  get_position;
//    retro_location_set_interval_t  set_interval;
// 
//    retro_location_lifetime_status_t initialized;
//    retro_location_lifetime_status_t deinitialized;
// };
// 
// enum retro_rumble_effect
// {
//    RETRO_RUMBLE_STRONG = 0,
//    RETRO_RUMBLE_WEAK = 1,
// 
//    RETRO_RUMBLE_DUMMY = INT_MAX
// };
// 
// typedef bool (*retro_set_rumble_state_t)(unsigned port, 
//       enum retro_rumble_effect effect, uint16_t strength);
// 
// struct retro_rumble_interface
// {
//    retro_set_rumble_state_t set_rumble_state;
// };
// 
// typedef void (*retro_audio_callback_t)(void);
// 
// typedef void (*retro_audio_set_state_callback_t)(bool enabled);
// 
// struct retro_audio_callback
// {
//    retro_audio_callback_t callback;
//    retro_audio_set_state_callback_t set_state;
// };
// 
// typedef int64_t retro_usec_t;
// typedef void (*retro_frame_time_callback_t)(retro_usec_t usec);
// struct retro_frame_time_callback
// {
//    retro_frame_time_callback_t callback;
//    retro_usec_t reference;
// };
// 
// #define RETRO_HW_FRAME_BUFFER_VALID ((void*)-1)
// 
// typedef void (*retro_hw_context_reset_t)(void);
// 
// typedef uintptr_t (*retro_hw_get_current_framebuffer_t)(void);
// 
// typedef retro_proc_address_t (*retro_hw_get_proc_address_t)(const char *sym);
// 
// enum retro_hw_context_type
// {
//    RETRO_HW_CONTEXT_NONE             = 0,
//    RETRO_HW_CONTEXT_OPENGL           = 1, 
//    RETRO_HW_CONTEXT_OPENGLES2        = 2,
//    RETRO_HW_CONTEXT_OPENGL_CORE      = 3,
//    RETRO_HW_CONTEXT_OPENGLES3        = 4,
//    RETRO_HW_CONTEXT_OPENGLES_VERSION = 5,
//    RETRO_HW_CONTEXT_DUMMY = INT_MAX
// };
// 
// struct retro_hw_render_callback
// {
//    enum retro_hw_context_type context_type;
//    retro_hw_context_reset_t context_reset;
//    retro_hw_get_current_framebuffer_t get_current_framebuffer;
//    retro_hw_get_proc_address_t get_proc_address;
//    bool depth;
//    bool stencil;
//    bool bottom_left_origin;
//    unsigned version_major;
//    unsigned version_minor;
//    bool cache_context;
//    retro_hw_context_reset_t context_destroy;
//    bool debug_context;
// };
// 
// typedef void (*retro_keyboard_event_t)(bool down, unsigned keycode, 
//       uint32_t character, uint16_t key_modifiers);
// 
// struct retro_keyboard_callback
// {
//    retro_keyboard_event_t callback;
// };
// 
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
// 
// struct retro_disk_control_callback
// {
//    retro_set_eject_state_t set_eject_state;
//    retro_get_eject_state_t get_eject_state;
// 
//    retro_get_image_index_t get_image_index;
//    retro_set_image_index_t set_image_index;
//    retro_get_num_images_t  get_num_images;
// 
//    retro_replace_image_index_t replace_image_index;
//    retro_add_image_index_t add_image_index;
// };
// 
// enum retro_pixel_format
// {
//    RETRO_PIXEL_FORMAT_0RGB1555 = 0,
//    RETRO_PIXEL_FORMAT_XRGB8888 = 1,
//    RETRO_PIXEL_FORMAT_RGB565   = 2,
//    RETRO_PIXEL_FORMAT_UNKNOWN  = INT_MAX
// };
// 
// struct retro_message
// {
//    const char *msg;
//    unsigned    frames;
// };
// 
// struct retro_input_descriptor
// {
//    unsigned port;
//    unsigned device;
//    unsigned index;
//    unsigned id;
//    const char *description; 
// };

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	//should be Utf8, but that doesn't exist. https://github.com/dotnet/coreclr/issues/1012
	//and even if it did exist, this module is designed to be compatible with .NET 2.0
	public struct retro_system_info {
		public string library_name;
		public string library_version;
		public string valid_extensions;
		[MarshalAs(UnmanagedType.I1)] public bool need_fullpath;
		[MarshalAs(UnmanagedType.I1)] public bool block_extract;
	};

// struct retro_game_geometry
// {
//    unsigned base_width;
//    unsigned base_height;
//    unsigned max_width;
//    unsigned max_height;
// 
//    float aspect_ratio;
// };
// 
// struct retro_system_timing
// {
//    double fps;
//    double sample_rate;
// };
// 
// struct retro_system_av_info
// {
//    struct retro_game_geometry geometry;
//    struct retro_system_timing timing;
// };
// 
// struct retro_variable
// {
//    const char *key;
//    const char *value;
// };
// 
// struct retro_game_info
// {
//    const char *path;
//    const void *data;
//    size_t      size;
//    const char *meta;
// };
// 
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate bool retro_environment_t(uint cmd, IntPtr data);
// 
// typedef void (*retro_video_refresh_t)(const void *data, unsigned width,
//       unsigned height, size_t pitch);
// 
// typedef void (*retro_audio_sample_t)(int16_t left, int16_t right);
// 
// typedef size_t (*retro_audio_sample_batch_t)(const int16_t *data,
//       size_t frames);
// 
// typedef void (*retro_input_poll_t)(void);
// 
// typedef int16_t (*retro_input_state_t)(unsigned port, unsigned device, 
//       unsigned index, unsigned id);
// 
	[DllImport("snes9x_libretro.dll")]
	public static extern void retro_set_environment([MarshalAs(UnmanagedType.FunctionPtr)]retro_environment_t env);
// void retro_set_video_refresh(retro_video_refresh_t);
// void retro_set_audio_sample(retro_audio_sample_t);
// void retro_set_audio_sample_batch(retro_audio_sample_batch_t);
// void retro_set_input_poll(retro_input_poll_t);
// void retro_set_input_state(retro_input_state_t);
// 
	[DllImport("snes9x_libretro.dll")]
	public static extern void retro_init();
	[DllImport("snes9x_libretro.dll")]
	public static extern void retro_deinit();
	
	[DllImport("snes9x_libretro.dll")]
	public static extern uint retro_api_version();
	
	[DllImport("snes9x_libretro.dll")]
	public static extern void retro_get_system_info(out retro_system_info info);

// void retro_get_system_av_info(struct retro_system_av_info *info);
// 
// void retro_set_controller_port_device(unsigned port, unsigned device);
// 
// void retro_reset(void);
// 
// void retro_run(void);
// 
// size_t retro_serialize_size(void);
// 
// bool retro_serialize(void *data, size_t size);
// bool retro_unserialize(const void *data, size_t size);
// 
// void retro_cheat_reset(void);
// void retro_cheat_set(unsigned index, bool enabled, const char *code);
// 
// bool retro_load_game(const struct retro_game_info *game);
// 
// bool retro_load_game_special(
//   unsigned game_type,
//   const struct retro_game_info *info, size_t num_info
// );
// 
// void retro_unload_game(void);
// 
// unsigned retro_get_region(void);
// 
// void *retro_get_memory_data(unsigned id);
// size_t retro_get_memory_size(unsigned id);
}

class TinyGUI
{
	static void Main()
	{
		System.Console.WriteLine(LibretroRaw.retro_api_version());
		LibretroRaw.retro_system_info info;
		LibretroRaw.retro_get_system_info(out info);
		System.Console.WriteLine(info.library_name);
		
		LibretroRaw.retro_set_environment(env);
	}
	
	static bool env(uint cmd, IntPtr data)
	{
		System.Console.WriteLine("Env "+cmd);
		return false;
	}
}
