using Newtonsoft.Json;

namespace RechatToolCore.Json
{
	public class JsonCommentMessage
	{
		[JsonProperty("body")]
		public string Body { get; set; }
		
		[JsonProperty("is_action")]
		public bool IsAction { get; set; }
		
		[JsonProperty("user_badges")]
		public JsonCommentUserBadge[] UserBadges { get; set; }
		
		[JsonProperty("emoticons")]
		public JsonEmoticon[] Emoticons { get; set; }
	}
}