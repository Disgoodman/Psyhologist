namespace Psychologist.Server.Configuration;

public class AuthConfig
{
    public const string SectionName = "Auth";
    public FirstUserConfig FirstUser { get; set; } = null!;
}


public class FirstUserConfig
{
    public const string SectionName = "FirstUser";
    
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}