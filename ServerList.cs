﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace TTTM
{
    public static class MasterServer
    {
        public static string API_URI { get; private set; } = @"http://tttm.apphb.com/TTTMAPI.asmx/";
        public static string UPD_URI { get; private set; } = @"http://tttm.apphb.com/TTTM.exe";

        public static void ChangeAPIUrl(string Url)
        {
            if (string.IsNullOrWhiteSpace(Url))
                return;

            if (Url.Last() != '/')
                API_URI = Url + '/';
            else
                API_URI = Url;
        }

        public static void ChangeUPDUrl(string Url)
        {
            UPD_URI = Url;
        }
    }

    public static class UpdatingSystem
    {
        private static string UpdHash = string.Empty;
        /*private const bool LOCAL_API = false;
        private const string API_URI = LOCAL_API ? @"http://localhost:51522/TTTMAPI.asmx/" : @"http://tttm.apphb.com/TTTMAPI.asmx/";
        private const string UPD_PATH = LOCAL_API ? @"http://localhost:51522/TTTM.exe" : @"http://tttm.apphb.com/TTTM.exe";*/

        public static event EventHandler UpdatingError;
        public static event EventHandler ClosingRequest;

        public static bool CheckUpd()
        {
            var my = GetMyUpdateData();
            var remote = GetLastUpdateData();

            if (remote == null)
                return false;

            if (my.Item1 == remote.Item1)
                return false;

            return my.Item2 < remote.Item2;
        }

        private static Tuple<string,DateTime> GetMyUpdateData()
        {
            var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            if (File.Exists(path + ".tmp"))
                File.Delete(path + ".tmp");
            File.Copy(path, path + ".tmp");
            var dt = File.GetLastWriteTimeUtc(path);
            path = path + ".tmp";

            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            var sb = new StringBuilder(64);

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                var hash = md5.ComputeHash(fs);
                
                foreach (byte b in hash)
                    sb.Append(b.ToString("X2"));
            }

            File.Delete(path);
            return new Tuple<string, DateTime>(sb.ToString(), dt);
        }

        private static Tuple<string,DateTime> GetLastUpdateData()
        {
            var xml = new XmlDocument();
            try
            {
                xml.Load(MasterServer.API_URI + "GetUpdatingData");
                var Hash = xml.DocumentElement["Hash"].InnerText;
                var dt = DateTime.Parse(xml.DocumentElement["Date"].InnerText).ToUniversalTime();
                return new Tuple<string, DateTime>(Hash, dt);
            }
            catch
            {
                return null;
            }
        }

        public static void Update()
        {
            var wc = new WebClient();
            wc.DownloadFileAsync(new Uri(MasterServer.UPD_URI), "Upd.tmp");
            wc.DownloadFileCompleted += NewVersionDownloaded;
        }

        private static void NewVersionDownloaded(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
                UpdatingError(null, new EventArgs());
            else
            {
                File.WriteAllText("upd" + UpdHash + ".bat", Tic_Tac_Toe_WPF_Remake.Properties.Resources.upd);
                Process p = Process.Start("upd" + UpdHash + ".bat", Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                ClosingRequest(null, new EventArgs());
            }
        }
    }

    public static class ServerList
    {
        public static List<ServerRecord> Servers;
        public struct ServerRecord
        {
            public string IP { get; private set; }
            public string Name { get; private set; }
            public string ServerName { get; private set; }
            public string PublicKey { get; private set; }
            public string Color { get; private set; }

            public ServerRecord(string ip, string servername, string name, string color, string publickey)
            {
                IP = ip;
                ServerName = servername;
                Name = name;
                Color = color;
                PublicKey = publickey;
            }
        }

        public static bool RemoveFromTheWeb(string AccessKey)
        {
            var xml = new XmlDocument();
            try
            {
                xml.Load(MasterServer.API_URI + "Remove?AccessKey=" + AccessKey);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Clear(string AccessKey)
        {
            var xml = new XmlDocument();
            try
            {
                xml.Load(MasterServer.API_URI + "Clear?AccessKey=" + AccessKey);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool WriteClientEP(string PublicKey, IPEndPoint ClientEP)
        {
            var parameters = "PublicKey=" + PublicKey + "&IP=" + ClientEP.Address.ToString() + "&Port=" + ClientEP.Port.ToString();
            var xml = new XmlDocument();
            try
            {
                xml.Load(MasterServer.API_URI + "WriteClientEP?" + parameters);
                var Result = xml.DocumentElement;
                return Result.InnerText == "true";
            }
            catch
            {
                return false;
            }
        }

        public static List<ServerRecord> GetServers()
        {
            var Result = new List<ServerRecord>();
            try
            {
                var xml = new XmlDocument();
                xml.Load(MasterServer.API_URI + "Get");
                var ServerList = xml.DocumentElement.ChildNodes;
                foreach (XmlNode ServerNode in ServerList)
                {
                    var ip = ServerNode["IP"].InnerText;
                    var name = ServerNode["Name"].InnerText;
                    var sname = ServerNode["ServerName"].InnerText;
                    var pkey = ServerNode["PublicKey"].InnerText;
                    var color = ServerNode["Color"].InnerText;
                    Result.Add(new ServerRecord(ip, sname, name, color, pkey));
                    Servers = Result;
                }
            }
            catch { }
            return Result;
        }

        public static IPEndPoint ReadClientEP(string AccessKey)
        {
            var parameters = "AccessKey=" + AccessKey;
            var xml = new XmlDocument();
            try
            {
                xml.Load(MasterServer.API_URI + "ReadClientEP?" + parameters);
                var Result = xml.DocumentElement;
                return new IPEndPoint(IPAddress.Parse(Result["IP"].InnerText), int.Parse(Result["Port"].InnerText));
            }
            catch
            {
                return null;
            }
        }

        public static IPEndPoint ReadReady(string PublicKey)
        {
            var parameters = "PublicKey=" + PublicKey;
            var xml = new XmlDocument();
            try
            {
                xml.Load(MasterServer.API_URI + "ReadReady?" + parameters);
                var Result = xml.DocumentElement;
                if (Result["Ready"].InnerText == "true")
                    return new IPEndPoint(IPAddress.Parse(Result["IP"].InnerText), int.Parse(Result["Port"].InnerText));
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

        public static bool CheckWhoWant(string AccessKey)
        {
            var parameters = "AccessKey=" + AccessKey;
            var xml = new XmlDocument();
            try
            {
                xml.Load(MasterServer.API_URI + "GetWant?" + parameters);
                var Result = xml.DocumentElement;
                return (Result.InnerText == "true");
            }
            catch
            {
                return false;
            }
        }

        public static bool WriteReady(string AccessKey, int Port)
        {
            var parameters = "AccessKey=" + AccessKey + "&Port=" + Port;
            var xml = new XmlDocument();
            try
            {
                xml.Load(MasterServer.API_URI + "WriteReady?" + parameters);
                var Result = xml.DocumentElement;
                return (Result.InnerText == "true");
            }
            catch
            {
                return false;
            }
        }

        public static string RegisterOnTheWeb(string Name, string ServerName, Color Color)
        {
            var parameters = "Name=" + WebUtility.UrlEncode(Name) + "&Color=" + Color.ToArgb().ToString() + "&ServerName=" + ServerName;
            var xml = new XmlDocument();
            try
            {
                xml.Load(MasterServer.API_URI + "Add?" + parameters);
                var CreatingResult = xml.DocumentElement;
                var AK = CreatingResult["AccessKey"].InnerText;
                //bool Ping = (CreatingResult["Ping"].InnerText == "true");
                return AK;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static bool WantToConnect(string publicKey)
        {
            var parameters = "PublicKey=" + publicKey;
            var xml = new XmlDocument();
            try
            {
                xml.Load(MasterServer.API_URI + "WantConnect?" + parameters);
                var Result = xml.DocumentElement;
                return (Result.InnerText == "true");
            }
            catch
            {
                return false;
            }
        }
    }
}
