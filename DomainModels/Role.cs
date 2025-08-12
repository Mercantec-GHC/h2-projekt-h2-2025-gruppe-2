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
}