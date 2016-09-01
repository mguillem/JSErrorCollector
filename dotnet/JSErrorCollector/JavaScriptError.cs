using System.Diagnostics;
using System.Reflection;
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
        private readonly string console;
        private readonly string stack;

        private JavaScriptError(Dictionary<string, object> map)
        {
            this.errorMessage = map["errorMessage"].ToString();
            this.sourceName = map["sourceName"].ToString();
            this.lineNumber = int.Parse(map["lineNumber"].ToString());
            if (map.ContainsKey("console") && map["console"] != null)
                this.console = map["console"].ToString();
            this.stack = map["stack"].ToString();
        }

        public JavaScriptError(string errorMessage, string sourceName, int lineNumber, string console)
        {
            this.errorMessage = errorMessage;
            this.sourceName = sourceName;
            this.lineNumber = lineNumber;
            this.console = console;
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

        public string Console
        {
            get
            {
                return this.console;
            }
        }

        public string Stack
        {
            get
            {
                return this.stack;
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
            string s = errorMessage + " [" + sourceName + ":" + lineNumber + "]";
            if (console != null)
            {
                s += "\nConsole: " + console;
            }
            s += "\nStack: \n" + stack;
            return s;
        }

        /// <summary>
        /// Gets the collected JavaScript errors that have occurred since last call to this method.
        /// </summary>
        /// <param name="driver">the driver providing the possibility to retrieved JavaScript errors (see AddExtension(FirefoxProfile)).</param>
        /// <returns>the errors or an empty list if the driver doesn't provide access to the JavaScript errors</returns>
        public static IList<JavaScriptError> ReadErrors(IWebDriver driver)
        {
            const string script = "return window.JSErrorCollector_errors ? window.JSErrorCollector_errors.pump() : []";
            ReadOnlyCollection<object> errors = (ReadOnlyCollection<object>)((IJavaScriptExecutor)driver).ExecuteScript(script);
            return errors.Select(rawErrorObject => (Dictionary<string, object>) rawErrorObject)
                         .Select(rawError => new JavaScriptError(rawError))
                         .ToList();
        }

        private const string xpiFilename = "JSErrorCollector.xpi";
        private const string xpiResourceName = "JSErrorCollector.JSErrorCollector.xpi";
        private static readonly string extractedXpiPath;

        /// <summary>
        /// Adds the Firefox extension collecting JS errors to the profile, which allows later use of
        /// ReadErrors(WebDriver), explicitly specifying the directory in which to find the XPI.
        /// </summary>
        /// <example><code>
        /// FirefoxProfile profile = new FirefoxProfile();
        /// JavaScriptError.AddExtension(profile, "./xpiDirectory");
        /// IWebDriver driver = new FirefoxDriver(profile);
        /// </code></example>
        public static void AddExtension(FirefoxProfile ffProfile, string xpiDirectory)
        {
            ffProfile.AddExtension(Path.Combine(xpiDirectory, xpiFilename));
        }

        /// <summary>
        /// Adds the Firefox extension collecting JS errors to the profile, which allows later use of
        /// ReadErrors(WebDriver).
        /// </summary>
        /// <example><code>
        /// FirefoxProfile profile = new FirefoxProfile();
        /// JavaScriptError.AddExtension(profile);
        /// IWebDriver driver = new FirefoxDriver(profile);
        /// </code></example>
        public static void AddExtension(FirefoxProfile ffProfile)
        {
            ffProfile.AddExtension(extractedXpiPath);
        }

        static JavaScriptError() {
            extractedXpiPath = ExtractXpiToTempFile();
        }

        private static string ExtractXpiToTempFile()
        {
            var xpiPath = Path.Combine(Path.GetTempPath(), xpiFilename);

            byte[] xpiData;
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(xpiResourceName)) {
                xpiData = BytesFromResource(resource);
            }

            // Only write out the XPI if it's not already present (or it's present but different)
            if (!File.Exists(xpiPath) || !File.ReadAllBytes(xpiPath).SequenceEqual(xpiData)) {
                WriteDataToFile(xpiData, xpiPath);
            }

            return xpiPath;
        }

        private static void WriteDataToFile(byte[] data, string path) {
            using (var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                file.Write(data, 0, data.Count());
            }
        }

        private static byte[] BytesFromResource(Stream resource) {
            byte[] data = new byte[resource.Length];
            resource.Read(data, 0, (int) resource.Length);
            return data;
        }
    }
}
