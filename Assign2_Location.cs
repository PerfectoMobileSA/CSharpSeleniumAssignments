using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using Reportium.test;
using Reportium.test.Result;
using Reportium.client;
using Reportium.model;

namespace LocationAssign
{
	/// <summary>
	/// This template is for users that use DigitalZoom Reporting (ReportiumClient).
	/// For any other use cases please see the basic template at https://github.com/PerfectoCode/Templates.
	/// For more programming samples and updated templates refer to the Perfecto Documentation at: http://developers.perfectomobile.com/
	/// </summary>
	[TestClass]
	public class RemoteWebDriverTest
	{
		private RemoteWebDriverExtended driver;
		private ReportiumClient reportiumClient;
        private String os;

		[TestInitialize]
		public void PerfectoOpenConnection()
		{
			var browserName = "mobileOS";
			var host = "[ENTER YOUR CQ LAB NAME HERE]";

			DesiredCapabilities capabilities = new DesiredCapabilities(browserName, string.Empty, new Platform(PlatformType.Any));
			capabilities.SetCapability("user", "[ENTER YOUR USER NAME HERE]");

			//TODO: Provide your password or security token
			capabilities.SetCapability("password", "[ENTER YOUR PASSWORD HERE]");
            //capabilities.SetCapability("securityToken", "[ENTER YOUR SECURITY TOKEN HERE]");

			//TODO: Provide your device selection criteria
			capabilities.SetCapability("platformName", "Android");
			//capabilities.SetCapability("deviceName", "84B7N16102002974");
			capabilities.SetCapability("automationName", "Appium");
            os = "Android";

			capabilities.SetPerfectoLabExecutionId(host);

			// Name your script
			capabilities.SetCapability("scriptName", "Location assignment");

			var url = new Uri(string.Format("http://{0}/nexperience/perfectomobile/wd/hub", host));
			driver = new RemoteWebDriverExtended(new HttpAuthenticatedCommandExecutor(url), capabilities);
			driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(15));

			// Reporting client. For more details, see http://developers.perfectomobile.com/display/PD/Reporting
			PerfectoExecutionContext perfectoExecutionContext = new PerfectoExecutionContext.PerfectoExecutionContextBuilder()
					.withProject(new Project("My Project", "1.0"))
					.withJob(new Job("My Job", 45))
					.withContextTags(new[] { "sample tag1", "location assignment", "c#" })
					.withWebDriver(driver)
					.build();
			reportiumClient = PerfectoClientFactory.createPerfectoReportiumClient(perfectoExecutionContext);
		}

		[TestCleanup]
		public void PerfectoCloseConnection()
		{
			driver.Quit();

			// Retrieve the URL of the Single Test Report, can be saved to your execution summary and used to download the report at a later point
			String reportURL = reportiumClient.getReportUrl();
			Trace.WriteLine("Find your execution report at: " + reportURL);

			// For documentation on how to export reporting PDF, see https://github.com/perfectocode/samples/wiki/reporting
			// String reportPdfUrl = (String)(driver.Capabilities.GetCapability("reportPdfUrl"));

			// For detailed documentation on how to export the Execution Summary PDF Report, the Single Test report and other attachments such as
			// video, images, device logs, vitals and network files - see http://developers.perfectomobile.com/display/PD/Exporting+the+Reports
		}

		[TestMethod]
		public void WebDriverTestMethod()
		{
			try
			{
				reportiumClient.testStart("Location assignment", new TestContextTags("assignment2", "location"));
				Dictionary<String, Object> pars = new Dictionary<String, Object>();
				String res;
				IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;

				// write your code here

				reportiumClient.stepStart("navigate to web page");
				driver.Navigate().GoToUrl("https://training.perfecto.io");

				// Verification of website is dependent on the environment
				// Branching needed as desktop does not support image analysis
				if (os.ToLower().Equals("windows"))
				{
					pars.Clear();
					pars.Add("content", "taking the first step");
					pars.Add("timeout", 20);
					res = (String)driver.ExecuteScript("mobile:checkpoint:text", pars);
				}
				else
				{
					pars.Clear();
					pars.Add("content", "PRIVATE:trainingHomepage.png"); //remember, this image needs to be created!
					pars.Add("timeout", 30);
					Object result1 = driver.ExecuteScript("mobile:checkpoint:image", pars);

					// clicking on hamburger icon, required only for mobile
					driver.FindElementByXPath("//*[@class=\"mobile-menu\"]").Click();
				}
				reportiumClient.stepEnd();

				reportiumClient.stepStart("location page");
				pars.Clear();
				pars.Add("label", "location");
				pars.Add("timeout", 20);
				res = (String)driver.ExecuteScript("mobile:button-text:click", pars);

				// The location page SOMETIMES asks for approval to share location, in a popup. We need to click allow to approve
				try
				{
					pars.Clear();
					pars.Add("label", "Allow location");
					pars.Add("timeout", 20);
					res = (String)driver.ExecuteScript("mobile:button-text:click", pars);
				}
				catch (Exception e)
				{
					// if not found then just print a note and continue
					Trace.WriteLine("Popup to allow location not displayed");
				}

				reportiumClient.stepStart("Visual Analysis to verify on right page");
				pars.Clear();
				pars.Add("content", "peek");
				pars.Add("timeout", 20);
				res = (String)driver.ExecuteScript("mobile:checkpoint:text", pars);

				if (os.ToLower().Equals("android"))
				{
                    pars.Clear();
                    pars.Add("content", "you are within");
                    pars.Add("scrolling", "scroll");
                    res = (String)driver.ExecuteScript("mobile:checkpoint:text", pars);
                } else {
                    for (int i = 0; i < 5; i++)
                    {
                        pars.Clear();
                        pars.Add("content", "You are within");
                        pars.Add("timeout", 10);
                        res = (String)driver.ExecuteScript("mobile:checkpoint:text", pars);

                        if (res.ToLower().Equals("false"))
                        {
                            // scroll down
                            jse.ExecuteScript("window.scrollBy(0, 200)");
                        } else {
                            break;
                        }
                    }
                }
				reportiumClient.stepEnd();

				if (res.ToLower().Equals("true"))
				{
					reportiumClient.testStop(TestResultFactory.createSuccess());
				}
				else
				{
					reportiumClient.testStop(TestResultFactory.createFailure("test failed", null));
				}

			}
			catch (Exception e)
			{
				reportiumClient.testStop(TestResultFactory.createFailure(e.Message, e));
				Trace.Write(e.StackTrace);
				Trace.WriteLine(" ");
			}
		}
	}
}
