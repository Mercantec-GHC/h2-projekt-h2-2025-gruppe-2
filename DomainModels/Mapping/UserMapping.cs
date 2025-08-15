namespace DomainModels.Mapping;

using DomainModels;

public class UserMapping
{
    public static UserGetDto ToUserGetDto(User user)
    {
        return new UserGetDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            RoleName = user.Roles?.Name ?? "No role added"
        };
    }
}