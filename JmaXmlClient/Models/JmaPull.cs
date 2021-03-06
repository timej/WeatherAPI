﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JmaXmlClient.Models
{
    class JmaPull
    {
        static private readonly XNamespace Xmlns = "http://www.w3.org/2005/Atom";

        internal static async Task PullAsync()
        {
            await Utils.WriteLog("PULL型のフィードの処理開始");
            await GetRegularFeeds("http://www.data.jma.go.jp/developer/xml/feed/regular.xml", false);
            //await GetExtraFeeds("http://www.data.jma.go.jp/developer/xml/feed/extra.xml", false);
            await Utils.WriteLog("PULL型のフィードの処理終了");
        }

        internal static async Task PullLAsync()
        {
            await Utils.WriteLog("PULL型のロングフィードの処理開始");
            await GetRegularFeeds("http://www.data.jma.go.jp/developer/xml/feed/regular_l.xml", true);
            //await GetExtraFeeds("http://www.data.jma.go.jp/developer/xml/feed/extra_l.xml", true);
            await Utils.WriteLog("PULL型のロングフィードの処理終了");
        }

        static async Task GetRegularFeeds(string url, bool isLong)
        {
            try
            {
                string xml = await JmaHttpClient.GetJmaXml(url);
                XDocument xdoc = XDocument.Parse(xml);

                string path = Path.Combine(AppIni.DataPath, "RegularFeedId.txt");
                string prev = "";

                if (!isLong && File.Exists(path))
                {
                    using (var sr = new StreamReader(File.Open(path, FileMode.Open, FileAccess.Read)))
                    {
                        prev = sr.ReadToEnd();
                    }
                }

                int n = 0;
                var feedList = new List<JmaFeedData2>();
                string nowId = null;
                foreach (var entry in xdoc.Descendants(Xmlns + "entry"))
                {
                    string feedId = entry.Element(Xmlns + "id").Value;
                    if (n == 0)
                    {
                        nowId = feedId;
                        n++;
                    }

                    if (prev == feedId)
                        break;
                    string task = null;
                    switch (entry.Element(Xmlns + "title").Value)
                    {
                        case "府県天気予報": //VPFD50
                            task = "vpfd50";
                            break;
                        case "府県天気概況": //VPFG50
                            task = "vpfg50";
                            break;
                        case "府県週間天気予報": //VPFW50
                            task = "vpfw50";
                            break;
                        case "地方週間天気予報": //VPCW50
                            task = "vpcw50";
                            break;
                        case "全般週間天気予報": //VPZW50
                            task = "vpzw50";
                            break;
                        default:
                            continue;
                    }

                    int id = AppIni.PublishingOffice[entry.Element(Xmlns + "author").Element(Xmlns + "name").Value];
                    //同じものがあった場合は除外
                    var feed = feedList.FirstOrDefault(x => x.Id == id && x.Task == task);
                    if (feed == null)
                    {
                        feedList.Add(new JmaFeedData2
                        {
                            Id = id,
                            Task = task,
                            UpdateTime = DateTime.Parse(entry.Element(Xmlns + "updated").Value),
                            Link = entry.Element(Xmlns + "link").Attribute("href").Value
                        });
                    }
                }

                await JmaDsRegularTask.UpsertData(feedList);

                using (var sw = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.Write)))
                {
                    sw.Write(nowId);
                }
            }
            catch (Exception e1)
            {
                await Utils.WriteLog("PULL型の定時フィードでエラーが発生しました。" + e1.Message);
            }
        }
        static async Task GetExtraFeeds(string url, bool isLong)
        {
            try
            {
                string xml = await JmaHttpClient.GetJmaXml(url);
                XDocument xdoc = XDocument.Parse(xml);

                string path = Path.Combine(AppIni.DataPath, "ExtraFeedId.txt");
                string prev = "";
                if (!isLong && File.Exists(path))
                {
                    using (var sr = new StreamReader(File.Open(path, FileMode.Open, FileAccess.Read)))
                    {
                        prev = sr.ReadToEnd();
                    }
                }

                int n = 0;
                var feedList = new List<JmaFeedData2>();
                string nowId = null;
                foreach (var entry in xdoc.Descendants(Xmlns + "entry"))
                {
                    string feedId = entry.Element(Xmlns + "id").Value;
                    if (n == 0)
                    {
                        nowId = feedId;
                        n++;
                    }

                    if (prev == feedId)
                        break;

                    string task = null;
                    switch (entry.Element(Xmlns + "title").Value)
                    {
                        case "気象特別警報・警報・注意報": //VPWW53
                            task = "vpww53";
                            break;
                        case "気象警報・注意報（Ｈ２７）": //VPWW54
                            task = "vpww54";
                            break;
                        default:
                            continue;
                    }

                    int id = AppIni.PublishingOffice[entry.Element(Xmlns + "author").Element(Xmlns + "name").Value];
                    //同じものがあった場合は除外
                    var feed = feedList.FirstOrDefault(x => x.Id == id && x.Task == task);
                    if (feed == null)
                    {

                        feedList.Add(new JmaFeedData2
                        {
                            Id = AppIni.PublishingOffice[entry.Element(Xmlns + "author").Element(Xmlns + "name").Value],
                            Task = task,
                            UpdateTime = DateTime.Parse(entry.Element(Xmlns + "updated").Value),
                            Link = entry.Element(Xmlns + "link").Attribute("href").Value
                        });
                    }
                }

                await JmaDsExtraTask.UpsertData(feedList);

                using (var sw = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.Write)))
                {
                    sw.Write(nowId);
                }
            }
            catch(Exception e1)
            {
                await Utils.WriteLog("PULL型の随時フィードでエラーが発生しました。" + e1.Message);
            }
        }

    }


}


