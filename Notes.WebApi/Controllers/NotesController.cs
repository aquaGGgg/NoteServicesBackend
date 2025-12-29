using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notes.Application.UseCases.Notes.CreateNote;
using Notes.Application.UseCases.Notes.DeleteNote;
using Notes.Application.UseCases.Notes.GetNote;
using Notes.Application.UseCases.Notes.ListNotes;
using Notes.Application.UseCases.Notes.UpdateNote;

namespace Notes.WebApi.Controllers;

[ApiController]
[Route("api/v1/notes")]
[Authorize]
public sealed class NotesController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromServices] CreateNoteHandler handler,
        [FromBody] CreateNoteCommand command,
        CancellationToken ct)
    {
        var created = await handler.Handle(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromServices] ListNotesHandler handler,
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0,
        [FromQuery] bool? archived = null,
        [FromQuery(Name = "q")] string? query = null,
        CancellationToken ct = default)
    {
        var result = await handler.Handle(new ListNotesQuery(limit, offset, archived, query), ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        [FromServices] GetNoteHandler handler,
        [FromRoute] int id,
        CancellationToken ct)
    {
        var note = await handler.Handle(new GetNoteQuery(id), ct);
        return Ok(note);
    }

    public sealed record UpdateBody(
        int ExpectedVersion,
        string? Title,
        string? Content,
        bool? IsArchived);

    [HttpPost("{id:int}/update")]
    public async Task<IActionResult> Update(
        [FromServices] UpdateNoteHandler handler,
        [FromRoute] int id,
        [FromBody] UpdateBody body,
        CancellationToken ct)
    {
        var updated = await handler.Handle(
            new UpdateNoteCommand(id, body.ExpectedVersion, body.Title, body.Content, body.IsArchived),
            ct);

        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        [FromServices] DeleteNoteHandler handler,
        [FromRoute] int id,
        CancellationToken ct)
    {
        await handler.Handle(new DeleteNoteCommand(id), ct);
        return NoContent();
    }
}
