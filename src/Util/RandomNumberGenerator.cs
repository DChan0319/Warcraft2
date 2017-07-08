namespace Warcraft.Util
{
	/// <summary>
	/// Very-Pseudo-Random Number Generator
	/// </summary>
	public class RandomNumberGenerator
	{
		protected uint RandomSeedHigh { get; set; }
		protected uint RandomSeedLow { get; set; }

		public RandomNumberGenerator()
		{
			RandomSeedHigh = 0x01234567;
			RandomSeedLow = 0x89ABCDEF;
		}

		public void Seed(ulong seed)
		{
			Seed((uint)(seed > 32 ? 1 : 0), (uint)seed);
		}

		public void Seed(uint high, uint low)
		{
			if (high != low && high != 0 && low != 0)
			{
				RandomSeedHigh = high;
				RandomSeedLow = low;
			}
		}

		public uint Random()
		{
			RandomSeedHigh = 36969 * (RandomSeedHigh & 65535) + (RandomSeedHigh >> 16);
			RandomSeedLow = 18000 * (RandomSeedLow & 65535) + (RandomSeedLow >> 16);
			return (RandomSeedHigh << 16) + RandomSeedLow;
		}
	}
}