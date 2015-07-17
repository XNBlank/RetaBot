using ChatSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace RetaBot
{
    class Program
    {

        private static IrcClient client;
        private static Thread InputThread = new Thread(Input);
        private static Version version = Assembly.GetEntryAssembly().GetName().Version;
        private static Settings ProgramSettings = new Settings();
        private static Random random = new Random(Environment.TickCount);
        private static string message;
        public static int ttlquestions;
        private static List<string> floodlist = new List<string>();

        private static string[] feeling = new string[] {
			"I am doing well",
			"I'm okay",
			"I could be better",
			"I'm doing fantastic",
			"Absolutely terrible",
			"I'm feeling depressed",
            "Just feeling a bit down"
		};


        static void Main(string[] args)
        {
            Startup();
        }

        private static void Startup(){

            Console.Title = "RetaSharp - " + version.ToString();
            ProgramSettings.LoadSettings();

            Console.WriteLine("RetaSharp. inspired by and based around Luigifan2010s LuigiBot. <3");
            DisplayOutputMessage(String.Format(" Please enter a nick for your bot ", ProgramSettings.settings.LastUsedNick, ":"), false);
            string nick = Console.ReadLine();
            if (nick.Trim() != "")
                ProgramSettings.settings.LastUsedNick = nick;

            DisplayOutputMessage(String.Format(" Please enter a server to connect to ", ProgramSettings.settings.LastJoinedServer, ":"), false);
            string server = Console.ReadLine();
            if (server.Trim() != "")
                ProgramSettings.settings.LastJoinedServer = server;

            DisplayOutputMessage(String.Format(" Please enter the channel you want to join, starting with # ", ProgramSettings.settings.LastJoinedChannel, ":"), false);
            string channel = Console.ReadLine();
            if (channel.Trim() != "")
                ProgramSettings.settings.LastJoinedChannel = channel;
            ProgramSettings.WriteSettings();
            RunBot(nick, server, channel);
        }

        private static void OutputStatusMessage(string message, char splitAt, bool newLine)
        {
            string[] split = message.Split(new char[] { splitAt }, 2);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(split[0]);
            Console.ForegroundColor = ConsoleColor.White;
            if (newLine)
                Console.Write(split[1] + "\n");
            else
                Console.Write(split[1]);
        }

        static void DisplayOutputMessage(string message, bool newLine)
        {
            string[] split = message.Split(new char[] { ' ' }, 2);
            if (newLine)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(split[1] + "\n");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(split[1]);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private static void RunBot(string nick, string server, string channel)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\nConnecting to " + ProgramSettings.settings.LastJoinedServer + " on port 6667....");
            Console.ForegroundColor = ConsoleColor.White;

            client = new IrcClient(ProgramSettings.settings.LastJoinedServer, new IrcUser(ProgramSettings.settings.LastUsedNick, "RetaSharp", "V54swg3!", "RetaReta"));
            client.NetworkError += (s, e) => Console.WriteLine("Error: " + e.SocketError);
            client.ConnectionComplete += (s, e) =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\nConnected! Joining " + ProgramSettings.settings.LastJoinedChannel + "...");
                Console.ForegroundColor = ConsoleColor.White;
                client.JoinChannel(ProgramSettings.settings.LastJoinedChannel);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\nSuccessfully joined " + ProgramSettings.settings.LastJoinedChannel + "\n");
                Console.ForegroundColor = ConsoleColor.White;

            };
            client.ConnectAsync();
            InputThread.Start();

            client.NoticeRecieved += (s, e) =>
            {
                Console.WriteLine("\nNOTICE FROM {0}: {1}", e.Source, e.Notice.ToString());
            };

            client.ChannelMessageRecieved += (s, e) =>
            {

                var channels = client.Channels[e.PrivateMessage.Source];
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("{0} - {1}: {2}", e.PrivateMessage.Source, e.PrivateMessage.User, e.PrivateMessage.Message); //just output the stuff boss
                Console.ForegroundColor = ConsoleColor.White;

                if (e.PrivateMessage.Message.StartsWith(ProgramSettings.settings.Prefix.ToString()))
                {
                    ircCommands(e.PrivateMessage.Message, e.PrivateMessage.User, client);
                }
                else if (e.PrivateMessage.Message.Contains(client.User.Nick))
                {
                    if (e.PrivateMessage.Message.Contains("help"))
                    {
                        char commandpre = ProgramSettings.settings.Prefix;
                        client.SendRawMessage("PRIVMSG {0} :Hi! I'm a bot! The current prefix is {1}. You can get a list of my commands with {1}commands!", client.Channels[0].Name, commandpre);
                    }
                    if (e.PrivateMessage.Message.Contains("Hello") || e.PrivateMessage.Message.Contains("Hi") || e.PrivateMessage.Message.Contains("Hey"))
                    {
                        
                        client.SendRawMessage("PRIVMSG {0} :Hello there!", client.Channels[0].Name);
                    }
                    if (e.PrivateMessage.Message.Contains("what time"))
                    {
                        string time = DateTime.Now.ToString("h:mm:ss tt");
                        client.SendRawMessage("PRIVMSG {0} :The time is currently {1} UTC-8 (PST).", client.Channels[0].Name, time);
                    }
                    if (e.PrivateMessage.Message.Contains("how are you"))
                    {
                        int feels = random.Next(0,6);
                        string imfeeling = feeling[feels];
                        client.SendRawMessage("PRIVMSG {0} :{1}.", client.Channels[0].Name, imfeeling);
                    }

                }
                else
                {
                    if (e.PrivateMessage.Message.Contains("http://") && !e.PrivateMessage.Message.Contains("dnp"))
                    {
                        try
                        {
                            WebClient x = new WebClient();
                            client.SendRawMessage("PRIVMSG {0} :Grabbing link title...", client.Channels[0].Name);
                            string url = e.PrivateMessage.Message.Substring(e.PrivateMessage.Message.LastIndexOf("http://"));
                            string[] cleaned = url.Split(new char[] { ' ' }, 2);
                            string source = x.DownloadString(cleaned[0]);
                            string title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
                            client.SendRawMessage("PRIVMSG {0} :{3} submitted a link : [ {1} ] - {2}.", client.Channels[0].Name, title, cleaned[0], e.PrivateMessage.User.Nick);
                        }
                        catch
                        {
                            //Do nothing.
                        }
                    }


                }
            };

            while (true)
                ; //just keeps everything going
        }



        private static void Shutdown()
        {
            ProgramSettings.WriteSettings();
            client.Quit("All your bots are belong to us!");
            DisplayOutputMessage(" RetaSharp has been shut down.", true);
            Console.ReadKey();
        }

        private static void Reconnect()
        {
            ProgramSettings.WriteSettings();
            client.Quit("Restarting...");
            Application.Restart();
            Environment.Exit(0);
        }

        private static void Input()
        {
            bool accept = true;
            while (accept)
            {
                string input = Console.ReadLine();
                if (input.StartsWith("/")){
                    if (input.StartsWith("/help"))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("List of available commands:");
                        Console.ForegroundColor = ConsoleColor.Green;
                        DisplayOutputMessage(" /help - asks for this list of commands.", true);
                        DisplayOutputMessage(" /quit - closes down RetaSharp.", true);
                        DisplayOutputMessage(" /restart - closes down RetaSharp and allows reconnecting.", true);
                    }
                    if (input.StartsWith("/quit"))
                    {
                        DisplayOutputMessage(" Closing down RetaSharp...", true);
                        Shutdown();
                    }
                    if (input.StartsWith("/restart"))
                    {
                        DisplayOutputMessage(" Restarting the bot...", true);
                        Reconnect();
                    }
                }
            }
        }

        private static void ircCommands(string command, IrcUser sender, IrcClient reciever)
        {
            command = command.TrimStart(ProgramSettings.settings.Prefix); //simply removes the prefix
            

            if (command.StartsWith("quit"))
            {
                foreach (string user in ProgramSettings.settings.UsersAllowedToDisable)
                {
                    if (user.ToLower() == sender.Nick.ToLower())
                    {
                        Shutdown();
                    }
                }
            }
            if (command.StartsWith("help") || command.StartsWith("info"))
            {
                client.SendRawMessage("PRIVMSG {0} :Hi! I'm a bot! The current prefix is {1}. You can get a list of my commands with {1}commands!", client.Channels[0].Name, ProgramSettings.settings.Prefix);
            }
            if (command.StartsWith("commands"))
            {
                client.SendRawMessage("PRIVMSG {0} :Hi {1}! I'm sending you a list of my commands!", client.Channels[0].Name, sender.Nick);
                client.SendRawMessage("PRIVMSG {0} :Here is a list of my commands!", sender.Nick);
                client.SendRawMessage("PRIVMSG {0} :quit, help, commands, dice, d, slap", sender.Nick);
                client.SendRawMessage("PRIVMSG {0} :If you have any command suggestions, feel free to ping my owner, Blank!", sender.Nick);
            }
            if (command.StartsWith("dice") || command.StartsWith("d"))
            {
                string[] splitcommand = command.Split(new char[] { ' ' }, 2);
                if (splitcommand.Length > 1){
                    if (splitcommand[1] == "blunt")
                    {
                        client.SendRawMessage("PRIVMSG {0} :Hey {1}, pass the weed!", client.Channels[0].Name, sender.Nick);
                        return;
                    }
                    try
                    {
                        string number = splitcommand[1].ToLower();
                        float diceroll = Convert.ToSingle(number);
                        if (diceroll > 999)
                        {
                            client.SendRawMessage("PRIVMSG {0} :I don't have that many die!", client.Channels[0].Name);
                        }
                        else if (diceroll <= 0)
                        {
                            client.SendRawMessage("PRIVMSG {0} :I can't roll negative dice!", client.Channels[0].Name);
                        }
                        else
                        {
                            float nextValue = random.Next(1, 7); // Returns a random number from 0-99
                            float finaloutcome = (1 + nextValue) * diceroll;
                            string finaloutcometxt = Convert.ToString(finaloutcome);

                            client.SendRawMessage("PRIVMSG {0} :You rolled a {1}", client.Channels[0].Name, finaloutcometxt);
                        }
                    }
                    catch
                    {
                        client.SendRawMessage("PRIVMSG {0} :I can't roll that!", client.Channels[0].Name);
                    }
                }
            }

            if (command.StartsWith("slap"))
            {
                string[] splitcommand = command.Split(new char[] { ' ' }, 2);
                
                if (splitcommand.Length > 1)
                {
                    if (splitcommand[1] != client.User.Nick)
                    {
                        client.SendRawMessage("PRIVMSG {0} :" + "\x01" + "ACTION was commanded by {1} to slap {2} with a giant fishbot, and does so.\x01", client.Channels[0].Name, sender.Nick, splitcommand[1]);
                    }
                }

            }
            /*
            if (command.StartsWith("join"))
            {
                string[] splitcommand = command.Split(new char[] { ' ' }, 2);
                if (splitcommand.Length > 1){
                    client.SendRawMessage("JOIN {0}", splitcommand[1]);
                }
            }*/
            if (command.StartsWith("setprefix"))
            {
                string[] splitcommand = command.Split(new char[] { ' ' }, 2);

                foreach (string user in ProgramSettings.settings.UsersAllowedToDisable)
                {
                    if (user.ToLower() == sender.Nick.ToLower())
                    {
                        if (splitcommand.Length > 1)
                        {
                            if (splitcommand[1] == "/")
                            {
                                client.SendRawMessage("PRIVMSG {0} :Ahahahaha I don't think so, smart ass.", client.Channels[0].Name);
                                return;
                            }
                            try
                            {
                                char newPrefix = char.Parse(splitcommand[1]);
                                ProgramSettings.settings.Prefix = newPrefix;

                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("\nWARNING! Prefix changed to " + newPrefix + "!");
                                Console.ForegroundColor = ConsoleColor.Gray;

                                client.SendRawMessage("PRIVMSG {0} :NOTICE : Prefix changed to {1}!", client.Channels[0].Name, newPrefix);
                                return;
                            }
                            catch (Exception ex)
                            {
                                client.SendRawMessage("PRIVMSG {0} :ERROR : Something went wrong! Blank HELP!", client.Channels[0].Name);
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("ERROR : PREFIX CHANGE FAILED!\n");
                                Console.ForegroundColor = ConsoleColor.Gray;
                            }
                        }
                        else
                        {
                            client.SendRawMessage("PRIVMSG {0} :The prefix must be a single character!", client.Channels[0].Name);
                        }
                    }
                }
            }

            if (command.StartsWith("trivia"))
            {
                string[] splitcommand = command.Split(new char[] { ' ' }, 2);
                if (splitcommand.Length >= 1)
                {
                    try
                    {
                        ttlquestions = Convert.ToInt32(splitcommand[1]);
                        Trivia.StartTrivia();

                        client.SendRawMessage("PRIVMSG {0} :TRIVIA HAS BEEN TOGGLED BY {1}!", client.Channels[0].Name, sender);
                        client.SendRawMessage("PRIVMSG {0} :Question {2} : {1}!", client.Channels[0].Name, Trivia.thisquestion, Trivia.currentquestion);

                        if (command.StartsWith("A"))
                        {
                            string[] splitcommand2 = command.Split(new char[] { ' ' });
                            if (splitcommand2[1].Equals(Trivia.thisanswer, StringComparison.InvariantCultureIgnoreCase))
                            {
                                client.SendRawMessage("PRIVMSG {0} :{1} Got the answer correct! ({2})", client.Channels[0].Name, sender, Trivia.thisanswer);
                                Trivia.StartTrivia();
                                return;
                            }
                        }
                    }
                    catch
                    {

                    }

                }
            }
            


        }






    }
}
