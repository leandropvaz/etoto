namespace EToto.ImportTool
{
    public interface IBatchImportService
    {
        Task ImportBatchAsync(int plantaId, string directoryPath, CancellationToken ct = default);
    }
}
