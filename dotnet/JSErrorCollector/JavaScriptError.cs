using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace JSErrorCollector
{
    public class JavaScriptError
    {
        private readonly string errorMessage;
        private readonly string sourceName;
        private readonly int lineNumber;

        public JavaScriptError(Dictionary<string, object> map)
        {
            this.errorMessage = map["errorMessage"].ToString();
            this.sourceName = map["sourceName"].ToString();
            this.lineNumber = int.Parse(map["lineNumber"].ToString());
        }

        public JavaScriptError(string errorMessage, string sourceName, int lineNumber)
        {
            this.errorMessage = errorMessage;
            this.sourceName = sourceName;
            this.lineNumber = lineNumber;
        }

        public string ErrorMessage
        {
            get
            {
                return this.errorMessage;
            }
        }

        public string SourceName
        {
            get
            {
                return this.sourceName;
            }
        }

        public int LineNumber
        {
            get
            {
                return this.lineNumber;
            }
        }

        public override int GetHashCode()
        {
            string str = this.ToString();
            return str.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
                return false;

            return string.Equals(this.ToString(), obj.ToString());
        }

        public override string ToString()
        {
            return errorMessage + " [" + sourceName + ":" + lineNumber + "]";
        }

        /// <summary>
        /// Gets the collected JavaScript errors that have occurred since last call to this method.
        /// </summary>
        /// <param name="driver">the driver providing the possibility to retrieved JavaScript errors (see AddExtension(FirefoxProfile)).</param>
        /// <returns>the errors or an empty list if the driver doesn't provide access to the JavaScript errors</returns>
        public static IEnumerable<JavaScriptError> ReadErrors(IWebDriver driver)
        {
            const string script = "return window.JSErrorCollector_errors ? window.JSErrorCollector_errors.pump() : []";
            ReadOnlyCollection<object> errors = (ReadOnlyCollection<object>)((IJavaScriptExecutor)driver).ExecuteScript(script);
            List<JavaScriptError> response = new List<JavaScriptError>();
            foreach (object rawError in errors)
            {
                response.Add(new JavaScriptError((Dictionary<string, object>)rawError));
            }
            return response;
        }

        /// <summary>
        /// Adds the Firefox extension collecting JS errors to the profile what allows later use of ReadErrors(WebDriver).
        ///  
        ///  Example:
        ///  
        ///     FirefoxProfile profile = new FirefoxProfile();
        ///     JavaScriptError.AddExtension(profile);
        ///     IWebDriver driver = new FirefoxDriver(profile);
        ///  
        /// </summary>
        /// <param name="ffProfile"></param>
        /// <param name="xpiDirectory"></param>
        public static void AddExtension(FirefoxProfile ffProfile, string xpiDirectory)
        {
            ffProfile.AddExtension(Path.Combine(xpiDirectory, "JSErrorCollector.xpi"));
        }
    }
}
