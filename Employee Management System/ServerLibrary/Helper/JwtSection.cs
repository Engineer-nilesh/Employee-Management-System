namespace ServerLibrary.Helper
{
    public class JwtSection
    {
        public string? Key { get; set; } = null!;
        public string? Issuer { get; set; } = null!;
        public string? Audience { get; set; } = null!;
    }
}
