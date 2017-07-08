using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Warcraft.App;
using Warcraft.Assets;
using Warcraft.Screens.Base;
using Warcraft.Screens.Manager;

namespace Warcraft.Screens
{
	public class FpsOverlay : GameOverlay
	{
		private long TotalFrames { get; set; }
		private double TotalSeconds { get; set; }
		private double AverageFramesPerSecond { get; set; }
		private double CurrentFramesPerSecond { get; set; }

		private const int MaximumSamples = 100;

		private readonly Queue<double> sampleBuffer = new Queue<double>();

		public void Update(double deltaTime)
		{
			CurrentFramesPerSecond = 1.0f / deltaTime;

			sampleBuffer.Enqueue(CurrentFramesPerSecond);

			if (sampleBuffer.Count > MaximumSamples)
			{
				sampleBuffer.Dequeue();
				AverageFramesPerSecond = sampleBuffer.Average(i => i);
			}
			else
			{
				AverageFramesPerSecond = CurrentFramesPerSecond;
			}

			TotalFrames++;
			TotalSeconds += deltaTime;
		}

		public override void Draw(GameTime gameTime)
		{
			Update(gameTime.ElapsedGameTime.TotalSeconds);

			var fps = $"FPS: {AverageFramesPerSecond:0}";

			var textDimensions = Data.Fonts[FontSize.Large].MeasureText(fps);
			Data.Fonts[FontSize.Large].DrawTextColored(0, 0, ScreenManager.Graphics.Viewport.Height - textDimensions.Height, fps);
			Data.Fonts[FontSize.Large].DrawTextColored(1, 0 + 1, ScreenManager.Graphics.Viewport.Height - textDimensions.Height + 1, fps);
		}
	}
}