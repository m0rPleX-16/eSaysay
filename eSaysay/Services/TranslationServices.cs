using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class TranslationService
{
    private static readonly HttpClient client = new HttpClient();

    public async Task<string> TranslateText(string text, string fromLang, string toLang)
    {
        try
        {
            string encodedText = Uri.EscapeDataString(text);
            string url = $"https://api.mymemory.translated.net/get?q={encodedText}&langpair={fromLang}|{toLang}&key=75634863867ec2f0994d";

            HttpResponseMessage response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return $"Error: {response.StatusCode}";
            }

            string responseString = await response.Content.ReadAsStringAsync();
            JObject jsonResponse = JObject.Parse(responseString);

            return jsonResponse["responseData"]?["translatedText"]?.ToString() ?? "Translation unavailable";
        }
        catch (Exception ex)
        {
            return $"Translation failed: {ex.Message}";
        }
    }

}
