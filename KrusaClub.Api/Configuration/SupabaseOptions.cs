namespace KrusaClub.Api.Configuration;

public sealed class SupabaseOptions
{
    public const string SectionName = "Supabase";

    public string ConnectionString { get; init; } = string.Empty;

    /// <summary>Optional. Project ref (e.g. rhknexgpqmdthfcbzqme). Used to build issuer https://{ref}.supabase.co/auth/v1 when not inferrable from ConnectionString.</summary>
    public string? ProjectRef { get; init; }

    /// <summary>Jwks = validate with Supabase Auth OpenID metadata + JWKS (ES256/RS256). Hmac = symmetric HS256 with JwtSecret.</summary>
    public string JwtSigningMode { get; init; } = "Jwks";

    public string JwtSecret { get; init; } = string.Empty;

    /// <summary>Full issuer URL, e.g. https://YOUR_REF.supabase.co/auth/v1. Legacy short value "supabase" is not a real Supabase iss claim.</summary>
    public string JwtIssuer { get; init; } = string.Empty;

    public string JwtAudience { get; init; } = "authenticated";
}
