using EToto.Application.Dto;


namespace EToto.Application.Interfaces
{
    public interface IEquipamentoExcelParser
    {
        TemplateValidationResult ValidateTemplate(Stream excelStream);
        Task<ParsedEquipamentoFile> ParseAsync(Stream excelStream, CancellationToken ct = default);
    }
}
