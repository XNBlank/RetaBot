using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetaBot
{
    public class SettingsObject
    {
        public string LastUsedNick { get; set; }
        public string LastJoinedServer { get; set; }
        public string LastJoinedChannel { get; set; }
        public string WelcomeMessage { get; set; }
        //This string is AES Encrypted
        public string NickServPass { get; set; }
        public string[] UsersAllowedToDisable { get; set; }
        public char Prefix { get; set; }

        public SettingsObject()
        {
            UsersAllowedToDisable = new string[] { "luigifan2010", "blank", "xerx" };
            LastUsedNick = "RetaSharp";
            LastJoinedServer = "irc.swiftirc.net";
            NickServPass = "";
            Prefix = '!';
            LastJoinedChannel = "#testReta";
        }
    }
}