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

    public Task<HttpResponseMessage> PutAsync<T>(string url, T body) =>
        http.PutAsJsonAsync(url, body);

    public Task<HttpResponseMessage> DeleteAsync(string url) => http.DeleteAsync(url);
}
