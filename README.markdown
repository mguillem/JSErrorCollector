JSErrorCollector
---------------

This project allows to capture JavaScript errors while running tests with [WebDriver].
Ideally this feature should be built-in [WebDriver] but it is not (yet?) the case (see WebDriver issue [API for checking for JavaScript errors on the page] [1] for details).
Other information concerning JavaScript error capturing with WebDriver is available in this [blog post] [3].


Features:

 - provide access to JavaScript errors while running tests with a FirefoxDriver 

Usage:

		FirefoxProfile ffProfile = new FirefoxProfile();
		JavaScriptError.addExtension(ffProfile);
		final WebDriver driver = new FirefoxDriver(ffProfile);

		driver.get("http://somesite");
		
		final List<JavaScriptError> jsErrors = JavaScriptError.readErrors(driver);
		assertTrue(jsErrors.toString(), jsErrors.isEmpty());

Download:

Pre-built jar file is available in [dist folder] [2].

  [WebDriver]: http://code.google.com/p/webdriver
  [1]: http://code.google.com/p/selenium/issues/detail?id=148
  [2]: https://github.com/mguillem/JSErrorCollector/tree/master/dist
  [3]: http://mguillem.wordpress.com/2011/10/11/webdriver-capture-js-errors-while-running-tests/

