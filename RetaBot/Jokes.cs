using Newtonsoft.Json;
using ChatSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace RetaBot
{
    class Jokes
    {
        public static string[] jokelist = new string[] {
            "Valtteri", //0
            "Three guys, stranded on a desert island, find a magic lantern containing a genie, who grants them each one wish. The first guy wishes he was off the island and back home. The second guy wishes the same. The third guy says 'I’m lonely. I wish my friends were back here.'",
            "A guy meets a hooker in a bar. She says, 'This is your lucky night. I’ve got a special game for you. I’ll do absolutely anything you want for $300, as long as you can say it in three words.' The guy replies, 'Hey, why not?' He pulls his wallet out of his pocket, and one at a time lays three hundred-dollar bills on the bar, and says, slowly: 'Paint…my…house.'",
            "A guy is sitting at home when he hears a knock at the door. He opens the door and sees a snail on the porch. He picks up the snail and throws it as far as he can. Three years later, there’s a knock on the door. He opens it and sees the same snail. The snail says 'What the hell was that all about?'",
            "My grandfather always said, 'Don't watch your money; watch your health.' So one day while I was watching my health, someone stole my money. It was my grandfather.",
            "Two guys are walking down the street when a mugger approaches them and demands their money. They both grudgingly pull out their wallets and begin taking out their cash. Just then one guy turns to the other and hands him a bill. 'Here’s that $20 I owe you,' he says.",
            "A Jewish grandmother is watching her grandchild playing on the beach when a huge wave comes and takes him out to sea. She pleads, 'please God, save my only grandson. I beg of you, bring him back.' And a big wave comes and washes the boy back onto the beach, good as new. She looks up to heaven and says: 'He had a hat!'",
            "I went to the psychiatrist, and he says 'You're crazy.' I tell him I want a second opinion. He says, 'Okay, you're ugly too!'",
            "A guy shows up late for work. The boss yells 'You should have been here at 8:30!' he replies: 'Why? What happened at 8:30?'"
		};

        public static string thisjoke;
        public static int randJoke;

        public static void Joke()
        {
            Random random = new Random();
            randJoke = random.Next(0, 9);
            thisjoke = jokelist[randJoke];
        }
    }
}
