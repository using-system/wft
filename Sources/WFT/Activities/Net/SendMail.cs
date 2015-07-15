using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using WFT.Activities.Designers;

namespace WFT.Activities.Net
{
    /// <summary>
    /// Send a mail
    /// </summary>
    [Description("Send mail")]
    [Designer(typeof(SendMailDesigner))]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.MailToolbox.bmp")]
    [DesignerIcon("Resources/MailDesigner.bmp")]
    public class SendMail : AsyncCodeActivity
    {
        /// <summary>
        /// To mail header
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("To mail header")]
        public InArgument<MailAddressCollection> To { get; set; }

        /// <summary>
        /// From mail header
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("From mail header")]
        public InArgument<MailAddress> From { get; set; }

        /// <summary>
        /// Subject of the mail
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("Subject of the mail")]
        public InArgument<string> Subject { get; set; }

        /// <summary>
        /// Mail Attachments
        /// </summary>
        [Description("Mail Attachments")]
        public InArgument<Collection<Attachment>> Attachments { get; set; }

        /// <summary>
        /// CC mail header
        /// </summary>
        [Description("CC mail header")]
        public InArgument<MailAddressCollection> CC { get; set; }
        
        /// <summary>
        /// Bcc mail header
        /// </summary>
        [Description("Bcc mail header")]
        public InArgument<MailAddressCollection> Bcc { get; set; }

        /// <summary>
        /// Mail body
        /// <remarks>
        /// Mail body
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("Mail body")]
        public InArgument<string> Body { get; set; }

        /// <summary>
        /// SMTP Hostname
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("SMTP Hostname")]
        public InArgument<string> Host { get; set; }
        
        /// <summary>
        /// SMTP Port
        /// </summary>
        [Description("SMTP Port")]
        public InArgument<int> Port { get; set; }

        /// <summary>
        /// Specify If ssl is enabled
        /// </summary>
        [Description("Specify If ssl is enabled")]
        public InArgument<bool> EnableSsl { get; set; }

        /// <summary>
        /// SMTP Server UserName
        /// </summary>
        [Description("SMTP Server UserName")]
        public InArgument<string> UserName { get; set; }

        /// <summary>
        /// SMTP Server Password
        /// </summary>
        [Description("SMTP Server Password")]
        public InArgument<string> Password { get; set; }


        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {

            base.CacheMetadata(metadata);
        }



        protected override void Cancel(AsyncCodeActivityContext context)
        {
            SendMailAsyncResult sendMailAsyncResult = (SendMailAsyncResult)context.UserState;
            sendMailAsyncResult.Client.SendAsyncCancel();
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            MailMessage message = new MailMessage();
            message.From = this.From.Get(context);

            foreach (MailAddress address in this.To.Get(context))
            {
                message.To.Add(address);
            }

            if(CC != null)
            {
                MailAddressCollection ccList = this.CC.Get(context);
                if (ccList != null)
                {
                    foreach (MailAddress address in ccList)
                    {
                        message.CC.Add(address);
                    }
                }

            }

            if(Bcc != null)
            {
                MailAddressCollection bccList = this.Bcc.Get(context);
                if (bccList != null)
                {
                    foreach (MailAddress address in bccList)
                    {
                        message.Bcc.Add(address);
                    }
                }

            }

            if(Attachments != null)
            {
                Collection<Attachment> attachments = this.Attachments.Get(context);
                if (attachments != null)
                {
                    foreach (Attachment attachment in attachments)
                    {
                        message.Attachments.Add(attachment);
                    }
                }
            }


            message.Subject = Subject.Get(context);
            message.Body = Body.Get(context);

            SmtpClient client = new SmtpClient();
            client.Host = Host.Get(context);
            if (Port != null
                && Port.Get(context) > 0)
                client.Port = Port.Get(context);
            else
                Port = 25;

            if (EnableSsl != null)
                client.EnableSsl = EnableSsl.Get(context);
            else
                client.EnableSsl = false;

            if (UserName == null || String.IsNullOrEmpty(UserName.Get(context)))
            {
                client.UseDefaultCredentials = true;
            }
            else
            {
                client.UseDefaultCredentials = false;
                if (Password == null)
                    client.Credentials = new NetworkCredential(UserName.Get(context), String.Empty);
                else
                    client.Credentials = new NetworkCredential(UserName.Get(context), Password.Get(context));
            }


            var sendMailAsyncResult = new SendMailAsyncResult(client, message, callback, state);
            context.UserState = sendMailAsyncResult;
            return sendMailAsyncResult;
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            // Nothing needs to be done to wrap up the execution.
        }

        class SendMailAsyncResult : IAsyncResult
        {
            SmtpClient client;
            AsyncCallback callback;
            object asyncState;
            EventWaitHandle asyncWaitHandle;

            public bool CompletedSynchronously { get { return false; } }
            public object AsyncState { get { return this.asyncState; } }
            public WaitHandle AsyncWaitHandle { get { return this.asyncWaitHandle; } }
            public bool IsCompleted { get { return true; } }
            public SmtpClient Client { get { return client; } }

            public SendMailAsyncResult(SmtpClient client, MailMessage message, AsyncCallback callback, object state)
            {
                this.client = client;
                this.callback = callback;
                this.asyncState = state;
                this.asyncWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                client.SendCompleted += new SendCompletedEventHandler(SendCompleted);
                client.SendAsync(message, null);
            }

            void SendCompleted(object sender, AsyncCompletedEventArgs e)
            {
                this.asyncWaitHandle.Set();
                callback(this);
            }
        }
    }
}
