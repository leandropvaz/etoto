namespace EToto.Application.Interfaces
{
    public class EmailMensagem
    {
        public IReadOnlyList<string> Destinatarios { get; set; } = Array.Empty<string>();
        public string Assunto { get; set; } = string.Empty;
        public string Corpo { get; set; } = string.Empty;
        public bool CorpoEhHtml { get; set; } = false;
    }

    public interface IEmailService
    {
        Task<bool> EnviarAsync(EmailMensagem mensagem, CancellationToken ct = default);
    }
}
