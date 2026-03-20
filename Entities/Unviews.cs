namespace SpectraLiveApi.Entities;

public class Unviews
{
	public Guid Id { get; private set; }
	public string UserId { get; private set; }
	public string TwitchId { get; private set; }

	public Unviews(string userId, string twitchId)
	{
		UserId = userId;
		TwitchId = twitchId;

		Id = Guid.NewGuid();
	}
}