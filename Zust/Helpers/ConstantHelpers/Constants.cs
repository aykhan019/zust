namespace Zust.Web.Helpers.ConstantHelpers
{
    /// <summary>
    /// A collection of constants in the application.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Represents the name of the app settings file ("appsettings.json").
        /// This constant is used to retrieve application configuration settings.
        /// </summary>
        public const string AppSettingsFile = "appsettings.json";

        /// <summary>
        /// Represents the connection string used to connect to the database.
        /// </summary>
        public static string ConnectionString = "";

        /// <summary>
        /// Represents the name of the connection string in the configuration.
        /// </summary>
        public const string ConnectionStringName = "Default";

        /// <summary>
        /// Static constructor for the Constants class that initializes the ConnectionString constant.
        /// </summary>
        static Constants()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(AppSettingsFile, optional: true, reloadOnChange: true)
                .Build();

            ConnectionString = configuration.GetConnectionString("Default");
        }

        /// <summary>
        /// Represents the name of the assembly containing database migrations.
        /// </summary>
        public const string MigrationsAssembly = "Zust.Web";

        /// <summary>
        /// Represents the expiration time span for a cookie in days.
        /// </summary>
        public const int CookieExpireTimeSpan = 30;

        /// <summary>
        /// Represents the default file path for user profile images.
        /// </summary>  
        public const string DefaultProfileImagePath = "/assets/images/user/defaultUserImage.png";

        /// <summary>
        /// Represents the avatar generator URL used for default user profile images.
        /// </summary>
        public const string DefaultProfileAvatarGeneratorUrl = "https://api.dicebear.com/9.x/avataaars/png?seed=";

        /// <summary>
        /// Represents the default file path for user cover images.
        /// </summary>
        public const string DefaultCoverImagePath = "/assets/images/user/defaultCoverImage.png";

        /// <summary>
        /// Builds a random generated avatar URL for a new user profile.
        /// </summary>
        public static string GetRandomProfileImagePath()
        {
            return $"{DefaultProfileAvatarGeneratorUrl}{Guid.NewGuid():N}";
        }

        /// <summary>
        /// Represents the "No Data" string.
        /// </summary>
        public const string NoData = "No Data";

        /// <summary>
        /// Represents the number of users to take in user-related operations.
        /// </summary>
        public const int TakeUserCount = 16;

        /// <summary>
        /// Represents an empty string.
        /// </summary>
        public const string StringEmpty = "";

        /// <summary>
        /// Represents the settings key for Cloudinary configuration.
        /// </summary>
        public const string CloudinarySettings = "CloudinarySettings";

        /// <summary>
        /// Represents the file type for images.
        /// </summary>
        public const string ImageFileType = "image/";

        /// <summary>
        /// Represents the file type for videos.
        /// </summary>
        public const string VideoFileType = "video/";

        /// <summary>
        /// Represents the URL for the default image when no content is found.
        /// </summary>
        public const string NoContentImageUrl = "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689665492/noContentFound_m74is3.png";

        /// <summary>
        /// Represents the number of status items to show in the news feed.
        /// </summary>
        public const int StatusCountInNewsFeed = 6;

        /// <summary>
        /// Represents the number of video items to show in the news feed.
        /// </summary>
        public const int VideoCountInNewsFeed = 6;

        /// <summary>   
        /// Represents the number of advertisement items to show in the news feed.
        /// </summary>
        public const int AdvertisementCountInNewsFeed = 2;

        /// <summary>
        /// Represents the number of random users featured in the "VIP Users" sidebar panel.
        /// </summary>
        public const int SpecialUserCount = 6;

        /// <summary>
        /// Represents the range in days for checking birthdays.
        /// </summary>
        public const int BirthdayRange = 7;

        /// <summary>
        /// Represents the number of random followers to show.
        /// </summary>
        public const int RandomFollowerCount = 5;
    }
}
