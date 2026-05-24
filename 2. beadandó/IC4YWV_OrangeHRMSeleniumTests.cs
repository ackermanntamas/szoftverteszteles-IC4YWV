using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace IC4YWV_OrangeHRMSelenium
{
    // Selenium UI tesztek az OrangeHRM demo rendszerhez
    // URL: https://opensource-demo.orangehrmlive.com
    // login: Admin / admin123

    [TestClass]
    public class IC4YWV_OrangeHRMTests
    {
        private IWebDriver driver = null!;
        private WebDriverWait wait = null!;

        private const string LoginUrl = "https://opensource-demo.orangehrmlive.com/web/index.php/auth/login";
        private const string ValidUsername = "Admin";
        private const string ValidPassword = "admin123";

        [TestInitialize]
        public void Setup()
        {
            var options = new ChromeOptions();
            // options.AddArgument("--headless=new"); // ha nem akarjuk hogy felugorjon a chrome
            driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            driver.Navigate().GoToUrl(LoginUrl);
            // várjuk meg amíg a login form megjelenik
            Find(By.Name("username"));
        }

        [TestCleanup]
        public void Teardown()
        {
            driver?.Quit();
        }

        // segédmetódus: vár amíg az elem létezik (a NoSuchElementException alapból ignorált)
        private IWebElement Find(By by) => wait.Until(d => d.FindElement(by));

        // segédmetódus a login lépésekre, hogy ne ismétlődjön
        private void LoginAsAdmin()
        {
            Find(By.Name("username")).SendKeys(ValidUsername);
            Find(By.Name("password")).SendKeys(ValidPassword);
            Find(By.ClassName("orangehrm-login-button")).Click();
            wait.Until(d => d.Url.Contains("/dashboard"));
        }

        // 1. teszt - sikeres bejelentkezés
        // használt lekérdezések: By.Id, By.Name, By.ClassName, By.CssSelector
        // műveletek: SendKeys, Click
        // assertek: IsNotNull, StringAssert.Contains, AreEqual
        [TestMethod]
        public void PositiveLoginTest_AdminFiokkal_DashboardMegjelenik()
        {
            // az alkalmazás gyökér eleme létezik
            var appRoot = driver.FindElement(By.Id("app"));
            Assert.IsNotNull(appRoot, "az #app elem nem található");

            // felhasználónév és jelszó kitöltése
            Find(By.Name("username")).SendKeys(ValidUsername);
            Find(By.Name("password")).SendKeys(ValidPassword);

            // bejelentkezés gomb keresése class alapján
            Find(By.ClassName("orangehrm-login-button")).Click();

            // URL ellenőrzés
            wait.Until(d => d.Url.Contains("/dashboard"));
            StringAssert.Contains(driver.Url, "/dashboard");

            // fejléc szöveg
            var heading = Find(By.CssSelector("h6.oxd-topbar-header-breadcrumb-module"));
            Assert.AreEqual("Dashboard", heading.Text);
        }

        // 2. teszt - hibás jelszó esetén hibaüzenet
        [TestMethod]
        public void NegativeLoginTest_RosszJelszo_HibauzenetMegjelenik()
        {
            Find(By.Name("username")).SendKeys(ValidUsername);
            Find(By.Name("password")).SendKeys("rossz_jelszo_xyz");

            // submit gomb xpath alapján
            Find(By.XPath("//button[@type='submit']")).Click();

            // hibariasztás megjelenésére várunk
            var errorAlert = Find(By.CssSelector(".oxd-alert-content--error"));

            Assert.IsTrue(errorAlert.Displayed);
            StringAssert.Contains(errorAlert.Text, "Invalid credentials");
        }

        // 3. teszt - üres űrlap esetén required validáció
        [TestMethod]
        public void EmptyCredentials_KotelezoMezoValidacio()
        {
            // submit üres mezőkkel
            Find(By.XPath("//button[@type='submit']")).Click();

            // legalább egy Required üzenet megjelenik
            Find(By.ClassName("oxd-input-field-error-message"));

            var errorMessages = driver.FindElements(By.ClassName("oxd-input-field-error-message"));

            // pontosan 2 üzenet (username + password)
            Assert.HasCount(2, errorMessages);
            Assert.AreEqual("Required", errorMessages[0].Text);
            Assert.AreEqual("Required", errorMessages[1].Text);
        }

        // 4. teszt - login után admin modul navigáció
        [TestMethod]
        public void Login_AdminModulNavigacio_HelyesOldal()
        {
            LoginAsAdmin();

            // admin menüpont szöveg alapján
            Find(By.XPath("//span[text()='Admin']")).Click();

            wait.Until(d => d.Url.Contains("/admin"));
            StringAssert.Contains(driver.Url, "/viewSystemUsers");

            // van h6 fejléc az oldalon (By.TagName)
            var pageHeading = Find(By.TagName("h6"));
            Assert.IsNotNull(pageHeading);
        }

        // 5. teszt - admin modulban felhasználó keresése
        [TestMethod]
        public void Login_AdminFelhasznaloKereses_TalalatMegjelenik()
        {
            LoginAsAdmin();

            Find(By.XPath("//span[text()='Admin']")).Click();
            wait.Until(d => d.Url.Contains("/admin"));

            // username keresőmező a label alapján
            var usernameSearchField = Find(
                By.XPath("//label[text()='Username']/../following-sibling::div//input"));

            usernameSearchField.Clear();
            usernameSearchField.SendKeys("Admin");

            // search gomb
            var searchButton = Find(
                By.XPath("//button[@type='submit' and contains(@class,'oxd-button')]"));
            searchButton.Click();

            // eredmények számára várunk
            var resultsCount = Find(
                By.CssSelector(".orangehrm-horizontal-padding .oxd-text--span"));

            Assert.IsTrue(resultsCount.Displayed);
            StringAssert.Contains(resultsCount.Text, "Record");
        }

        // 6. teszt - kijelentkezés
        [TestMethod]
        public void Login_Logout_VisszaLoginOldalra()
        {
            LoginAsAdmin();

            // user dropdown a jobb felső sarokban
            Find(By.CssSelector(".oxd-userdropdown-tab")).Click();

            // logout link
            Find(By.XPath("//a[text()='Logout']")).Click();

            // vissza a login oldalra
            wait.Until(d => d.Url.Contains("/auth/login"));
            StringAssert.Contains(driver.Url, "/auth/login");

            // login form újra látható
            var usernameField = Find(By.Name("username"));
            Assert.IsTrue(usernameField.Displayed);
        }
    }
}
