using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace TTTM
{
    public static class ServerList
    {
        public struct ServerRecord
        {
            public string IP;
            public string Port;
            public string Name;
            public string Color;

            public ServerRecord(string ip, string port, string name, string color)
            {
                IP = ip;
                Port = port;
                Name = name;
                Color = color;
            }
        }

        public static bool RemoveFromTheWeb(string AccessKey)
        {
            var xml = new XmlDocument();
            try
            {
                xml.Load(@"http://tttm.apphb.com/TTTMAPI.asmx/Remove?AccessKey=" + AccessKey);
                return true;
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
                xml.Load(@"http://tttm.apphb.com/TTTMAPI.asmx/Get");
                var ServerList = xml.DocumentElement.ChildNodes;
                foreach (XmlNode ServerNode in ServerList)
                {
                    var ip = ServerNode["IP"].InnerText;
                    var port = ServerNode["Port"].InnerText;
                    var name = ServerNode["Name"].InnerText;
                    var color = ServerNode["Color"].InnerText;
                    Result.Add(new ServerRecord(ip, port, name, color));
                }
            }
            catch { }
            return Result;
        }

        public static string RegisterOnTheWeb(string Name, Color Color, string Port)
        {
            var parameters = "Name=" + HttpUtility.UrlEncode(Name) + "&Color=" + Color.ToArgb().ToString() + "&Port=" + Port;
            var xml = new XmlDocument();
            try
            {
                xml.Load(@"http://tttm.apphb.com/TTTMAPI.asmx/Add?" + parameters);
                var CreatingResult = xml.DocumentElement;
                var AK = CreatingResult["AccessKey"].InnerText;
                bool Ping = (CreatingResult["Ping"].InnerText == "true");
                return AK;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
