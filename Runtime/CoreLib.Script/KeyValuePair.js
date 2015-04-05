///////////////////////////////////////////////////////////////////////////////
// KeyValuePair

var ss_KeyValuePair = function#? DEBUG KeyValuePair$##() { };

ss_KeyValuePair.__typeName = 'ss.KeyValuePair';
ss.KeyValuePair = ss_KeyValuePair;
ss.initClass(ss_KeyValuePair, ss, {});
ss_KeyValuePair.__class = false;

ss_KeyValuePair.getDefaultValue = ss_KeyValuePair.createInstance = function#? DEBUG KeyValuePair$getDefaultValue##() {
	return { key: null, value: null };
};

ss_KeyValuePair.isInstanceOfType = function#? DEBUG KeyValuePair$isInstanceOfType##(o) {
	return typeof o === 'object' && 'key' in o && 'value' in o;
};
