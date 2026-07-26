using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AlpineGearHub.Api.Tests.Helpers;

public sealed class ApiClient(HttpClient http)
{
    public void SetBearerToken(string token) =>
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    public Task<HttpResponseMessage> GetAsync(string url) => http.GetAsync(url);

    public Task<HttpResponseMessage> PostAsync(string url) => http.PostAsync(url, null);

    public Task<HttpResponseMessage> PostAsync<T>(string url, T body) =>
        http.PostAsJsonAsync(url, body);

    public Task<HttpResponseMessage> PostFileAsync(string url, byte[] fileBytes, string fileName, string contentType)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);
        return http.PostAsync(url, content);
    }

    public Task<HttpResponseMessage> PutAsync<T>(string url, T body) =>
        http.PutAsJsonAsync(url, body);

    public Task<HttpResponseMessage> DeleteAsync(string url) => http.DeleteAsync(url);

    // The refresh token now travels only as an httpOnly cookie, never in a request/response body
    // (see AuthEndpoints.SetRefreshTokenCookie) - WebApplicationFactory's HttpClient auto-tracks
    // and resends Set-Cookie values by default, so most tests don't need this. It exists for the
    // handful that must explicitly replay a specific (e.g. stale, pre-rotation) cookie value,
    // which the auto-jar wouldn't do since it always has the latest one.
    public Task<HttpResponseMessage> PostWithCookieAsync(string url, string cookieName, string cookieValue)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Cookie", $"{cookieName}={Uri.EscapeDataString(cookieValue)}");
        return http.SendAsync(request);
    }

    // Reads a cookie's value straight off the raw Set-Cookie response header, bypassing the
    // HttpClient's own cookie jar - needed to assert on/capture a specific cookie value (e.g. to
    // prove it rotates, or to replay an old one via PostWithCookieAsync above).
    public static string? GetCookieValue(HttpResponseMessage response, string cookieName)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies)) return null;

        foreach (var cookie in cookies)
        {
            var nameAndValue = cookie.Split(';')[0].Split('=', 2);
            if (nameAndValue.Length == 2 && nameAndValue[0].Trim() == cookieName)
                return Uri.UnescapeDataString(nameAndValue[1]);
        }

        return null;
    }
}
