using System.Text.Json;
using EToto.Domain.Entities;
using EToto.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EToto.Infrastructure.Data
{
    // Coleta entradas de auditoria a partir do ChangeTracker (#5a).
    // Conjunto de entidades auditadas é declarado em AuditedTypes — usuários, plantas,
    // equipamentos, bloqueios (PLE) e avaliações de risco, conforme playbook.
    public static class AuditoriaCapture
    {
        private static readonly HashSet<Type> AuditedTypes = new()
        {
            typeof(Usuarios),
            typeof(Plantas),
            typeof(Equipamento),
            typeof(Ple),
            typeof(PleEquipamento),
            typeof(PleHistorico),
            typeof(AvaliacaoRisco),
            typeof(AvaliacaoRiscoItem),
            typeof(AvaliacaoRiscoHistorico),
            // Campanhas de revalidação (#6b) — toda decisão precisa ficar registrada.
            typeof(CampanhaRevalidacao),
            typeof(ItemCampanhaRevalidacao)
        };

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
        };

        // Snapshot pré-SaveChanges — captura ValoresAntes (precisa rodar enquanto Modified/Deleted
        // ainda têm o OriginalValues acessível). Retorna uma lista parcial que é completada em
        // CompletePending após o SaveChanges (para pegar PKs geradas em Adicionados).
        public static List<PendingEntry> Snapshot(DbContext ctx, int? executorUsuarioId)
        {
            var pending = new List<PendingEntry>();
            var agoraUtc = DateTime.UtcNow;

            foreach (var entry in ctx.ChangeTracker.Entries())
            {
                if (entry.Entity is AuditoriaEntrada) continue; // nunca audita a si próprio
                if (!AuditedTypes.Contains(entry.Entity.GetType())) continue;

                AcaoAuditoria? acao = entry.State switch
                {
                    EntityState.Added => AcaoAuditoria.Criar,
                    EntityState.Modified => AcaoAuditoria.Atualizar,
                    EntityState.Deleted => AcaoAuditoria.Excluir,
                    _ => null
                };
                if (acao is null) continue;

                pending.Add(new PendingEntry
                {
                    Entry = entry,
                    Acao = acao.Value,
                    ExecutadoEm = agoraUtc,
                    UsuarioId = executorUsuarioId,
                    NomeTabela = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                    ValoresAntes = acao == AcaoAuditoria.Criar ? null : SerializeOriginal(entry),
                    ValoresDepois = acao == AcaoAuditoria.Excluir ? null : SerializeCurrent(entry)
                });
            }

            return pending;
        }

        // Após SaveChanges as PKs já estão preenchidas — gera as AuditoriaEntrada finais.
        public static List<AuditoriaEntrada> Materialize(List<PendingEntry> pending)
        {
            var result = new List<AuditoriaEntrada>(pending.Count);
            foreach (var p in pending)
            {
                result.Add(new AuditoriaEntrada
                {
                    Id = Guid.NewGuid(),
                    NomeTabela = p.NomeTabela,
                    ChaveRegistro = ExtractKey(p.Entry),
                    Acao = p.Acao,
                    UsuarioId = p.UsuarioId,
                    ExecutadoEm = p.ExecutadoEm,
                    ValoresAntes = p.ValoresAntes,
                    ValoresDepois = p.ValoresDepois
                });
            }
            return result;
        }

        private static string ExtractKey(EntityEntry entry)
        {
            var key = entry.Metadata.FindPrimaryKey();
            if (key is null) return string.Empty;

            var partes = key.Properties
                .Select(p => entry.Property(p.Name).CurrentValue?.ToString() ?? "")
                .ToArray();
            return string.Join("|", partes);
        }

        private static string SerializeOriginal(EntityEntry entry)
        {
            var dict = new Dictionary<string, object?>();
            foreach (var prop in entry.Metadata.GetProperties())
            {
                dict[prop.Name] = entry.Property(prop.Name).OriginalValue;
            }
            return JsonSerializer.Serialize(dict, JsonOpts);
        }

        private static string SerializeCurrent(EntityEntry entry)
        {
            var dict = new Dictionary<string, object?>();
            foreach (var prop in entry.Metadata.GetProperties())
            {
                dict[prop.Name] = entry.Property(prop.Name).CurrentValue;
            }
            return JsonSerializer.Serialize(dict, JsonOpts);
        }

        // Estado intermediário — guarda a EntityEntry para extrair a PK após Save.
        public sealed class PendingEntry
        {
            public EntityEntry Entry { get; set; } = default!;
            public AcaoAuditoria Acao { get; set; }
            public int? UsuarioId { get; set; }
            public DateTime ExecutadoEm { get; set; }
            public string NomeTabela { get; set; } = default!;
            public string? ValoresAntes { get; set; }
            public string? ValoresDepois { get; set; }
        }
    }
}
