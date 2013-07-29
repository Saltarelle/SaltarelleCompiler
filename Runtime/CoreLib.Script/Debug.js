///////////////////////////////////////////////////////////////////////////////
// Debug Extensions

ss.Debug = global.Debug || function() {};
ss.Debug.__typeName = 'Debug';

if (!ss.Debug.writeln) {
	ss.Debug.writeln = function#? DEBUG Debug$writeln##(text) {
		if (global.console) {
			if (global.console.debug) {
				global.console.debug(text);
				return;
			}
			else if (global.console.log) {
				global.console.log(text);
				return;
			}
		}
		else if (global.opera &&
			global.opera.postError) {
			global.opera.postError(text);
			return;
		}
	}
};

ss.Debug._fail = function#? DEBUG Debug$_fail##(message) {
	ss.Debug.writeln(message);
	debugger;
};

ss.Debug.assert = function#? DEBUG Debug$assert##(condition, message) {
	if (!condition) {
		message = 'Assert failed: ' + message;
		if (confirm(message + '\r\n\r\nBreak into debugger?')) {
			ss.Debug._fail(message);
		}
	}
};

ss.Debug.fail = function#? DEBUG Debug$fail##(message) {
	ss.Debug._fail(message);
};
