namespace API.Service;

/// <summary>
/// A user on the Active Directory server
/// </summary>
public class ADUser
{
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string DistinguishedName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Office { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Manager { get; set; } = string.Empty;
    public DateTime? LastLogon { get; set; }
    public DateTime? PasswordLastSet { get; set; }
    public bool IsEnabled { get; set; } = true;
    public List<string> Groups { get; set; } = new List<string>();
}

/// <summary>
/// Config for an Active Directory server
/// </summary>
public class ADConfig
{
    public string Server { get; set; } = "10.133.71.102";
    public string Username { get; set; } = "Admin";
    public string Password { get; set; } = "Qwerty123!";
    public string Domain { get; set; } = "karambit.local";
}

public partial class ActiveDirectoryService
{
    private ADConfig _config;
    
    /// <summary>
    /// Dependency injection for ADService
    /// </summary>
    /// <param name="config">A login user</param>
    public ActiveDirectoryService(ADConfig config)
    {
        _config = config;
    }
    
    // Public properties for at få adgang til konfiguration
    /// <summary>
    /// Sets the config
    /// </summary>
    public ADConfig Config => _config;
    
    /// <summary>
    /// Sets the server of the config
    /// </summary>
    public string Server => _config.Server;
    
    /// <summary>
    /// Sets the configs username
    /// </summary>
    public string Username => _config.Username;
    
    /// <summary>
    /// Sets configs domain
    /// </summary>
    public string Domain => _config.Domain;
}