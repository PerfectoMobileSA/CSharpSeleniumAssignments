using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Appium.MultiTouch;
using System.Threading;
using System.Collections.ObjectModel;
//Reportium required using statements
using Reportium.test;
using Reportium.test.Result;
using Reportium.client;
using Reportium.model;

namespace Assign4_Audio
{
    /// <summary>
    /// Summary description for AppiumTest
    /// 
    /// For programming samples and updated templates refer to the Perfecto GitHub at: https://github.com/PerfectoCode
    /// </summary>
    [TestClass]
    public class AppiumTest
    {
        private AndroidDriver<IWebElement> driver;
        //private IOSDriver<IWebElement> driver;
        private ReportiumClient reportClient;

        [TestInitialize]
        public void PerfectoOpenConnection()
        {
            DesiredCapabilities capabilities = new DesiredCapabilities(string.Empty, string.Empty, new Platform(PlatformType.Any));

            var host = "[ENTER YOUR CQ LAB NAME HERE]";
            capabilities.SetCapability("user", "[ENTER YOUR USER NAME HERE]");

            //TODO: Provide your password
            capabilities.SetCapability("password", "[ENTER YOUR PASSWORD HERE]");

            //TODO: Provide your device selection criteria
            capabilities.SetCapability("platformName", "Android");

            // Use this method if you want the script to share the devices with the Perfecto Lab plugin.
            capabilities.SetPerfectoLabExecutionId(host);

            // Use the automationName capability to defined the required framework - Appium (this is the default) or PerfectoMobile.
            //capabilities.SetCapability("automationName", "PerfectoMobile"); 


            // Application settings examples.
            // capabilities.SetCapability("app", "PRIVATE:applications/Errands.ipa");
            // For Android:
            //capabilities.SetCapability("appPackage", "com.google.android.keep");
            //capabilities.SetCapability("appActivity", ".activities.BrowseActivity");
            // For iOS:
            // capabilities.SetCapability("bundleId", "com.yoctoville.errands");

            // Name your script
            capabilities.SetCapability("scriptName", "Audio Search");

            var url = new Uri(string.Format("https://{0}/nexperience/perfectomobile/wd/hub", host));
            driver = new AndroidDriver<IWebElement>(url, capabilities);
            //driver = new IOSDriver<IWebElement>(url, capabilities);
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(15));

            // prepare the DigitalZoom Reporting client. for more information see https://developers.perfectomobile.com/pages/viewpage.action?pageId=12419423
            // Create the PerfectoExecutionContext instance to provide the Execution Test Report metadata
            PerfectoExecutionContext peContext = new PerfectoExecutionContext.PerfectoExecutionContextBuilder()
              .withContextTags(new[] { "sample tag1", "you tube exercise", "c#" })
              .withWebDriver(driver)
              .build();
            reportClient = PerfectoClientFactory.createPerfectoReportiumClient(peContext);
        }

        [TestCleanup]
        public void PerfectoCloseConnection()
        {
            // Retrieve the URL of the Single Test Report, can be saved to your execution summary and used to download the report at a later point
            string reportUrl = (string)(driver.Capabilities.GetCapability(WindTunnelUtils.SINGLE_TEST_REPORT_URL_CAPABILITY));

            driver.Close();

            driver.Quit();
            // retrieve the STR URL
            String reportLoc = reportClient.getReportUrl();
            Trace.WriteLine("Find your execution report at: " + reportLoc);
        }

        [TestMethod]
        public void AppiumTestMethod()
        {
            //Write your test here
            // Notify Reporting Server that test is starting
            reportClient.testStart("Wikipedia assignment", new TestContextTags("assignment1", "apple entry"));

            Dictionary<String, Object> pars = new Dictionary<String, Object>();

            try
            {
                reportClient.stepStart("Open YouTube");
                try
                {
                    // close the calendar if open to verify that starting from fresh app
                    pars.Add("name", "YouTube");
                    driver.ExecuteScript("mobile:application:close", pars);
                }
                catch (Exception e)
                {
                    Trace.Write(e.StackTrace);
                    Trace.WriteLine(" ");
                }
                pars.Clear();
                pars.Add("name", "YouTube");
                driver.ExecuteScript("mobile:application:open", pars);
                driver.Context = "NATIVE_APP";

                reportClient.stepEnd();

                reportClient.stepStart("Adjust Volume");
                MaxVolume(driver);
                reportClient.stepEnd();

                reportClient.stepStart("Activate the audio search");
                driver.FindElementByXPath("//*[@content-desc=\"Search\"]").Click();
                driver.FindElementByXPath("//*[@resource-id=\"com.google.android.youtube:id/voice_search\"]").Click();
                reportClient.stepEnd();

                reportClient.stepStart("Play name for audio search");
                // Create audio file from String
                String key = "PRIVATE:mysong.wav";
                pars.Clear();
                pars.Add("text", "Metallica Master of Puppets");
                pars.Add("repositoryFile", key);
                driver.ExecuteScript("mobile:text:audio", pars);
                // inject Audio to device
                pars.Clear();
                pars.Add("key", key);
                pars.Add("wait", "wait");
                driver.ExecuteScript("mobile:audio:inject", pars);
                reportClient.stepEnd();

                reportClient.stepStart("Play the music clip and validate");
                driver.FindElementByXPath("//*[@content-desc=\"Play album\"]").Click();

                pars.Clear();
                pars.Add("timeout", 30);
                pars.Add("duration", 1);
                String audioR = (String) driver.ExecuteScript("mobile:checkpoint:audio", pars);
                reportClient.stepEnd();

                reportClient.stepStart("Close the app");
                // inject Audio to device
                pars.Clear();
                pars.Add("name", "YouTube");
                driver.ExecuteScript("mobile:application:close", pars);
                reportClient.stepEnd();

                if (audioR.ToLower().Equals("true"))
                {
                    reportClient.reportiumAssert("Audio is playing", true);
                    reportClient.testStop(TestResultFactory.createSuccess());
                }
                else
                {
                    reportClient.reportiumAssert("Audio failed", false);
                    reportClient.testStop(TestResultFactory.createFailure("Audio failed", null));
                }
            }
            catch (Exception e)
            {
                reportClient.testStop(TestResultFactory.createFailure(e.Message, e));
                Trace.Write(e.StackTrace);
                Trace.WriteLine(" ");
            }
        }

        private void MaxVolume(RemoteWebDriver driver)
        {
            Dictionary<String, Object> pars = new Dictionary<String, Object>();

            pars.Add("keySequence", "VOL_UP");
            for (int i = 0; i < 12; i++)
                driver.ExecuteScript("mobile:presskey", pars);
        }
    }
}
