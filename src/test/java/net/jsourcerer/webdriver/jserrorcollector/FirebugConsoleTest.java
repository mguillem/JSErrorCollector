package net.jsourcerer.webdriver.jserrorcollector;

import static org.junit.Assert.assertEquals;

import java.io.File;
import java.util.Arrays;
import java.util.List;

import org.junit.AfterClass;
import org.junit.BeforeClass;
import org.junit.Ignore;
import org.junit.Test;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.firefox.FirefoxDriver;
import org.openqa.selenium.firefox.FirefoxProfile;


/**
 * Tests with Firebug ensuring that we get JS errors & console content.
 * @author Marc Guillemot
 * @version $Revision:  $
 */
@Ignore // don't know why but Firebug doesn't automatically open. Need to investigate
public class FirebugConsoleTest {
	private static WebDriver webDriver;


	@BeforeClass
	public static void setup() throws Exception {
		FirefoxProfile ffProfile = new FirefoxProfile();
		ffProfile.addExtension(new File("firefox")); // assuming that the test is started in project's root
		ffProfile.addExtension(FirebugConsoleTest.class, "/firebug-2.0.8-fx.xpi");

		ffProfile.setPreference("extensions.firebug.showStackTrace", "true");
		ffProfile.setPreference("extensions.firebug.delayLoad", "false");
		ffProfile.setPreference("extensions.firebug.showFirstRunPage", "false");
		ffProfile.setPreference("extensions.firebug.allPagesActivation", "on");
		ffProfile.setPreference("extensions.firebug.console.enableSites", "true");
		ffProfile.setPreference("extensions.firebug.defaultPanelName", "console");

		webDriver = new FirefoxDriver(ffProfile);
	}

	@AfterClass
	public static void tearDown() {
		webDriver.quit();
	}

	/**
	 *
	 */
	@Test
	public void simple() throws Exception {
		final String url = getResource("withConsoleOutput.html");
		webDriver.get(url);

		final JavaScriptError errorSimpleHtml = new JavaScriptError("TypeError: null has no properties", url, 8, "before JS error");
		
		final List<JavaScriptError> expectedErrors = Arrays.asList(errorSimpleHtml);
		List<JavaScriptError> jsErrors = JavaScriptError.readErrors(webDriver);
		assertEquals(expectedErrors, jsErrors);
	}

	private String getResource(final String string) {
		String resource = getClass().getClassLoader().getResource(string).toExternalForm();
		if (resource.startsWith("file:/") && !resource.startsWith("file:///")) {
			resource = "file://" + resource.substring(5);
		}
		return resource;
	}
}
