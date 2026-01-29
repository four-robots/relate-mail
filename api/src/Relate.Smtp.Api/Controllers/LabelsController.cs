using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relate.Smtp.Api.Models;
using Relate.Smtp.Api.Services;
using Relate.Smtp.Core.Entities;
using Relate.Smtp.Core.Interfaces;

namespace Relate.Smtp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LabelsController : ControllerBase
{
    private readonly ILabelRepository _labelRepository;
    private readonly IEmailLabelRepository _emailLabelRepository;
    private readonly IEmailRepository _emailRepository;
    private readonly UserProvisioningService _userProvisioningService;

    public LabelsController(
        ILabelRepository labelRepository,
        IEmailLabelRepository emailLabelRepository,
        IEmailRepository emailRepository,
        UserProvisioningService userProvisioningService)
    {
        _labelRepository = labelRepository;
        _emailLabelRepository = emailLabelRepository;
        _emailRepository = emailRepository;
        _userProvisioningService = userProvisioningService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LabelDto>>> GetLabels(CancellationToken cancellationToken = default)
    {
        var user = await _userProvisioningService.GetOrCreateUserAsync(User, cancellationToken);
        var labels = await _labelRepository.GetByUserIdAsync(user.Id, cancellationToken);
        return Ok(labels.Select(l => l.ToDto()).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<LabelDto>> CreateLabel(
        [FromBody] CreateLabelRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userProvisioningService.GetOrCreateUserAsync(User, cancellationToken);

        var label = new Label
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = request.Name,
            Color = request.Color,
            SortOrder = request.SortOrder,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _labelRepository.AddAsync(label, cancellationToken);
        return CreatedAtAction(nameof(GetLabels), new { id = label.Id }, label.ToDto());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<LabelDto>> UpdateLabel(
        Guid id,
        [FromBody] UpdateLabelRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userProvisioningService.GetOrCreateUserAsync(User, cancellationToken);
        var label = await _labelRepository.GetByIdAsync(id, cancellationToken);

        if (label == null || label.UserId != user.Id)
        {
            return NotFound();
        }

        if (request.Name != null)
        {
            label.Name = request.Name;
        }

        if (request.Color != null)
        {
            label.Color = request.Color;
        }

        if (request.SortOrder.HasValue)
        {
            label.SortOrder = request.SortOrder.Value;
        }

        await _labelRepository.UpdateAsync(label, cancellationToken);
        return Ok(label.ToDto());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteLabel(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userProvisioningService.GetOrCreateUserAsync(User, cancellationToken);
        var label = await _labelRepository.GetByIdAsync(id, cancellationToken);

        if (label == null || label.UserId != user.Id)
        {
            return NotFound();
        }

        await _labelRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("emails/{emailId:guid}")]
    public async Task<IActionResult> AddLabelToEmail(
        Guid emailId,
        [FromBody] AddLabelRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userProvisioningService.GetOrCreateUserAsync(User, cancellationToken);

        // Verify email access
        var email = await _emailRepository.GetByIdWithDetailsAsync(emailId, cancellationToken);
        if (email == null || !email.Recipients.Any(r => r.UserId == user.Id))
        {
            return NotFound("Email not found");
        }

        // Verify label ownership
        var label = await _labelRepository.GetByIdAsync(request.LabelId, cancellationToken);
        if (label == null || label.UserId != user.Id)
        {
            return NotFound("Label not found");
        }

        var emailLabel = new EmailLabel
        {
            Id = Guid.NewGuid(),
            EmailId = emailId,
            UserId = user.Id,
            LabelId = request.LabelId,
            AssignedAt = DateTimeOffset.UtcNow
        };

        await _emailLabelRepository.AddAsync(emailLabel, cancellationToken);
        return Ok();
    }

    [HttpDelete("emails/{emailId:guid}/{labelId:guid}")]
    public async Task<IActionResult> RemoveLabelFromEmail(
        Guid emailId,
        Guid labelId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userProvisioningService.GetOrCreateUserAsync(User, cancellationToken);

        // Verify email access
        var email = await _emailRepository.GetByIdWithDetailsAsync(emailId, cancellationToken);
        if (email == null || !email.Recipients.Any(r => r.UserId == user.Id))
        {
            return NotFound("Email not found");
        }

        await _emailLabelRepository.DeleteAsync(emailId, labelId, cancellationToken);
        return NoContent();
    }

    [HttpGet("{labelId:guid}/emails")]
    public async Task<ActionResult<EmailListResponse>> GetEmailsByLabel(
        Guid labelId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var user = await _userProvisioningService.GetOrCreateUserAsync(User, cancellationToken);

        // Verify label ownership
        var label = await _labelRepository.GetByIdAsync(labelId, cancellationToken);
        if (label == null || label.UserId != user.Id)
        {
            return NotFound("Label not found");
        }

        var skip = (page - 1) * pageSize;
        var emails = await _emailLabelRepository.GetEmailsByLabelIdAsync(user.Id, labelId, skip, pageSize, cancellationToken);
        var totalCount = await _emailLabelRepository.GetEmailCountByLabelIdAsync(user.Id, labelId, cancellationToken);
        var unreadCount = await _emailRepository.GetUnreadCountByUserIdAsync(user.Id, cancellationToken);

        var items = emails.Select(e => e.ToListItemDto(user.Id)).ToList();

        return Ok(new EmailListResponse(items, totalCount, unreadCount, page, pageSize));
    }
}

public record AddLabelRequest(Guid LabelId);
