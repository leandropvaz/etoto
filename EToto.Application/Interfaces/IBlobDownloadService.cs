using System;
using System.Collections.Generic;
using System.Text;

namespace EToto.Application.Interfaces
{
    public interface IBlobDownloadService
    {
        /// <summary>
        /// Baixa um arquivo específico do blob storage
        /// </summary>
        Task<BlobDownloadResult> DownloadFileAsync(
            string containerName,
            string blobName,
            string localFilePath,
            CancellationToken ct = default);

        /// <summary>
        /// Baixa múltiplos arquivos de um container
        /// </summary>
        Task<List<BlobDownloadResult>> DownloadFilesAsync(
            string containerName,
            string localDirectoryPath,
            string? prefixFilter = null,
            CancellationToken ct = default);

        /// <summary>
        /// Baixa um arquivo e retorna como Stream (sem salvar em disco)
        /// </summary>
        Task<Stream> DownloadToStreamAsync(
            string containerName,
            string blobName,
            CancellationToken ct = default);

        /// <summary>
        /// Verifica se um blob existe
        /// </summary>
        Task<bool> ExistsAsync(
            string containerName,
            string blobName,
            CancellationToken ct = default);

        /// <summary>
        /// Lista todos os blobs de um container
        /// </summary>
        Task<List<BlobInfo>> ListBlobsAsync(
            string containerName,
            string? prefixFilter = null,
            CancellationToken ct = default);
    }

    public class BlobDownloadResult
    {
        public bool Success { get; set; }
        public string BlobName { get; set; } = string.Empty;
        public string LocalFilePath { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime DownloadedAt { get; set; }
    }

    public class BlobInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Container { get; set; } = string.Empty;
        public long SizeInBytes { get; set; }
        public DateTimeOffset? LastModified { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }
}
