if (typeof(global) === "undefined") {
	if (typeof(window) !== "undefined")
		global = window;
	else if (typeof(self) !== "undefined")
		global = self;
}
(function(global) {
	"use strict";
	var ss = global.ss;

#include "Contract.js"
#include "ContractException.js"

})(global);
