using Application.Auth;
using Application.Loans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sistema_bancario_backend.Controllers;

[ApiController]
[Authorize]
[Route("api/loans")]
public sealed class LoansController(ILoanService loans, TokenInfo tokenInfo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await loans.GetVisibleAsync(tokenInfo.GetUserId(), tokenInfo.Role, cancellationToken);
        return new ObjectResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await loans.GetByIdAsync(id, tokenInfo.GetUserId(), tokenInfo.Role, cancellationToken);
        return new ObjectResult(result);
    }

    [HttpGet("{id:guid}/status")]
    public async Task<IActionResult> GetStatus(Guid id, CancellationToken cancellationToken)
    {
        var result = await loans.GetStatusAsync(id, tokenInfo.GetUserId(), tokenInfo.Role, cancellationToken);
        return new ObjectResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateLoanRequest request,
        CancellationToken cancellationToken)
    {
        var result = await loans.CreateAsync(request, tokenInfo.GetUserId(), tokenInfo.Role, cancellationToken);
        return new ObjectResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateLoanRequest request,
        CancellationToken cancellationToken)
    {
        var result = await loans.UpdateAsync(id, request, tokenInfo.GetUserId(), tokenInfo.Role, cancellationToken);
        return new ObjectResult(result);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var result = await loans.ApproveAsync(id, tokenInfo.GetUserId(), tokenInfo.Role, cancellationToken);
        return new ObjectResult(result);
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid id,
        RejectLoanRequest request,
        CancellationToken cancellationToken)
    {
        var result = await loans.RejectAsync(id, tokenInfo.GetUserId(), tokenInfo.Role, request, cancellationToken);
        return new ObjectResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await loans.DeleteAsync(id, tokenInfo.GetUserId(), tokenInfo.Role, cancellationToken);
        return new ObjectResult(result);
    }
}
