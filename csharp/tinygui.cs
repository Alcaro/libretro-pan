class TinyGUI
{
	static void Main()
	{
		//Libretro core = new Libretro("snes9x_libretro.dll");
		Libretro core = new Libretro("minir_testcore_libretro.dll");
		core.log_cb = log;
		core.init();
	}
	
	static void log(Libretro.LogLevel level, string text)
	{
		System.Console.WriteLine("Log: "+text);
	}
}
