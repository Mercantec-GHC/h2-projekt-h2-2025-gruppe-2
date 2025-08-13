namespace DomainModels.Mapping;
using DomainModels;

public class UserMapping
{
    public static User.UserGetDto ToUserGetDto(User user)
    {
        return new User.UserGetDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            RoleName = user.Role?.Name ?? "No role added"
        };
    }
}