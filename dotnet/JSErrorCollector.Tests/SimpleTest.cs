using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System.Collections.Generic;

namespace JSErrorCollector.Tests
{
    [TestClass]
    public class SimpleTest
    {
        public TestContext TestContext { get; set; }

        private string urlSimpleHtml;
        private JavaScriptError errorSimpleHtml;

        private string urlWithNestedFrameHtml;
        private JavaScriptError errorWithNestedFrameHtml;

        private string urlWithPopupHtml;
        private string urlPopupHtml;
        private JavaScriptError errorPopupHtml;

        private string urlWithExternalJs;
        private string urlExternalJs;
        private JavaScriptError errorExternalJs;

        [TestInitialize]
        public void Initialize()
        {
            urlSimpleHtml = GetResource("simple.html");
            errorSimpleHtml = new JavaScriptError("TypeError: null has no properties", urlSimpleHtml, 9);

            urlWithNestedFrameHtml = GetResource("withNestedFrame.html");
            errorWithNestedFrameHtml = new JavaScriptError("TypeError: \"foo\".notHere is not a function", urlWithNestedFrameHtml, 7);

            urlWithPopupHtml = GetResource("withPopup.html");
            urlPopupHtml = GetResource("popup.html");
            errorPopupHtml = new JavaScriptError("ReferenceError: error is not defined", urlPopupHtml, 5);

            urlWithExternalJs = GetResource("withExternalJs.html");
            urlExternalJs = GetResource("external.js");
            errorExternalJs = new JavaScriptError("TypeError: document.notExisting is undefined", urlExternalJs, 1);
        }

        /**
	 *
	 */
        [TestMethod]
        public void Simple()
        {
            using (IWebDriver driver = BuildFFDriver())
            {
                driver.Navigate().GoToUrl(urlSimpleHtml);

                IEnumerable<JavaScriptError> expectedErrors = new List<JavaScriptError>() { errorSimpleHtml };
                IEnumerable<JavaScriptError> jsErrors = JavaScriptError.ReadErrors(driver);
                AssertErrorsEqual(expectedErrors, jsErrors);
            }
        }

        /**
         *
         */
        [TestMethod]
        public void ErrorInNestedFrame()
        {
            IEnumerable<JavaScriptError> expectedErrors = new List<JavaScriptError>() { errorWithNestedFrameHtml, errorSimpleHtml };

            using (IWebDriver driver = BuildFFDriver())
            {
                driver.Navigate().GoToUrl(urlWithNestedFrameHtml);

                IEnumerable<JavaScriptError> jsErrors = JavaScriptError.ReadErrors(driver);
                AssertErrorsEqual(expectedErrors, jsErrors);
            }
        }

        /**
         *
         */
        [TestMethod]
        public void ErrorInPopup()
        {
            IEnumerable<JavaScriptError> expectedErrors = new List<JavaScriptError>() { errorPopupHtml };

            using (IWebDriver driver = BuildFFDriver())
            {
                driver.Navigate().GoToUrl(urlWithPopupHtml);
                driver.FindElement(By.TagName("button")).Click();

                IEnumerable<JavaScriptError> jsErrors = JavaScriptError.ReadErrors(driver);
                AssertErrorsEqual(expectedErrors, jsErrors);
            }
        }

        /**
         *
         */
        [TestMethod]
        public void ErrorInExternalJS()
        {
            IEnumerable<JavaScriptError> expectedErrors = new List<JavaScriptError>() { errorExternalJs };

            using (IWebDriver driver = BuildFFDriver())
            {
                driver.Navigate().GoToUrl(urlWithExternalJs);

                IEnumerable<JavaScriptError> jsErrors = JavaScriptError.ReadErrors(driver);
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
            
            string xpiPath;
            int depth = 0;
            do
            {
                string relative = "";
                for (int i = 0; i < depth; i++)
                    relative += @"..\";

                xpiPath = Path.Combine(TestContext.DeploymentDirectory, relative, @"dist\JSErrorCollector.xpi");

                depth++;
                if (depth > 10)
                    throw new FileNotFoundException("Could not find JSErrorCollector.xpi after 10 iterations.");

            } while (!File.Exists(xpiPath));

            ffProfile.AddExtension(xpiPath);

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
    }
}
