using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreOrderConfirmationEmailToPatient
{
    public class MSGraphApiService
    {
        private static object _lock = new object();
        private static MSGraphApiService _instance;
        private AppConfig _appConfig;
        private MSGraphApiService(AppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        public static MSGraphApiService GetInstance(AppConfig appConfig)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new MSGraphApiService(appConfig);
                    }
                }
            }
            return _instance;
        }


        public async Task SendEmail(string subject, string Body, string fromEmailAddress, List<string> toEmailAddresses, List<string> ccEmailAddresses = null, Dictionary<string, byte[]> attachments = null, int failureCount = 0)
        {
            try
            {
                var graphServiceClient = GetGraphClient();
                var toRecipients = new List<Recipient>();
                var ccRecipients = new List<Recipient>();
                var messageAttachments = new List<Attachment>();
                foreach (var toEmailAddress in toEmailAddresses)
                {
                    toRecipients.Add(
                    new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = toEmailAddress.Trim()
                        }
                    });
                }
                if (ccEmailAddresses != null && ccEmailAddresses.Count > 0)
                {

                    foreach (var ccEmailAddress in ccEmailAddresses)
                    {
                        ccRecipients.Add(
                                            new Recipient
                                            {
                                                EmailAddress = new EmailAddress
                                                {
                                                    Address = ccEmailAddress
                                                }
                                            });
                    }
                }
                if (attachments != null && attachments.Count > 0)
                {
                    foreach (var attachment in attachments)
                    {
                        messageAttachments.Add(new FileAttachment
                        {
                            ContentBytes = attachment.Value,
                            Name = attachment.Key
                        });
                    }
                }
                var requestBody = new SendMailPostRequestBody
                {
                    Message = new Message
                    {
                        Subject = subject,
                        Body = new ItemBody
                        {
                            ContentType = BodyType.Html,
                            Content = Body
                        },
                        ToRecipients = toRecipients,
                        CcRecipients = ccRecipients,
                        Attachments = messageAttachments
                    }
                };

                await graphServiceClient.Users[fromEmailAddress].SendMail.PostAsync(requestBody);
            }
            catch (ServiceException ex)
            {
                string message = ex.Message;
                if (failureCount == 0) // retry atleast once incase of token expiration
                {
                    failureCount++;
                    await SendEmail(subject, Body, fromEmailAddress, toEmailAddresses, ccEmailAddresses, attachments, failureCount);
                }
                else throw;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                throw;
            }
        }

        private GraphServiceClient GetGraphClient()
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            // Values from app registration
            var clientId = _appConfig.AppId;
            var tenantId = _appConfig.TenantId;
            var clientSecret = _appConfig.AppSecret;

            // using Azure.Identity;
            var options = new ClientSecretCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            };

            // https://learn.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
            var clientSecretCredential = new ClientSecretCredential(
                tenantId, clientId, clientSecret, options);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);
            return graphClient;
        }

    }
}