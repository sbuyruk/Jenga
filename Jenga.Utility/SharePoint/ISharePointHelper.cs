namespace Jenga.Utility.SharePoint
{

    public interface ISharePointHelper
    {
        /// <summary>
        /// Uploads a file to the specified SharePoint library.
        /// </summary>
        /// <param name="libraryName">The name of the SharePoint document library.</param>
        /// <param name="fileName">The name of the file to be uploaded.</param>
        /// <param name="fileContent">The content of the file as a stream.</param>
        /// <returns>Returns the URL of the uploaded file.</returns>
        Task<string> UploadFileAsync(string libraryName, string fileName, Stream fileContent);
    }
}
