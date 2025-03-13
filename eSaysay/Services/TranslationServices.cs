using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using eSaysay.Configuration;
using Newtonsoft.Json.Linq;

public class TranslationService
{
    private readonly AppSettings _appSettings;

    public TranslationService(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings.Value;
    }

    public string GetTranslationUrl(string text)
    {
        return $"{_appSettings.TranslationApi.BaseUrl}?" +
               $"q={Uri.EscapeDataString(text)}&" +
               $"langpair={_appSettings.TranslationApi.DefaultLangPair}";
    }

    public string GetResponsiveVoiceScriptUrl()
    {
        return $"{_appSettings.ResponsiveVoice.ScriptUrl}?key={_appSettings.ResponsiveVoice.ApiKey}";
    }
}
