using EToto.Domain.Enums;

namespace EToto.Domain.Entities
{
    // Registro de auditoria de uma operação CUD sobre uma entidade alvo (#5a).
    // Retenção permanente. Valores serializados em JSON.
    public class AuditoriaEntrada
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Nome da tabela/entidade afetada (ex.: "Usuarios").
        public string NomeTabela { get; set; } = default!;

        // Chave primária do registro afetado, serializada como string
        // (cobre int, Guid e PKs compostas como "5|7").
        public string ChaveRegistro { get; set; } = default!;

        public AcaoAuditoria Acao { get; set; }

        // Quem executou. Null quando rodou em contexto sem usuário identificado
        // (jobs, ImportTool, scripts SQL).
        public int? UsuarioId { get; set; }
        public Usuarios? Usuario { get; set; }

        public DateTime ExecutadoEm { get; set; } = DateTime.UtcNow;

        // JSON com o estado anterior. Null em Criar.
        public string? ValoresAntes { get; set; }

        // JSON com o estado posterior. Null em Excluir.
        public string? ValoresDepois { get; set; }
    }
}
