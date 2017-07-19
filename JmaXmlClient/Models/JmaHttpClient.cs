using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JmaXmlClient.Models
{
    public class JmaHttpClient
    {
        private static readonly HttpClient Client = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip
                                     | DecompressionMethods.Deflate
        });

        private const string UserAgent = "bot by WeatherAPI";

        public static async Task<string> GetJmaXml(string link, int ntry = 0)
        {
            Client.DefaultRequestHeaders.Add("user-agent", UserAgent);
            try
            {
                return await Client.GetStringAsync(link);
            }
            catch (Exception e1)
            {
                if (ntry < 1)
                {
                    await Task.Delay(300);
                    return await GetJmaXml(link, ++ntry);
                }
                else
                {
                    await Utils.WriteLog($"気象庁XMLデータ取得エラー url={link} メッセージ:{e1.Message}");
                    return null;
                }
            }
        }

        /*
        public static async Task<XElement> GetJmaXElement(string link, string dir)
        {
            string xml = await GetJmaXml(link);
            if (xml == null)
                return null;

            XElement xe = XElement.Parse(xml);

            string status = xe.Descendants(Utils.XmlnsJmx + "Status").First().Value;
            if (status != "通常")
            {
                await Utils.WriteLog("通常以外のStatusがありました: " + status + " url=" + link);
                return null;
            }

            string publishingOffice = xe.Descendants(Utils.XmlnsJmx + "PublishingOffice").First().Value;

            using (StreamWriter sw = new StreamWriter(File.Open(Path.Combine(dir, AppIni.PublishingOffice[publishingOffice] + ".xml"), FileMode.Create, FileAccess.Write)))
            {
                await sw.WriteAsync(xml);
            }

            return xe;
        }
        */
    }
}
