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

namespace ImageInjectAssignment
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
            capabilities.SetCapability("user", "[ENTER YOUR PASSWORD HERE]");
			//capabilities.SetCapability("securityToken", "[ENTER YOUR SECURITY TOKEN HERE]");

			//TODO: Provide your device ID
			capabilities.SetCapability("platformName", "iOS");

			// Use this method if you want the script to share the devices with the Perfecto Lab plugin.
			capabilities.SetPerfectoLabExecutionId(host);

			// Use the automationName capability to defined the required framework - Appium (this is the default) or PerfectoMobile.
			capabilities.SetCapability("automationName", "Appium"); 


			// Application settings examples.
			// capabilities.SetCapability("app", "PRIVATE:applications/Errands.ipa");
			// For Android:
			//capabilities.SetCapability("appPackage", "com.google.android.keep");
			//capabilities.SetCapability("appActivity", ".activities.BrowseActivity");
			// For iOS:
			// capabilities.SetCapability("bundleId", "com.yoctoville.errands");

			// Name your script
			capabilities.SetCapability("scriptName", "ImageInjection");

			var url = new Uri(string.Format("http://{0}/nexperience/perfectomobile/wd/hub", host));
			//driver = new AndroidDriver<IWebElement>(url, capabilities);
			driver = new IOSDriver<IWebElement>(url, capabilities);
			driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(15));

			// Reporting client. For more details, see http://developers.perfectomobile.com/display/PD/Reporting
			PerfectoExecutionContext perfectoExecutionContext = new PerfectoExecutionContext.PerfectoExecutionContextBuilder()
					.withProject(new Project("C# Training", "1.0"))
					.withJob(new Job("My Job", 45))
					.withContextTags(new[] { "imageInject", "assignment", "c#" })
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
		public void AppiumTestMethod()
		{
			Dictionary<String, Object> pars = new Dictionary<String, Object>();
			String res;
			try
			{
				reportiumClient.testStart("Image Injection Assignment", new TestContextTags("assignment4", "image"));

				// write your code here

				reportiumClient.stepStart("Start the app and upload the image");

				try
				{
					// close the calendar if open to verify that starting from fresh app
					pars.Add("name", "RealTimeFilter");
					driver.ExecuteScript("mobile:application:close", pars);
				}
				catch (Exception e)
				{
					Trace.Write(e.StackTrace);
					Trace.WriteLine(" ");
				}
				try
				{
					pars.Clear();
					pars.Add("name", "RealTimeFilter");
					driver.ExecuteScript("mobile:application:open", pars);
				}
				catch (Exception e)
				{
					// unable to start the app, probably because it isn't installed
					pars.Clear();
					pars.Add("file", "PUBLIC:ImageInjection\\RealTimeFilter.ipa");
					pars.Add("sensorInstrument", "sensor");
					driver.ExecuteScript("mobile:application:install", pars);

					pars.Clear();
					pars.Add("name", "RealTimeFilter");
					driver.ExecuteScript("mobile:application:open", pars);
				}

				driver.Context = "NATIVE_APP";
				reportiumClient.stepEnd();

				reportiumClient.stepStart("inject the image");

				pars.Clear();
				pars.Add("repositoryFile", "PRIVATE:perfectoL.png");
				pars.Add("identifier", "Victor.RealTimeFilter");
				driver.ExecuteScript("mobile:image.injection:start", pars);
				reportiumClient.stepEnd();

				reportiumClient.stepStart("dismiss popup if it appears");
				// rotate the image (device) to show in landscape
				pars.Clear();
				pars.Add("content", "Access the Camera");
				pars.Add("timeout", 20);
				res = (String) driver.ExecuteScript("mobile:checkpoint:text", pars);

				if (res.ToLower().Equals("true"))
				{
					pars.Clear();
					pars.Add("label", "OK");
					pars.Add("timeout", 10);

					res = (String)driver.ExecuteScript("mobile:button-text:click", pars);
				}
				reportiumClient.stepEnd();

				reportiumClient.stepStart("verify the image is displayed");
				// rotate the image (device) to show in landscape
				pars.Clear();
				pars.Add("state", "landscape");
				driver.ExecuteScript("mobile:device:rotate", pars);

				pars.Clear();
				pars.Add("content", "Perfection");
				pars.Add("timeout", 20);

				res = (String)driver.ExecuteScript("mobile:checkpoint:text", pars);

				if (res.ToLower().Equals("true"))
				{
					reportiumClient.testStop(TestResultFactory.createSuccess());
				}
				else
				{
					pars.Clear();
					pars.Add("tail", 50);
					String logStr = (String)driver.ExecuteScript("mobile:device:log", pars);
					reportiumClient.reportiumAssert("Failed to view image", false);
					reportiumClient.testStop(TestResultFactory.createFailure("Failed to view image", null));
				}

			}
			catch (Exception e)
			{
				reportiumClient.testStop(TestResultFactory.createFailure(e.Message, e));
			}
		}
	}
}
