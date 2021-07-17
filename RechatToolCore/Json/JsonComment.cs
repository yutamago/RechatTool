using System;
using Newtonsoft.Json;

namespace RechatToolCore.Json
{
	public class JsonComment {
		[JsonProperty("created_at")]
		public DateTime CreatedAt { get; set; }
		[JsonProperty("content_offset_seconds")]
		public double ContentOffsetSeconds { get; set; }
		[JsonProperty("source")]
		public string Source { get; set; }
		[JsonProperty("commenter")]
		public JsonCommentCommenter Commenter { get; set; }
		[JsonProperty("message")]
		public JsonCommentMessage Message { get; set; }
	}
}