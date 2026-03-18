using System.Net;

namespace SpectraLiveApi.DTOs.Twitch;

public record TwitchUserResponse(HttpStatusCode Status, string Message);