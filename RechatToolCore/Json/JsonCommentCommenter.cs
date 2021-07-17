using Newtonsoft.Json;

namespace RechatToolCore.Json
{
	public class JsonCommentCommenter
	{
		[JsonProperty("display_name")]
		public string DisplayName { get; set; }
		
		[JsonProperty("name")]
		public string Name { get; set; }
	}
}