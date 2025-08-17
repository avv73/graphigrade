using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace GraphiGrade.Contracts.ValidatorAttributes;

public class PasswordValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        string? password = value as string;
        if (password == null)
        {
            return new ValidationResult("Password Validator on unexpected type!");
        }

        // Checks for password length, lowercase, uppercase and special symbols.
        if (password.Length >= 6 &&
            Regex.Match(password, @"\d+").Success &&
            Regex.Match(password, @"[a-z]").Success &&
            Regex.Match(password, @"[A-Z]").Success &&
            Regex.Match(password, @"[!,@,#,$,%,^,&,*,?,_,~,\-,£,(,)]").Success)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult("The password should be at least 6 characters, contain lowercase letter, uppercase letter and at least 1 special symbol.");
    }
}
