///////////////////////////////////////////////////////////////////////////////
// RegExp Extensions
ss.regexpEscape = function#? DEBUG ss$regexpEscape##(s) {
	return s.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&');
};
