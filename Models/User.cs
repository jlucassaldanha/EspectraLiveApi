namespace SpectraLiveApi.Models;

public class User
{
	public Guid Id { get; private set; }
	public string AccessToken { get; set; }
	public string RefreshToken { get; set; }
	public int ExpiresIn { get; set; }
	public string TwitchId { get; set; }
	public string Login { get; set; }
	public string DisplayName { get; set; }
	public string ProfileImgUrl { get; set; }

	public User(string accessToken, string refreshToken, int expiresIn, string twitchId, string login, string displayName, string profileImgUrl)
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