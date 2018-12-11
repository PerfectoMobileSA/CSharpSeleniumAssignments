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
using Reportium.test;
using Reportium.test.Result;
using Reportium.client;
using Reportium.model;

namespace CalendarSample
{
	/// <summary>
	/// This template is for users that use DigitalZoom Reporting (ReportiumClient).
	/// For any other use cases please see the basic template at https://github.com/PerfectoCode/Templates.
	/// For more programming samples and updated templates refer to the Perfecto Documentation at: http://developers.perfectomobile.com/
	/// </summary>
	[TestClass]
	public class AppiumTest
	{
		//private AndroidDriver<IWebElement> driver;
		private IOSDriver<IWebElement> driver;
		private ReportiumClient reportiumClient;

		[TestInitialize]
		public void PerfectoOpenConnection()
		{
			DesiredCapabilities capabilities = new DesiredCapabilities(string.Empty, string.Empty, new Platform(PlatformType.Any));

			var host = "[ENTER YOUR CQ LAB NAME HERE]";
			capabilities.SetCapability("user", "[ENTER YOUR USER NAME HERE]");

			//TODO: Provide your password or security token
			capabilities.SetCapability("password", "[ENTER YOUR PASSWORD HERE]");
			//capabilities.SetCapability("securityToken", "[ENTER YOUR SECURITY TOKEN HERE]");

			//TODO: Provide your device ID
			capabilities.SetCapability("platformName", "iOS");

			// Use this method if you want the script to share the devices with the Perfecto Lab plugin.
			capabilities.SetPerfectoLabExecutionId(host);

			// Use the automationName capability to defined the required framework - Appium (this is the default) or PerfectoMobile.
			capabilities.SetCapability("automationName", "Appium");
			capabilities.SetCapability("automationInfrastructure", "XCUITest");

			// Application settings examples.
			// capabilities.SetCapability("app", "PRIVATE:applications/Errands.ipa");
			// For Android:
			//capabilities.SetCapability("appPackage", "com.google.android.keep");
			//capabilities.SetCapability("appActivity", ".activities.BrowseActivity");
			// For iOS:
			// capabilities.SetCapability("bundleId", "com.yoctoville.errands");

			// Name your script
			capabilities.SetCapability("scriptName", "Calendar Sample");

			var url = new Uri(string.Format("http://{0}/nexperience/perfectomobile/wd/hub", host));
			//driver = new AndroidDriver<IWebElement>(url, capabilities);
			driver = new IOSDriver<IWebElement>(url, capabilities);
			driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(15));

			// Reporting client. For more details, see http://developers.perfectomobile.com/display/PD/Reporting
			PerfectoExecutionContext perfectoExecutionContext = new PerfectoExecutionContext.PerfectoExecutionContextBuilder()
					.withProject(new Project("My Project", "1.0"))
					.withJob(new Job("My Job", 45))
					.withContextTags(new[] { "sample tag1", "calendar sample", "c#" })
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
			//String reportPdfUrl = (String)(driver.Capabilities.GetCapability("reportPdfUrl"));

			// For detailed documentation on how to export the Execution Summary PDF Report, the Single Test report and other attachments such as
			// video, images, device logs, vitals and network files - see http://developers.perfectomobile.com/display/PD/Exporting+the+Reports
		}

