package net.jsourcerer.webdriver.jserrorcollector;

import org.openqa.selenium.JavascriptExecutor;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.firefox.FirefoxDriver;
import org.openqa.selenium.firefox.FirefoxProfile;

import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;

/**
 * Holds information about a JavaScript error that has occurred in the browser.
 * This can be currently only used with the {@link FirefoxDriver} (see {@link #addExtension(FirefoxProfile)}.
 *
 * @author Marc Guillemot
 * @version $Revision:  $
 */
public class JavaScriptError {
    private final String errorMessage;
    private final String sourceName;
    private final int lineNumber;
    private final String console;
    private final String pageUrl;

    JavaScriptError(final Map<String, ? extends Object> map) {
        errorMessage = (String) map.get("errorMessage");
        sourceName = (String) map.get("sourceName");
        lineNumber = ((Number) map.get("lineNumber")).intValue();
        console = (String) map.get("console");
        pageUrl = (String) map.get("pageUrl");
    }

    JavaScriptError(final String errorMessage, final String sourceName, final int lineNumber, String console, final String pageUrl) {
        this.errorMessage = errorMessage;
        this.sourceName = sourceName;
        this.lineNumber = lineNumber;
        this.console = console;
        this.pageUrl = pageUrl;
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

    /**
     * If Firebug is installed and active, this will contain the content of the Firebug Console since
     * the previous JavaScript error.
     *
     * @return
     */
    public String getConsole() {
        return console;
    }

    /**
     * Url of the web page with JavaScript error
     */
    public String getPageUrl() {
        return pageUrl;
    }

    /* (non-Javadoc)
     * @see java.lang.Object#hashCode()
     */
    @Override
    public int hashCode() {
        final int prime = 31;
        int result = 1;
        result = prime * result + ((console == null) ? 0 : console.hashCode());
        result = prime * result
                + ((errorMessage == null) ? 0 : errorMessage.hashCode());
        result = prime * result + lineNumber;
        result = prime * result
                + ((sourceName == null) ? 0 : sourceName.hashCode());
        result = prime * result
                + ((pageUrl == null) ? 0 : pageUrl.hashCode());
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
        if (console == null) {
            if (other.console != null) {
                return false;
            }
        } else if (!console.equals(other.console)) {
            return false;
        }
        if (errorMessage == null) {
            if (other.errorMessage != null) {
                return false;
            }
        } else if (!errorMessage.equals(other.errorMessage)) {
            return false;
        }
        if (lineNumber != other.lineNumber) {
            return false;
        }
        if (sourceName == null) {
            if (other.sourceName != null) {
                return false;
            }
        } else if (!sourceName.equals(other.sourceName)) {
            return false;
        }
        if (pageUrl == null) {
            if (other.pageUrl != null) {
                return false;
            }
        } else if (!pageUrl.equals(other.pageUrl)) {
            return false;
        }
        return true;
    }

    @Override
    public String toString() {
        String s = errorMessage + " [" + sourceName + ":" + lineNumber + "]";
        s += "\nUrl: " + pageUrl;
        if (console != null) {
            s += "\nConsole: " + console;
        }
        return s;
    }

    /**
     * Gets the collected JavaScript errors that have occurred since last call to this method.
     *
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
     * <p/>
     * Example:<br>
     * <code><pre>
     * final FirefoxProfile profile = new FirefoxProfile();
     * JavaScriptError.addExtension(profile);
     * final WebDriver driver = new FirefoxDriver(profile);
     * </pre></code>
     *
     * @param ffProfile the Firefox profile to which the extension should be added.
     * @throws IOException in case of problem
     */
    public static void addExtension(final FirefoxProfile ffProfile) throws IOException {
        ffProfile.addExtension(JavaScriptError.class, "JSErrorCollector.xpi");
    }
}
