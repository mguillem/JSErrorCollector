package JSErrorCollector;

import static org.junit.Assert.assertEquals;

import java.io.File;
import java.io.IOException;
import java.util.Arrays;
import java.util.List;

import org.junit.Test;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.firefox.FirefoxBinary;
import org.openqa.selenium.firefox.FirefoxDriver;
import org.openqa.selenium.firefox.FirefoxProfile;

/**
 * 
 * @author Marc Guillemot
 * @version $Revision:  $
 */
public class SimpleTest {

	/**
	 *
	 */
	@Test
	public void simple() throws Exception {
		final WebDriver driver = buildFFDriver();
		final String resource = getResource("simple.html");
		driver.get(resource);
		
		final List<JavaScriptError> expectedErrors = Arrays.asList(new JavaScriptError("null has no properties", resource, 9));
		final List<JavaScriptError> jsErrors = JavaScriptError.readErrors(driver);
		assertEquals(expectedErrors, jsErrors);
		
		driver.close();
	}
	
	WebDriver buildFFDriver() throws IOException {
		FirefoxProfile ffProfile = new FirefoxProfile();
		ffProfile.addExtension(new File("firefox")); // assuming that the test is started in project's root
		FirefoxBinary ffBinary = new FirefoxBinary(new File("/usr/bin/firefox"));
		return new FirefoxDriver(ffBinary , ffProfile);
	}

	private String getResource(final String string) {
		String resource = getClass().getClassLoader().getResource(string).toExternalForm();
		if (resource.startsWith("file:/") && !resource.startsWith("file:///")) {
			resource = "file://" + resource.substring(5);
		}
		return resource;
	}
}
