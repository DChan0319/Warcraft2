using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Warcraft.App;

namespace Warcraft.Player
{
	[TypeConverter(typeof(PositionConverter))]
	public struct Position
	{
		private static Direction[,] octant;

		private static readonly Direction[,] TileDirections = {
			{ Direction.NorthWest, Direction.North, Direction.NorthEast },
			{ Direction.West, Direction.Max, Direction.East },
			{ Direction.SouthWest, Direction.South, Direction.SouthEast }
		};

		public int X { get; set; }
		public int Y { get; set; }

		public static int TileWidth { get; private set; } = 1;
		public static int TileHeight { get; private set; } = 1;
		public static int HalfTileWidth { get; private set; } = 0;
		public static int HalfTileHeight { get; private set; } = 0;

		/// <summary>
		/// Returns whether the position is at the center of a tile.
		/// </summary>
		[JsonIgnore]
		public bool IsTileAligned
		{
			get { return X % TileWidth == HalfTileWidth && Y % TileHeight == HalfTileHeight; }
		}

		public Position(int x, int y)
		{
			X = x;
			Y = y;
		}

		public Position(Point other)
		{
			X = other.X;
			Y = other.Y;
		}

		public Position(Position other)
		{
			X = other.X;
			Y = other.Y;
		}

		public Point ToPoint()
		{
			return new Point(X, Y);
		}

		public void SetFromTile(Position pos)
		{
			SetXFromTile(pos.X);
			SetYFromTile(pos.Y);
		}

		public void SetXFromTile(int x)
		{
			X = x * TileWidth + HalfTileWidth;
		}

		public void SetYFromTile(int y)
		{
			Y = y * TileHeight + HalfTileHeight;
		}

		public void SetToTile(Position pos)
		{
			SetXToTile(pos.X);
			SetYToTile(pos.Y);
		}

		public void SetXToTile(int x)
		{
			X = x / TileWidth;
		}

		public void SetYToTile(int y)
		{
			Y = y / TileHeight;
		}

		public Direction TileOctant()
		{
			return octant[Y % TileHeight, X % TileWidth];
		}

