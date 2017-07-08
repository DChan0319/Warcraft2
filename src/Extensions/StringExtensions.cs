using System.Text;

namespace Warcraft.Extensions
{
	/// <summary>
	/// Static class for <see cref="string"/> extensions.
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Returns a string with a space in front of every upper case character
		/// in the string, excluding the first.
		/// </summary>
		/// <param name="text">The string to add spaces to</param>
		public static string AddSpaces(this string text)
		{
			var result = new StringBuilder();

			foreach (var c in text)
			{
				if (result.Length != 0 && char.IsUpper(c))
					result.Append(' ');
				result.Append(c);
			}

			return result.ToString();
		}
	}
}