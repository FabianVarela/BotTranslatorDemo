using Microsoft.Bot.Builder.Dialogs;
using System;

namespace BotTranslatorDemo.Translator
{
    public class StateHelper
    {
        public static void SetUserLanguageCode(IDialogContext context, string languageCode)
        {
            try
            {
                context.UserData.SetValue("LanguageCode", languageCode);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
