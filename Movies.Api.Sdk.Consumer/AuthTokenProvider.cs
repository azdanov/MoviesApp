using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;

namespace Movies.Api.Sdk.Consumer;

public class AuthTokenProvider
{
    private static readonly SemaphoreSlim _lock = new(1, 1);
    private readonly HttpClient _httpClient;
    private string _cachedToken = string.Empty;

    public AuthTokenProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_cachedToken))
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(_cachedToken);
            var expires = jwt.ValidTo;

            if (expires > DateTime.UtcNow)
            {
                return _cachedToken;
            }
        }

        await _lock.WaitAsync();

        var response = await _httpClient.PostAsJsonAsync("http://localhost:5281/token", new
        {
            userId = "9dfdebf2-b7b1-4526-9358-850b21eb6aad",
            email = "anton@azdanov.dev",
            customClaims = new
            {
                admin = "true",
                trusted_member = "false"
            }
        });

        var token = await response.Content.ReadAsStringAsync();
        _cachedToken = token;
        _lock.Release();
        return token;
    }
}