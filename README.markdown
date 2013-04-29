JSErrorCollector.NET
---------------

(Note: This is .NET port of the [JSErrorCollector project by mguillem] [JSErrorCollectorOriginal])

This project allows to capture JavaScript errors while running tests with [WebDriver].
Ideally this feature should be built-in [WebDriver] but it is not (yet?) the case (see WebDriver issue [API for checking for JavaScript errors on the page] [1] for details).
Other information concerning JavaScript error capturing with WebDriver is available in this [blog post] [3].


Features:

 - provide access to JavaScript errors while running tests with a FirefoxDriver (in .NET)

Usage in .NET test code:

	FirefoxProfile ffProfile = new FirefoxProfile();
	JavaScriptError.AddExtension(ffProfile);
	using (IWebDriver driver = new FirefoxDriver(ffProfile))
	{
		driver.Navigate().GoToUrl("http://somesite");

		List<JavaScriptError> jsErrors = JavaScriptError.ReadErrors(driver);
		// Assert that the expected JavaScript errors were returned.
	}

Download:

Pre-built .xpi and .NET assembly are available in the [dist](dist/) folder.

For other platforms:

If you're using Java, please see the original [JSErrorCollector project] [JSErrorCollectorOriginal].  Otherwise, 
the Firefox extension (the [.xpi file](dist/JSErrorCollector.xpi)) can be used from any language having a WebDriver binding.
Here is an example about [how it can be used in Ruby from Cucumber + Capybara] [4].

JSErrorCollector.NET is licensed under the terms of the [Apache License 2] [6].

  [JSErrorCollectorOriginal]: https://github.com/mguillem/JSErrorCollector
  [WebDriver]: http://code.google.com/p/webdriver
  [1]: http://code.google.com/p/selenium/issues/detail?id=148
  [3]: http://mguillem.wordpress.com/2011/10/11/webdriver-capture-js-errors-while-running-tests/
  [4]: https://gist.github.com/1371962
  [6]: http://www.apache.org/licenses/LICENSE-2.0.txt
