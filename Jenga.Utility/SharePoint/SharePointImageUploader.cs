using Microsoft.SharePoint.Client;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Jenga.Utility.SharePoint
{
    public class SharePointImageUploader
    {
        // Method to crop an image and return it as a byte array
        public byte[] CropImage(Stream imageStream, Rectangle cropArea)
        {
            using (var originalImage = new Bitmap(imageStream))
            {
                // Crop the image
                using (var croppedBitmap = originalImage.Clone(cropArea, originalImage.PixelFormat))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        // Save the cropped image to a memory stream
                        croppedBitmap.Save(memoryStream, ImageFormat.Jpeg);
                        return memoryStream.ToArray(); // Return as byte array
                    }
                }
            }
        }

        // Method to upload a cropped image to a SharePoint Picture Library
        public async Task UploadImageToSharePoint(byte[] imageBytes, string fileName)
        {
            try
            {
                // SharePoint Picture Library URL
                string siteUrl = "http://tskgv-dev3/YonetimBirimleri/InsaatVeEmlakYonetimiSubesi/BagisciResimleri/";

                // Initialize the ClientContext
                using (var context = new ClientContext(siteUrl))
                {
                    // Use Windows Authentication for intranet
                    context.Credentials = CredentialCache.DefaultNetworkCredentials;

                    // Access the Picture Library folder
                    var folder = context.Web.GetFolderByServerRelativeUrl("YonetimBirimleri/InsaatVeEmlakYonetimiSubesi/BagisciResimleri");

                    // Create a memory stream for the image
                    using (var memoryStream = new MemoryStream(imageBytes))
                    {
                        // Set up the file creation information
                        FileCreationInformation newFile = new FileCreationInformation
                        {
                            ContentStream = memoryStream,
                            Url = fileName,
                            Overwrite = true // Overwrite the file if it already exists
                        };

                        // Upload the file to the library
                        var uploadFile = folder.Files.Add(newFile);
                        context.Load(uploadFile);
                        await context.ExecuteQueryAsync(); // Send the upload request to SharePoint
                    }

                    Console.WriteLine("Cropped image uploaded successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading image to SharePoint: {ex.Message}");
            }
        }

        // Main process: Crop and upload an image
        public async Task ProcessAndUploadImage(Stream imageStream, string fileName, Rectangle cropArea)
        {
            // Crop the image
            byte[] croppedImage = CropImage(imageStream, cropArea);

            // Upload the cropped image to SharePoint
            await UploadImageToSharePoint(croppedImage, fileName);
        }
    }
}