		public static void SetTileDimensions(int width, int height)
		{
			if (width > 0 && height > 0)
			{
				TileWidth = width;
				TileHeight = height;
				HalfTileWidth = width / 2;
				HalfTileHeight = height / 2;

				octant = new Direction[TileHeight, TileWidth];
				for (var y = 0; y < TileHeight; y++)
				{
					for (var x = 0; x < TileWidth; x++)
					{
						var xDistance = x - HalfTileWidth;
						var yDistance = y - HalfTileHeight;
						var negativeX = xDistance < 0;
						var negativeY = yDistance > 0;

						xDistance *= xDistance;
						yDistance *= yDistance;

						if (xDistance + yDistance == 0)
						{
							octant[y, x] = Direction.Max;
							continue;
						}

						var sineSquared = (double)yDistance / (xDistance + yDistance);

						if (sineSquared < 0.1464466094)
						{
							if (negativeX)
								octant[y, x] = Direction.West;
							else
								octant[y, x] = Direction.East;
						}
						else if (sineSquared < 0.85355339059)
						{
							if (negativeY)
							{
								if (negativeX)
									octant[y, x] = Direction.SouthWest;
								else
									octant[y, x] = Direction.SouthEast;
							}
							else
							{
								if (negativeX)
									octant[y, x] = Direction.NorthWest;
								else
									octant[y, x] = Direction.NorthEast;
							}
						}
						else
						{
							if (negativeY)
								octant[y, x] = Direction.South;
							else
								octant[y, x] = Direction.North;
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets the position closest to <paramref name="objectPos"/>.
		/// </summary>
		public Position ClosestPosition(Position objectPos, int objectSize)
		{
			var currentPosition = new Position(objectPos);
			var bestPosition = new Position();
			var bestDistance = -1;

			for (var y = 0; y < objectSize; y++)
			{
				for (var x = 0; x < objectSize; x++)
				{
					var currentDistance = currentPosition.DistanceSquared(this);

					if (bestDistance == -1 || currentDistance < bestDistance)
					{
						bestDistance = currentDistance;
						bestPosition = currentPosition;
					}

					currentPosition.X += TileWidth;
				}

				currentPosition.X = objectPos.X;
				currentPosition.Y += TileHeight;
			}

			return bestPosition;
		}

		/// <summary>
		/// Returns the distance between this point and <paramref name="pos"/>.
		/// </summary>
		/// <remarks>
		/// Adapted from http://www.codecodex.com/wiki/Calculate_an_integer_square_root#C.23.
		/// </remarks>
		public int Distance(Position pos)
		{
			var num = DistanceSquared(pos);

			if (0 == num)
				return 0;

			var n = num / 2 + 1;
			var n1 = (n + num / n) / 2;

			while (n1 < n)
			{
				n = n1;
				n1 = (n + num / n) / 2;
			}

			return n;
		}

		/// <summary>
		/// Returns the distance squared between this point and <paramref name="pos"/>.
		/// </summary>
		public int DistanceSquared(Position pos)
		{
			int deltaX = X - pos.X, deltaY = Y - pos.Y;
			return deltaX * deltaX + deltaY * deltaY;
		}

		/// <summary>
		/// Returns the tile direction adjacent to <paramref name="pos"/>.
		/// </summary>
		public Direction AdjacentTileDirection(Position pos, int objectSize = 1)
		{
			if (objectSize == 1)
			{
				int deltaX = pos.X - X, deltaY = pos.Y - Y;

				if (deltaX * deltaX > 1 || deltaY * deltaY > 1)
					return Direction.Max;

				return TileDirections[deltaY + 1, deltaX + 1];
			}

			Position thisPosition = new Position(), targetPosition = new Position();
			thisPosition.SetFromTile(this);
			targetPosition.SetFromTile(pos);

			targetPosition.SetToTile(thisPosition.ClosestPosition(targetPosition, objectSize));
			return AdjacentTileDirection(targetPosition);
		}

		/// <summary>
		/// Returns the direction facing <paramref name="pos"/>.
		/// </summary>
		public Direction DirectionTo(Position pos)
		{
			var deltaPosition = new Position(pos.X - X, pos.Y - Y);
			var divX = deltaPosition.X / HalfTileWidth;
			var divY = deltaPosition.Y / HalfTileHeight;

			divX = Math.Abs(divX);
			divY = Math.Abs(divY);
			var div = MathHelper.Max(divX, divY);

			if (div != 0)
			{
				deltaPosition.X /= div;
				deltaPosition.Y /= div;
			}

			deltaPosition.X += HalfTileWidth;
			deltaPosition.Y += HalfTileHeight;

			deltaPosition.X = MathHelper.Clamp(deltaPosition.X, 0, TileWidth - 1);
			deltaPosition.Y = MathHelper.Clamp(deltaPosition.Y, 0, TileWidth - 1);

			return deltaPosition.TileOctant();
		}

		public static bool operator ==(Position a, Position b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Position a, Position b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj == null) return false;
			if (obj.GetType() != typeof(Position)) return false;

			return Equals((Position)obj);
		}

		public bool Equals(Position other)
		{
			return X == other.X && Y == other.Y;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (X * 397) ^ Y;
			}
		}
	}

	/// <summary>
	/// Converts an object to/from a <see cref="Position"/>.
	/// </summary>
	public class PositionConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (!(value is string)) return base.ConvertFrom(context, culture, value);

			var v = ((string)value).Split(',');
			return new Position(int.Parse(v[0]), int.Parse(v[1]));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType != typeof(string))
				return base.ConvertTo(context, culture, value, destinationType);

			var position = (Position)value;
			return position.X + "," + position.Y;
		}
	}
}