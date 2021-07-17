using Newtonsoft.Json;

namespace RechatToolCore.Json
{
	public class JsonCommentUserBadge
	{
		[JsonProperty("_id")] public string Id { get; set; }
		[JsonProperty("version")] public string Version { get; set; }

		public UserBadge ToUserBadge() => new()
		{
			Id = Id,
			Version = Version
		};
	}
}