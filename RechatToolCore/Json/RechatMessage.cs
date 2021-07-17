using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace RechatToolCore.Json
{
	public class RechatMessage {
		public JObject SourceJson { get; }

		private JsonComment Comment { get; }
		private JsonCommentCommenter Commenter => Comment.Commenter;
		private JsonCommentMessage Message => Comment.Message;

		public RechatMessage(JObject sourceJson) {
			SourceJson = sourceJson;
			Comment = sourceJson.ToObject<JsonComment>();
		}

		public DateTime CreatedAt => Comment.CreatedAt;

		public TimeSpan ContentOffset => TimeSpan.FromSeconds(Comment.ContentOffsetSeconds);

		// User said something with "/me"
		public bool IsAction => Message.IsAction;

		// Not from the live chat (i.e. user posted a comment on the VOD)
		public bool IsNonChat => !Comment.Source.Equals("chat", StringComparison.OrdinalIgnoreCase);

		public string MessageText => Message.Body;

		public string UserName => Commenter?.Name;

		public string UserDisplayName => Commenter?.DisplayName.TrimEnd(' ');

		public bool UserIsAdmin => HasBadge("admin");
		public bool UserIsStaff => HasBadge("staff");
		public bool UserIsGlobalModerator => HasBadge("global_mod");
		public bool UserIsBroadcaster => HasBadge("broadcaster");
		public bool UserIsModerator => HasBadge("moderator");
		public bool UserIsSubscriber => HasBadge("subscriber");
		public bool UserIsBitDonator => BitsDonated > 0;

		public long BitsDonated => HasBadge("bits") && UserBadgeMap != null ? long.Parse(UserBadgeMap["bits"].Version) : 0L;
		

		public IEnumerable<UserBadge> UserBadges => Message.UserBadges?.Select(n => n.ToUserBadge()) ?? Enumerable.Empty<UserBadge>();

		private ImmutableDictionary<string, UserBadge> UserBadgeMap => Message.UserBadges?
			.Select(n => n.ToUserBadge())
			.ToImmutableDictionary(k => k.Id.ToLower()) ?? ImmutableDictionary<string, UserBadge>.Empty;

		public int EmoticonsCount => Message.Emoticons?.Length ?? 0;

		private bool HasBadge(string id) => Message.UserBadges?.Any(n => n.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) ?? false;
		
	}
}