var JSErrorCollector = new function() {
	var list = [];
	this.collectedErrors = {
		push: function (jsError) {
			list.push(jsError);
		},
		read: function() {
			return list.slice();
		},
		clear: function() {
			list = [];
		},
		pump: function() {
			var resp = list.slice();
			list = [];
			return resp;
		},
		toString: function() {
			var s = "";
			for (var i=0; i<list.length; ++i) {
				s += i + ": " + list[i] + "\n";
			}
			return s;
		}
	};
	
	this.onLoad = function(event) {
	    // initialization code
		this.initialize(event);
	    this.initialized = true;
	};
  
	this.initialize = function(event) {
		var windowContent = window.getBrowser();

		var consoleService = Components.classes["@mozilla.org/consoleservice;1"].getService().QueryInterface(Components.interfaces.nsIConsoleService);
		if (consoleService)
		{
			consoleService.registerListener(JSErrorCollector_ErrorConsoleListener);
		}

		var onPageLoad = function(aEvent) {
			var doc = aEvent.originalTarget;  
			var win = doc.defaultView;
			if (win) {
				win.wrappedJSObject.JSErrorCollector_errors = Components.utils.cloneInto(JSErrorCollector.collectedErrors,win.wrappedJSObject,{cloneFunctions: true});
			}
		};

		windowContent.addEventListener("load", onPageLoad, true);
	};

	this.addError = function(error) {
		this.collectedErrors.push(error);
		
		var labelField = document.getElementById("JSErrorCollector-nb");
		labelField.nb = labelField.nb || 0;
		labelField.nb++;
		labelField.value = labelField.nb;
	}
};

//Error console listener
var JSErrorCollector_ErrorConsoleListener =
{
    observe: function(consoleMessage)
    {
        if (document && consoleMessage)
        {
            // Try to convert the error to a script error
            try
            {
                var scriptError = consoleMessage.QueryInterface(Components.interfaces.nsIScriptError);

                var errorCategory = scriptError.category;
                var sourceName    = scriptError.sourceName;
                if (sourceName.indexOf("about:") == 0 || sourceName.indexOf("chrome:") == 0) {
                	return; // not interested in internal errors
                }

                // We're just looking for content JS errors (see https://developer.mozilla.org/en/XPCOM_Interface_Reference/nsIScriptError#Categories)
                if (errorCategory == "content javascript")
                {
                	var consoleContent = null;
                	// try to get content from Firebug's console if it exists
                	try {
                    	if (window.Firebug && window.Firebug.currentContext) {
                        	var doc = Firebug.currentContext.getPanel("console").document;
//                        	console.log("doc", doc.body.innerHTML, doc)
                        	var logNodes = doc.querySelectorAll(".logRow .logContent span");
                        	var consoleLines = [];
                        	for (var i=0; i<logNodes.length; ++i) {
                        		var logNode = logNodes[i];
                        		if (!logNode.JSErrorCollector_extracted) {
                            		consoleLines.push(logNodes[i].textContent);
                            		logNode.JSErrorCollector_extracted = true;
                        		}
                        	}
                        	
                        	consoleContent = consoleLines.join("\n");
                        }
                    } catch (e) {
                    	consoleContent = "Error extracting content of Firebug console: " + e.message;
                    }

                    function getStack() {
                    	var stack = [];
                    	var current = scriptError.stack;
                    	while (current) {
                    		stack.push(
                    			(current.functionDisplayName || '(anonymous function)') + ' @ ' +
                    			current.source + ' : ' +
                    			current.line + ' : ' +
                    			current.column);
                    		current = current.parent;
                    	}
                    	return stack.join('\n');
                    } 

                    var err = {
						errorMessage: scriptError.errorMessage,
						sourceName: scriptError.sourceName,
						lineNumber: scriptError.lineNumber,
            			console: consoleContent,
            			stack: getStack()
            		};
                	console.log("collecting JS error", err)
                	JSErrorCollector.addError(err);
                }
            }
            catch (exception)
            {
                // ignore
            }
        }

        return false;
    }
};

window.addEventListener("load", function(e) { JSErrorCollector.onLoad(e); }, false); 

