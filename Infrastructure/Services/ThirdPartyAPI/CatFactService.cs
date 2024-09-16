using Newtonsoft.Json.Linq;

public class CatFactService
{
    private readonly HttpClient _httpClient;

    public CatFactService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetRandomCatFactAsync()
    {
        var response = await _httpClient.GetAsync("https://catfact.ninja/fact");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(content);

        return json["fact"].ToString();
    }
}
