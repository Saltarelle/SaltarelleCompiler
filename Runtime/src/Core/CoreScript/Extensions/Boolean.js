///////////////////////////////////////////////////////////////////////////////
// Boolean Extensions

Boolean.__typeName = 'Boolean';
Boolean.__baseType = Object;

Boolean.getDefaultValue = Boolean.createInstance = function#? DEBUG Boolean$getDefaultValue##() {
	return false;
};

Boolean.parse = function#? DEBUG Boolean$parse##(s) {
    return (s.toLowerCase() == 'true');
};

Boolean.prototype.getHashCode = function#? DEBUG Boolean$getHashCode##() {
	return this == true ? 1 : 0;
};

Boolean.prototype.equals = function#? DEBUG Boolean$equals##(b) {
	return this == b;
};
