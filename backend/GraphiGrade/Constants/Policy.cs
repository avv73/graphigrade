using GraphiGrade.Authorization.SameUserOrAdmin;

namespace GraphiGrade.Constants;

public static class Policy
{
    public const string SameUserOrAdmin = nameof(SameUserOrAdminRequirement);
}
