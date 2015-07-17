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
    public class Trivia
    {
        public static string[] questions = new string[] {
            "Which singer joined Mel Gibson in the movie Mad Max: Beyond The Thunderdome?"

		};

        public static string[] answers = new string[] {
            "TINATURNER"
        };

        List<int> points = new List<int>();

        List<string> users = new List<string>();

        public static string thisquestion;
        public static string thisanswer;
        public static int currentquestion;

        public TrivaData database = new TrivaData();

        public void WriteSettings()
        {

            JsonSerializer js = new JsonSerializer();
            js.Formatting = Formatting.Indented;

            using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\database.json"))
            {
                using (JsonWriter jsw = new JsonTextWriter(sw))
                {
                    js.Serialize(jsw, this.database);
                }
            }

        }

        public static void StartTrivia()
        {
            int maxqs = Program.ttlquestions;
            Random random = new Random();
            currentquestion = random.Next(0,1);
            thisquestion = questions[currentquestion];
            thisanswer = answers[currentquestion];
            Program.ttlquestions -= 1;
        }
    }
}
