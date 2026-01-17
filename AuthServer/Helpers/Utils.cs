using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AuthServer.Helpers
{
    public static class Utils
    {
        /// <summary>
        /// Default options to use for JSON serialization. 
        /// </summary>
        public static readonly JsonSerializerOptions DefaultJsonSerializerOptions;

        static Utils()
        {
            DefaultJsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true,
            };
            DefaultJsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        }

        /// <summary>
        /// Extracts a list of errors from a model state.
        /// </summary>
        /// <param name="modelState">The ModelState from a controller.</param>
        /// <returns>List of error strings.</returns>
        public static List<string> GetModelErrors(ModelStateDictionary modelState)
        {
            var modelErrors = new List<string>();
            foreach (var modelStateEntry in modelState.Values)
            {
                foreach (var modelError in modelStateEntry.Errors)
                {
                    modelErrors.Add(modelError.ErrorMessage);
                }
            }
            return modelErrors;
        }
    }
}
