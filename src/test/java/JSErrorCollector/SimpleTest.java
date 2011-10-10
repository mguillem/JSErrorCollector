package JSErrorCollector;

import static org.junit.Assert.assertEquals;

import java.io.File;
import java.io.IOException;
import java.util.Arrays;
import java.util.List;

import org.junit.Test;
import org.openqa.selenium.By;
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
	private final String urlSimpleHtml = getResource("simple.html");
	private final JavaScriptError errorSimpleHtml = new JavaScriptError("null has no properties", urlSimpleHtml, 9);
	
	private final String urlWithNestedFrameHtml = getResource("withNestedFrame.html");
	private final JavaScriptError errorWithNestedFrameHtml = new JavaScriptError("\"foo\".notHere is not a function", urlWithNestedFrameHtml, 7);

	private final String urlWithPopupHtml = getResource("withPopup.html");
	private final String urlPopupHtml = getResource("popup.html");
	private final JavaScriptError errorPopupHtml = new JavaScriptError("error is not defined", urlPopupHtml, 5);

	private final String urlWithExternalJs = getResource("withExternalJs.html");
	private final String urlExternalJs = getResource("external.js");
	private final JavaScriptError errorExternalJs = new JavaScriptError("document.notExisting is undefined", urlExternalJs, 1);

	/**
	 *
	 */
	@Test
	public void simple() throws Exception {
		final WebDriver driver = buildFFDriver();
		driver.get(urlSimpleHtml);
		
		final List<JavaScriptError> expectedErrors = Arrays.asList(errorSimpleHtml);
		final List<JavaScriptError> jsErrors = JavaScriptError.readErrors(driver);
		assertEquals(expectedErrors, jsErrors);
		
		driver.quit();
	}
	
	/**
	 *
	 */
	@Test
	public void errorInNestedFrame() throws Exception {
		final List<JavaScriptError> expectedErrors = Arrays.asList(errorWithNestedFrameHtml, errorSimpleHtml);

		final WebDriver driver = buildFFDriver();
		driver.get(urlWithNestedFrameHtml);
		
		final List<JavaScriptError> jsErrors = JavaScriptError.readErrors(driver);
		assertEquals(expectedErrors.toString(), jsErrors.toString());
		
		driver.quit();
	}
	
	/**
	 *
	 */
	@Test
	public void errorInPopup() throws Exception {
		final List<JavaScriptError> expectedErrors = Arrays.asList(errorPopupHtml);

		final WebDriver driver = buildFFDriver();
		driver.get(urlWithPopupHtml);
		driver.findElement(By.tagName("button")).click();

		final List<JavaScriptError> jsErrors = JavaScriptError.readErrors(driver);
		assertEquals(expectedErrors.toString(), jsErrors.toString());
		
		driver.quit();
	}

	/**
	 *
	 */
	@Test
	public void errorInExternalJS() throws Exception {
		final List<JavaScriptError> expectedErrors = Arrays.asList(errorExternalJs);

		final WebDriver driver = buildFFDriver();
		driver.get(urlWithExternalJs);

		final List<JavaScriptError> jsErrors = JavaScriptError.readErrors(driver);
		assertEquals(expectedErrors.toString(), jsErrors.toString());
		
		driver.quit();
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
