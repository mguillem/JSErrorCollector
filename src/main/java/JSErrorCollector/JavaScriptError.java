package JSErrorCollector;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

import org.openqa.selenium.JavascriptExecutor;
import org.openqa.selenium.WebDriver;

/**
 * Holds information about a JavaScript error.
 * @author Marc Guillemot
 * @version $Revision:  $
 */
public class JavaScriptError {
	private final String errorMessage;
	private final String sourceName;
	private final int lineNumber;	

	public JavaScriptError(final Map<String, ? extends Object> map) {
		errorMessage = (String) map.get("errorMessage");
		sourceName = (String) map.get("sourceName");
		lineNumber = ((Number) map.get("lineNumber")).intValue();
	}

	public JavaScriptError(final String errorMessage, final String sourceName, final int lineNumber) {
		this.errorMessage = errorMessage;
		this.sourceName = sourceName;
		this.lineNumber = lineNumber;
	}

	public String getErrorMessage() {
		return errorMessage;
	}
	
	public int getLineNumber() {
		return lineNumber;
	}
	
	public String getSourceName() {
		return sourceName;
	}
	
	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result
				+ ((errorMessage == null) ? 0 : errorMessage.hashCode());
		result = prime * result + lineNumber;
		result = prime * result
				+ ((sourceName == null) ? 0 : sourceName.hashCode());
		return result;
	}

	@Override
	public boolean equals(Object obj) {
		if (this == obj)
			return true;
		if (obj == null || getClass() != obj.getClass())
			return false;

		final JavaScriptError other = (JavaScriptError) obj;
		if (errorMessage == null) {
			if (other.errorMessage != null)
				return false;
		} else if (!errorMessage.equals(other.errorMessage))
			return false;
		if (lineNumber != other.lineNumber)
			return false;
		if (sourceName == null) {
			if (other.sourceName != null)
				return false;
		} else if (!sourceName.equals(other.sourceName))
			return false;
		return true;
	}

	@Override
	public String toString() {
		return errorMessage + " [" + sourceName + ":" + lineNumber + "]";
	}

	@SuppressWarnings("unchecked")
	public static List<JavaScriptError> readErrors(final WebDriver driver) {
		final List<Object> errors = (List<Object>) ((JavascriptExecutor) driver).executeScript("return window.JSErrorCollector_errors.pump()");
		final List<JavaScriptError> response = new ArrayList<JavaScriptError>();
		for (final Object rawError : errors) {
			response.add(new JavaScriptError((Map<String, ? extends Object>) rawError));
		}
		
		return response;
	}
}
