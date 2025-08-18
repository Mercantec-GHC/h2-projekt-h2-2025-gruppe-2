using System.ComponentModel.DataAnnotations;

namespace DomainModels;

public class Role : Common
{
    public required string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    public virtual ICollection<User> Users { get; set; } = new List<User>();

    public static class Names
    {
        public const string User = "User";
        public const string CleaningStaff = "CleaningStaff";
        public const string Reception = "Reception";
        public const string Admin = "Admin";
    }
    
    public string ResolveRoleId(string roleId)
    {
        return roleId switch
        {
            "1" => Names.User,
            "2" => Names.CleaningStaff,
            "3" => Names.Reception,
            "4" => Names.Admin,
            _ => "Unknown"
        };
    }

}