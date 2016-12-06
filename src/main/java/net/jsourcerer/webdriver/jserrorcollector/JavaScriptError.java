package net.jsourcerer.webdriver.jserrorcollector;

import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;

import org.openqa.selenium.JavascriptExecutor;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.firefox.FirefoxDriver;
import org.openqa.selenium.firefox.FirefoxProfile;

/**
 * Holds information about a JavaScript error that has occurred in the browser.
 * This can be currently only used with the {@link FirefoxDriver} (see {@link #addExtension(FirefoxProfile)}.
 * @author Marc Guillemot
 * @version $Revision:  $
 */
public class JavaScriptError {
	private final String errorMessage;

	JavaScriptError(final Map<String, ? extends Object> map) {
		errorMessage = (String) map.get("errorMessage");
	}

	JavaScriptError(final String errorMessage, final String sourceName, final int lineNumber, String console) {
		this.errorMessage = errorMessage;
	}

	public String getErrorMessage() {
		return errorMessage;
	}

	/* (non-Javadoc)
	 * @see java.lang.Object#hashCode()
	 */
	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result
				+ ((errorMessage == null) ? 0 : errorMessage.hashCode());
		return result;
	}

	/* (non-Javadoc)
	 * @see java.lang.Object#equals(java.lang.Object)
	 */
	@Override
	public boolean equals(Object obj) {
		if (this == obj) {
			return true;
		}
		if (obj == null) {
			return false;
		}
		if (getClass() != obj.getClass()) {
			return false;
		}
		JavaScriptError other = (JavaScriptError) obj;
		if (errorMessage == null) {
			if (other.errorMessage != null) {
				return false;
			}
		} else if (!errorMessage.equals(other.errorMessage)) {
			return false;
		}
		return true;
	}

	@Override
	public String toString() {
		String s = errorMessage;
		return s;
	}

	/**
	 * Gets the collected JavaScript errors that have occurred since last call to this method.
	 * @param driver the driver providing the possibility to retrieved JavaScript errors (see {@link #addExtension(FirefoxProfile)}.
	 * @return the errors or an empty list if the driver doesn't provide access to the JavaScript errors
	 */
	@SuppressWarnings("unchecked")
	public static List<JavaScriptError> readErrors(final WebDriver driver) {
		final String script = "return window.JSErrorCollector_errors ? window.JSErrorCollector_errors.pump() : []";
		final List<Object> errors = (List<Object>) ((JavascriptExecutor) driver).executeScript(script);
		final List<JavaScriptError> response = new ArrayList<JavaScriptError>();
		for (final Object rawError : errors) {
			response.add(new JavaScriptError((Map<String, ? extends Object>) rawError));
		}

		return response;
	}

	/**
	 * Adds the Firefox extension collecting JS errors to the profile what allows later use of {@link #readErrors(WebDriver)}.
	 * <p>
	 * Example:<br>
	 * <code><pre>
	 * final FirefoxProfile profile = new FirefoxProfile();
	 * JavaScriptError.addExtension(profile);
	 * final WebDriver driver = new FirefoxDriver(profile);
	 * </pre></code>
	 * @param ffProfile the Firefox profile to which the extension should be added.
	 * @throws IOException in case of problem
	 */
	public static void addExtension(final FirefoxProfile ffProfile) throws IOException {
		ffProfile.addExtension(JavaScriptError.class, "JSErrorCollector.xpi");
	}
}
