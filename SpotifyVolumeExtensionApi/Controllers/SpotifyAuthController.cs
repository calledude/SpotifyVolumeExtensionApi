using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SpotifyVolumeExtensionApi.Controllers
{
    [ApiController]
    [Route("~/")]
    public class SpotifyAuthController : ControllerBase
    {
        private readonly ILogger<SpotifyAuthController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private static readonly string _clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
        private static readonly string _clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
        private const string _redirectUri = "https://spotifyvolumeextension.herokuapp.com";

        public SpotifyAuthController(
            ILogger<SpotifyAuthController> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult SpotifyAuthorizationCallback([FromQuery] string code, [FromQuery] string state)
        {
            var query = new Dictionary<string, string>()
            {
                { "code", code },
                { "state", state }
            };

            var queryString = QueryHelpers.AddQueryString("http://localhost:4002/auth", query);
            return Redirect(queryString);
        }

        [HttpPost("refresh")]
        public async Task<string> RefreshToken(
            [RequiredFromForm] string grant_type,
            [RequiredFromForm] string refresh_token)
        {
            return await GetToken(grant_type, refreshToken: refresh_token);
        }

        [HttpPost("authorize")]
        public async Task<string> Authorize(
            [RequiredFromForm] string grant_type,
            [RequiredFromForm] string code)
        {
            return await GetToken(grant_type, code);
        }

        private async Task<string> GetToken(string grantType, string code = "", string refreshToken = "")
        {
            var client = _httpClientFactory.CreateClient();

            var query = new Dictionary<string, string>()
            {
                { "grant_type", grantType },
                { "redirect_uri", _redirectUri },
                { "code", code },
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "refresh_token", refreshToken }
            };

            var content = new FormUrlEncodedContent(query);
            var response = await client.PostAsync("https://accounts.spotify.com/api/token", content);

            return await response.Content.ReadAsStringAsync();
        }

        [HttpGet("authorize")]
        public IActionResult Authorize(
            [FromQuery] string response_type,
            [FromQuery] string state,
            [FromQuery] string scope,
            [FromQuery] string show_dialog)
        {
            var query = new Dictionary<string, string>()
            {
                { "client_id", _clientId },
                { "redirect_uri", _redirectUri },
                { "response_type", response_type },
                { "scope", scope },
                { "state", state },
                { "show_dialog", show_dialog }
            };

            var queryString = QueryHelpers.AddQueryString("https://accounts.spotify.com/authorize/", query);
            return Redirect(queryString);
        }
    }
}
