namespace busfy_api
{
    public static class Constants
    {
        public static readonly string serverUrl = Environment.GetEnvironmentVariable("ASPNETCORE_URLS").Split(";").First();

        public static readonly string localPathToStorages = @"Resources/";
        public static readonly string localPathToProfileIcons = $"{localPathToStorages}Profile/";
        public static readonly string localPathToPublicContentFiles = $"{localPathToStorages}Content/Public/";
        public static readonly string localPathToPrivateContentFiles = $"{localPathToStorages}Content/Private/";

        public static readonly string webPathToProfileIcons = $"{serverUrl}/api/upload/profileIcon/";
        public static readonly string webPathToPublicContentFile = $"{serverUrl}/api/upload/content/pub/";
        public static readonly string webPathToPrivateContentFile = $"{serverUrl}/api/upload/content/private/";

    }
}