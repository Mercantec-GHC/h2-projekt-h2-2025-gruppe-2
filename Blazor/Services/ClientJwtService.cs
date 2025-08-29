namespace Blazor.Services;

public class ClientJwtService(LocalStorageService storage)
{
	private readonly LocalStorageService _storage = storage;

	public async Task<string?> GetTokenAsync()
	{
		var token = await _storage.GetItemFromStorageAsync("authToken");
		return string.IsNullOrWhiteSpace(token) ? null : token;
	}

	public async Task<string?> GetUserIdAsync()
	{
		var token = await GetTokenAsync();
		if (string.IsNullOrEmpty(token)) return null;

		try
		{
			var payload = ReadJwtPayload(token);
			if (payload == null) return null;

			// Try common keys for user id
			var keys = new[]
			{
				"nameid",
				"sub",
				"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
			};
			foreach (var k in keys)
			{
				if (payload.TryGetValue(k, out var v) && v is string s && !string.IsNullOrWhiteSpace(s))
					return s;
			}
			return null;
		}
		catch
		{
			return null;
		}
	}

	public async Task<bool> IsExpiredAsync()
	{
		var token = await GetTokenAsync();
		if (string.IsNullOrEmpty(token)) return true;
		try
		{
			var payload = ReadJwtPayload(token);
			if (payload == null) return true;
			if (payload.TryGetValue("exp", out var expObj) &&
				long.TryParse(expObj?.ToString(), out var expSeconds))
			{
				var exp = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
				return exp <= DateTime.UtcNow;
			}
			return true; // if no exp, treat as expired
		}
		catch
		{
			return true;
		}
	}

	private static Dictionary<string, object>? ReadJwtPayload(string token)
	{
		var parts = token.Split('.');
		if (parts.Length < 2) return null;
		var json = Base64UrlDecodeToString(parts[1]);
		if (string.IsNullOrWhiteSpace(json)) return null;
		try
		{
			// Minimal JSON parser into dictionary
			var doc = System.Text.Json.JsonDocument.Parse(json);
			var dict = new Dictionary<string, object>();
			foreach (var p in doc.RootElement.EnumerateObject())
			{
				dict[p.Name] = p.Value.ValueKind switch
				{
					System.Text.Json.JsonValueKind.String => p.Value.GetString()!,
					System.Text.Json.JsonValueKind.Number => p.Value.GetRawText(),
					System.Text.Json.JsonValueKind.True => true,
					System.Text.Json.JsonValueKind.False => false,
					_ => p.Value.GetRawText()
				};
			}
			return dict;
		}
		catch
		{
			return null;
		}
	}

	private static string Base64UrlDecodeToString(string base64Url)
	{
		try
		{
			string padded = base64Url.Replace('-', '+').Replace('_', '/');
			switch (padded.Length % 4)
			{
				case 2: padded += "=="; break;
				case 3: padded += "="; break;
			}
			var bytes = Convert.FromBase64String(padded);
			return System.Text.Encoding.UTF8.GetString(bytes);
		}
		catch
		{
			return string.Empty;
		}
	}
}