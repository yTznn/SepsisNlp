using System.Text.Json.Serialization;

namespace SepsisNlp.Application.Common.Models;

// O C# agora manda o modelo e o prompt
public record EvolucaoRequest(string texto_clinico, int cenario, string modelo_alvo, int prompt_id);

// O C# agora recebe o texto_processado de volta (caso tenha sido traduzido)
public record InferenciasResponse(
    string status,
    string predicao,
    float confianca,
    string modelo,
    string hardware_utilizado,
    string? texto_processado
);