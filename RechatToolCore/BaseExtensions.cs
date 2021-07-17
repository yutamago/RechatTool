using System;

namespace RechatToolCore
{
	public static class Extensions
	{
		public static string NullIfEmpty(this string s)
		{
			return string.IsNullOrEmpty(s) ? null : s;
		}

		public static int? TryParseInt32(this string s)
		{
			return int.TryParse(s, out int n) ? n : (int?) null;
		}

		public static long? TryParseInt64(this string s)
		{
			return long.TryParse(s, out long n) ? n : (long?) null;
		}

		public static string ToDisplayString(this Version v)
		{
			return $"{v.Major}.{v.Minor}.{v.Revision}";
		}
	}
}