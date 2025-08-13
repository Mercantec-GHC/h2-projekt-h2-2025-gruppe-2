using System.ComponentModel.DataAnnotations;

namespace DomainModels;

public class Common
{
    [Key]
    public string Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}