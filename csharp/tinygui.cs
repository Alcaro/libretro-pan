using Microsoft.DirectX.DirectDraw;

using System;
using System.Runtime.InteropServices;

using System.Windows.Forms;

using System.Drawing;
using System.Drawing.Imaging;

class TinyGUI
{
	Libretro.pixel_format pixfmt;
	
	Bitmap bmp;
	System.Drawing.Imaging.PixelFormat[] bmp_formats = {
		System.Drawing.Imaging.PixelFormat.Format16bppRgb555, // ew, too long.
		System.Drawing.Imaging.PixelFormat.Format32bppRgb,
		System.Drawing.Imaging.PixelFormat.Format16bppRgb565
	};
	
	TinyGUI()
	{
		Libretro core = new Libretro("snes9x_libretro.dll");
		//Libretro core = new Libretro("minir_testcore_libretro.dll");
		core.log_cb = log;
		unsafe { core.video_cb = video; } // why is that needed, it's declared unsafe on both sides
		unsafe { core.audio_batch_cb = audio; }
		core.input_state_cb = input;
		core.init();
		
		core.load_game(new Libretro.game_info("smw.sfc"));
		
		Libretro.system_av_info av = core.get_system_av_info();
		this.pixfmt = av.pixfmt;
		
		System.Console.WriteLine(av.geometry.base_width);
		System.Console.WriteLine(av.geometry.base_height);
		
		Form form = new Form();
		PictureBox pic = new PictureBox();
		bmp = new Bitmap((int)av.geometry.base_width, (int)av.geometry.base_height, bmp_formats[(int)av.pixfmt]);
		pic.Image = bmp;
		form.Controls.Add(pic);
		form.Show();
		
		for (int i=0;i<1200;i++) core.run();
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
	
	void video(IntPtr data, uint width, uint height, uint pitch)
	{
		BitmapData bmpdat = bmp.LockBits(new Rectangle(0, 0, (int)width, (int)height), ImageLockMode.WriteOnly, bmp_formats[(int)pixfmt]);
		video_copy(bmpdat.Scan0, (uint)bmpdat.Stride, data, pitch, (pixfmt == Libretro.pixel_format.XRGB8888 ? 4u : 2u) * width, height);
		bmp.UnlockBits(bmpdat);
		this.Invalidate();
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
