using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using BotTranslatorDemo.Extensions;
using BotTranslatorDemo.Models;
using BotTranslatorDemo.Translator;
using BotTranslatorDemo.Utils;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;

namespace BotTranslatorDemo
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            Trace.TraceInformation($"Incoming Activity is {activity.ToJson()}");

            if (activity.Type == ActivityTypes.Message)
            {
                if (!string.IsNullOrEmpty(activity.Text))
                {
                    var userLanguage = TranslationHandler.DetectLanguage(activity);
                    var message = activity as IMessageActivity;

                    try
                    {
                        using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                        {
                            var botDataStore = scope.Resolve<IBotDataStore<BotData>>();
                            var key = new AddressKey
                            {
                                BotId = message.Recipient.Id,
                                ChannelId = message.ChannelId,
                                UserId = message.From.Id,
                                ConversationId = message.Conversation.Id,
                                ServiceUrl = message.ServiceUrl
                            };

                            var userData = await botDataStore.LoadAsync(key, BotStoreType.BotUserData, CancellationToken.None);
                            var storedLanguageCode = userData.GetProperty<string>(StringConstants.UserLanguageKey);

                            if (storedLanguageCode != userLanguage)
                            {
                                userData.SetProperty(StringConstants.UserLanguageKey, userLanguage);

                                await botDataStore.SaveAsync(key, BotStoreType.BotUserData, userData, CancellationToken.None);
                                await botDataStore.FlushAsync(key, CancellationToken.None);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    activity.Text = TranslationHandler.TranslateTextToDefaultLanguage(activity, userLanguage);

                    await Conversation.SendAsync(activity, MakeRoot);
                }
            }
            else
            {
                HandleSystemMessageAsync(activity);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        internal static IDialog<object> MakeRoot()
        {
            try
            {
                return Chain.From(() => new ChatDialog());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<Activity> HandleSystemMessageAsync(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                IConversationUpdateActivity conversationupdate = message;

                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                {
                    var client = scope.Resolve<IConnectorClient>();
                    if (conversationupdate.MembersAdded.Any())
                    {
                        var reply = message.CreateReply();
                        foreach (var newMember in conversationupdate.MembersAdded)
                        {
                            if (newMember.Id == message.Recipient.Id)
                            {
                                reply.Text = ChatResponse.Greeting;

                                await client.Conversations.ReplyToActivityAsync(reply);
                            }
                        }
                    }
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}
