using GraphiGrade.Business.Authorization.Policies.Abstractions;
using GraphiGrade.Business.Authorization.Policies.Admin;
using GraphiGrade.Business.Authorization.Policies.SameUser;
using GraphiGrade.Business.Authorization.Policies.UserBelongsToGroup;
using GraphiGrade.Business.Authorization.Policies.UserHasExercise;

namespace GraphiGrade.Business.Authorization;

public static class RequirementsFactory
{
    private static readonly AdminRequirement AdminRequirement = new(Role.AdminRole);

    private static readonly SameUserRequirement SameUserRequirement = new();

    private static readonly UserBelongsToGroupRequirement UserBelongsToGroupRequirement = new();

    private static readonly UserHasExerciseRequirement UserHasExerciseRequirement = new();

    public static AdminRequirement CreateAdminRequirement() => AdminRequirement;

    public static SameUserRequirement CreateSameUserRequirement() => SameUserRequirement;

    public static UserBelongsToGroupRequirement CreateUserBelongsToGroupRequirement() => UserBelongsToGroupRequirement;

    public static UserHasExerciseRequirement CreateUserHasExerciseRequirement() => UserHasExerciseRequirement;
}
