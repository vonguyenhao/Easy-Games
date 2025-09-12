// EasyGames.Web/Helpers/SessionExtensions.cs
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace EasyGames.Web.Helpers;

public static class SessionExtensions
{
    // Save any object to session as JSON
    public static void SetJson(this ISession session, string key, object value) =>
        session.SetString(key, JsonSerializer.Serialize(value));

    // Read any object from session
    public static T? GetJson<T>(this ISession session, string key) =>
        session.GetString(key) is string s ? JsonSerializer.Deserialize<T>(s) : default;
}
