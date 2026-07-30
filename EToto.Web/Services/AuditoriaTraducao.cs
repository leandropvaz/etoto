using System.Globalization;
using System.Text;

namespace EToto.Web.Services;

// Tradução amigável (PT-BR) dos campos e valores técnicos exibidos no diff de Auditoria.
// A auditoria captura os nomes de propriedade do EF e valores crus (enums como números,
// bool como true/false, datas em ISO). Aqui convertemos para algo legível ao usuário.
public static class AuditoriaTraducao
{
    // Rótulos amigáveis por nome técnico de campo (cobre os campos mais comuns das entidades
    // auditadas; o que não estiver aqui cai no humanizador de CamelCase).
    private static readonly Dictionary<string, string> CamposComuns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Id"] = "Código",
        ["Login"] = "Login",
        ["NomeCompleto"] = "Nome completo",
        ["Nome"] = "Nome",
        ["SenhaHash"] = "Senha (hash)",
        ["Perfil"] = "Perfil",
        ["Ativa"] = "Ativo",
        ["Ativo"] = "Ativo",
        ["PlantaId"] = "Planta",
        ["TipoVinculo"] = "Tipo de vínculo",
        ["NomeEmpresa"] = "Empresa",
        ["DataValidadeAcesso"] = "Validade do acesso",
        ["DataTreinamento"] = "Data do treinamento",
        ["DataValidadeTreinamento"] = "Validade do treinamento",
        ["TreinamentoConcluido"] = "Treinamento concluído",
        ["DataUltimoLogin"] = "Último login",
        ["DataCriacao"] = "Criado em",
        ["DataAtualizacao"] = "Atualizado em",
        ["CriadoPorId"] = "Criado por",
        ["CriadoEm"] = "Criado em",
        ["AlteradoPorId"] = "Alterado por",
        ["AlteradoEm"] = "Alterado em",
        // Campanhas de revalidação
        ["Status"] = "Status",
        ["Periodicidade"] = "Periodicidade",
        ["DataInicio"] = "Início",
        ["DataFimPrevista"] = "Conclusão prevista",
        ["DataFimReal"] = "Conclusão real",
        ["Notas"] = "Observações",
        // Itens de campanha
        ["CampanhaId"] = "Campanha",
        ["UsuarioId"] = "Usuário",
        ["Decisao"] = "Decisão",
        ["DecididoPorId"] = "Decidido por",
        ["DecididoEm"] = "Decidido em",
        ["Observacao"] = "Observação",
        ["SnapshotUsuarioJson"] = "Dados do usuário (snapshot)",
        // Plantas
        ["Codigo"] = "Código",
        ["Localizacao"] = "Localização",
        ["Descricao"] = "Descrição",
        // Comuns de processo (PLE / Avaliação de Risco)
        ["Numero"] = "Número",
        ["DataModificacao"] = "Modificado em",
        ["ModificadoPorId"] = "Modificado por",
        ["FinalizadoPorId"] = "Finalizado por",
        ["DataFinalizacao"] = "Finalizado em",
        ["DataFim"] = "Fim",
        ["MotivoCancelamento"] = "Motivo do cancelamento",
        ["IsDeleted"] = "Excluído",
        ["Departamento"] = "Departamento",
        ["Operacao"] = "Operação",
        ["Tarefa"] = "Tarefa",
        ["Data"] = "Data",
        ["Observacoes"] = "Observações",
        // Equipamento (propriedades em inglês na entidade)
        ["Tag"] = "Tag",
        ["EquipmentName"] = "Nome do equipamento",
        ["FactoryName"] = "Fábrica",
        ["RevisionInfo"] = "Revisão",
        ["LineNumber"] = "Linha",
        ["EnergyType"] = "Tipo de energia",
        ["HazardDescription"] = "Descrição do perigo",
        ["IsolationDeviceTag"] = "Tag do dispositivo de isolamento",
        ["IsolationDeviceLocation"] = "Local do dispositivo de isolamento",
        ["IsolationDeviceDescription"] = "Descrição do dispositivo de isolamento",
        ["LockoutType"] = "Tipo de bloqueio",
        ["ZeroEnergyVerification"] = "Verificação de energia zero",
        ["Test"] = "Teste",
        ["SourceExcelBlobUrl"] = "Arquivo Excel de origem",
        ["ImageBlobUrl"] = "Imagem",
        ["ImageNotes"] = "Notas da imagem",
        ["CreatedAt"] = "Criado em",
        ["UpdatedAt"] = "Atualizado em",
        ["CreateUserId"] = "Criado por",
        ["UpdateUserId"] = "Atualizado por",
    };

    private static readonly Dictionary<int, string> Perfil = new()
    {
        [1] = "Usuário",
        [2] = "Administrador",
        [3] = "Super Gestor",
        [4] = "Líder de Bloqueio",
        [5] = "Comando Central",
    };

    private static readonly Dictionary<int, string> Vinculo = new()
    {
        [1] = "Funcionário",
        [2] = "Parceiro",
    };

    private static readonly Dictionary<int, string> StatusCampanha = new()
    {
        [1] = "Planejada",
        [2] = "Em andamento",
        [3] = "Concluída",
        [4] = "Cancelada",
    };

    private static readonly Dictionary<int, string> Periodicidade = new()
    {
        [1] = "Mensal",
        [3] = "Trimestral",
        [6] = "Semestral",
        [12] = "Anual",
    };

    private static readonly Dictionary<int, string> Decisao = new()
    {
        [1] = "Manter",
        [2] = "Ajustar",
        [3] = "Revogar",
    };

    private static readonly Dictionary<int, string> StatusPle = new()
    {
        [1] = "Criado",
        [2] = "Em andamento",
        [3] = "Cancelado",
        [4] = "Início do desbloqueio",
        [5] = "Finalizado",
    };

    private static readonly Dictionary<int, string> StatusAvaliacaoRisco = new()
    {
        [1] = "Ativa",
        [2] = "Inativa",
    };

    public static string CampoAmigavel(string? tabela, string campo)
    {
        if (CamposComuns.TryGetValue(campo, out var rotulo))
            return rotulo;
        return Humanizar(campo);
    }

    public static string ValorAmigavel(string? tabela, string campo, string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor) || valor == "—")
            return "—";

        // Booleanos
        if (string.Equals(valor, "true", StringComparison.OrdinalIgnoreCase)) return "Sim";
        if (string.Equals(valor, "false", StringComparison.OrdinalIgnoreCase)) return "Não";

        // Enums / códigos (decodificados pelo nome do campo, com consciência da tabela quando ambíguo)
        if (int.TryParse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
        {
            var decodificado = DecodificarEnum(tabela, campo, n);
            if (decodificado is not null) return decodificado;
        }

        // Datas (ISO) — formata dd/MM/yyyy [HH:mm]
        if (valor.Contains('T') &&
            DateTime.TryParse(valor, CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind, out var dt))
        {
            return dt.TimeOfDay == TimeSpan.Zero
                ? dt.ToString("dd/MM/yyyy")
                : dt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
        }

        return valor;
    }

    private static string? DecodificarEnum(string? tabela, string campo, int valor)
    {
        switch (campo)
        {
            case "Perfil":
                return Perfil.TryGetValue(valor, out var p) ? p : null;
            case "TipoVinculo":
                return Vinculo.TryGetValue(valor, out var v) ? v : null;
            case "Periodicidade":
                return Periodicidade.TryGetValue(valor, out var per) ? per : null;
            case "Decisao":
                return Decisao.TryGetValue(valor, out var d) ? d : null;
            case "IsDeleted":
                return valor == 1 ? "Sim" : "Não";
            case "Status" when EhTabela(tabela, "CampanhasRevalidacao"):
                return StatusCampanha.TryGetValue(valor, out var s) ? s : null;
            case "Status" when EhTabela(tabela, "Ple", "Bloqueios"):
                return StatusPle.TryGetValue(valor, out var sp) ? sp : null;
            case "Status" when EhTabela(tabela, "AvaliacaoRisco", "AvaliacoesRisco"):
                return StatusAvaliacaoRisco.TryGetValue(valor, out var sa) ? sa : null;
            default:
                return null;
        }
    }

    // Casa o nome da tabela auditada com qualquer um dos candidatos (tolerante ao nome real no banco).
    private static bool EhTabela(string? tabela, params string[] candidatos)
    {
        if (string.IsNullOrWhiteSpace(tabela)) return false;
        foreach (var c in candidatos)
            if (tabela.Contains(c, StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    // Quebra CamelCase em palavras: "DataValidadeAcesso" -> "Data validade acesso".
    private static string Humanizar(string campo)
    {
        if (string.IsNullOrWhiteSpace(campo)) return campo;

        var sb = new StringBuilder(campo.Length + 8);
        for (int i = 0; i < campo.Length; i++)
        {
            var c = campo[i];
            if (i > 0 && char.IsUpper(c) && !char.IsUpper(campo[i - 1]))
                sb.Append(' ');
            sb.Append(i == 0 ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }
}
