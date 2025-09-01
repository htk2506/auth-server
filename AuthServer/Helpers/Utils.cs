using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AuthServer.Helpers
{
    public static class Utils
    {
        /// <summary>
        /// Extracts a list of errors from a model state.
        /// </summary>
        /// <param name="modelState">The ModelState from a controller.</param>
        /// <returns></returns>
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
