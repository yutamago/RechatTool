using Newtonsoft.Json;

namespace RechatToolCore.Json
{
	public class JsonEmoticon
	{
		[JsonProperty("_id")] public string Id { get; set; }

		[JsonProperty("begin")] public int Begin { get; set; }

		[JsonProperty("end")] public int End { get; set; }
	}
}