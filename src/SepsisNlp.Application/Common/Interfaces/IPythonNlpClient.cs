using SepsisNlp.Application.Common.Models;

namespace SepsisNlp.Application.Common.Interfaces;

public interface IPythonNlpClient
{
    Task<InferenciasResponse?> ProcessarEvolucaoAsync(EvolucaoRequest request, CancellationToken cancellationToken);
}