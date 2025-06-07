using GraphiGrade.Business.Authorization.Policies.SameUserOrAdmin;

namespace GraphiGrade.Business.Authorization;

public static class Policy
{
    public const string SameUserOrAdmin = nameof(SameUserOrAdminRequirement);
}
