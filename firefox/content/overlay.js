var JSErrorCollector = {
	collectedErrors: {
		list: [],
		push: function (jsError) {
			this.list[this.list.length] = jsError;
		},
		pump: function() {
			var resp = [];
			for (var i=0; i<this.list.length; ++i) {
				var scriptError = this.list[i];
				resp[i] = {
						errorMessage: scriptError.errorMessage,
						sourceName: scriptError.sourceName,
						lineNumber: scriptError.lineNumber
						};
			}
			this.list.length = 0;
			return resp;
		},
		toString: function() {
			var s = "";
			for (var i=0; i<this.list.length; ++i) {
				s += i + ": " + this.list[i] + "\n";
			}
			return s;
		}
	},
	onLoad: function(event) {
	    // initialization code
		this.initialize(event);
	    this.initialized = true;
	},
  
	initialize: function(event) {
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
				win.wrappedJSObject.JSErrorCollector_errors = JSErrorCollector.collectedErrors;
		    }
		};
			 
		windowContent.addEventListener("load", onPageLoad, true);
	},

	addError: function(error) {
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
                var error = consoleMessage.QueryInterface(Components.interfaces.nsIScriptError);

                var errorCategory = error.category;
                var sourceName    = error.sourceName;
                if (sourceName.indexOf("about:") == 0 || sourceName.indexOf("chrome:") == 0) {
                	return; // not interested in internal errors
                }

                // We're just looking for content JS errors (see https://developer.mozilla.org/en/XPCOM_Interface_Reference/nsIScriptError#Categories)
                if (errorCategory == "content javascript")
                {
                	JSErrorCollector.addError(error);
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
