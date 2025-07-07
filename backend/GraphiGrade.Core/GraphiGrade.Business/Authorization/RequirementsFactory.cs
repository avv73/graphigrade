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

    public static IEnumerable<IAuthorizationRequirementErrorProducer> CreateRequirements(params string[] policies)
    {
        List<IAuthorizationRequirementErrorProducer> requirements = new();

        foreach (string policy in policies)
        {
            switch (policy)
            {
                case Policy.Admin:
                    requirements.Add(AdminRequirement);
                    break;
                case Policy.SameUser:
                    requirements.Add(SameUserRequirement);
                    break;
                case Policy.UserBelongsToGroup:
                    requirements.Add(UserBelongsToGroupRequirement);
                    break;
                case Policy.UserHasExercise:
                    requirements.Add(UserHasExerciseRequirement);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        return requirements;
    }
}
