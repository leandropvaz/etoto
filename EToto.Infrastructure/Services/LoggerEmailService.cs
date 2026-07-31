using EToto.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace EToto.Infrastructure.Services
{
    // Implementação default de IEmailService — apenas registra a mensagem no log.
    // Troque por uma implementação SMTP/SendGrid/Azure Communication quando o ambiente
    // tiver as credenciais configuradas. Toda a camada de campanhas (#6b) continua
    // funcionando com esta implementação durante o desenvolvimento.
    public class LoggerEmailService : IEmailService
    {
        private readonly ILogger<LoggerEmailService> _logger;

        public LoggerEmailService(ILogger<LoggerEmailService> logger)
        {
            _logger = logger;
        }

        public Task<bool> EnviarAsync(EmailMensagem mensagem, CancellationToken ct = default)
        {
            _logger.LogInformation(
                "[EMAIL-STUB] Para: {Destinatarios}; Assunto: {Assunto}; Corpo: {Corpo}",
                string.Join(", ", mensagem.Destinatarios),
                mensagem.Assunto,
                mensagem.Corpo);

            return Task.FromResult(true);
        }
    }
}
