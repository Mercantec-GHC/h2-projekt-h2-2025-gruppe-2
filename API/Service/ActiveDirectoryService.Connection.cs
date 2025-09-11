using System.DirectoryServices.Protocols;
using System.Net;

namespace API.Service;

public partial class ActiveDirectoryService
{
    /// <summary>
    /// Opretter en forbindelse til Active Directory
    /// </summary>
    /// <returns>LdapConnection objekt</returns>
    public LdapConnection GetConnection()
    {
        var credential = new NetworkCredential($"{_config.Username}@{_config.Domain}", _config.Password);
        var connection = new LdapConnection(_config.Server)
        {
            Credential = credential,
            AuthType = AuthType.Negotiate
        };

        connection.Bind(); // Test forbindelse
        return connection;
    }

    /// <summary>
    /// Calls SearchUsers and checks if one single user exists with the given params
    /// </summary>
    /// <param name="name">The name has to be only the first name of an account</param>
    /// <param name="email">The email of an account</param>
    /// <param name="role">The role of an account</param>
    /// <returns></returns>
    public bool IsUserInAD(string name, string email, string role)
    {
        return SearchUsers(name, email, role).Count switch
        {
            0 => false,
            1 => true,
            _ => false
        };
    }

    /// <summary>
    /// Søger efter brugere baseret på søgeterm
    /// </summary>
    /// <param name="searchTerm">Søgeterm til at søge efter</param>
    /// <returns>Liste af matchende ADUser objekter</returns>
    private List<ADUser> SearchUsers(string name, string email, string role)
    {
        var users = new List<ADUser>();

        using (var connection = GetConnection())
        {
            var filter =
                $"(|(firstName=*{name}*)(mail=*{email}*)(memberOf=*{role}*))";

            var searchRequest = new SearchRequest(
                GetBaseDN(),
                filter,
                SearchScope.Subtree,
                "cn",
                "samAccountName",
                "mail",
                "department",
                "title",
                "distinguishedName",
                "givenName",
                "sn",
                "displayName",
                "company",
                "physicalDeliveryOfficeName",
                "telephoneNumber",
                "mobile",
                "manager",
                "lastLogon",
                "pwdLastSet",
                "userAccountControl"
            );

            try
            {
                var response = (SearchResponse)connection.SendRequest(searchRequest);

                foreach (SearchResultEntry entry in response.Entries)
                {
                    var user = new ADUser
                    {
                        Name = entry.Attributes["cn"]?[0]?.ToString() ?? "N/A",
                        Username = entry.Attributes["samAccountName"]?[0]?.ToString() ?? "N/A",
                        Email = entry.Attributes["mail"]?[0]?.ToString() ?? "N/A",
                        Department = entry.Attributes["department"]?[0]?.ToString() ?? "N/A",
                        Title = entry.Attributes["title"]?[0]?.ToString() ?? "N/A",
                        DistinguishedName = entry.Attributes["distinguishedName"]?[0]?.ToString() ?? "N/A",
                        FirstName = entry.Attributes["givenName"]?[0]?.ToString() ?? "N/A",
                        LastName = entry.Attributes["sn"]?[0]?.ToString() ?? "N/A",
                        DisplayName = entry.Attributes["displayName"]?[0]?.ToString() ?? "N/A",
                        Company = entry.Attributes["company"]?[0]?.ToString() ?? "N/A",
                        Office = entry.Attributes["physicalDeliveryOfficeName"]?[0]?.ToString() ?? "N/A",
                        Phone = entry.Attributes["telephoneNumber"]?[0]?.ToString() ?? "N/A",
                        Mobile = entry.Attributes["mobile"]?[0]?.ToString() ?? "N/A",
                        Manager = entry.Attributes["manager"]?[0]?.ToString() ?? "N/A"
                    };

                    // Parse lastLogon (kan være byte array)
                    if (entry.Attributes.Contains("lastLogon"))
                    {
                        var lastLogonValue = entry.Attributes["lastLogon"][0];
                        if (lastLogonValue is byte[] lastLogonBytes && lastLogonBytes.Length == 8)
                        {
                            var ticks = BitConverter.ToInt64(lastLogonBytes, 0);
                            if (ticks > 0)
                            {
                                user.LastLogon = DateTime.FromFileTime(ticks);
                            }
                        }
                    }

                    // Parse passwordLastSet
                    if (entry.Attributes.Contains("pwdLastSet"))
                    {
                        var pwdLastSetValue = entry.Attributes["pwdLastSet"][0];
                        if (pwdLastSetValue is byte[] pwdLastSetBytes && pwdLastSetBytes.Length == 8)
                        {
                            var ticks = BitConverter.ToInt64(pwdLastSetBytes, 0);
                            if (ticks > 0)
                            {
                                user.PasswordLastSet = DateTime.FromFileTime(ticks);
                            }
                        }
                    }

                    // Parse userAccountControl for enabled status
                    if (entry.Attributes.Contains("userAccountControl"))
                    {
                        var uacValue = entry.Attributes["userAccountControl"][0]?.ToString();
                        if (int.TryParse(uacValue, out int uac))
                        {
                            // Bit 2 (0x0002) = ACCOUNTDISABLE
                            user.IsEnabled = (uac & 0x0002) == 0;
                        }
                    }

                    users.Add(user);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Fejl ved søgning efter brugere: {ex.Message}");
            }
        }

        return users;
    }

    /// <summary>
    /// Viser detaljerede oplysninger om den aktuelle bruger (/me kommando)
    /// </summary>
    public (ADUser? adUser, List<string>? roles) ShowCurrentUserInfo()
    {
        Console.WriteLine("=== Min Bruger Information (/me) ===");
        Console.WriteLine();

        try
        {
            var adUser = GetCurrentUserDetails();
            if (adUser == null)
            {
                return (null, null);
            }
            Console.WriteLine("Henter brugerens roller/grupper...");
            var groups = GetCurrentUserGroups();
            Console.WriteLine($"Fundet {groups.Count} roller/grupper");

            // Grundlæggende information
            Console.WriteLine("👤 GRUNDLÆGGENDE INFORMATION");
            Console.WriteLine($"   Navn: {adUser.DisplayName}");
            Console.WriteLine($"   Brugernavn: {adUser.Username}");
            Console.WriteLine($"   Email: {adUser.Email}");
            Console.WriteLine($"   Firma: {adUser.Company}");
            Console.WriteLine($"   Afdeling: {adUser.Department}");
            Console.WriteLine($"   Titel: {adUser.Title}");
            Console.WriteLine($"   Kontor: {adUser.Office}");
            Console.WriteLine();

            // Kontakt information
            Console.WriteLine("📞 KONTAKT INFORMATION");
            Console.WriteLine($"   Telefon: {adUser.Phone}");
            Console.WriteLine($"   Mobil: {adUser.Mobile}");
            Console.WriteLine($"   Manager: {adUser.Manager}");
            Console.WriteLine();

            // Konto status
            Console.WriteLine("🔐 KONTO STATUS");
            Console.WriteLine($"   Status: {(adUser.IsEnabled ? "✅ Aktiv" : "❌ Deaktiveret")}");
            if (adUser.LastLogon.HasValue)
            {
                Console.WriteLine($"   Sidste login: {adUser.LastLogon.Value:dd/MM/yyyy HH:mm:ss}");
            }
            else
            {
                Console.WriteLine("   Sidste login: Aldrig");
            }

            if (adUser.PasswordLastSet.HasValue)
            {
                Console.WriteLine($"   Adgangskode ændret: {adUser.PasswordLastSet.Value:dd/MM/yyyy HH:mm:ss}");
            }
            else
            {
                Console.WriteLine("   Adgangskode ændret: Ukendt");
            }

            Console.WriteLine();

            // Grupper/Roller
            Console.WriteLine($"👥 ROLLER/GRUPPER ({groups.Count})");
            if (groups.Count > 0)
            {
                foreach (var group in groups.OrderBy(g => g))
                {
                    Console.WriteLine($"   • {group}");
                }
            }
            else
            {
                Console.WriteLine("   Ingen roller/grupper fundet");
            }

            Console.WriteLine();

            // Tekniske detaljer
            Console.WriteLine("🔧 TEKNISKE DETALJER");
            Console.WriteLine($"   Distinguished Name: {adUser.DistinguishedName}");
            Console.WriteLine($"   Forbindelse: {_config.Server}");
            Console.WriteLine($"   Domæne: {_config.Domain}");

            return (adUser, groups);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fejl ved hentning af brugeroplysninger: {ex.Message}");
            return (null, null);
        }
    }

    /// <summary>
    /// Henter detaljerede oplysninger om den aktuelle bruger
    /// </summary>
    /// <returns>ADUser objekt med detaljerede oplysninger</returns>
    private ADUser? GetCurrentUserDetails()
    {
        using (var connection = GetConnection())
        {
            var searchRequest = new SearchRequest(
                GetBaseDN(),
                $"(samAccountName={_config.Username})",
                SearchScope.Subtree,
                "cn",
                "samAccountName",
                "mail",
                "department",
                "title",
                "distinguishedName",
                "givenName",
                "sn",
                "displayName",
                "company",
                "physicalDeliveryOfficeName",
                "telephoneNumber",
                "mobile",
                "manager",
                "lastLogon",
                "pwdLastSet",
                "userAccountControl"
            );

            try
            {
                var response = (SearchResponse)connection.SendRequest(searchRequest);

                if (response.Entries.Count == 0)
                {
                    Console.WriteLine("Bruger ikke fundet i Active Directory");
                    return null;
                }

                var entry = response.Entries[0];
                var user = new ADUser
                {
                    Name = entry.Attributes["cn"]?[0]?.ToString() ?? "N/A",
                    Username = entry.Attributes["samAccountName"]?[0]?.ToString() ?? "N/A",
                    Email = entry.Attributes["mail"]?[0]?.ToString() ?? "N/A",
                    Department = entry.Attributes["department"]?[0]?.ToString() ?? "N/A",
                    Title = entry.Attributes["title"]?[0]?.ToString() ?? "N/A",
                    DistinguishedName = entry.Attributes["distinguishedName"]?[0]?.ToString() ?? "N/A",
                    FirstName = entry.Attributes["givenName"]?[0]?.ToString() ?? "N/A",
                    LastName = entry.Attributes["sn"]?[0]?.ToString() ?? "N/A",
                    DisplayName = entry.Attributes["displayName"]?[0]?.ToString() ?? "N/A",
                    Company = entry.Attributes["company"]?[0]?.ToString() ?? "N/A",
                    Office = entry.Attributes["physicalDeliveryOfficeName"]?[0]?.ToString() ?? "N/A",
                    Phone = entry.Attributes["telephoneNumber"]?[0]?.ToString() ?? "N/A",
                    Mobile = entry.Attributes["mobile"]?[0]?.ToString() ?? "N/A",
                    Manager = entry.Attributes["manager"]?[0]?.ToString() ?? "N/A"
                };

                // Parse lastLogon (kan være byte array)
                if (entry.Attributes.Contains("lastLogon"))
                {
                    var lastLogonValue = entry.Attributes["lastLogon"][0];
                    if (lastLogonValue is byte[] lastLogonBytes && lastLogonBytes.Length == 8)
                    {
                        var ticks = BitConverter.ToInt64(lastLogonBytes, 0);
                        if (ticks > 0)
                        {
                            user.LastLogon = DateTime.FromFileTime(ticks);
                        }
                    }
                }

                // Parse passwordLastSet
                if (entry.Attributes.Contains("pwdLastSet"))
                {
                    var pwdLastSetValue = entry.Attributes["pwdLastSet"][0];
                    if (pwdLastSetValue is byte[] pwdLastSetBytes && pwdLastSetBytes.Length == 8)
                    {
                        var ticks = BitConverter.ToInt64(pwdLastSetBytes, 0);
                        if (ticks > 0)
                        {
                            user.PasswordLastSet = DateTime.FromFileTime(ticks);
                        }
                    }
                }

                // Parse userAccountControl for enabled status
                if (entry.Attributes.Contains("userAccountControl"))
                {
                    var uacValue = entry.Attributes["userAccountControl"][0]?.ToString();
                    if (int.TryParse(uacValue, out int uac))
                    {
                        // Bit 2 (0x0002) = ACCOUNTDISABLE
                        user.IsEnabled = (uac & 0x0002) == 0;
                    }
                }

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception($"Fejl ved hentning af brugeroplysninger: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Henter alle grupper som den aktuelle bruger er medlem af
    /// </summary>
    /// <returns>Liste af gruppenavne</returns>
    private List<string> GetCurrentUserGroups()
    {
        var groups = new List<string>();

        using (var connection = GetConnection())
        {
            // Først find brugerens distinguished name
            var userSearchRequest = new SearchRequest(
                GetBaseDN(),
                $"(samAccountName={_config.Username})",
                SearchScope.Subtree,
                "distinguishedName"
            );

            try
            {
                var userResponse = (SearchResponse)connection.SendRequest(userSearchRequest);

                if (userResponse.Entries.Count == 0)
                {
                    Console.WriteLine("Bruger ikke fundet for gruppeopslag");
                    return groups;
                }

                var userDN = userResponse.Entries[0].Attributes["distinguishedName"][0]?.ToString();
                if (string.IsNullOrEmpty(userDN))
                {
                    Console.WriteLine("Kunne ikke finde brugerens DN");
                    return groups;
                }

                // Søg efter grupper hvor brugeren er medlem
                var groupSearchRequest = new SearchRequest(
                    GetBaseDN(),
                    $"(member={userDN})",
                    SearchScope.Subtree,
                    "cn",
                    "description"
                );

                var groupResponse = (SearchResponse)connection.SendRequest(groupSearchRequest);

                foreach (SearchResultEntry entry in groupResponse.Entries)
                {
                    if (entry.Attributes.Contains("cn"))
                    {
                        var groupName = entry.Attributes["cn"][0]?.ToString();
                        if (!string.IsNullOrEmpty(groupName))
                        {
                            groups.Add(groupName);
                        }
                    }
                }

                // Prøv også med recursive group membership (hvis understøttet)
                try
                {
                    var recursiveGroupSearchRequest = new SearchRequest(
                        GetBaseDN(),
                        $"(member:1.2.840.113556.1.4.1941:={userDN})",
                        SearchScope.Subtree,
                        "cn"
                    );

                    var recursiveResponse = (SearchResponse)connection.SendRequest(recursiveGroupSearchRequest);

                    foreach (SearchResultEntry entry in recursiveResponse.Entries)
                    {
                        if (entry.Attributes.Contains("cn"))
                        {
                            var groupName = entry.Attributes["cn"][0]?.ToString();
                            if (!string.IsNullOrEmpty(groupName) && !groups.Contains(groupName))
                            {
                                groups.Add(groupName);
                            }
                        }
                    }
                }
                catch
                {
                    // Ignorer fejl for recursive search - det er ikke altid understøttet
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Fejl ved hentning af brugerens grupper: {ex.Message}");
            }
        }

        return groups;
    }

    /// <summary>
    /// Hjælpe metode til at få base DN for domænet
    /// </summary>
    /// <returns>Base DN string</returns>
    private string GetBaseDN()
    {
        return $"DC={_config.Domain.Split('.')[0]},DC={_config.Domain.Split('.')[1]}";
    }
}