using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relate.Smtp.Api.Models;
using Relate.Smtp.Api.Services;
using Relate.Smtp.Core.Interfaces;

namespace Relate.Smtp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmailsController : ControllerBase
{
    private readonly IEmailRepository _emailRepository;
    private readonly UserProvisioningService _userProvisioningService;

    public EmailsController(
        IEmailRepository emailRepository,
        UserProvisioningService userProvisioningService)
    {
        _emailRepository = emailRepository;
        _userProvisioningService = userProvisioningService;
    }

    [HttpGet]
    public async Task<ActionResult<EmailListResponse>> GetEmails(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var user = await _userProvisioningService.GetOrCreateUserAsync(User, cancellationToken);

        var skip = (page - 1) * pageSize;
        var emails = await _emailRepository.GetByUserIdAsync(user.Id, skip, pageSize, cancellationToken);
        var totalCount = await _emailRepository.GetCountByUserIdAsync(user.Id, cancellationToken);
        var unreadCount = await _emailRepository.GetUnreadCountByUserIdAsync(user.Id, cancellationToken);

        var items = emails.Select(e => e.ToListItemDto(user.Id)).ToList();

        return Ok(new EmailListResponse(items, totalCount, unreadCount, page, pageSize));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmailDetailDto>> GetEmail(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userProvisioningService.GetOrCreateUserAsync(User, cancellationToken);
        var email = await _emailRepository.GetByIdWithDetailsAsync(id, cancellationToken);

        if (email == null)
        {
            return NotFound();
        }

        // Check if user has access to this email
        if (!email.Recipients.Any(r => r.UserId == user.Id))
        {
            return NotFound();
        }

        return Ok(email.ToDetailDto(user.Id));
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<EmailDetailDto>> UpdateEmail(
        Guid id,
        [FromBody] UpdateEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userProvisioningService.GetOrCreateUserAsync(User, cancellationToken);
        var email = await _emailRepository.GetByIdWithDetailsAsync(id, cancellationToken);

        if (email == null)
        {
            return NotFound();
        }

        var recipient = email.Recipients.FirstOrDefault(r => r.UserId == user.Id);
        if (recipient == null)
        {
            return NotFound();
        }

        if (request.IsRead.HasValue)
        {
            recipient.IsRead = request.IsRead.Value;
        }

        await _emailRepository.UpdateAsync(email, cancellationToken);

        return Ok(email.ToDetailDto(user.Id));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEmail(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userProvisioningService.GetOrCreateUserAsync(User, cancellationToken);
        var email = await _emailRepository.GetByIdWithDetailsAsync(id, cancellationToken);

        if (email == null)
        {
            return NotFound();
        }

        if (!email.Recipients.Any(r => r.UserId == user.Id))
        {
            return NotFound();
        }

        await _emailRepository.DeleteAsync(id, cancellationToken);

        return NoContent();
    }

    [HttpGet("{id:guid}/attachments/{attachmentId:guid}")]
    public async Task<IActionResult> GetAttachment(
        Guid id,
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userProvisioningService.GetOrCreateUserAsync(User, cancellationToken);
        var email = await _emailRepository.GetByIdWithDetailsAsync(id, cancellationToken);

        if (email == null)
        {
            return NotFound();
        }

        if (!email.Recipients.Any(r => r.UserId == user.Id))
        {
            return NotFound();
        }

        var attachment = email.Attachments.FirstOrDefault(a => a.Id == attachmentId);
        if (attachment == null)
        {
            return NotFound();
        }

        return File(attachment.Content, attachment.ContentType, attachment.FileName);
    }
}
