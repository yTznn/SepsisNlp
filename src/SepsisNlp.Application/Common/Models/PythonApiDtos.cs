using System.Text.Json.Serialization;

namespace SepsisNlp.Application.Common.Models;

public record EvolucaoRequest(
    [property: JsonPropertyName("texto_clinico")] string TextoClinico,
    [property: JsonPropertyName("cenario")] int Cenario
);

public record InferenciasResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("predicao")] string Predicao,
    [property: JsonPropertyName("confianca")] float Confianca,
    [property: JsonPropertyName("modelo")] string Modelo,
    [property: JsonPropertyName("hardware_utilizado")] string HardwareUtilizado
);