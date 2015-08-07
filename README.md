libretro-pan
============

The libretro-pan project aims to ensure libretro frontends exist in all languages anyone cares about.

A language is considered covered once a tool written in this language can
- Load Super Mario World with an unmodified(\*) Snes9x core
- Send both audio and video to the user, at approximately 60fps (slight stuttering is fine)
- Take any form of input (probably keyboard)

Hardcoding any Snes9x-related parameters is fine. All dependencies are allowed, in all languages; however, all code written for libretro-pan (except build scripts) must be in the noted language.

If a language is covered by an external project, libretro-pan has no interest in making another one. Our goal is their existence, not being the owner.

(\*) Renaming the core is allowed, if the chosen FFI system demands a 'lib' prefix or similar.

| Language | Status |
| ---- | ---- |
| C | External: Multiple; [nanoarch](https://github.com/heuripedes/nanoarch/) is the best example of how to make more |
| C++ | External: Multiple; [Pantheon](https://github.com/Druage/Pantheon) and [minir](https://github.com/Alcaro/minir), for example |
| C# | In progress |
| Python 3 | Halted because Linux Mint 17's pygame does not support Python 3 |
| Python 2 | Halted because pygame does not support vsync nor streaming audio (seemingly poor CFFI integration, too) |
| Objective-C | Not started |
| Visual Basic .NET | Not started |
| Assembly (any) | Not started |
| Rust | FFI capabilities being researched |
| PHP | FFI capabilities being researched |
| Perl | FFI capabilities being researched |
| Java | FFI capabilities being researched |
| JavaScript | FFI capabilities being researched |
| Ruby | FFI capabilities being researched |
| Unix shell (any) | FFI capabilities being researched |
| D | No interest shown yet |
| Dart | No interest shown yet |
| Pascal | No interest shown yet |
| Go | No interest shown yet |
| Others | [Suggest 'em!](https://github.com/Alcaro/libretro-pan/issues) |
