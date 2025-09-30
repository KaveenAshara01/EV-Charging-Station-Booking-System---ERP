using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.Interfaces;
using Domain.Entities;

[ApiController]
[Route("api/[controller]")]
public class EvOwnerController : ControllerBase
{
    private readonly IOwnerRepository _ownerRepo;
    public EvOwnerController(IOwnerRepository ownerRepo) => _ownerRepo = ownerRepo;

    [HttpPost]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> Create(Owner owner)
    {
        await _ownerRepo.InsertAsync(owner);
        return Ok();
    }

    [HttpPut("{nic}")]
    [Authorize(Roles = "Backoffice,StationOperator,EVOwner")]
    public async Task<IActionResult> Update(string nic, Owner owner)
    {
        var existing = await _ownerRepo.FindByEmailAsync(nic);
        if (existing == null)
            return NotFound();
        // Update properties (example)
        existing.Name = owner.Name;
        existing.Email = owner.Email;
        // ...other properties...
        await _ownerRepo.InsertAsync(existing); // or UpdateAsync if available
        return Ok(existing);
    }

    [HttpDelete("{nic}")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> Delete(string nic)
    {
        var existing = await _ownerRepo.FindByEmailAsync(nic);
        if (existing == null)
            return NotFound();
        // Implement delete logic (e.g., set IsDeleted flag or remove from DB)
        // await _ownerRepo.DeleteAsync(nic); // if available
        return Ok();
    }

    [HttpPost("activate/{nic}")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> Activate(string nic)
    {
        var existing = await _ownerRepo.FindByEmailAsync(nic);
        if (existing == null)
            return NotFound();
        // Activate account
        existing.IsActive = true;
        await _ownerRepo.InsertAsync(existing); // or UpdateAsync
        return Ok(existing);
    }

    [HttpPost("deactivate/{nic}")]
    [Authorize(Roles = "Backoffice,EVOwner")]
    public async Task<IActionResult> Deactivate(string nic)
    {
        var existing = await _ownerRepo.FindByEmailAsync(nic);
        if (existing == null)
            return NotFound();
        // Deactivate account
        existing.IsActive = false;
        await _ownerRepo.InsertAsync(existing); // or UpdateAsync
        return Ok(existing);
    }
}
