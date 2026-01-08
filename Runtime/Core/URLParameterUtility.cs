namespace qb.Network
{
    /// <summary>
    /// Provides utility methods for manipulating URL query parameters.
    /// </summary>
    public static class URLParameterUtility
    {
        public static string AddParameters(string url, string parameters)
        {
            if (url.Contains("?"))
            {
                if (!parameters.Contains("?"))
                    return $"{url}&{parameters}";
                else 
                    throw new System.Exception($"Dupplicate ? separator on url[{url}] and parameters[{parameters}]");
            }
            else
            {
                if (parameters[0]=='?')
                    return $"{url}{parameters}";
                else
                    return $"{url}&{parameters}";
            }
        }
    }
}
