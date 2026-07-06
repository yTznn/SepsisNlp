using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SepsisNlp.Application.Evolutions.Events;
using SepsisNlp.Application.Patients.Commands.ComplementDischargeData;
using SepsisNlp.Application.Patients.Commands.ImportPatient;
using SepsisNlp.Domain.Enums;
using System.Text;
using System.Text.RegularExpressions;

namespace SepsisNlp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImportsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IPublishEndpoint _publishEndpoint;

    public ImportsController(ISender sender, IPublishEndpoint publishEndpoint)
    {
        _sender = sender;
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost("seed-attendances")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue)]
    public async Task<IActionResult> ImportAttendancesCsv(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0) return BadRequest("Arquivo não enviado.");

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);

        string? currentMedicalRecord = null;
        string? currentName = null;
        var currentAttendances = new List<string>();
        int patientsImported = 0;

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            line = line.Replace("\uFEFF", "");
            var columns = line.Split(',');

            if (columns.Length > 8 && columns[2].Contains("Paciente"))
            {
                currentMedicalRecord = columns[3].Trim();
                currentName = columns.Length > 8 ? columns[8].Trim() : "";
                currentAttendances.Clear();
                continue;
            }

            if (columns.Length > 0 && long.TryParse(columns[0], out _))
            {
                currentAttendances.Add(columns[0].Trim());
                continue;
            }

            if (columns.Length > 1 && columns[1].Contains("Total do Paciente"))
            {
                if (currentMedicalRecord != null)
                {
                    var command = new ImportPatientWithAttendancesCommand(
                        currentMedicalRecord, currentName ?? "Desconhecido", new List<string>(currentAttendances));
                    await _sender.Send(command, cancellationToken);
                    patientsImported++;
                }
            }
        }
        return Ok(new { Message = "Semente importada com sucesso!", PatientsImported = patientsImported });
    }

    [HttpPost("assistential-evolutions")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue)]
    public async Task<IActionResult> ImportAssistentialEvolutionsCsv(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0) return BadRequest("Arquivo não enviado.");

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);

        string? currentAttendance = null;
        string? currentCid = null;
        string? currentEvolutionCode = null;
        DateOnly? currentBirthDate = null;

        EvolutionType currentType = EvolutionType.Assistencial;

        DateOnly currentDate = DateOnly.MinValue;
        TimeSpan currentTime = TimeSpan.Zero;

        var currentText = new StringBuilder();
        int evolutionsSentToQueue = 0;

        async Task FlushCurrentEvolutionAsync()
        {
            if (currentEvolutionCode != null && currentAttendance != null && currentText.Length > 0)
            {
                var lines = currentText.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                string? role = null; string? council = null; string? profName = null;

                for (int i = lines.Count - 1; i >= 0; i--)
                {
                    string cleanLine = lines[i].Trim(' ', '"', ',', '\r', '\n');
                    if (string.IsNullOrWhiteSpace(cleanLine)) { lines.RemoveAt(i); continue; }

                    bool isSig = false;
                    if (cleanLine.StartsWith("COREN") || cleanLine.StartsWith("CRM") || cleanLine.StartsWith("CRN") || cleanLine.StartsWith("CRP") || cleanLine.StartsWith("CREFITO") || cleanLine.StartsWith("RMS"))
                    { council = cleanLine; isSig = true; }
                    else if (new[] { "TECNICO", "TÉCNICO", "ENFERMEIRO", "ENFERMEIRA", "MEDICO", "MÉDICO", "FISIOTERAPEUTA", "NUTRICIONISTA", "PSICOLOGO", "PSICÓLOGO", "FONOAUDIOLOGO", "ASSISTENTE SOCIAL" }.Any(r => cleanLine.ToUpper().StartsWith(r)))
                    { role = cleanLine; isSig = true; }
                    else if (Regex.IsMatch(cleanLine, @"^\d+,,.+"))
                    {
                        var parts = cleanLine.Split(new[] { ",," }, StringSplitOptions.None);
                        if (parts.Length >= 2) { profName = parts[1].Trim(' ', '"', ','); isSig = true; }
                    }

                    if (isSig) lines.RemoveAt(i);
                }

                for (int j = 0; j < lines.Count; j++)
                {
                    string lineText = lines[j];
                    if (lineText.StartsWith("\"") && lineText.EndsWith("\"") && lineText.Length > 1)
                        lineText = lineText.Substring(1, lineText.Length - 2);
                    lines[j] = lineText.Replace("\"\"", "\"").Trim(',', ' ');
                }

                var evento = new EvolutionCsvRowReceivedEvent(
                    currentEvolutionCode, currentType, currentAttendance, currentCid,
                    currentDate, currentTime, role, council, profName, string.Join("\n", lines).Trim(), currentBirthDate
                );

                await _publishEndpoint.Publish(evento, cancellationToken);
                evolutionsSentToQueue++;
            }
        }

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            line = line.Replace("\uFEFF", "");
            var columns = line.Split(',');

            if (columns.Length > 6 && columns[0].Trim().Contains("Atendimento"))
            {
                await FlushCurrentEvolutionAsync();
                currentAttendance = columns[6].Trim();
                currentCid = null; currentEvolutionCode = null; currentText.Clear();

                var matchNasc = Regex.Match(line, @"(?i)Nascimento.*?(\d{2}/\d{2}/\d{4})");
                if (matchNasc.Success && DateOnly.TryParseExact(matchNasc.Groups[1].Value, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                {
                    currentBirthDate = parsedDate;
                }
                continue;
            }

            if (columns.Length > 0 && columns[0].Trim().Contains("Interna"))
            {
                int cidIdx = line.IndexOf("CID:");
                if (cidIdx != -1)
                {
                    var tokens = line[(cidIdx + 4)..].Replace(",", " ").Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length > 0) currentCid = tokens[0].Trim();
                }
                continue;
            }

            if (columns.Length > 4 && (columns[1].Trim().Contains("Anota") || columns[1].Trim().Contains("Evolu")))
            {
                await FlushCurrentEvolutionAsync();
                currentText.Clear();
                currentEvolutionCode = columns[4].Trim();

                int dateIdx = Array.IndexOf(columns, "Data:");
                int hourIdx = Array.IndexOf(columns, "Hora:");
                string dataStr = dateIdx > -1 && columns.Length > dateIdx + 1 ? columns[dateIdx + 1].Trim() : "";
                string horaStr = hourIdx > -1 && columns.Length > hourIdx + 1 ? columns[hourIdx + 1].Trim() : "";

                DateOnly.TryParseExact(dataStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out currentDate);
                TimeSpan.TryParse(horaStr, out currentTime);
                continue;
            }

            if (currentEvolutionCode != null)
            {
                if (line.StartsWith(",,Status:") || string.IsNullOrWhiteSpace(line.Replace(",", "").Trim())) continue;
                currentText.AppendLine(line);
            }
        }

        await FlushCurrentEvolutionAsync();
        return Ok(new { Message = "Processamento Assistencial iniciado!", EvolutionsSentToQueue = evolutionsSentToQueue });
    }

    [HttpPost("medical-evolutions")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue)]
    public async Task<IActionResult> ImportMedicalEvolutionsCsv(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0) return BadRequest("Arquivo não enviado.");

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);

        string? lastSeenId = null;
        string? currentAttendance = null;
        string? currentCid = null;
        string? currentEvolutionCode = null;
        string? currentDoctorName = null;
        string? currentDoctorCrm = null;
        DateOnly? currentBirthDate = null;

        EvolutionType currentType = EvolutionType.Clinica;
        DateOnly currentDate = DateOnly.MinValue;
        TimeSpan currentTime = TimeSpan.Zero;

        var currentText = new StringBuilder();
        int evolutionsSentToQueue = 0;

        async Task FlushCurrentEvolutionAsync()
        {
            if (currentEvolutionCode != null && currentAttendance != null && currentText.Length > 0)
            {
                var lines = currentText.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                for (int j = 0; j < lines.Count; j++)
                {
                    string lineText = lines[j];
                    if (lineText.StartsWith("\"") && lineText.EndsWith("\"") && lineText.Length > 1)
                        lineText = lineText.Substring(1, lineText.Length - 2);
                    lines[j] = lineText.Replace("\"\"", "\"").Trim(',', ' ');
                }

                var evento = new EvolutionCsvRowReceivedEvent(
                    currentEvolutionCode, currentType, currentAttendance, currentCid,
                    currentDate, currentTime, "MÉDICO", currentDoctorCrm, currentDoctorName, string.Join("\n", lines).Trim(), currentBirthDate
                );

                await _publishEndpoint.Publish(evento, cancellationToken);
                evolutionsSentToQueue++;
            }
        }

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            line = line.Replace("\uFEFF", "");
            var columns = line.Split(',');

            if (columns.Length > 8 && long.TryParse(columns[8].Trim(), out _))
            {
                lastSeenId = columns[8].Trim();
            }

            if (line.Contains(",Atendimento:"))
            {
                await FlushCurrentEvolutionAsync();

                currentAttendance = lastSeenId;
                currentCid = null; currentEvolutionCode = null; currentText.Clear();
                currentDoctorName = null; currentDoctorCrm = null;

                var matchNasc = Regex.Match(line, @"(?i)Nascimento.*?(\d{2}/\d{2}/\d{4})");
                if (matchNasc.Success && DateOnly.TryParseExact(matchNasc.Groups[1].Value, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                {
                    currentBirthDate = parsedDate;
                }

                var matchCid = Regex.Match(line, @"([A-Z]\d{2,3})");
                if (matchCid.Success) currentCid = matchCid.Groups[1].Value;

                continue;
            }

            if (line.Contains(",,,,,,Evolução:,,,") || line.StartsWith("Código,,,,,,Médico") || line.Contains("Data de Atendimento:")) continue;

            if (columns.Length > 0 && long.TryParse(columns[0].Trim(), out _))
            {
                var cleanCols = columns.Select(c => c.Trim()).Where(c => !string.IsNullOrEmpty(c)).ToList();

                if (cleanCols.Count >= 5 && DateOnly.TryParseExact(cleanCols[^2], "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var extractedDate) && TimeSpan.TryParse(cleanCols[^1], out var extractedTime))
                {
                    await FlushCurrentEvolutionAsync();
                    currentText.Clear();

                    currentEvolutionCode = cleanCols[0];
                    currentDate = extractedDate;
                    currentTime = extractedTime;

                    if (long.TryParse(cleanCols[1], out _))
                    { currentDoctorCrm = $"CRM {cleanCols[1]}"; currentDoctorName = cleanCols[2]; }
                    else
                    { currentDoctorCrm = "NÃO INFORMADO"; currentDoctorName = cleanCols[1]; }

                    continue;
                }
            }

            if (currentEvolutionCode != null)
            {
                if (string.IsNullOrWhiteSpace(line.Replace(",", "").Replace("\"", "").Trim())) continue;
                currentText.AppendLine(line);
            }
        }

        await FlushCurrentEvolutionAsync();
        return Ok(new { Message = "Processamento Clínico iniciado!", EvolutionsSentToQueue = evolutionsSentToQueue });
    }

    [HttpPost("complement-discharge")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue)]
    public async Task<IActionResult> ComplementDischargeCsv(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0) return BadRequest("Arquivo não enviado.");

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);

        string? currentCid = null;
        string? currentCidDescription = null;
        int recordsProcessed = 0;

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            line = line.Replace("\uFEFF", "").Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            // O TRUNFO: Divide por vírgula, limpa os espaços e remove TODAS as colunas vazias
            var columns = line.Split(',');
            var cleanCols = columns.Select(c => c.Trim()).Where(c => !string.IsNullOrEmpty(c)).ToList();

            if (cleanCols.Count == 0) continue;

            // 1. Identifica a linha de cabeçalho/quebra e pula
            if (line.Contains("Atendimento") && line.Contains("Dt Atendimento") && line.Contains("Data da Alta"))
            {
                continue;
            }

            // 2. Identifica a linha que define o CID e sua Descrição
            // Removendo os vazios, o CID vira o índice [0] e o hífen vira o índice [1]
            if (cleanCols.Count >= 2 && cleanCols[1] == "-")
            {
                currentCid = cleanCols[0];

                // Pega do índice 2 para frente e junta com vírgula novamente 
                // Isso protege descrições de CID que possuam vírgulas internas no texto
                currentCidDescription = string.Join(",", cleanCols.Skip(2)).Trim(' ', '"');
                continue;
            }

            // 3. Ignora linhas de totalizadores do CID
            if (line.Contains("Total de Altas do Cid:"))
            {
                continue;
            }

            // 4. Identifica a linha de dados do paciente (Se o primeiro item for o número do atendimento)
            // Com o filtro de vazios, as posições dos dados ficam perfeitamente fixas!
            if (cleanCols.Count >= 10 && long.TryParse(cleanCols[0], out _))
            {
                var realAttendance = cleanCols[0];     // Índice 0: Atendimento
                var realMedicalRecord = cleanCols[2];  // Índice 2: Prontuário
                var gender = cleanCols[5].ToUpper();    // Índice 5: Sexo (M ou F)
                var dischargeDateStr = cleanCols[9];    // Índice 9: Data da Alta

                DateOnly? dischargeDate = null;
                if (DateOnly.TryParseExact(dischargeDateStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                {
                    dischargeDate = parsedDate;
                }

                if (!string.IsNullOrEmpty(currentCid))
                {
                    var command = new SepsisNlp.Application.Patients.Commands.ComplementDischargeData.ComplementDischargeDataCommand(
                        realAttendance,
                        realMedicalRecord,
                        currentCid,
                        currentCidDescription,
                        gender,
                        dischargeDate
                    );

                    await _sender.Send(command, cancellationToken);
                    recordsProcessed++;
                }
            }
        }

        return Ok(new { Message = "Complemento de dados de alta concluído com sucesso!", RegistrosProcessados = recordsProcessed });
    }
}