using System.ComponentModel.DataAnnotations;

namespace smartPark.Models.ViewModels;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class DateGreaterThanAttribute : ValidationAttribute
{
    private readonly string _comparisonProperty;

    public DateGreaterThanAttribute(string comparisonProperty)
    {
        _comparisonProperty = comparisonProperty;
        ErrorMessage = "{0} mora biti poslije {1}";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        var currentValue = (DateTime)value;
        var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

        if (property == null)
            return new ValidationResult($"Nepoznato svojstvo: {_comparisonProperty}");

        var comparisonValue = (DateTime)property.GetValue(validationContext.ObjectInstance)!;

        if (currentValue <= comparisonValue)
        {
            return new ValidationResult(
                string.Format(ErrorMessage ?? "{0} mora biti poslije {1}", validationContext.DisplayName, _comparisonProperty)
            );
        }

        return ValidationResult.Success;
    }
}
