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
public class ProfileController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailRepository _emailRepository;
    private readonly UserProvisioningService _userProvisioningService;

    public ProfileController(
        IUserRepository userRepository,
        IEmailRepository emailRepository,
        UserProvisioningService userProvisioningService)
    {
        _userRepository = userRepository;
        _emailRepository = emailRepository;
        _userProvisioningService = userProvisioningService;
    }

    [HttpGet]
    public async Task<ActionResult<ProfileDto>> GetProfile(CancellationToken cancellationToken = default)
    {
        var user = await _userProvisioningService.GetOrCreateUserAsync(User, cancellationToken);
        return Ok(user.ToDto());
    }

    [HttpPut]
    public async Task<ActionResult<ProfileDto>> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userProvisioningService.GetOrCreateUserAsync(User, cancellationToken);

        if (request.DisplayName != null)
        {
            user.DisplayName = request.DisplayName;
        }

        await _userRepository.UpdateAsync(user, cancellationToken);

        return Ok(user.ToDto());
    }

    [HttpPost("addresses")]
    public async Task<ActionResult<EmailAddressDto>> AddEmailAddress(
        [FromBody] AddEmailAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userProvisioningService.GetOrCreateUserAsync(User, cancellationToken);

        // Check if address already exists
        if (user.Email.Equals(request.Address, StringComparison.OrdinalIgnoreCase) ||
            user.AdditionalAddresses.Any(a => a.Address.Equals(request.Address, StringComparison.OrdinalIgnoreCase)))
        {
            return BadRequest(new { error = "Email address already registered" });
        }

        var address = new UserEmailAddress
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Address = request.Address,
            IsVerified = false, // Would need email verification in production
            AddedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.AddEmailAddressAsync(address, cancellationToken);

        // Link existing emails to this user
        await _emailRepository.LinkEmailsToUserAsync(user.Id, new[] { request.Address }, cancellationToken);

        return Ok(new EmailAddressDto(address.Id, address.Address, address.IsVerified, address.AddedAt));
    }

    [HttpDelete("addresses/{addressId:guid}")]
    public async Task<IActionResult> RemoveEmailAddress(Guid addressId, CancellationToken cancellationToken = default)
    {
        var user = await _userProvisioningService.GetOrCreateUserAsync(User, cancellationToken);

        var address = user.AdditionalAddresses.FirstOrDefault(a => a.Id == addressId);
        if (address == null)
        {
            return NotFound();
        }

        await _userRepository.RemoveEmailAddressAsync(addressId, cancellationToken);

        return NoContent();
    }
}
