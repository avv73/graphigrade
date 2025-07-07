using GraphiGrade.Business.Authorization.Policies.Admin;
using GraphiGrade.Business.Authorization.Policies.SameUser;
using GraphiGrade.Business.Authorization.Policies.UserBelongsToGroup;
using GraphiGrade.Business.Authorization.Policies.UserHasExercise;

namespace GraphiGrade.Business.Authorization;

public static class Policy
{
    public const string Admin = nameof(AdminRequirement);

    public const string SameUser = nameof(SameUserRequirement);

    public const string UserBelongsToGroup = nameof(UserBelongsToGroupRequirement);

    public const string UserHasExercise = nameof(UserHasExerciseRequirement);
}
