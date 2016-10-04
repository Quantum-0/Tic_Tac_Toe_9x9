using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace TTTM
{
    public static class ServerList
    {
        private const string API_URI = "http://localhost:51522/TTTMAPI.asmx/";//@"http://tttm.apphb.com/TTTMAPI.asmx/";

        public static List<ServerRecord> Servers;
        public struct ServerRecord
        {
            public string IP;
            public string Name;
            public string ServerName;
            public string PublicKey;
            public string Color;

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
                xml.Load(API_URI + "Remove?AccessKey=" + AccessKey);
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
                xml.Load(API_URI + "Clear?AccessKey=" + AccessKey);
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
                xml.Load(API_URI + "WriteClientEP?" + parameters);
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
                xml.Load(API_URI + "Get");
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
                xml.Load(API_URI + "ReadClientEP?" + parameters);
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
                xml.Load(API_URI + "ReadReady?" + parameters);
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
                xml.Load(API_URI + "GetWant?" + parameters);
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
                xml.Load(API_URI + "WriteReady?" + parameters);
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
            var parameters = "Name=" + HttpUtility.UrlEncode(Name) + "&Color=" + Color.ToArgb().ToString() + "&ServerName=" + ServerName;
            var xml = new XmlDocument();
            try
            {
                xml.Load(API_URI + "Add?" + parameters);
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

        internal static bool WantToConnect(string publicKey)
        {
            var parameters = "PublicKey=" + publicKey;
            var xml = new XmlDocument();
            try
            {
                xml.Load(API_URI + "WantConnect?" + parameters);
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
