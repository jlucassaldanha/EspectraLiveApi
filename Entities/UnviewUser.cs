namespace SpectraLiveApi.Entities;

public class UnviewUser
{
	public Guid Id { get; private set; }
	public string UserId { get; private set; }
	public string TwitchId { get; private set; }

	public UnviewUser(string userId, string twitchId)
	{
		UserId = userId;
		TwitchId = twitchId;

		Id = Guid.NewGuid();
	}
}