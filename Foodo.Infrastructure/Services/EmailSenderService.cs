using Foodo.Application.Abstraction;
using Foodo.Application.Models.Response;
using Foodo.Infrastructure.Helper;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;


namespace Foodo.Infrastructure.Services
{
	public class EmailSenderService : IEmailSenderService
	{
		private readonly IOptions<SmtpSettings> _options;

		public EmailSenderService(IOptions<SmtpSettings> options)
		{
			_options = options;
		}
		public async Task<ApiResponse> SendEmailAsync( string ReceiverEmail, string ReceiverName, string subject, string body,bool isHtml=false)
		{
			string SenderEmail = _options.Value.Login;
			string SenderName = "FoodoSupport";
			var message = new MimeMessage();
			message.From.Add(new MailboxAddress(SenderName, SenderEmail));
			message.To.Add(new MailboxAddress(ReceiverName, ReceiverEmail));
			message.Subject = subject;

			message.Body = new TextPart(isHtml ? "html" : "plain")
			{ 
				Text = body 
			};

			using var client = new SmtpClient() ;

			try
			{
				await client.ConnectAsync(_options.Value.SMTPServer, _options.Value.Port, false);

				// Note: only needed if the SMTP server requires authentication
				await client.AuthenticateAsync(_options.Value.Login, _options.Value.Password);

				await client.SendAsync(message);
			}
			catch
			{
				return ApiResponse.Failure("Email sending failed");
			}
			finally
			{
				await client.DisconnectAsync(true);

			}
			return ApiResponse.Success("Email sent successfully");

		}
	}
}
