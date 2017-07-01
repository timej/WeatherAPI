using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.PlatformAbstractions;
using System.Xml.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using JmaXmlServer.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace JmaXmlServer.Controllers
{
    public class SubscriberController : Controller
    {
        private readonly XNamespace Xmlns = "http://www.w3.org/2005/Atom";

        [HttpGet]
        public IActionResult Index()
        {
            //HUBから、登録確認のため初回および５日間隔で、アメリカ太平洋時間の5:00（日本時間だと20:00か21:00）にチャレンジコード付きリクエストが飛んでくる。
            //チャレンジコードをそのまま返す。
            //テスト用 http://(yourdomain)/subscriber?hub.challenge=challenge_code&hub.topic=http://xml.kishou.go.jp/feed/regular.xml&hub.mode=subscribe&hub.lease_seconds=432000

            string hubMode = Request.Query["hub.mode"];
            string hubTopic = Request.Query["hub.topic"];
            string hubchallenge = Request.Query["hub.challenge"];
            try
            {
                Log.Warning("{topic}: Mode:{hubMode} Topic:{hubTopic}", "challenge", hubMode, hubTopic);
            }
            catch{}
            if (hubMode == "subscribe" || hubMode == "unsubscribe")
                return Content(hubchallenge);
            return NotFound();
        }


        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost()
        {
            try
            {
                if (Request.Body.CanSeek)
                {
                    // Reset the position to zero to read from the beginning.
                    Request.Body.Position = 0;
                }
                var sr = new StreamReader(Request.Body);
                var input = await sr.ReadToEndAsync();               

                XDocument xdoc = XDocument.Parse(input);

                //feedtypeの取得 定時:regular、随時:extra、地震火山:eqvol、その他other:
                var xfeed = xdoc.Element(Xmlns + "feed");
                var self = xfeed.Elements(Xmlns + "link").Where(x => x.Attribute("rel").Value == "self").First();
                var uris = self.Attribute("href").Value.Split('/');
                string feedtype = uris[uris.Length - 1].Split('.')[0];  
                
                LoggerClass.LogInfo(string.Format($"ID:{xfeed.Element(Xmlns + "id").Value} Update:{xfeed.Element(Xmlns + "updated").Value} Type:{feedtype}"));

                var jmaXmlFeedList = new List<JmaXmlFeed>();

                foreach (var item in xdoc.Descendants(Xmlns + "entry"))
                {
                    string title = item.Element(Xmlns + "title").Value;
                    string name = item.Element(Xmlns + "author").Element(Xmlns + "name").Value;
                    string updatetime = item.Element(Xmlns + "updated").Value;
                    string link = item.Element(Xmlns + "link").Attribute("href").Value;
                    {
                        LoggerClass.LogInfo(string.Format($"{title} {name} Update:{updatetime} uri:{link}"));
                        string task;
                        switch (title)
                        {
                            case "府県天気予報": //VPFG50
                                task = "vpfg50";
                                break;
                            case "府県天気概況": //VPFD50
                                task = "vpfd50";
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
                            case "気象特別警報・警報・注意報": //VPWW53
                                task = "vpww53";
                                break;
                            case "気象警報・注意報（Ｈ２７）": //VPWW54
                                task = "vpww54";
                                break;
                            default:
                                continue;
                        }
                        jmaXmlFeedList.Add(new JmaXmlFeed
                        {
                           Task = task,
                           Author = name,
                           UpdateTime = DateTime.Parse(updatetime),
                           Link = link
                        });
                    }
                }

                if (jmaXmlFeedList.Any())
                {
                    Datastore datastore = new Datastore("JmaXml" + char.ToUpper(feedtype[0]) + feedtype.Substring(1));
                    datastore.AddTask(JsonConvert.SerializeObject(jmaXmlFeedList));

                    //プロセスが<defunct>というゾンビになって残るため EnableRaisingEvents = true が必要
                    //https://stackoverflow.com/questions/43515360/net-core-process-start-leaving-defunct-child-process-behind
                    using (Process proc = new Process
                    {
                        StartInfo = new ProcessStartInfo("dotnet", AppConst.ClientPath + " -" + feedtype[0]),
                        EnableRaisingEvents = true
                    })
                    {
                        proc.Start();
                    }
                }
            }
            catch(Exception e1)
            {
                LoggerClass.LogError("Subscriber/IndexPost Request.Body ReadError: " + e1.Message);
            }

            return Content("");
        }

        public IActionResult Test()
        {
            Datastore datastore = new Datastore("JmaXmlTest");
            datastore.AddTask("Datastoreのテスト");

            return Content("OK");
        }

        public IActionResult Test2()
        {
            var startInfo = new ProcessStartInfo("dotnet", AppConst.ClientPath + " -r");
            var process = Process.Start(startInfo);

            return Content("OK");
        }
    }
}
