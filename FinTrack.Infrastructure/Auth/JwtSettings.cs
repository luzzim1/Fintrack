namespace FinTrack.Infrastructure.Auth;

public class JwtSettings
{
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "FinTrack";
    public string Audience { get; set; } = "FinTrack.Web";
    public int LifetimeMinutes { get; set; } = 60;
}
