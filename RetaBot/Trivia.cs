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
            "In 1964 Tanganyika united with the island of Zanzibar, forming what modern African country?",
            "If the eastern end of the earth is called the Orient, what is the western end called?",
            "What Catholic church official ranks just below Pope?",
            "What is the name for the French dish of thin pancakes filled with sweet or spicy fillings?",
            "Beginning in 1987, Air Canada became the first air carrier to prohibit its passengers from doing what on board?",
            "The Baby Ruth candy bar was named after which real person?",
            "The name of which plant, cultivated for its edible leaves, comes from Latin word for milk, because of its milky juice?",
            "Which Charles Dickens literary work featured the mean-spirited miserly character named Ebenezer Scrooge?",
            "This word came to English from the Turkish and Persian languages, and referred to a beverage of sweetened, diluted fruit juice. Later in Europe this word referred to a carbonated drink. Today it refers to a fruit-flavored treat.",
            "What single city contains about 20% of the residents of its entire continent?",
            "Which 5-letter word can refer to a spicy Latin dance or a spicy Latin sauce?",
            "In 1927, what adventurous 25 year old man was the first Time Magazine Person of the Year?",
            "What is the name of the basic container in which most Chinese dishes are prepared?",
            "Name the soft white Greek cheese usually made from goat's milk.",
            "Churchills, Coronas, Lonsdales, and Torpedos are examples of what consumable products?",
            "What musical instrument plays the ascending opening notes of George Gershwin's masterpiece, 'Rhapsody in Blue'?",
		    "What food is made from vegetable oil, egg yolks, and lemon juice, and is possibly named after a town in Spain?",
            "What is the Japanese dish of deep fried vegetables and seafood?",
            "What country borders Peru and Panama?",
            "During a soccer (football) game at an English school in 1864, a player picked up the ball and began to run with it. What sport, named for the school where it happened, was thus born?",
            "What is the name for the long crisp bread loaf which French people buy and carry home under their arms?",
            "Which military forces gained control of Britain, in 1066, at the Battle of Hastings?",
            "U.S. flags are to be used until they are worn out, and then they are destroyed, preferably by what method?",
            "The whiskey called bourbon is distilled from a fermented mash composed at least 51% of what grain?",

        };

        public static string[] answers = new string[] {
            "Tanzania",
            "Occident",
            "Cardinal",
            "Crepes",
            "Smoking",
            "Ruth Cleveland",
            "Lettuce",
            "A Christmas Carol",
            "Sherbet",
            "Sydney",
            "Salsa",
            "Charles Lindbergh",
            "Wok",
            "Feta",
            "Cigars",
            "Clarinet",
            "Mayonnaise",
            "Tempura",
            "Columbia",
            "Rugby",
            "Baguette",
            "Normans-French",
            "Burning",
            "Corn",
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
            currentquestion = random.Next(0,24);
            thisquestion = questions[currentquestion];
            thisanswer = answers[currentquestion];
        }
    }
}
