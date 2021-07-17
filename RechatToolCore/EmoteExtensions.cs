using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace RechatToolCore
{
	public class EmoteExtensions
	{
		
		
		public static long GetUserId(string username)
		{
			var API = $"https://api.twitch.tv/kraken/users?login={username}";
			var request = (HttpWebRequest) WebRequest.Create(API);
			Rechat.AddTwitchApiHeaders(request);
			
			// request.Accept = "application/vnd.twitchtv.v5+json";
			// request.Headers["client-id"] = "ekj09tcx24qymrl1wl5c6er2qjkpryz";
			
			using var response = (HttpWebResponse) request.GetResponse();
			using var responseStream = new StreamReader(response.GetResponseStream());
			var userInfo = JObject.Parse(responseStream.ReadToEnd());
			var users = (JArray) userInfo["users"];
			var user = (JObject) users[0];
			var userId = user["_id"];
			
			return 0L;
		}
		public static string[] GetAllEmotes(string username)
		{
			return System.Array.Empty<string>();
		}
		
		public static string[] GetAllEmotes(long userId)
		{
			return System.Array.Empty<string>();
		}
		
		public static string[] FFZEmotes(long userId)
		{
			var API = $"https://api.betterttv.net/3/cached/frankerfacez/users/twitch/{userId}";
			return System.Array.Empty<string>();
		}
		
		public static string[] BTTVEmotesGlobal()
		{
			var API = $"https://api.betterttv.net/3/cached/emotes/global";
			return System.Array.Empty<string>();
			
		}
		public static string[] BTTVEmotesChannel(long userId)
		{
			var API = $"https://api.betterttv.net/3/cached/users/twitch/{userId}";
			return System.Array.Empty<string>();
		}
	}
}