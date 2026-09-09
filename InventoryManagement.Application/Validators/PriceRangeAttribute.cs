using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace InventoryManagement.Application.Validators
{
    public class PriceRangeAttribute : ValidationAttribute
    {
        private readonly decimal _min;
        private readonly decimal _max;

        public PriceRangeAttribute(double min, double max)
        {
            _min = (decimal)min;
            _max = (decimal)max;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Price is required.");
            }

            // Try to parse with proper culture
            if (value is decimal decimalValue)
            {
                if (decimalValue >= _min && decimalValue <= _max)
                {
                    return ValidationResult.Success;
                }
                return new ValidationResult($"Price must be between {_min} and {_max}.");
            }

            // If value is a string (from model binding)
            if (value is string stringValue)
            {
                if (decimal.TryParse(stringValue, NumberStyles.Any, new CultureInfo("id-ID"), out decimal parsed))
                {
                    if (parsed >= _min && parsed <= _max)
                    {
                        return ValidationResult.Success;
                    }
                    return new ValidationResult($"Price must be between {_min} and {_max}.");
                }
                return new ValidationResult("Invalid price format.");
            }

            return new ValidationResult("Invalid price format.");
        }
    }
}
