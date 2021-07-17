using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RechatToolCore
{
	internal static class Program
	{
		private const string Version = "2.0.0.0";

		private static int Main(string[] args)
		{
			
			try
			{
				var currentArgIndex = 0;
				var currentArg = GetArg(ref currentArgIndex, args);

				switch (currentArg)
				{
					case "-h":
					case "-H":
					case "-?":
					case "/?":
					case "help":
					{
						PrintHelp();
						break;
					}
					case "-d":
					case "-D":
					{
						ChooseDownload(currentArg, ref currentArgIndex, args);
						break;
					}
					default:
						throw new InvalidArgumentException();
				}

				return 0;
			}
			catch (InvalidArgumentException)
			{
				PrintHelp();
				return 1;
			}
			catch (Exception ex)
			{
				Console.WriteLine("\rError: " + ex.Message);
				return 1;
			}
		}

		private static string GetArg(ref int iArg, IReadOnlyList<string> args, bool optional = false)
		{
			if (iArg < args.Count) return args[iArg++];
			if (optional) return null;

			throw new InvalidArgumentException();
		}

		private static string PeekArg(int iArg, IReadOnlyList<string> args)
		{
			return iArg < args.Count ? args[iArg] : null;
		}

		private static void ChooseDownload(string arg, ref int iArg, IReadOnlyList<string> args)
		{
			var videoIdStr = GetArg(ref iArg, args);
			var videoId = videoIdStr.TryParseInt64() ??
			              TryParseVideoIdFromUrl(videoIdStr) ??
			              throw new InvalidArgumentException();
			var path = PeekArg(iArg, args)?.StartsWith("-", StringComparison.Ordinal) == false
				? GetArg(ref iArg, args)
				: $"{videoId}.json";
			var overwrite = false;
			while ((arg = GetArg(ref iArg, args, true)) != null)
			{
				if (arg == "-o")
				{
					overwrite = true;
				}
				else
				{
					throw new InvalidArgumentException();
				}
			}

			try
			{
				Rechat.DownloadFile(videoId, path, overwrite, UpdateProgress);
				Console.WriteLine();
			}
			catch (WarningException ex)
			{
				Console.WriteLine();
				Console.WriteLine($"Warning: {ex.Message}");
			}

			Console.WriteLine("Done!");
		}

		private static void PrintHelp()
		{
			Console.WriteLine($"RechatTool v{new Version(Version).ToDisplayString()}");
			Console.WriteLine();
			Console.WriteLine("Modes:");
			Console.WriteLine("   -d videoid [filename] [-o]");
			Console.WriteLine("      Downloads chat replay for the specified videoid.");
			Console.WriteLine("        filename: Output location as relative or absolute filename, otherwise");
			Console.WriteLine("          defaults to the current directory and named as videoid.json.");
			Console.WriteLine("        -o: Overwrite the existing output file.");
			Console.WriteLine("   -D (same parameters as -d)");
			Console.WriteLine("      Downloads and processes chat replay (combines -d and -p).");
			Console.WriteLine("   -p filename [output_filename] [-o] [-b]");
			Console.WriteLine("      Processes a JSON chat replay file and outputs a human-readable text file.");
			Console.WriteLine("        output_filename: Output location as relative or absolute filename,");
			Console.WriteLine("            otherwise defaults to the same location as the input file with the");
			Console.WriteLine("            extension changed to .txt.");
			Console.WriteLine("        -o: Overwrite the existing output file. ");
			Console.WriteLine("        -b: Show user badges (e.g. moderator/subscriber).");
		}

		private static void UpdateProgress(int segmentCount, TimeSpan? contentOffset)
		{
			var message = $"Downloaded {segmentCount} segment{(segmentCount == 1 ? "" : "s")}";
			if (contentOffset != null)
			{
				message += $", offset = {Rechat.TimestampToString(contentOffset.Value, false)}";
			}

			Console.Write($"\r{message}");
		}

		private static long? TryParseVideoIdFromUrl(string s)
		{
			string[] hosts = {"twitch.tv", "www.twitch.tv"};
			if (!s.StartsWith("http", StringComparison.OrdinalIgnoreCase))
			{
				s = "https://" + s;
			}

			if (!Uri.TryCreate(s, UriKind.Absolute, out var uri)) return null;
			if (!hosts.Any(h => uri.Host.Equals(h, StringComparison.OrdinalIgnoreCase))) return null;
			var match = Regex.Match(uri.AbsolutePath, "^/(videos|[^/]+/video)/(?<videoId>[0-9]+)$");
			
			if (!match.Success) return null;
			
			return match.Groups["videoId"].Value.TryParseInt64();
		}
	}
	
	public class InvalidArgumentException : Exception
	{
	}
}