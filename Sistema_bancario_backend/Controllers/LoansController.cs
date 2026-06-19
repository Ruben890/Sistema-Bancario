using Application.Auth;
using Application.Common;
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
        var result = User.IsInRole("Admin")
            ? await loans.GetAllAsync(cancellationToken)
            : await loans.GetByUserIdAsync(tokenInfo.GetUserId(), cancellationToken);

        return new ObjectResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await loans.GetByIdAsync(id, cancellationToken);
        if (result.Entity is null)
            return new ObjectResult(result);

        if (!CanAccess(result.Entity.UserId))
            return AccessDenied();

        return new ObjectResult(result);
    }

    [HttpGet("{id:guid}/status")]
    public async Task<IActionResult> GetStatus(Guid id, CancellationToken cancellationToken)
    {
        var loan = await loans.GetByIdAsync(id, cancellationToken);
        if (loan.Entity is null)
            return new ObjectResult(loan);

        if (!CanAccess(loan.Entity.UserId))
            return AccessDenied();

        var result = await loans.GetStatusAsync(id, cancellationToken);
        return new ObjectResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateLoanRequest request,
        CancellationToken cancellationToken)
    {
        if (!CanAccess(request.UserId))
            return AccessDenied();

        var result = await loans.CreateAsync(request, cancellationToken);
        return new ObjectResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateLoanRequest request,
        CancellationToken cancellationToken)
    {
        var current = await loans.GetByIdAsync(id, cancellationToken);
        if (current.Entity is null)
            return new ObjectResult(current);

        if (!CanAccess(current.Entity.UserId))
            return AccessDenied();

        var result = await loans.UpdateAsync(id, request, cancellationToken);
        return new ObjectResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var result = await loans.ApproveAsync(id, tokenInfo.GetUserId(), cancellationToken);
        return new ObjectResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid id,
        RejectLoanRequest request,
        CancellationToken cancellationToken)
    {
        var result = await loans.RejectAsync(id, tokenInfo.GetUserId(), request, cancellationToken);
        return new ObjectResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var current = await loans.GetByIdAsync(id, cancellationToken);
        if (current.Entity is null)
            return new ObjectResult(current);

        if (!CanAccess(current.Entity.UserId))
            return AccessDenied();

        var result = await loans.DeleteAsync(id, cancellationToken);
        return new ObjectResult(result);
    }

    private bool CanAccess(Guid ownerUserId)
        => User.IsInRole("Admin") || ownerUserId == tokenInfo.GetUserId();

    private static ObjectResult AccessDenied()
        => new ObjectResult(Result<object>.Failure(
            "LOAN_ACCESS_DENIED",
            "You cannot access this loan.",
            System.Net.HttpStatusCode.Forbidden));
}
