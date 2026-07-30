// EToto.Infrastructure/DependencyInjection.cs
using Azure.Storage.Blobs;
using EToto.Application.Interfaces;
using EToto.Application.Services;
using EToto.Domain.Interfaces;
using EToto.Infrastructure.Data;
using EToto.Infrastructure.Data.Repositories;
using EToto.Infrastructure.Data.UnitOfWork;
using EToto.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace EToto.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        var connectionString = configuration.GetConnectionString("Lototo");
        services.AddDbContext<LototoContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddSingleton<BlobServiceClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var connString = config.GetConnectionString("BlobStorage");
            return new BlobServiceClient(connString);
        });

        // Repositories — TryAdd para permitir que projetos host sobrescrevam.
        services.AddScoped<IEquipamentoRepository, EquipamentoRepository>();
        services.TryAddScoped<IUsuarioRepository, UsuarioRepository>();
        services.TryAddScoped<IPlantaRepository, PlantaRepository>();
        services.TryAddScoped<IUnitOfWork, UnitOfWork>();

        // Executor context (#5a auditoria). Default = anônimo; Web sobrescreve com BlazorExecutorContext.
        services.TryAddScoped<IExecutorContext, AnonymousExecutorContext>();

        // Infrastructure Services
        services.AddScoped<IEquipamentoExcelParser, EquipmentExcelParser>();
        services.AddScoped<IBlobStorageService, AzureBlobStorageService>();
        services.AddScoped<IBlobDownloadService, BlobDownloadService>();
        services.AddScoped<IImageOcrService, AzureImageOcrService>();

        // Application Services
        services.AddScoped<IEquipamentoAppService, EquipamentoAppService>();

        return services;
    }
}