namespace SpectraLiveApi.Entities;

public class UnviewUser
{
	public Guid Id { get; private set; }
	public string ChannelId { get; private set; }
	public string TwitchUserId { get; private set; }

	public UnviewUser(string channelId, string twitchUserId)
	{
		ChannelId = channelId;
		TwitchUserId = twitchUserId;

		Id = Guid.NewGuid();
	}
}