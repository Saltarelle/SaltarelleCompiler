///////////////////////////////////////////////////////////////////////////////
// Boolean Extensions

Boolean.__typeName = 'Boolean';

Boolean.getDefaultValue = Boolean.createInstance = function#? DEBUG Boolean$getDefaultValue##() {
	return false;
}

Boolean.parse = function#? DEBUG Boolean$parse##(s) {
    return (s.toLowerCase() == 'true');
}
