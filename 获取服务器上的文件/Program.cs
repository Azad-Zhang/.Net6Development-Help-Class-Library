using System;
using System.IO;
using System.Net;

class Program
{
    static void Main(string[] args)
    {
        string ftpServer = "ftp://42.194.206.48:21";
        string ftpUsername = "admin";
        string ftpPassword = "6YxY3bxMZCXLcTNr";

        try
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpServer);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);

            Console.WriteLine("FTP File List:");
            while (!reader.EndOfStream)
            {
                string fileName = reader.ReadLine();
                Console.WriteLine(fileName);
            }

            reader.Close();
            response.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
