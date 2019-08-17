using System;

namespace BurningDownTheHouse
{
	public static class Extensions
	{
		public static ulong ToHex(this string s) => Convert.ToUInt64(s, 16);
	}
}
