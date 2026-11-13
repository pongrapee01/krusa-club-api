namespace KrusaClub.Api.Configuration;

public sealed class MasterCacheOptions
{
    public const string SectionName = "MasterCache";

    /// <summary>Master data list cache lifetime (minutes).</summary>
    public int AbsoluteExpirationMinutes { get; set; } = 60;

    /// <summary>Optional sliding window; set 0 to disable.</summary>
    public int SlidingExpirationMinutes { get; set; } = 15;
}
