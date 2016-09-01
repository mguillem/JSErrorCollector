using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JSErrorCollector.Tests
{
    /// <summary>
    /// Tests with Firebug ensuring that we get JS errors & console content.
    /// 
    /// @author Marc Guillemot, ported by Joel Sanderson
    /// </summary>
    [TestClass]
    public class FirebugConsoleTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void FirebugConsole_Simple()
        {
            string url = GetResource("withConsoleOutput.html");
            JavaScriptError errorSimpleHtml = new JavaScriptError("TypeError: null has no properties", url, 8, "before JS error");
            IEnumerable<JavaScriptError> expectedErrors = new List<JavaScriptError>() { errorSimpleHtml };

            using (IWebDriver driver = BuildFFDriver())
            {
                driver.Navigate().GoToUrl(url);
                IList<JavaScriptError> jsErrors = JavaScriptError.ReadErrors(driver);
                AssertErrorsEqual(expectedErrors, jsErrors);
            }
        }

        private void AssertErrorsEqual(IEnumerable<JavaScriptError> expectedErrors, IEnumerable<JavaScriptError> actualErrors)
        {
            string expected = "";
            foreach (JavaScriptError err in expectedErrors)
            {
                expected += err.ErrorMessage + " (line " + err.LineNumber + ")";
            }

            string actual = "";
            foreach (JavaScriptError err in actualErrors)
            {
                actual += err.ErrorMessage + " (line " + err.LineNumber + ")";
            }

            Assert.AreEqual(expected, actual);
        }

        private IWebDriver BuildFFDriver()
        {
            FirefoxProfile ffProfile = new FirefoxProfile();
            JavaScriptError.AddExtension(ffProfile);

            ffProfile.AddExtension(SaveBinaryResource("firebug-2.0.8-fx.xpi", TestResources.firebug_2_0_8_fx));

            ffProfile.SetPreference("extensions.firebug.showStackTrace", "true");
            ffProfile.SetPreference("extensions.firebug.delayLoad", "false");
            ffProfile.SetPreference("extensions.firebug.showFirstRunPage", "false");
            ffProfile.SetPreference("extensions.firebug.allPagesActivation", "on");
            ffProfile.SetPreference("extensions.firebug.console.enableSites", "true");
            ffProfile.SetPreference("extensions.firebug.defaultPanelName", "console");

            return new FirefoxDriver(ffProfile);
        }

        private String GetResource(String fileName)
        {
            string resourceContent = TestResources.ResourceManager.GetString(Path.GetFileNameWithoutExtension(fileName));

            string tempResourcePath = Path.Combine(TestContext.DeploymentDirectory, fileName);
            using (StreamWriter sw = new StreamWriter(tempResourcePath))
            {
                sw.Write(resourceContent);
            }

            string resourceUrl = "file://" + tempResourcePath.Replace(Path.DirectorySeparatorChar, '/');
            return resourceUrl;
        }

        private string SaveBinaryResource(String fileName, byte[] data)
        {
            string tempResourcePath = Path.Combine(TestContext.DeploymentDirectory, fileName);
            using (FileStream sw = new FileStream(tempResourcePath, FileMode.Create, FileAccess.Write))
            {
                sw.Write(data, 0, data.Length);
            }

            return tempResourcePath;
        }
    }
}
