namespace qb.Network
{
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
