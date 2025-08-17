using GraphiGrade.Data.Repositories.Abstractions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using GraphiGrade.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphiGrade.Business.Authorization.Policies.UserHasExercise;

public class UserHasExerciseHandler : AuthorizationHandler<UserHasExerciseRequirement, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public UserHasExerciseHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        UserHasExerciseRequirement requirement, 
        int exerciseId) // resource is exerciseId
    {
        string username = context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        Exercise? exercise = await _unitOfWork.Exercises.GetByIdWithIncludesAsync(exerciseId, 
            query => 
                query.Include(ex => ex.ExercisesGroups)
                        .ThenInclude(eg => eg.Group)
                        .ThenInclude(g => g.UsersGroups)
                        .ThenInclude(ug => ug.User));

        if (exercise == null)
        {
            return;
        }

        // Check if the user has the exercise assigned
        if (exercise.ExercisesGroups
            .Select(eg => eg.Group.UsersGroups)
            .Any(usersGroups => usersGroups != null &&
                                usersGroups.Any(ug => ug.User.Username == username)))
        {
            context.Succeed(requirement);
        }
    }
}
