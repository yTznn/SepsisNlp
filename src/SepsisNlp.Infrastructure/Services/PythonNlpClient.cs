using System.Net.Http.Json;
using SepsisNlp.Application.Common.Interfaces;
using SepsisNlp.Application.Common.Models;

namespace SepsisNlp.Infrastructure.Services;

public class PythonNlpClient : IPythonNlpClient
{
    private readonly HttpClient _httpClient;

    public PythonNlpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<InferenciasResponse?> ProcessarEvolucaoAsync(EvolucaoRequest request, CancellationToken cancellationToken)
    {
        // Faz o POST batendo na rota que criamos no FastAPI em Python
        var response = await _httpClient.PostAsJsonAsync("/api/processar-evolucao", request, cancellationToken);

        response.EnsureSuccessStatusCode();

        // Converte o JSON do Python de volta para o Record do C#
        return await response.Content.ReadFromJsonAsync<InferenciasResponse>(cancellationToken: cancellationToken);
    }
}