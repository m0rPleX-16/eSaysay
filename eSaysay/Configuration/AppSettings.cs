namespace eSaysay.Configuration
{
    public class AppSettings
    {
        public string EncryptionKey { get; set; }
        public TranslationApiSettings TranslationApi { get; set; }
        public ResponsiveVoiceSettings ResponsiveVoice { get; set; }
    }

    public class TranslationApiSettings
    {
        public string BaseUrl { get; set; }
        public string DefaultLangPair { get; set; }
    }

    public class ResponsiveVoiceSettings
    {
        public string ScriptUrl { get; set; }
        public string ApiKey { get; set; }
    }
}
