using Interview.Backend.Responses;
using Interview.Domain;
using Interview.Domain.Tags;
using Interview.Domain.Tags.Records.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace Interview.Backend.Tags;

[ApiController]
[Route("api/tags")]
public class TagController(ITagService tagService) : ControllerBase
{
    /// <summary>
    /// Getting a available tags page.
    /// </summary>
    /// <param name="value">Tag value.</param>
    /// <param name="request">Page Parameters.</param>
    /// <returns>A page of questions with metadata about the pages.</returns>
    [Authorize]
    [HttpGet("tag")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPagedList<TagItem>), StatusCodes.Status200OK)]
    public Task<IPagedList<TagItem>> GetTagPage([FromQuery] string? value, [FromQuery] PageRequest request)
    {
        return tagService.FindTagsPageAsync(value, request.PageNumber, request.PageSize, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Creating a new tag.
    /// </summary>
    /// <param name="request">Tag edit request.</param>
    /// <returns>The object of the new question.</returns>
    [Authorize]
    [HttpPost("tag")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(TagItem), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public Task<ActionResult<TagItem>> CreateTag(TagEditRequest request)
    {
        return tagService.CreateTagAsync(request, HttpContext.RequestAborted).ToResponseAsync();
    }

    /// <summary>
    /// Updating the tag by ID.
    /// </summary>
    /// <param name="id">ID of the of tag.</param>
    /// <param name="request">The object with the tag data for which you need to update.</param>
    /// <returns>Updated question object.</returns>
    [Authorize]
    [HttpPut("tag/{id:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(TagItem), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public Task<ActionResult<TagItem>> UpdateTag(Guid id, TagEditRequest request)
    {
        return tagService.UpdateTagAsync(id, request, HttpContext.RequestAborted).ToResponseAsync();
    }
}
