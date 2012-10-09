///////////////////////////////////////////////////////////////////////////////
// MutableDateTime

var ss_JsDate = function#? DEBUG JsDate$##() { };

Type.registerClass(global, 'ss.JsDate', ss_JsDate);

ss_JsDate.createInstance = function#? DEBUG JsDate$createInstance##() {
	return new Date();
};

ss_JsDate.isInstanceOfType = function#? DEBUG JsDate$isInstanceOfType##(instance) {
	return instance instanceof Date;
};
