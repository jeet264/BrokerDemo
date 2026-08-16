using BrokerOS.Application.QuickNotes;

namespace BrokerOS.Application.Abstractions;

public interface IQuickNoteService
{
    Task<QuickNoteDto> CreateAsync(CreateQuickNoteRequest request, CancellationToken cancellationToken);
}
