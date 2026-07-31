namespace EToto.Application.Dto
{
    public class UsuariosDto
    {
        public int Id { get; set; }

        public string Login { get; set; } = string.Empty;

        public string NomeCompleto { get; set; } = string.Empty;

        public string Senha { get; set; } = string.Empty; // senha em texto (somente entrada)

        // Perfil "primário" (legado): SuperGestor quando presente, senão o primeiro. Mantido por compat.
        public int Perfil { get; set; }

        // Múltiplos perfis (#1c). Quando vazio, o serviço cai para `Perfil` legado.
        public List<int> Perfis { get; set; } = new();

        public bool Ativa { get; set; }

        public int? PlantaId { get; set; }

        public List<int> PlantasIds { get; set; } = new();

        public bool TreinamentoConcluido { get; set; }
        public DateTime? DataTreinamento { get; set; }
        // Validade do treinamento (#3) — diferente de DataTreinamento.
        public DateTime? DataValidadeTreinamento { get; set; }

        // Vínculo Funcionário/Terceiro (#2). Default = 1 (Funcionario) coerente com a entidade.
        public int TipoVinculo { get; set; } = 1;
        public string? NomeEmpresa { get; set; }
        public DateTime? DataValidadeAcesso { get; set; }

        // Auditoria de cadastro (#1a) — saída
        public int? CriadoPorId { get; set; }
        public string? CriadoPorNome { get; set; }
        public DateTime? CriadoEm { get; set; }

        public int? AlteradoPorId { get; set; }
        public string? AlteradoPorNome { get; set; }
        public DateTime? AlteradoEm { get; set; }

        // Quem está executando a operação (entrada). O serviço usa para preencher CriadoPorId/AlteradoPorId.
        public int? ExecutadoPorId { get; set; }
    }
}
