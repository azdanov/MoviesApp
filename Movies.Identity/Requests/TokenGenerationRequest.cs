namespace Movies.Identity.Requests;

public class TokenGenerationRequest
{
    public Guid UserId { get; set; }

    public string Email { get; set; } = null!;

    public Dictionary<string, object> CustomClaims { get; set; } = new();
}