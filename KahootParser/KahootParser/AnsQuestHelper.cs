using System;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace KahootParser
{
    public static class AnsQuestHelper
    {
        private static readonly Dictionary<string, List<string>> QuestionAnswersDictionary =
            new Dictionary<string, List<string>>();

        private static string _type = "";

        private static void FindRightAnswer(IWebDriver driver)
        {
            _type = "";
            string question = null;

            KeepFindingQuestionElement(driver, ref question);

            if (_type == string.Empty)
                _type = CheckTypeOfTask(driver);

            switch (_type)
            {
                case "quiz":
                    driver.FindElement(By.XPath("/html/body/div/div/div/div/div/main/div[2]/button[1]")).Click();
                    Thread.Sleep(2000);
                    QuizMultiAddRightAnswer(driver);
                    break;
                case "type":
                    driver.FindElement(By.XPath("/html/body/div/div/div/div/div/main/div[2]/section/div/input"))
                        .SendKeys("123");
                    driver.FindElement(By.XPath("/html/body/div/div/div/div/div/main/div[1]/div[3]/div/button"))
                        .Click();
                    Thread.Sleep(2000);
                    TypeAddRightAnswer(driver);
                    break;
                case "slide":
                    driver.FindElement(
                        By.XPath("/html/body/div/div/div/div/div/main/main/div[2]/section/div[1]/div/button")).Click();
                    break;
                case "multi":
                    driver.FindElement(By.XPath("/html/body/div/div/div/div/div/main/div[2]/button[1]")).Click();
                    driver.FindElement(By.XPath("/html/body/div/div/div/div/div/main/div[1]/div[3]/div/button"))
                        .Click();
                    Thread.Sleep(2000);
                    QuizMultiAddRightAnswer(driver);
                    break;
                case "error":
                    Console.WriteLine("Not available task");
                    Environment.Exit(0);
                    break;
            }
        }

        private static void KeepFindingQuestionElement(IWebDriver driver, ref string question)
        {
            while (true) // keep finding question element
            {
                try
                {
                    if ((question = driver
                            .FindElement(By.XPath("//*[@id=\"challenge-game-router\"]/main/header/div[1]/span"))
                            .Text) !=
                        null)
                        break;
                }
                catch
                {
                    try
                    {
                        if (driver.FindElement(
                                By.XPath(
                                    "/html/body/div/div/div/div/div/main/main/div[2]/section/div[1]/div/button")) !=
                            null)
                        {
                            _type = "slide";
                            break;
                        }
                    }
                    catch
                    {
                        // ignored
                    }

                    Thread.Sleep(500);
                }
            }
        }

        private static void QuizMultiAddRightAnswer(IWebDriver driver)
        {
            var answerButtonElems =
                driver.FindElements(By.XPath("/html/body/div/div/div/div/div/main/div[3]/div"));

            foreach (var item in answerButtonElems) // check if button backColor is green -> right answer
                if (item.GetCssValue("background-color") == "rgba(102, 191, 57, 1)")
                {
                    var questionElem = driver
                        .FindElement(By.XPath("/html/body/div/div/div/div/div/main/header/div[1]/span"));

                    if (!QuestionAnswersDictionary.ContainsKey(questionElem.GetAttribute("innerText")))
                        QuestionAnswersDictionary.Add(questionElem.GetAttribute("innerText"), new List<string>());

                    QuestionAnswersDictionary[questionElem.GetAttribute("innerText")]
                        .Add(item.GetAttribute("innerText"));
                }
        }

        private static void TypeAddRightAnswer(IWebDriver driver)
        {
            var answerButtonElem =
                driver.FindElement(By.XPath(
                    "/html/body/div/div/div/div/div/main/div[2]/div[2]/div/div[2]/div/div[1]/div/div/button"));

            var questionElem = driver
                .FindElement(By.XPath("/html/body/div/div/div/div/div/main/header/div[1]/span"));

            if (!QuestionAnswersDictionary.ContainsKey(questionElem.GetAttribute("innerText")))
                QuestionAnswersDictionary.Add(questionElem.GetAttribute("innerText"), new List<string>());

            QuestionAnswersDictionary[questionElem.GetAttribute("innerText")]
                .Add(answerButtonElem.GetAttribute("innerText"));
        }

        public static void AnswerQuestions(WebDriverWait wait, IWebDriver driver)
        {
            if (driver.FindElement(By.XPath("/html/body/div/div/div/div/div/div/header/h1")).Displayed)
                driver.Navigate().Refresh();

            wait.Until(d => ((IJavaScriptExecutor) d).ExecuteScript("return document.readyState").Equals("complete"));
            Thread.Sleep(1000);

            IWebElement noNotMeElem;
            while (true)
            {
                try
                {
                    noNotMeElem = driver.FindElement(By.XPath("/html/body/div/div/div/div/div/main/section/div[1]/div/button[1]"));
                    break;
                }
                catch
                {
                    Thread.Sleep(500);
                }
            }

            // driver.FindElement(By.XPath("/html/body/div/div/div/div/div/main/section/div[1]/div/button[1]"))
            //     .Click(); // click no it's not me btn
            noNotMeElem.Click(); // click no it's not me btn
            Thread.Sleep(500);

            var amountOfQuestionsElement =
                driver.FindElement(
                    By.XPath("/html/body/div/div/div/div/div/main/div[2]/section/div[2]/footer/div[1]/span"));
            var amountOfQuestions = Convert.ToInt32(amountOfQuestionsElement.GetAttribute("innerText"));

            Helper.SignIn(driver);

            for (var i = 0; i < amountOfQuestions; i++)
            {
                _type = "";
                string question = null;

                KeepFindingQuestionElement(driver, ref question);

                if (_type == string.Empty)
                    _type = CheckTypeOfTask(driver);

                switch (_type)
                {
                    case "quiz":
                        AnswerQuiz(driver, question);
                        break;
                    case "type":
                        AnswerType(driver, question);
                        break;
                    case "slide":
                        driver.FindElement(
                                By.XPath("/html/body/div/div/div/div/div/main/main/div[2]/section/div[1]/div/button"))
                            .Click();
                        break;
                    case "multi":
                        AnswerMulti(driver, question);
                        break;
                    case "error":
                        Console.WriteLine("Not available task");
                        Environment.Exit(0);
                        break;
                }

                Thread.Sleep(2000);

                if (_type != "slide")
                    Helper.ClickNextButton("/html/body/div/div/div/div/div/main/div[2]/div[3]/div/button", driver);

                bool lastPage = FindLastPage(driver);

                if (!lastPage) // next btn doesn't exist after last question
                {
                    if (_type != "slide")
                        Helper.ClickNextButton("/html/body/div/div/div/div/div/div/main/div[2]/button", driver);
                }
            }
        }

        private static void AnswerMulti(IWebDriver driver, string question)
        {
            var answerButtonElems = driver.FindElements(
                By.XPath(
                    "//*[@id=\"challenge-game-router\"]/main/div[2]/button"));

            foreach (var item in answerButtonElems) // check all answer buttons and press the correct one 
                if (QuestionAnswersDictionary[question].Contains(item.GetAttribute("innerText")))
                {
                    item.Click();

                    if (QuestionAnswersDictionary[question].Count == 1)
                        break;
                }

            driver.FindElement(By.XPath("/html/body/div/div/div/div/div/main/div[1]/div[3]/div/button")).Click();
            Thread.Sleep(2000);
        }

        private static void AnswerType(IWebDriver driver, string question)
        {
            driver.FindElement(By.XPath("/html/body/div/div/div/div/div/main/div[2]/section/div/input"))
                .SendKeys(QuestionAnswersDictionary[question][0]);
            driver.FindElement(By.XPath("/html/body/div/div/div/div/div/main/div[1]/div[3]/div/button")).Click();
            Thread.Sleep(2000);
        }

        private static void AnswerQuiz(IWebDriver driver, string question)
        {
            var answerButtonElems = driver.FindElements(
                By.XPath(
                    "//*[@id=\"challenge-game-router\"]/main/div[2]/button"));

            foreach (var item in answerButtonElems) // check all answer buttons and press the correct one 
                if (QuestionAnswersDictionary[question].Contains(item.GetAttribute("innerText")))
                {
                    item.Click();

                    if (QuestionAnswersDictionary[question].Count == 1)
                        break;
                }

            Thread.Sleep(2000);
        }

        public static void GetAnswersAndQuestions(IWebDriver driver)
        {
            Helper.GenerateNickname();
            Helper.SignIn(driver);

            // ----------

            while (true)
            {
                FindRightAnswer(driver);

                Thread.Sleep(2000);

                if (_type != "slide") 
                    Helper.ClickNextButton("/html/body/div/div/div/div/div/main/div[2]/div[3]/div/button", driver);
                Thread.Sleep(1000);

                var lastPage = FindLastPage(driver);

                if (!lastPage)
                {
                    if (_type != "slide")
                        Helper.ClickNextButton("/html/body/div/div/div/div/div/div/main/div[2]/button", driver);
                }
                else
                    break;
            }
        }

        private static bool FindLastPage(IWebDriver driver)
        {
            try
            {
                if (driver.FindElement(By.XPath("/html/body/div/div/div/div/div/div/main/h1")) !=
                    null) // You have completed the challenge
                    return true;
            }
            catch
            {
                // ignored
            }

            return false;
        }

        private static string CheckTypeOfTask(IWebDriver driver)
        {
            try
            {
                // check for type answer
                if (driver.FindElement(By.XPath("/html/body/div/div/div/div/div/main/div[2]/section/div/input")) !=
                    null)
                    return "type";
            }
            catch
            {
                // ignored
            }

            try
            {
                // check for slide
                if (driver.FindElement(
                    By.XPath("/html/body/div/div/div/div/div/main/main/div[2]/section/div[1]/div/button")) != null)
                    return "slide";
            }
            catch
            {
                // ignored
            }

            try
            {
                // check for multi-select
                if (driver.FindElement(By.XPath("/html/body/div/div/div/div/div/main/div[1]/div[3]/div/button")) !=
                    null)
                    return "multi";
            }
            catch
            {
                // ignored
            }

            try
            {
                // check for quiz
                if (driver.FindElement(By.XPath("/html/body/div/div/div/div/div/main/div[2]/button[1]")) != null)
                    return "quiz";
            }
            catch
            {
                // ignored
            }

            return "error";
        }
    }
}