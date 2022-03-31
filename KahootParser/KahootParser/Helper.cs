using System;
using System.Threading;
using OpenQA.Selenium;

namespace KahootParser
{
    public static class Helper
    {
        public static string Nickname;

        public static void GenerateNickname()
        {
            var random = new Random();
            Nickname = random.Next(0, 999999999).ToString();
        }

        public static void SignIn(IWebDriver driver)
        {
            while (true)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div/div/div/div/div/main/section/div[1]/form/input"));
                    break;
                }
                catch
                {
                    Thread.Sleep(500);
                }
            }

            var nicknameElement =
                driver.FindElement(By.XPath("/html/body/div/div/div/div/div/main/section/div[1]/form/input"));
            Thread.Sleep(500);
            nicknameElement.SendKeys(Nickname);

            var buttonSubmitNickname =
                driver.FindElement(By.XPath("/html/body/div/div/div/div/div/main/section/div[1]/form/button"));
            Thread.Sleep(1500);
            buttonSubmitNickname.Submit();
        }

        public static void ClickNextButton(string xPath, IWebDriver driver)
        {
            while (true)
                try
                {
                    var buttonNext =
                        driver.FindElement(By.XPath(xPath));
                    buttonNext.Click();
                    break;
                }
                catch
                {
                    Thread.Sleep(500);
                }
        }
    }
}