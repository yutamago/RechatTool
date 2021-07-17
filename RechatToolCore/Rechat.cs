using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RechatToolCore.Json;

namespace RechatToolCore
{
	public static class Rechat
	{
		private static string API_COMMENTS_URL(long videoId) =>
			$"{"https"}://api.twitch.tv/v5/videos/{videoId}/comments";

		public static void DownloadFile(long videoId, string path, bool overwrite = false,
			DownloadProgressCallback progressCallback = null)
		{
			path += "2";
			if (File.Exists(path) && overwrite) File.Delete(path);
			if (File.Exists(path + ".tsv") && overwrite) File.Delete(path + ".tsv");

			var baseUrl = API_COMMENTS_URL(videoId);
			string nextCursor = null;
			var segmentCount = 0;
			JObject firstComment = null;
			JObject lastComment = null;

			do
			{
				var jsonOutputString = new StringBuilder();
				using var writer = new JsonTextWriter(new StringWriter(jsonOutputString))
				{
					Formatting = Formatting.Indented
				};

				writer.WriteStartArray();
				var url = nextCursor == null
					? $"{baseUrl}?content_offset_seconds=0"
					: $"{baseUrl}?cursor={nextCursor}";
				var response = JObject.Parse(DownloadUrlAsString(url, withRequest: AddTwitchApiHeaders));
				if (response["comments"] != null)
				{
					foreach (var jToken in (JArray) response["comments"])
					{
						var comment = (JObject) jToken;
						comment.WriteTo(writer);
						firstComment ??= comment;
						lastComment = comment;
					}

					nextCursor = (string) response["_next"];
				}

				writer.WriteEndArray();
				ProcessString(jsonOutputString.ToString(), path + ".tsv");

				segmentCount++;
				progressCallback?.Invoke(segmentCount, TryGetContentOffset(lastComment));
			} while (nextCursor != null);
		}

		private static string DownloadUrlAsString(string url, Action<HttpWebRequest> withRequest = null)
		{
			var request = (HttpWebRequest) WebRequest.Create(url);
			withRequest?.Invoke(request);
			using var response = (HttpWebResponse) request.GetResponse();
			using var responseStream = new StreamReader(response.GetResponseStream());
			return responseStream.ReadToEnd();
		}

		public static void AddTwitchApiHeaders(HttpWebRequest request)
		{
			request.Accept = "application/vnd.twitchtv.v5+json";
			request.Headers.Add("Client-ID", "jzkbprff40iqj646a697cyrvl0zt2m6");
		}

		private static TimeSpan? TryGetContentOffset(JObject comment)
		{
			try
			{
				return comment == null ? null : new RechatMessage(comment).ContentOffset;
			}
			catch
			{
				return null;
			}
		}

		private static void ProcessString(string content, string pathOut, bool showBadges = false)
		{
			var lines = ParseMessagesFromString(content)
				.Select(ToDatasetString)
				.Where(n => n != null);

			var newFile = !File.Exists(pathOut);
			using var appender = File.AppendText(pathOut);
			if(newFile) appender.WriteLine(GetDatasetHeader());
			
			foreach (var line in lines)
			{
				appender.WriteLine(line);
			}
		}

		private static IEnumerable<RechatMessage> ParseMessagesFromString(string contentString)
		{
			using var stringReader = new StringReader(contentString);
			using var reader = new JsonTextReader(stringReader);
			while (reader.Read())
			{
				if (reader.TokenType != JsonToken.StartObject) continue;
				yield return new RechatMessage(JObject.Load(reader));
			}
		}

		public static string TimestampToString(TimeSpan value, bool showMilliseconds)
		{
			return $"{(int) value.TotalHours:00}:{value:mm}:{value:ss}{(showMilliseconds ? $".{value:fff}" : "")}";
		}

		private static string ToReadableString(RechatMessage m, bool showBadges)
		{
			var userBadges =
				$"{(m.UserIsAdmin || m.UserIsStaff ? "*" : "")}{(m.UserIsBroadcaster ? "#" : "")}{(m.UserIsModerator || m.UserIsGlobalModerator ? "@" : "")}{(m.UserIsSubscriber ? "+" : "")}";
			var userName = m.UserName == null
				? "???"
				: string.Equals(m.UserDisplayName, m.UserName, StringComparison.OrdinalIgnoreCase)
					? m.UserDisplayName
					: $"{m.UserDisplayName} ({m.UserName})";
			return
				$"[{TimestampToString(m.ContentOffset, true)}] {(showBadges ? userBadges : "")}{userName}{(m.IsAction ? "" : ":")} {m.MessageText}";
		}

		private static string GetDatasetHeader()
		{
			return "Timestamp\t" +
			       "Message\t" +
			       "EmoticonsCount\t" +
			       "IsAction\t" +
			       "UserName\t" +
			       "IsModerator\t" +
			       "IsSubscriber\t" +
			       "BitsDonated\t" +
			       

			       "IsAdvice\t" +
			       "IsComment\t" +
			       "IsConversation\t" +
			       "IsReaction";
		}
		
		private static string ToDatasetString(RechatMessage m)
		{
			var userName = m.UserName == null
				? "???"
				: string.Equals(m.UserDisplayName, m.UserName, StringComparison.OrdinalIgnoreCase)
					? m.UserDisplayName
					: $"{m.UserDisplayName} ({m.UserName})";
			
			return $"{(ulong) m.ContentOffset.TotalMilliseconds}\t" +
			       $"{m.MessageText.Replace('\t', ' ')}\t" +
			       $"{m.EmoticonsCount}\t" +
			       $"{m.IsAction}\t" +
			       $"{userName}\t" +
			       $"{m.UserIsModerator || m.UserIsGlobalModerator || m.UserIsAdmin || m.UserIsBroadcaster || m.UserIsStaff}\t" +
			       $"{m.UserIsSubscriber}\t" +
			       $"{m.BitsDonated}";
		}

		public delegate void DownloadProgressCallback(int segmentCount, TimeSpan? contentOffset);
	}
}