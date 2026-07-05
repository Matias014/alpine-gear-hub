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
}
