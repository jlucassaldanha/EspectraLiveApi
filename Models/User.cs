namespace SpectraLiveApi.Models;

public class User
{
	public Guid Id { get; private set; }
	public string AccessToken { get; private set; }
	public string RefreshToken { get; private set; }
	public string ExpiresIn { get; private set; }
	public string TwitchId { get; private set; }
	public string Login { get; private set; }
	public string DisplayName { get; private set; }
	public string ProfileImgUrl { get; private set; }

	public User(string accessToken, string refreshToken, string expiresIn, string twitchId, string login, string displayName, string profileImgUrl)
	{
		AccessToken = accessToken;
		RefreshToken = refreshToken;
		ExpiresIn = expiresIn;
		TwitchId = twitchId;
		Login = login;
		DisplayName = displayName;
		ProfileImgUrl = profileImgUrl;

		Id = Guid.NewGuid();
	}
}