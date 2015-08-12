using Microsoft.DirectX.DirectDraw;

using System;
using System.Runtime.InteropServices;

using System.Windows.Forms;

using System.Drawing;
using System.Drawing.Imaging;

class TinyGUI
{
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
		
		System.Console.WriteLine(av.geometry.base_width);
		System.Console.WriteLine(av.geometry.base_height);
		
		Form form = new Form();
		form.ClientSize = new Size((int)av.geometry.base_width, (int)av.geometry.base_height);
		draw = new TinyDDraw(form, av.geometry.base_width, av.geometry.base_height, av.pixfmt);
		form.Show();
		
		for (int i=0;i<6000;i++)
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
	
	Libretro.pixel_format pixfmt;
	
	public TinyDDraw(Form parent, uint width, uint height, Libretro.pixel_format pixfmt)
	{
		dev = new Device();
		dev.SetCooperativeLevel(parent, CooperativeLevelFlags.Normal);
		
		SurfaceDescription desc = new SurfaceDescription();
		desc.SurfaceCaps.PrimarySurface = true;
		surf_front = new Surface(desc, dev);
		
		desc.Clear();
		desc.Width = (int)width;
		desc.Height = (int)height;
		desc.SurfaceCaps.OffScreenPlain = true;
		surf_back = new Surface(desc, dev);
		
		Clipper clip = new Clipper(dev);
		clip.Window = parent;
		surf_front.Clipper = clip;
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
		try
		{
			LockedData target = surf_back.Lock(LockFlags.WriteOnly);
System.Console.WriteLine(target.Pitch);
			uint rowwidth = (pixfmt == Libretro.pixel_format.XRGB8888 ? 4u : 2u) * width;
			video_copy(target.Data.InternalData, (uint)target.Pitch, data, pitch, rowwidth, height);
			surf_back.Unlock();
			
			surf_front.Draw(surf_back, DrawFlags.Wait);
		}
		catch(WasStillDrawingException)
		{
			return;
		}
		catch(SurfaceLostException)
		{
			//display.RestoreAllSurfaces();
		}
	}
}
