using System.Configuration;

namespace BotTranslatorDemo.Utils
{
    public class Settings
    {
        public static string GetSubscriptionKey() => ConfigurationManager.AppSettings["SubscriptionKey"];

        public static string GetCognitiveServicesTokenUri() => ConfigurationManager.AppSettings["CognitiveServicesTokenUri"];

        public static string GetTranslatorUri() => ConfigurationManager.AppSettings["TranslatorUri"];
    }
}
