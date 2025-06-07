using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GraphiGrade.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;

    public GroupController(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    //[HttpGet]
    //public async Task<IActionResult> GetGroupByIdAsync(int id, CancellationToken cancellationToken)
    //{
    //    AuthorizationResult authResult = await _authorizationService.AuthorizeAsync(User, username, Policy.SameUserOrAdmin);
    //
    //    if (!authResult.Succeeded)
    //    {
    //        return new ObjectResult(ServiceResultFactory<GetUserResponse>.CreateError(HttpStatusCode.Forbidden))
    //        {
    //            StatusCode = (int)HttpStatusCode.Forbidden
    //        };
    //    }
    //
    //
    //}
}
