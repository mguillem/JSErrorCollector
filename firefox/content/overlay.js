var JSErrorCollector = new function() {
	var list = [];
	this.collectedErrors = {
		push: function (jsError) {
			list.push(jsError);
		},
		pump: function() {
			var resp = [];
			for (var i=0; i<list.length; ++i) {
				var script = list[i];
				resp[i] = {
						errorMessage: script.message
						};
			}
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
          var err = {
						message: consoleMessage
      		};
        	console.log("collecting JS message", err)
        	JSErrorCollector.addError(err);
        }

        return false;
    }
};

window.addEventListener("load", function(e) { JSErrorCollector.onLoad(e); }, false);
