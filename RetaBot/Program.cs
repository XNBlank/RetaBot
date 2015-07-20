using ChatSharp;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
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
        public static bool triviaToggle;
        public static int triviaTimer;
        public static System.Timers.Timer timer = new System.Timers.Timer();
        public static int currentQuestion;

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
                    if (e.PrivateMessage.Message.Contains("Hello".ToLower()) || e.PrivateMessage.Message.Contains("Hi".ToLower()) || e.PrivateMessage.Message.Contains("Hey".ToLower()) || e.PrivateMessage.Message.Contains("Howdy".ToLower()) || e.PrivateMessage.Message.Contains("Hola".ToLower()) || e.PrivateMessage.Message.Contains("Bonjour".ToLower()))
                    {
                        
                        client.SendRawMessage("PRIVMSG {0} :Hello there!", client.Channels[0].Name);
                    }
                    if (e.PrivateMessage.Message.Contains("time"))
                    {
                        string time = DateTime.Now.ToString("h:mm:ss tt");
                        client.SendRawMessage("PRIVMSG {0} :The time is currently {1} UTC-8 (PST).", client.Channels[0].Name, time);
                    }
                    if (e.PrivateMessage.Message.Contains("how") && e.PrivateMessage.Message.Contains("are") || e.PrivateMessage.Message.Contains("you"))
                    {
                        int feels = random.Next(0,6);
                        string imfeeling = feeling[feels];
                        client.SendRawMessage("PRIVMSG {0} :{1}.", client.Channels[0].Name, imfeeling);
                    }


                    if (e.PrivateMessage.Message.Contains("hug") || e.PrivateMessage.Message.Contains("hugs"))
                    {
                        client.SendRawMessage("PRIVMSG {0} :" + "\x01" + "ACTION hugs {1} back.\x01", client.Channels[0].Name, e.PrivateMessage.User.Nick);
                    }

                    if (e.PrivateMessage.Message.Contains("kiss") || e.PrivateMessage.Message.Contains("kisses") || e.PrivateMessage.Message.Contains("makes out"))
                    {
                        client.SendRawMessage("PRIVMSG {0} :" + "\x01" + "ACTION pushes {1} away.\x01", client.Channels[0].Name, e.PrivateMessage.User.Nick);
                        client.SendRawMessage("PRIVMSG {0} :Uhm...no kissing me, {1}.", client.Channels[0].Name, e.PrivateMessage.User.Nick);
                    }
                    if (e.PrivateMessage.Message.Contains("sex") || e.PrivateMessage.Message.Contains("fuck") || e.PrivateMessage.Message.Contains("fucks") || e.PrivateMessage.Message.Contains("humps") || e.PrivateMessage.Message.Contains("rapes") || e.PrivateMessage.Message.Contains("inappropriate") && e.PrivateMessage.Message.Contains("place") || e.PrivateMessage.Message.Contains("places"))
                    {
                        client.SendRawMessage("PRIVMSG {0} :" + "\x01" + "ACTION punches {1}.\x01", client.Channels[0].Name, e.PrivateMessage.User.Nick);
                        client.SendRawMessage("PRIVMSG {0} :DON'T TOUCH ME, {1}!", client.Channels[0].Name, e.PrivateMessage.User.Nick.ToUpper());
                        try
                        {
                            client.SendRawMessage("PRIVMSG CHANSERV :op");
                            try
                            {
                                client.KickUser(client.Channels[0].Name, e.PrivateMessage.User.Nick, "EXPLOIT!");
                            }
                            catch
                            {
                                client.SendRawMessage("PRIVMSG {0} :IF ONLY I COULD KICK YOU...!", client.Channels[0].Name);
                            }
                            client.SendRawMessage("PRIVMSG CHANSERV :deop");
                        }
                        catch
                        {
                            client.SendRawMessage("PRIVMSG {0} :I NEED AN ADMIN!", client.Channels[0].Name);
                        }
                    }

                    if (e.PrivateMessage.Message.Contains("joke"))
                    {
                        Jokes.Joke();
                        client.SendRawMessage("PRIVMSG {0} :{1}", client.Channels[0].Name, Jokes.thisjoke);
                        Console.WriteLine("The joke was " + Jokes.randJoke);
                    }

                }
                else
                {
                    string testurl = e.PrivateMessage.Message;
                    Uri uriResult;
                    bool result = Uri.TryCreate(testurl, UriKind.RelativeOrAbsolute, out uriResult)
                                  && (uriResult.Scheme == Uri.UriSchemeHttp
                                      || uriResult.Scheme == Uri.UriSchemeHttps);
                    
                    if (result == true && !e.PrivateMessage.Message.Contains("dnp"))
                    {
                        
                            try
                            {
                                string checkURL = Regex.Match(testurl, @"[(http(s)?):\/\/(www\.)?a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)", RegexOptions.IgnoreCase).Groups["URL"].Value;

                                WebClient x = new WebClient();
                                client.SendRawMessage("NOTICE {0} :{0} Hold on, grabbing link title...", e.PrivateMessage.User.Nick);
                                string url = testurl.Substring(e.PrivateMessage.Message.LastIndexOf(checkURL));
                                string[] cleaned = url.Split(new char[] { ' ' }, 2);
                                if (testurl.Contains(".xxx") || testurl.Contains("porn"))
                                {
                                    client.SendRawMessage("PRIVMSG {0} :Thanks for posting porn here, {1}", client.Channels[0].Name, e.PrivateMessage.User.Nick);
                                    return;
                                }
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
            System.Windows.Forms.Application.Restart();
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

        public static void triviaQuestion()
        {

            Trivia.StartTrivia();
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Interval = 1001;
            triviaTimer = 240;
            timer.Enabled = true;
            triviaToggle = true;
            int questionNum = Trivia.currentquestion + 1;


            client.SendRawMessage("PRIVMSG {0} :Question {2} : {1}", client.Channels[0].Name, Trivia.thisquestion, currentQuestion);
        }


        public static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (triviaTimer <= 0)
            {
                triviaTimer = 240;
            }
            else
            {
                triviaTimer--;
            }


            if (triviaTimer <= 0 && triviaToggle == true)
            {
                triviaTimer = 0;
                client.SendRawMessage("PRIVMSG {0} :TIME IS UP! THE CORRECT ANSWER IS ({1})", client.Channels[0].Name, Trivia.thisanswer);
                if (currentQuestion < ttlquestions)
                {
                    timer.Enabled = false;
                    currentQuestion += 1;
                    triviaQuestion();
                }
                else
                {
                    triviaTimer = 0;
                    timer.Enabled = false;
                    triviaToggle = false;
                    client.SendRawMessage("PRIVMSG {0} :THAT'S THE END OF THE LIST FOLKS! TRIVIA IS OVER!", client.Channels[0].Name);
                    return;
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
                client.SendRawMessage("PRIVMSG {0} :quit, help, info, commands, roll, slap, trivia", sender.Nick);
                client.SendRawMessage("PRIVMSG {0} :If you have any command suggestions, feel free to ping my owner, Blank!", sender.Nick);
            }
            if (command.StartsWith("roll"))
            {
                string[] splitcommand = command.Split(new char[] { ' ', 'd' }, 3);
                if (splitcommand.Length > 1){
                    string secondnumber;
                    int secondDice = 0;

                    if (splitcommand[1].Contains("blunt"))
                    {
                        client.SendRawMessage("PRIVMSG {0} :Hey {1}, pass the weed!", client.Channels[0].Name, sender.Nick);
                        return;
                    }
                    if (command.Contains("d"))
                    {
                        try{
                        secondnumber = splitcommand[2].ToLower();
                        secondDice = Convert.ToInt32(secondnumber);
                        }
                        catch
                        {
                            client.SendRawMessage("PRIVMSG {0} :{1}, make sure you type in a # or #d#!", client.Channels[0].Name, sender.Nick);
                            return;
                        }
                    }
                    try
                    {
                        string number = splitcommand[1].ToLower();
                        float diceroll = Convert.ToSingle(number);
                        if (diceroll > 999)
                        {
                            client.SendRawMessage("PRIVMSG {0} :I don't have that many dice!", client.Channels[0].Name);
                        }
                        else if (diceroll <= 0)
                        {
                            client.SendRawMessage("PRIVMSG {0} :I can't roll negative dice!", client.Channels[0].Name);
                        }
                        else
                        {
                            if (secondDice > 0)
                            {
                                float nextValue = random.Next(1, secondDice); // Returns a random number from 0-99
                                float finaloutcome = (1 + nextValue) * diceroll;
                                string finaloutcometxt = Convert.ToString(finaloutcome);

                                client.SendRawMessage("PRIVMSG {0} :You rolled a {1}", client.Channels[0].Name, finaloutcometxt);
                            }
                            else if (secondDice <= 0)
                            {
                                float nextValue = random.Next(1, 7); // Returns a random number from 0-99
                                float finaloutcome = (1 + nextValue) * diceroll;
                                string finaloutcometxt = Convert.ToString(finaloutcome);

                                client.SendRawMessage("PRIVMSG {0} :You rolled a {1}", client.Channels[0].Name, finaloutcometxt);
                            }
                            else
                            {
                                float nextValue = random.Next(1, 7); // Returns a random number from 0-99
                                float finaloutcome = (1 + nextValue) * diceroll;
                                string finaloutcometxt = Convert.ToString(finaloutcome);

                                client.SendRawMessage("PRIVMSG {0} :You rolled a {1}", client.Channels[0].Name, finaloutcometxt);
                            }
                            
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
                                client.SendRawMessage("PRIVMSG {0} :The prefix must be a single character!", client.Channels[0].Name);
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("ERROR : "+ ex +"\n");
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
                currentQuestion = 1;
                    if (splitcommand.Length >= 1)
                    {

                        if (triviaToggle == true)
                        {
                            if (splitcommand.Length > 1)
                            {
                                if (splitcommand[1].Contains("end"))
                                {
                                    client.SendRawMessage("PRIVMSG {0} :{1} TOGGLED TRIVIA OFF!", client.Channels[0].Name, sender.Nick);
                                    triviaToggle = false;
                                    timer.Enabled = false;
                                    return;
                                }
                                else if (triviaToggle == false && !splitcommand[1].Contains("end"))
                                {
                                    client.SendRawMessage("PRIVMSG {0} :{1}, Trivia isn't toggled.", client.Channels[0].Name, sender.Nick);
                                    return;
                                }
                            }

                        }


                        if (triviaToggle == false)
                        {

                            try
                            {

                                bool result = Int32.TryParse(splitcommand[1], out ttlquestions);
                                if (result)
                                {
                                    Console.WriteLine("\nConverted '{0}' to {1}.", splitcommand[1], ttlquestions);
                                    client.SendRawMessage("PRIVMSG {0} :TRIVIA HAS BEEN TOGGLED! If you know the answer, respond with '-A answer'!", client.Channels[0].Name);
                                }

                                triviaQuestion();

                            }
                            catch
                            {
                                //Nothing
                            }
                        }
                   
                }




            }
            if (triviaToggle == true && triviaTimer > 0)
            {
                if (command.StartsWith("A"))
                {
                    string[] splitcommand2 = command.Split(new char[] { ' ' }, 2);
                    string input = splitcommand2[1];

                    if (input.Equals(Trivia.thisanswer, StringComparison.CurrentCultureIgnoreCase))
                    {
                        client.SendRawMessage("PRIVMSG {0} :{1} Got the answer correct! ({2})", client.Channels[0].Name, sender.Nick, Trivia.thisanswer);
                        timer.Enabled = false;
                        if (currentQuestion < ttlquestions)
                        {
                            currentQuestion += 1;

                            triviaQuestion();
                        }
                        else
                        {
                            timer.Interval = 1;
                            triviaToggle = false;
                            client.SendRawMessage("PRIVMSG {0} :THAT'S THE END OF THE LIST FOLKS! TRIVIA IS OVER!", client.Channels[0].Name);
                            return;
                        }
                    }
                }
            }
            else if (triviaTimer <= 0 && triviaToggle == true)
            {
                triviaTimer = 0;
                client.SendRawMessage("PRIVMSG {0} :TIME IS UP! THE CORRECT ANSWER IS ({1})", client.Channels[0].Name, Trivia.thisanswer);
                triviaToggle = false;
            }

            

            if (command.StartsWith("reverse"))
            {
                string[] splitcommand = command.Split(new char[] { ' ' }, 2);
                char[] charArray = splitcommand[1].ToCharArray();
                Array.Reverse(charArray);
                String reverse = new String(charArray);
                string returntext = reverse;
                client.SendRawMessage("PRIVMSG {0} :{1}", client.Channels[0].Name, returntext);

            }


            

        }





    }
}