		[TestMethod]
		public void AppiumTestMethod()
		{
			try
			{
				reportiumClient.testStart("Calendar assignment", new TestContextTags("assignment3", "calendarApp"));

				// write your code here

				reportiumClient.stepStart("Open the Calendar app");
				Dictionary<String, Object> pars = new Dictionary<String, Object>();
				try
				{
					// close the calendar if open to verify that starting from fresh app
					pars.Add("name", "Calendar");
					driver.ExecuteScript("mobile:application:close", pars);
				}
				catch (Exception e)
				{
					Trace.Write(e.StackTrace);
					Trace.WriteLine(" ");
				}
				pars.Clear();
				pars.Add("name", "Calendar");
				driver.ExecuteScript("mobile:application:open", pars);
				reportiumClient.stepEnd();

				// Check for What's New Screen and dismiss
				reportiumClient.stepStart("Dismiss popup");
				pars.Clear();
				pars.Add("content", "What's New");
				pars.Add("timeout", 10);
				String result = (String)driver.ExecuteScript("mobile:checkpoint:text", pars);
				if (result.ToLower().Equals("true"))
				{
					pars.Clear();
					pars.Add("label", "Continue");
					pars.Add("threshold", 80);
					pars.Add("ignorecase", "nocase");
					driver.ExecuteScript("mobile:button-text:click", pars);
				}
				pars.Clear();
				pars.Add("content", "Access");
				pars.Add("timeout", 10);
				result = (String)driver.ExecuteScript("mobile:checkpoint:text", pars);
				if (result.ToLower().Equals("true"))
				{
					pars.Clear();
					pars.Add("label", "Don't Allow");
					pars.Add("threshold", 80);
					pars.Add("ignorecase", "nocase");
					driver.ExecuteScript("mobile:button-text:click", pars);
				}
				reportiumClient.stepEnd();

				// Go to add the event screen
				reportiumClient.stepStart("Open the event add screen");
				driver.Context = "NATIVE_APP";
				driver.FindElementByName("Add").Click();

				// use a checkpoint to visually check that we are on the correct screen
				pars.Clear();
				pars.Add("content", "new event");
				pars.Add("timeout", 20);
				result = (String)driver.ExecuteScript("mobile:checkpoint:text", pars);

				reportiumClient.stepEnd();

				reportiumClient.stepStart("Add location & title");
				IWebElement inField = driver.FindElementsByClassName("XCUIElementTypeTextField").ElementAt(1);
				inField.SendKeys("fun day");
				System.Threading.Thread.Sleep(2000);
				inField = driver.FindElementByName("Location");
				inField.SendKeys("London");
				driver.FindElementByName("Done").Click();

				reportiumClient.stepEnd();

				reportiumClient.stepStart("Set start time");
				driver.FindElementByName("Starts").Click();
				System.Threading.Thread.Sleep(1000);
				IWebElement hour = driver.FindElementsByClassName("XCUIElementTypePickerWheel").ElementAt(1);
				String hr = hour.GetAttribute("value");
				hour.SendKeys("9");
				IWebElement mins = driver.FindElementsByClassName("XCUIElementTypePickerWheel").ElementAt(2);
				String min = mins.GetAttribute("value");
				mins.SendKeys("45");
				Trace.WriteLine("old time: " + hr + ':' + min);
				hr = hour.GetAttribute("value");
				min = mins.GetAttribute("value");
				Trace.WriteLine("new time: " + hr + ':' + min);
				reportiumClient.stepEnd();

				reportiumClient.stepStart("Set End Time");
				driver.FindElementByName("Ends").Click();
				System.Threading.Thread.Sleep(2000);
				hour = driver.FindElementsByClassName("XCUIElementTypePickerWheel").ElementAt(1);
				hr = hour.GetAttribute("value");
				hour.SendKeys("11");
				mins = driver.FindElementsByClassName("XCUIElementTypePickerWheel").ElementAt(2);
				min = mins.GetAttribute("value");
				mins.SendKeys("15");
				Trace.WriteLine("old end time: " + hr + ':' + min);
				hr = hour.GetAttribute("value");
				min = mins.GetAttribute("value");
				Trace.WriteLine("new end time: " + hr + ':' + min);
				driver.FindElementByName("Ends").Click();
				reportiumClient.stepEnd();

				reportiumClient.stepStart("Recurring Event");
				driver.FindElementByName("Repeat").Click();
				driver.FindElementByName("Every Month").Click();
				driver.FindElementByName("End Repeat").Click();
				driver.FindElementByName("On Date").Click();
				System.Threading.Thread.Sleep(3000);
				IWebElement month = driver.FindElementsByClassName("XCUIElementTypePickerWheel").ElementAt(0);
				String endMonth = month.GetAttribute("value");
				month.SendKeys("December");
				IWebElement day = driver.FindElementsByClassName("XCUIElementTypePickerWheel").ElementAt(1);
				String endDate = day.GetAttribute("value");
				day.SendKeys("5");
				IWebElement year = driver.FindElementsByClassName("XCUIElementTypePickerWheel").ElementAt(2);
				String endYear = year.GetAttribute("value");
				year.SendKeys("2020");
				Trace.WriteLine("old end date: " + endMonth + ' ' + endDate + ", " + endYear);
				endMonth = month.GetAttribute("value");
				endDate = day.GetAttribute("value");
				endYear = year.GetAttribute("value");
				Trace.WriteLine("new end date: " + endMonth + ' ' + endDate + ", " + endYear);

				// go back to Event definition screen
				driver.FindElementByName("New Event").Click();
				reportiumClient.stepEnd();

				reportiumClient.stepStart("Add Travel time");
				driver.FindElementByName("Travel Time").Click();
				IWebElement travelToggle = driver.FindElementsByClassName("XCUIElementTypeSwitch").ElementAt(0);
				travelToggle.Click();
				driver.FindElementByName("2 hours").Click();
				// go back to Event definition screen
				driver.FindElementByName("New Event").Click();

				reportiumClient.stepEnd();

				reportiumClient.stepStart("Close application");
				pars.Clear();
				pars.Add("name", "Calendar");
				driver.ExecuteScript("mobile:application:close", pars);

				reportiumClient.stepEnd();
				reportiumClient.testStop(TestResultFactory.createSuccess());
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
