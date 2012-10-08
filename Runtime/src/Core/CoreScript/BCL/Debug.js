///////////////////////////////////////////////////////////////////////////////
// Debug Extensions

ss.Debug = globals.Debug || function() {};
ss.Debug.__typeName = 'Debug';

if (!ss.Debug.writeln) {
    ss.Debug.writeln = function#? DEBUG Debug$writeln##(text) {
        if (globals.console) {
            if (globals.console.debug) {
                globals.console.debug(text);
                return;
            }
            else if (globals.console.log) {
                globals.console.log(text);
                return;
            }
        }
        else if (globals.opera &&
            globals.opera.postError) {
            globals.opera.postError(text);
            return;
        }
    }
};

ss.Debug._fail = function#? DEBUG Debug$_fail##(message) {
    ss.Debug.writeln(message);
    eval('debugger;');
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
