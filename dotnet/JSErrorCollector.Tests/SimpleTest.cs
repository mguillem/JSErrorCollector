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

        private string urlUnbrokenHtml;

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
            urlUnbrokenHtml = GetResource("unbroken.html");

            urlSimpleHtml = GetResource("simple.html");
            errorSimpleHtml = new JavaScriptError("TypeError: null has no properties", urlSimpleHtml, 9, null);

            urlWithNestedFrameHtml = GetResource("withNestedFrame.html");
            errorWithNestedFrameHtml = new JavaScriptError("TypeError: \"foo\".notHere is not a function", urlWithNestedFrameHtml, 7, null);

            urlWithPopupHtml = GetResource("withPopup.html");
            urlPopupHtml = GetResource("popup.html");
            errorPopupHtml = new JavaScriptError("ReferenceError: error is not defined", urlPopupHtml, 5, null);

            urlWithExternalJs = GetResource("withExternalJs.html");
            urlExternalJs = GetResource("external.js");
            errorExternalJs = new JavaScriptError("TypeError: document.notExisting is undefined", urlExternalJs, 1, null);
        }

        [TestMethod]
        public void ShouldNotDetectErrorsInWorkingPage()
        {
            using (IWebDriver driver = BuildFFDriver())
            {
                driver.Navigate().GoToUrl(urlUnbrokenHtml);

                IList<JavaScriptError> jsErrors = JavaScriptError.ReadErrors(driver);
                Assert.AreEqual(0, jsErrors.Count);
            }
        }

        [TestMethod]
        public void ShouldDetectErrorsOnPage()
        {
            using (IWebDriver driver = BuildFFDriver())
            {
                driver.Navigate().GoToUrl(urlSimpleHtml);

                IEnumerable<JavaScriptError> expectedErrors = new List<JavaScriptError>() { errorSimpleHtml };
                IEnumerable<JavaScriptError> jsErrors = JavaScriptError.ReadErrors(driver);
                AssertErrorsEqual(expectedErrors, jsErrors);
            }
        }

        [TestMethod]
        public void ShouldAllowExplicitlySpecifyingXpiPath()
        {
            FirefoxProfile ffProfile = new FirefoxProfile();
            JavaScriptError.AddExtension(ffProfile, xpiDirectory());

            using (IWebDriver driver = new FirefoxDriver(ffProfile))
            {
                driver.Navigate().GoToUrl(urlSimpleHtml);

                IEnumerable<JavaScriptError> expectedErrors = new List<JavaScriptError>() { errorSimpleHtml };
                IEnumerable<JavaScriptError> jsErrors = JavaScriptError.ReadErrors(driver);
                AssertErrorsEqual(expectedErrors, jsErrors);
            }
        }

        [TestMethod]
        public void ShouldDetectErrorsInNestedFrame()
        {
            IEnumerable<JavaScriptError> expectedErrors = new List<JavaScriptError>() { errorWithNestedFrameHtml, errorSimpleHtml };

            using (IWebDriver driver = BuildFFDriver())
            {
                driver.Navigate().GoToUrl(urlWithNestedFrameHtml);

                IEnumerable<JavaScriptError> jsErrors = JavaScriptError.ReadErrors(driver);
                AssertErrorsEqual(expectedErrors, jsErrors);
            }
        }

        [TestMethod]
        public void ShouldDetectErrorsInPopup()
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

        [TestMethod]
        public void ShouldDetectErrorsInExternalJS()
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

        private string xpiDirectory() {
            string xpiPath = "dist/JSErrorCollector.xpi";

            for (int depth = 0; depth <= 10 && !File.Exists(xpiPath); depth++) {
                xpiPath = Path.Combine("../", xpiPath);
            }

            if (File.Exists(xpiPath)) {
                return Path.GetDirectoryName(xpiPath);
            } else {
                throw new FileNotFoundException("Could not find JSErrorCollector.xpi after 10 iterations.");
            }
        }

        private IWebDriver BuildFFDriver()
        {
            FirefoxProfile ffProfile = new FirefoxProfile();
            JavaScriptError.AddExtension(ffProfile);
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
