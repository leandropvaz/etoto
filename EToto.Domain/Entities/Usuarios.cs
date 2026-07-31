using EToto.Domain.Enums;

namespace EToto.Domain.Entities
{
    public class Usuarios
    {
        public int Id { get; set; }
        public string Login { get; set; } = default!;
        public string NomeCompleto { get; set; } = default!;
        public string SenhaHash { get; set; } = default!;

        // Campo legado mantido por compatibilidade durante a transição para múltiplos perfis (#1a).
        // Plano de remoção: depois de #1b (claims/permissões) e #1c (UI) usarem somente a coleção `Perfis`,
        // este campo será removido em migration dedicada.
        public PerfilUsuario Perfil { get; set; }

        public bool Ativa { get; set; } = true;

        // Último login bem-sucedido (#6a)
        public DateTime? DataUltimoLogin { get; set; }

        // Auditoria de existência
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataAtualizacao { get; set; }

        // Auditoria de cadastro (quem criou / quem alterou)
        public int? CriadoPorId { get; set; }
        public Usuarios? CriadoPor { get; set; }
        public DateTime? CriadoEm { get; set; }

        public int? AlteradoPorId { get; set; }
        public Usuarios? AlteradoPor { get; set; }
        public DateTime? AlteradoEm { get; set; }

        // Vinculado à Planta (obrigatório para Usuário e Administrador)
        public int? PlantaId { get; set; }
        public Plantas? Planta { get; set; }

        // Treinamento LOTOTO
        public bool TreinamentoConcluido { get; set; }
        public DateTime? DataTreinamento { get; set; }
        // Validade do treinamento (#3) — distinta de DataTreinamento (data em que treinou).
        public DateTime? DataValidadeTreinamento { get; set; }

        public StatusValidadeAcesso StatusValidadeTreinamento(DateTime? referenciaUtc = null)
            => VinculoValidacao.AvaliarStatus(DataValidadeTreinamento, referenciaUtc);

        // Verdadeiro quando ao menos um dos perfis do usuário exige treinamento válido (#3).
        // Considera a coleção Perfis (multi-perfil de #1a/#1b); fallback ao campo legado.
        public bool ExigeTreinamentoValido()
        {
            var perfisAtivos = Perfis is { Count: > 0 }
                ? Perfis.Select(p => p.Perfil)
                : new[] { Perfil };
            return TreinamentoValidacao.AlgumPerfilExigeTreinamento(perfisAtivos);
        }

        // Vínculo Funcionário/Terceiro (#2)
        public TipoVinculo TipoVinculo { get; set; } = Enums.TipoVinculo.Funcionario;
        public string? NomeEmpresa { get; set; }
        public DateTime? DataValidadeAcesso { get; set; }

        public bool EhTerceiro => TipoVinculo == Enums.TipoVinculo.Terceiro;

        public StatusValidadeAcesso StatusValidade(DateTime? referenciaUtc = null)
            => VinculoValidacao.AvaliarStatus(DataValidadeAcesso, referenciaUtc);

        public ICollection<UsuarioPlanta> PlantasAssociadas { get; set; } = new List<UsuarioPlanta>();
        public ICollection<UsuarioPerfil> Perfis { get; set; } = new List<UsuarioPerfil>();

        public bool EhSuperGestor =>
            Perfis.Count > 0
                ? Perfis.Any(p => p.Perfil == PerfilUsuario.SuperGestor)
                : Perfil == PerfilUsuario.SuperGestor;

        public bool EhUsuarioFinal =>
            Perfis.Count > 0
                ? Perfis.Any(p => p.Perfil == PerfilUsuario.UsuarioFinal)
                : Perfil == PerfilUsuario.UsuarioFinal;

        public bool EhComandoCentral =>
            Perfis.Count > 0
                ? Perfis.Any(p => p.Perfil == PerfilUsuario.ComandoCentral)
                : Perfil == PerfilUsuario.ComandoCentral;

        public bool EhAdministrador =>
            Perfis.Count > 0
                ? Perfis.Any(p => p.Perfil == PerfilUsuario.Administrador)
                : Perfil == PerfilUsuario.Administrador;

        public void DefinirPerfis(IEnumerable<PerfilUsuario> perfis)
        {
            var distintos = perfis.Distinct().ToList();
            PerfilUsuarioValidacao.ValidarCombinacao(distintos);

            Perfis.Clear();
            foreach (var p in distintos)
            {
                Perfis.Add(new UsuarioPerfil
                {
                    UsuarioId = Id,
                    Perfil = p,
                    DataAssociacao = DateTime.UtcNow
                });
            }

            // Mantém o campo legado sincronizado: SuperGestor tem prioridade; caso contrário usa o primeiro.
            Perfil = distintos.Contains(PerfilUsuario.SuperGestor)
                ? PerfilUsuario.SuperGestor
                : distintos.First();
        }
    }
}
