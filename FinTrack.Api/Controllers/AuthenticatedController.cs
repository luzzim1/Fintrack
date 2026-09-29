using FinTrack.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Controllers;

[ApiController]
[Authorize]
public abstract class AuthenticatedController : ControllerBase
{
    protected Guid UserId => Guid.TryParse(User.FindFirst("sub")?.Value, out var id) && id != Guid.Empty
        ? id : throw new AuthenticationException();
}
