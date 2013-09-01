///////////////////////////////////////////////////////////////////////////////
// MutableDateTime

var ss_JsDate = function#? DEBUG JsDate$##() { };

ss_JsDate.__typeName = 'ss.JsDate';
ss.JsDate = ss_JsDate;
ss.initClass(ss_JsDate, ss, {}, Object, [ ss_IEquatable, ss_IComparable ]);

ss_JsDate.createInstance = function#? DEBUG JsDate$createInstance##() {
	return new Date();
};

ss_JsDate.isInstanceOfType = function#? DEBUG JsDate$isInstanceOfType##(instance) {
	return instance instanceof Date;
};
