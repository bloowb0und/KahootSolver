using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace KahootParser
{
    internal static class Program
    {
        private static IWebDriver _driver;
        private static string _linkUrl;

        public static void Main(string[] args)
        {
            // Entering data
            Console.WriteLine("Enter a kahoot test link:");
            _linkUrl = Console.ReadLine();

            /*
                TypeAnswer/Quiz/Slide: https://kahoot.it/challenge/07852038?challenge-id=6d8ab154-a4b9-44ac-9349-587048c104f3_1638829163051
                Quiz/TrueFalse/Multi-Select: https://kahoot.it/challenge/07264911?challenge-id=6d8ab154-a4b9-44ac-9349-587048c104f3_1638817487741
                Slide: https://kahoot.it/challenge/08972443?challenge-id=6d8ab154-a4b9-44ac-9349-587048c104f3_1638807504660
             */

            Console.WriteLine("Enter nickname:");
            var realNickname = Console.ReadLine();

            Console.WriteLine(new string('=', 20));
            Console.WriteLine("Data was successfully saved. Executing the answers checker.");
            Console.WriteLine(new string('=', 20));

            // Setting up the driver
            var options = new ChromeOptions();
            options.AddArgument("mute-audio");
            _driver = new ChromeDriver(options);

            _driver.Navigate().GoToUrl(_linkUrl);

            var wait = new WebDriverWait(_driver, TimeSpan.FromMilliseconds(1000));
            wait.Until(d => ((IJavaScriptExecutor) d).ExecuteScript("return document.readyState").Equals("complete"));

            // Define answers for questions
            Thread.Sleep(4000);
            AnsQuestHelper.GetAnswersAndQuestions(_driver);

            Console.WriteLine(new string('=', 20));
            Console.WriteLine("Answers and questions successfully written. Executing the test passer.");
            Console.WriteLine(new string('=', 20));

            // Sign in with real nickname and answer questions
            Helper.Nickname = realNickname;
            AnsQuestHelper.AnswerQuestions(wait, _driver);

            Thread.Sleep(5000);
            _driver.Quit();

            Console.WriteLine(new string('=', 20));
            Console.WriteLine("The test was successfully passed.");
            Console.WriteLine(new string('=', 20));
            Console.WriteLine("Press any key to continue...");

            Console.ReadKey();
        }
    }
}