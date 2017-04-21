using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using AngleSharp;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows;
using OpenQA;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Drawing.Imaging;
using OpenQA.Selenium.Firefox;
using System.Threading;
using System.Globalization;
using OpenQA.Selenium.Support.UI;

namespace XGA
{
    class XboxLiveWorker
    {

        /// <summary>
        /// The program scarps into the Xbox Live by given logins and check if tag is available for claiming.
        /// If it is, it claims it and exits.
        /// Program supports multiple user data from file.txt.
        /// </summary>


        public string[] gamer_tags;
        public string username;
        public string password;
        public bool [] status = new bool[128];
        private int [] tries = new int[128];

        public XboxLiveWorker()
        {
            gamer_tags = new string[128];

            status = status.Select(x => true).ToArray();
        }

        public void input(GUI form)
        {
            Thread.Sleep(300);
            string text = form.textBox3.Text.Replace(" ", String.Empty);
            string [] temp = text.Split(',');

            username = form.textBox1.Text;
            password = form.textBox2.Text;
            for (int i = 0; i < temp.Length; i++)
            {
                gamer_tags[i] = temp[i];
            }
        }

        public void loginXboxLive(string username, string password, GUI form)
        {
            Thread.Sleep(200);
            string login_url = "https://account.xbox.com/en-US/ChangeGamertag";
            using (var driver = new FirefoxDriver())
            {
                try
                {
                    driver.Navigate().GoToUrl(login_url); // go to URL above.

                    form.Status.Text = "Status: Logging In...";

                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    wait.Until(ExpectedConditions.ElementIsVisible(By.Id("i0116")));

                    var user_name_field = driver.FindElementById("i0116"); // here we just find an elements to click/insert data by their IDs, CSS-selectors or names.
                    var login_button = driver.FindElementById("idSIButton9");

                    user_name_field.SendKeys(username);
                    login_button.Click(); // some obvious code.

                    wait.Until(ExpectedConditions.ElementIsVisible(By.Id("i0118")));

                    var password_field = driver.FindElementById("i0118");
                    password_field.SendKeys(password);
                    login_button = driver.FindElementById("idSIButton9");
                    login_button.Click();

                openTabsCheck(driver, form);
                }
                catch (Exception e) // common exception case
                {
                    form.Status.Text = "Error, re-running...";
                    var culture = new CultureInfo("en-US");
                    DateTime local = DateTime.Now;
                    Console.WriteLine("An error ocurred while parsing the account " + username); // if we have an exception, we write it to file to let the user (or administrator) let know what happened.
                    File.AppendAllText("./excepts.txt", local.ToString(culture) + "\n" + "ERROR\n" + e.ToString());
                    for (int i = 0; i < form.Tags.Items.Count; i++)
                        form.Tags.Items.RemoveAt(i);
                }
            }
        }

        private void openTabsCheck(FirefoxDriver driver, GUI form)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            wait.Until(ExpectedConditions.ElementIsVisible(By.Name("NewGamertag")));

            int i = 0;
            for (i = 0; i < gamer_tags.Count(s => s != null); i++) // open as much new tabs as the gamer tag is long
            {
                driver.ExecuteScript(@"window.open(""https://account.xbox.com/en-US/ChangeGamertag"", ""_blank"")");
                form.Tags.Items.Add("The gamertag name is: " + gamer_tags[i] + ".");
            }

            status = status.Select(x => true).ToArray();

            while (status.Contains(true)) // do while status contains trues
            {
                for (i = 1; i < driver.WindowHandles.Count; i++)
                {
                    driver.SwitchTo().Window(driver.WindowHandles[i]); // switch to window[index]
                    int number = i - 1;
                    tries[number]++;
                    status[i] = claimTag(driver, number, form); // try to claim tag at this window
                }
            }
        }

        private bool claimTag(FirefoxDriver driver, int count, GUI form)
        {
            form.Status.Text = "Checking tags.";

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(7.5));
            wait.Until(ExpectedConditions.ElementIsVisible(By.Name("NewGamertag")));
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".checkAvailability")));

            var tag_field = driver.FindElementByName("NewGamertag");
            var check_button = driver.FindElementByCssSelector(".checkAvailability");

                tag_field.SendKeys(Keys.Control + "a");
                tag_field.SendKeys(Keys.Delete);

            tag_field.SendKeys(gamer_tags[count]);

            bool isElementDisplayed = true;

                check_button.Click();

            Thread.Sleep(1918);

            isElementDisplayed = driver.FindElementByCssSelector(".gamertagNotAvailable").Displayed; // bool variable which checks if element is displayed
            if (isElementDisplayed == false)
            {
                Thread.Sleep(1500);
                var claim = driver.FindElementByCssSelector(".claimIt"); // if the "not available gametag" don't display, we click "claim it" button which appears if the tag is free.
                claim.Click();
                Console.WriteLine("The tag " + gamer_tags[count] + " is now taken by your account");
                Thread.Sleep(6000);
                File.AppendAllText("./successful_tags.txt", "Account name: " + username + "\n" + "Tag name: \n" + gamer_tags[count] + "\n");
                gamer_tags = gamer_tags.Where(val => val != gamer_tags[count]).ToArray();
                form.Tags.Items[count] = "The gamertag name is succesfully claimed: " + gamer_tags[count] + "." + " The number of tries: " + tries[count] + " check the succesfully claimed tags in successful_tags.txt";
                Thread.Sleep(4000);
                form.Tags.Items.RemoveAt(count);
                driver.Close();
            }
            else
            {
                DateTime local = DateTime.Now;
                var culture = new CultureInfo("en-US");
                Console.WriteLine(local.ToString(culture) + "\nThe tag " + gamer_tags[count] + " is taken, retrying");
                form.Tags.Items[count] = "The gamertag name is taken: " + gamer_tags[count] + "." + " The number of tries: " + tries[count];
            }
            return isElementDisplayed;
        }
    }
}
