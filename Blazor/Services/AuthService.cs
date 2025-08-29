namespace Blazor.Services;

// AuthService.cs
public class AuthService
{
    public event Action? OnUserChangedSubscribtion;

    public void NotifyUserChanged()
    {
        OnUserChangedSubscribtion?.Invoke();
    }
}