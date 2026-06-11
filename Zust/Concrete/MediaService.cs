using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Zust.Web.Abstract;
using Zust.Web.Helpers.ConstantHelpers;
using Zust.Web.Entities;

namespace Zust.Web.Concrete
{
    /// <summary>
    /// Concrete implementation of the <see cref="IMediaService"/> interface.
    /// Provides functionality to upload media files to Cloudinary and check if a file is a video file.
    /// </summary>
    public class MediaService : IMediaService
    {
        /// <summary>
        /// Configuration object used for managing Cloudinary settings.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Cloudinary settings instance.
        /// </summary>
        private readonly CloudinarySettings? _cloudinarySettings;

        /// <summary>
        /// Cloudinary instance for interacting with the Cloudinary service.
        /// </summary>
        private readonly Cloudinary _cloudinary;

        /// <summary>
        /// Initializes a new instance of the MediaService class with the provided configuration.
        /// </summary>
        /// <param name="configuration">The configuration object providing access to Cloudinary settings.</param>
        public MediaService(IConfiguration configuration)
        {
            _configuration = configuration;
            _cloudinarySettings = _configuration.GetSection(Constants.CloudinarySettings)
                                                .Get<CloudinarySettings>();

            if (_cloudinarySettings is null
                || string.IsNullOrWhiteSpace(_cloudinarySettings.CloudName)
                || string.IsNullOrWhiteSpace(_cloudinarySettings.ApiKey)
                || string.IsNullOrWhiteSpace(_cloudinarySettings.ApiSecret))
            {
                // Cloudinary is not configured (e.g. local dev without credentials).
                // The app still starts; media uploads will fail fast at call time.
                throw new InvalidOperationException(
                    "Cloudinary is not configured. Set CloudinarySettings (CloudName, ApiKey, ApiSecret) " +
                    "via environment variables or appsettings.Development.json to enable media uploads.");
            }

            Account account = new(
                _cloudinarySettings.CloudName,
                _cloudinarySettings.ApiKey,
                _cloudinarySettings.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
        }

        /// <summary>
        /// Uploads the provided media file to Cloudinary and returns the URL of the uploaded file.
        /// </summary>
        /// <param name="file">The media file to be uploaded.</param>
        /// <returns>The URL of the uploaded file.</returns>
        public async Task<string> UploadMediaAsync(IFormFile file)
        {
            // Check the file type
            var fileType = file.ContentType;

            // Set the upload parameters based on the file type
            RawUploadParams uploadParams;

            if (fileType.StartsWith(Constants.ImageFileType))
            {
                uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream())
                };
            }
            else if (fileType.StartsWith(Constants.VideoFileType))
            {
                uploadParams = new VideoUploadParams
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream())
                };
            }
            else
            {
                throw new NotSupportedException(Errors.FileTypeNotSupportedError);
            }

            // Upload the file to Cloudinary
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            return uploadResult.SecureUrl.ToString();
        }

        /// <summary>
        /// Checks if the provided media file is a video file.
        /// </summary>
        /// <param name="mediaFile">The media file to be checked.</param>
        /// <returns>True if the file is a video file; otherwise, false.</returns>
        public bool IsVideoFile(IFormFile mediaFile)
        {
            if (mediaFile.ContentType.StartsWith(Constants.VideoFileType))
            {
                return true;
            }
            return false;
        }
    }
}
