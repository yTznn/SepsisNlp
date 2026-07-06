using MassTransit;
using MediatR;
using SepsisNlp.Application.Evolutions.Commands.ImportEvolution;
using SepsisNlp.Application.Evolutions.Events;

namespace SepsisNlp.Application.Evolutions.Consumers;

public class EvolutionCsvRowReceivedConsumer : IConsumer<EvolutionCsvRowReceivedEvent>
{
    private readonly ISender _sender;

    public EvolutionCsvRowReceivedConsumer(ISender sender)
    {
        _sender = sender;
    }

    public async Task Consume(ConsumeContext<EvolutionCsvRowReceivedEvent> context)
    {
        var message = context.Message;

        var command = new ImportEvolutionCommand(
            message.OriginalEvolutionCode,
            message.Type,
            message.AttendanceNumber,
            message.Cid,
            message.EvolutionDate,
            message.EvolutionTime,
            message.ProfessionalRole,
            message.ProfessionalCouncil,
            message.ProfessionalName,
            message.RawText,
            message.PatientBirthDate
        );

        await _sender.Send(command);
    }
}