using System.Net.Http.Json;
using SpectraLiveApi.DTOs;
using SpectraLiveApi.Models;

namespace SpectraLiveApi.Integrations;

public class TwitchApiClient
{
	private readonly HttpClient _httpClient;

	public TwitchApiClient(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	
}
