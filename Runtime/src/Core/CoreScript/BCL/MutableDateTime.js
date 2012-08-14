///////////////////////////////////////////////////////////////////////////////
// MutableDateTime

ss.MutableDateTime = function#? DEBUG MutableDateTime$##() { };

ss.MutableDateTime.registerClass('ss.MutableDateTime');

ss.MutableDateTime.isInstanceOfType = function#? DEBUG MutableDateTime$isInstanceOfType##(instance) {
	return instance instanceof Date;
}
