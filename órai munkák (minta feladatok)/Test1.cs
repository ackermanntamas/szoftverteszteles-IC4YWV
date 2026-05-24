using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace LoginTest
{
    [TestClass]
    public sealed class LoginTests
    {
        IWebDriver driver;

        [TestInitialize]
        public void Setup()
        {
            driver = new ChromeDriver();
            // Open page
            driver.Navigate().GoToUrl("https://practicetestautomation.com/practice-test-login/");
        }

        [TestCleanup]
        public void Teardown()
        {
            // Thread.Sleep(3000);
            driver.Quit();
        }

        [TestMethod]
        public void PositiveLoginTest()
        {
            
            // 2. Type username student into Username field
            var username = driver.FindElement(By.Id("username"));
            username.SendKeys("student");

            // 3. Type password Password123 into Password field     
            var password = driver.FindElement(By.Name("password"));
            password.SendKeys("Password123");

            // 4. Push Submit button
            var submit = driver.FindElement(By.Id("submit"));
            submit.Click();

            // 5. Verify new page URL contains practicetestautomation.com/logged-in-successfully /
            Assert.IsTrue(driver.Url.Contains("practicetestautomation.com/logged-in-successfully/"));

            // Verify new page contains expected text('Congratulations' or 'successfully logged in')
            // var message = driver.FindElement(By.XPath("//*[text()='Congratulations']"));
            // Assert.IsNotNull(message);

            // Verify button Log out is displayed on the new page
            // <a href="https://practicetestautomation.com/practice-test-login/">Log out</a>

            // var logout = driver.FindElement(By.XPath("/html/body/div/div/section/div/div/article/div[2]/div/div/div/a"));
            // var logout = driver.FindElement(By.XPath("//*[@id=\"loop-container\"]/div/article/div[2]/div/div/div/a"));
            var logout = driver.FindElement(By.XPath("//a[text()='Log out']"));
            Assert.IsTrue(logout.Displayed);
        }

        [TestMethod]
        public void NegativeUsernameTest()
        {
            // 2. Type username student into Username field
            var username = driver.FindElement(By.Id("username"));
            username.SendKeys("incorrectUser");

            // 3. Type password Password123 into Password field     
            var password = driver.FindElement(By.Name("password"));
            password.SendKeys("Password123");

            // 4. Push Submit button
            var submit = driver.FindElement(By.Id("submit"));
            submit.Click();

            // 5. Verify error message is displayed
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("error")));
            var error = driver.FindElement(By.Id("error"));
            Assert.IsTrue(error.Displayed);

            // 6. Verify error message text is Your username is invalid!
            Assert.AreEqual("Your username is invalid!", error.Text);
        }
    }
}
