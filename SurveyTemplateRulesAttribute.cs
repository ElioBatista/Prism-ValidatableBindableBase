using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Apollo.Survey.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SurveyTemplateRulesAttribute: ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value is SurveyTemplate)
            {
                int total = ((SurveyTemplate)value).Details
                    .Where(e => !e.IsDeletedState)
                    .Sum(e => e.Weight);

                if (total == 100)
                    return ValidationResult.Success;
                else
                    return new ValidationResult("The sum of the weights of the Details should be equal to 100.");

            }
            else
                return ValidationResult.Success;
        }

    }
}
