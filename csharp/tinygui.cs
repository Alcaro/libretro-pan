using Microsoft.DirectX.DirectDraw;

using System;
using System.Runtime.InteropServices;

using System.Windows.Forms;

using System.Drawing;
using System.Drawing.Imaging;

class TinyGUI
{
	Libretro.pixel_format pixfmt;
	
	TinyDDraw draw;
	
	TinyGUI()
	{
		Libretro core = new Libretro("snes9x_libretro.dll");
		//Libretro core = new Libretro("minir_testcore_libretro.dll");
		
		core.log_cb = log;
		core.video_cb = video;
		core.audio_batch_cb = audio;
		core.input_state_cb = input;
		core.init();
		
		core.load_game(new Libretro.game_info("smw.sfc"));
		
		Libretro.system_av_info av = core.get_system_av_info();
		this.pixfmt = av.pixfmt;
		
		System.Console.WriteLine(av.geometry.base_width);
		System.Console.WriteLine(av.geometry.base_height);
		
		Form form = new Form();
		form.Size = new Size((int)av.geometry.base_width, (int)av.geometry.base_height);
		draw = new TinyDDraw(form);
		form.Show();
		
		for (int i=0;i<60;i++)
		{
			core.run();
			Application.DoEvents();
		}
	}
	
	void video(IntPtr data, uint width, uint height, uint pitch)
	{
		draw.video(data, width, height, pitch);
	}
	
	void audio(IntPtr data, ulong frames)
	{
		
	}
	
	short input(uint port, uint device, uint index, uint id)
	{
		return 0;
	}
	
	void log(Libretro.LogLevel level, string text)
	{
		System.Console.WriteLine("Log ("+level+"): "+text);
	}
	
	static void Main()
	{
		new TinyGUI();
	}
}

class TinyDDraw
{
	Device dev;
	Surface surf_front;
	Surface surf_back;
	
	public TinyDDraw(Form parent)
	{
		dev = new Device();
		dev.SetCooperativeLevel(parent, CooperativeLevelFlags.Normal);
		
		SurfaceDescription desc = new SurfaceDescription();
		desc.SurfaceCaps.PrimarySurface = true;
		surf_front = new Surface(desc, dev);
		
		desc.Clear();
		desc.Width = surf_front.SurfaceDescription.Width;
		desc.Height = surf_front.SurfaceDescription.Height;
System.Console.WriteLine("XXY");
System.Console.WriteLine(desc.Width);
System.Console.WriteLine(desc.Height);
		desc.SurfaceCaps.OffScreenPlain = true;

System.Console.WriteLine("A");
		surf_back = new Surface(desc, dev);
System.Console.WriteLine("b");
		
		Clipper clip = new Clipper(dev);
System.Console.WriteLine("c");
		clip.Window = parent;
System.Console.WriteLine("d");
		surf_front.Clipper = clip;
System.Console.WriteLine("e");
	}
	
	[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);
	
	void video_copy(IntPtr dst, uint dstpitch, IntPtr src, uint srcpitch, uint rowlen, uint height)
	{
		ulong dst_i = (ulong)dst.ToInt64();
		ulong src_i = (ulong)src.ToInt64();
		for (uint i=0;i<height;i++)
		{
			memcpy(new IntPtr((long)(dst_i + dstpitch*i)), new IntPtr((long)(src_i + srcpitch*i)), new UIntPtr(rowlen));
		}
	}
	
	public void video(IntPtr data, uint width, uint height, uint pitch)
	{
		//BitmapData bmpdat = bmp.LockBits(new Rectangle(0, 0, (int)width, (int)height), ImageLockMode.WriteOnly, bmp_formats[(int)pixfmt]);
		//video_copy(bmpdat.Scan0, (uint)bmpdat.Stride, data, pitch, (pixfmt == Libretro.pixel_format.XRGB8888 ? 4u : 2u) * width, height);
		//bmp.UnlockBits(bmpdat);
	}
}
