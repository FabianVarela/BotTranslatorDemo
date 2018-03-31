using BotTranslatorDemo.Utils;
using Microsoft.Bot.Connector;

namespace BotTranslatorDemo.Translator
{
    public class TranslationHandler
    {
        public static string DetectLanguage(Activity activity) => DoLanguageDetection(activity.Text);

        public static string TranslateTextToDefaultLanguage(Activity activity, string inputLanguage)
        {
            if (inputLanguage != StringConstants.DefaultLanguage)
                return DoTranslation(activity.Text, inputLanguage, StringConstants.DefaultLanguage);

            return activity.Text;
        }

        public static string TranslateText(string inputText, string inputLocale, string outputLocale) =>
            DoTranslation(inputText, inputLocale, outputLocale);

        private static string DoTranslation(string inputText, string inputLocale, string outputLocale)
        {
            var translator = new Translator();
            var translation = translator.Translate(inputText, inputLocale, outputLocale);

            return translation;
        }

        private static string DoLanguageDetection(string input)
        {
            var translator = new Translator();
            return translator.Detect(input);
        }
    }
}
