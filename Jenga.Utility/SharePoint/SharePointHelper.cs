using Microsoft.SharePoint.Client;
using System;
using System.IO;
using System.Net;
using System.Security;

public class SharePointHelper
{

    public static void UploadFileToSharePoint(string siteUrl, string libraryName, string filePath,string username,string password)
    {
        using (var context = new ClientContext(siteUrl))
        {
            context.Credentials=new NetworkCredential(username, password, "TSKGV");
            // Windows Authentication kullanımı
            context.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine("------------------------Bağlandı---------------------------");

            Console.WriteLine("----------------------------------------------------------------");
            // Hedef SharePoint kütüphanesi
            var web = context.Web;
            var list = web.Lists.GetByTitle(libraryName);

            // Dosya yükleme
            var fileCreationInfo = new FileCreationInformation
            {
                Content = System.IO.File.ReadAllBytes(filePath),
                Url = Path.GetFileName(filePath),
                Overwrite = true
            };

            var uploadFile = list.RootFolder.Files.Add(fileCreationInfo);
            context.Load(uploadFile);
            context.ExecuteQuery();

            Console.WriteLine("Resim başarıyla yüklendi.");
        }
    }
}
