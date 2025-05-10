using System.ComponentModel.DataAnnotations;

namespace News.Application.Helpers
{
    public static class ValidationHelper
    {
        public static bool TryValidate(object obj, out List<ValidationResult> results)
        {
            var context = new ValidationContext(obj, null, null);
            results = [];
            return Validator.TryValidateObject(obj, context, results, true);
        }
    }
}