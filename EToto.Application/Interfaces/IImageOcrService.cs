namespace EToto.Application.Interfaces
{
    public interface IImageOcrService
    {
        Task<string?> ExtractNotesAsync(Stream imageStream, CancellationToken ct = default);
    }
}
