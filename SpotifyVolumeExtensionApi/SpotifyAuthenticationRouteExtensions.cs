using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;

namespace SpotifyVolumeExtensionApi;

public static class SpotifyAuthenticationRouteExtensions
{
	private const string _redirectUri = "https://svetokenswap.azurewebsites.net";

	public static IEndpointRouteBuilder MapSpotifyAuthenticationRoutes(this IEndpointRouteBuilder builder, string clientId, string clientSecret)
	{
		return builder
			.MapSpotifyAuthorizationCallbackEndpoint()
			.MapAuthorizeEndpoint(clientId)
			.MapTokenSwapEndpoints(clientId, clientSecret);
	}

	private static IEndpointRouteBuilder MapSpotifyAuthorizationCallbackEndpoint(this IEndpointRouteBuilder builder)
	{
		// This is the endpoint that will be hit after '/authorize' completes its Redirect to spotify, which later on will return here
		// See: _redirectUri
		builder.MapGet("/", ([FromQuery] string code, [FromQuery] string? state) =>
		{
			var query = new Dictionary<string, string?>()
			{
				{ "code", code },
				{ "state", state }
			};

			var queryString = QueryHelpers.AddQueryString("http://localhost:4002/auth", query);
			return Results.Redirect(queryString);
		}).DisableAntiforgery();

		return builder;
	}

	private static IEndpointRouteBuilder MapAuthorizeEndpoint(this IEndpointRouteBuilder builder, string clientId)
	{
		builder.MapGet("/authorize", (
			[FromQuery] string response_type,
			[FromQuery] string? state,
			[FromQuery] string scope,
			[FromQuery] string? show_dialog) =>
		{
			var query = new Dictionary<string, string?>()
			{
				{ "client_id", clientId },
				{ "redirect_uri", _redirectUri },
				{ "response_type", response_type },
				{ "scope", scope },
				{ "state", state },
				{ "show_dialog", show_dialog }
			};

			var queryString = QueryHelpers.AddQueryString("https://accounts.spotify.com/authorize/", query);
			return Results.Redirect(queryString);
		}).DisableAntiforgery();

		return builder;
	}

	private static IEndpointRouteBuilder MapTokenSwapEndpoints(this IEndpointRouteBuilder builder, string clientId, string clientSecret)
	{
		builder.MapPost("/swap", async ([RequiredFromForm] string code, IHttpClientFactory httpClientFactory) =>
		{
			var tokenResponse = await GetToken(httpClientFactory, "authorization_code", code: code);
			return Results.Content(tokenResponse, MediaTypeNames.Application.Json);
		}).DisableAntiforgery();

		builder.MapPost("/refresh", async ([RequiredFromForm] string refresh_token, IHttpClientFactory httpClientFactory) =>
		{
			var tokenResponse = await GetToken(httpClientFactory, "refresh_token", refreshToken: refresh_token);
			return Results.Content(tokenResponse, MediaTypeNames.Application.Json);
		}).DisableAntiforgery();

		return builder;

		async Task<string> GetToken(IHttpClientFactory httpClientFactory, string grantType, string code = "", string refreshToken = "")
		{
			var client = httpClientFactory.CreateClient();

			var query = new Dictionary<string, string>()
			{
				{ "grant_type", grantType },
				{ "redirect_uri", _redirectUri },
				{ "code", code },
				{ "client_id", clientId },
				{ "client_secret", clientSecret },
				{ "refresh_token", refreshToken }
			};

			var content = new FormUrlEncodedContent(query);
			var response = await client.PostAsync("https://accounts.spotify.com/api/token", content);

			return await response.Content.ReadAsStringAsync();
		}
	}
}
