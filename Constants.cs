namespace busfy_api
{
    public static class Constants
    {
        public static readonly string serverUrl = Environment.GetEnvironmentVariable("ASPNETCORE_URLS").Split(";").First();

        public static readonly string localPathToStorages = @"Resources/";
        public static readonly string localPathToProfileIcons = $"{localPathToStorages}Profile/";
        public static readonly string localPathToBackground = $"{localPathToStorages}Background/";
        public static readonly string localPathToPostFiles = $"{localPathToStorages}Post/";
        public static readonly string localPathToPublicContentFiles = $"{localPathToStorages}Content/Public/";
        public static readonly string localPathToPrivateContentFiles = $"{localPathToStorages}Content/Private/";
        public static readonly string localPathToContentCategoryFiles = $"{localPathToStorages}Content/Categories/";

        public static readonly string webPathToProfileIcons = $"{serverUrl}/api/upload/profile/";
        public static readonly string webPathToProfileBackground = $"{serverUrl}/api/upload/profile-background/";
        public static readonly string webPathToPublicContentFile = $"{serverUrl}/api/upload/content/pub/";
        public static readonly string webPathToPrivateContentFile = $"{serverUrl}/api/upload/content/private/";
        public static readonly string webPathToContentCategories = $"{serverUrl}/api/upload/content-category/";
        public static readonly string webPathToPostFiles = $"{serverUrl}/api/upload/post/";

    }
}