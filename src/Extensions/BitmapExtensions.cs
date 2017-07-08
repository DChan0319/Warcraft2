using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Warcraft.Screens.Manager;

namespace Warcraft.Extensions
{
	/// <summary>
	/// Static class for <see cref="Bitmap"/> extensions.
	/// </summary>
	public static class BitmapExtensions
	{
		/// <summary>
		/// Converts <paramref name="bitmap"/> into a <see cref="Texture2D"/>.
		/// </summary>
		/// <param name="bitmap">The <see cref="Bitmap"/> to convert.</param>
		/// <returns>A <see cref="Texture2D"/> of the <paramref name="bitmap"/>.</returns>
		public static Texture2D ToTexture2D(this Bitmap bitmap)
		{
			var bufferSize = bitmap.Height * bitmap.Width * 4;

			using (var memoryStream = new MemoryStream(bufferSize))
			{
				bitmap.Save(memoryStream, ImageFormat.Png);
				return Texture2D.FromStream(ScreenManager.Graphics, memoryStream);
			}
		}

		/// <summary>
		/// Clones the area defined on <paramref name="area"/> from <paramref name="bitmap"/>
		/// and returns it as a new <see cref="Bitmap"/> object.
		/// </summary>
		/// <remarks>
		/// Caution: Uses unsafe code!
		/// Much faster than using Bitmap.Clone().
		/// Adapted from http://stackoverflow.com/a/1563170.
		/// </remarks>
		public static unsafe Bitmap QuickClone(this Bitmap bitmap, Rectangle area)
		{
			var bitmapData = bitmap.LockBits(area, ImageLockMode.ReadOnly, bitmap.PixelFormat);
			var bytesPerPixel = Image.GetPixelFormatSize(bitmapData.PixelFormat) / 8;

			var clone = new Bitmap(area.Width, area.Height, bitmap.PixelFormat);
			var cloneData = clone.LockBits(new Rectangle(0, 0, area.Width, area.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

			var scan0 = (byte*)bitmapData.Scan0.ToPointer();
			var cloneScan0 = (byte*)cloneData.Scan0.ToPointer();

			for (var i = 0; i < bitmapData.Height; ++i)
			{
				for (var j = 0; j < bitmapData.Width; ++j)
				{
					var bitmapPtr = scan0 + i * bitmapData.Stride + j * bytesPerPixel;
					var clonePtr = cloneScan0 + i * cloneData.Stride + j * bytesPerPixel;

					for (var k = 0; k < bytesPerPixel; k++)
						clonePtr[k] = bitmapPtr[k];
				}
			}

			bitmap.UnlockBits(bitmapData);
			clone.UnlockBits(cloneData);

			return clone;
		}
	}
}
