///////////////////////////////////////////////////////////////////////////////
// MutableDateTime

ss.JsDate = function#? DEBUG JsDate$##() { };

ss.JsDate.registerClass('ss.JsDate');

ss.JsDate.createInstance = function#? DEBUG JsDate$createInstance##() {
	return new Date();
}

ss.JsDate.isInstanceOfType = function#? DEBUG JsDate$isInstanceOfType##(instance) {
	return instance instanceof Date;
}
