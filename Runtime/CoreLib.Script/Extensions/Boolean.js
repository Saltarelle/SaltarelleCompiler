///////////////////////////////////////////////////////////////////////////////
// Boolean Extensions

Boolean.__typeName = 'Boolean';
Boolean.__baseType = Object;
Boolean.__interfaces = [ ss_IEquatable, ss_IComparable ];

Boolean.getDefaultValue = Boolean.createInstance = function#? DEBUG Boolean$getDefaultValue##() {
	return false;
};

Boolean.parse = function#? DEBUG Boolean$parse##(s) {
	return (s.toLowerCase() == 'true');
};
