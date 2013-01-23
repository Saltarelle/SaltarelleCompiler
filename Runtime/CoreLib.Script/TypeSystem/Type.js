///////////////////////////////////////////////////////////////////////////////
// Type System Implementation

ss.registerType = function#? DEBUG Type$registerType##(root, typeName, type) {
	var ns = root;
	var nameParts = typeName.split('.');

	for (var i = 0; i < nameParts.length - 1; i++) {
		var part = nameParts[i];
		var nso = ns[part];
		if (!nso) {
			ns[part] = nso = {};
		}
		ns = nso;
	}
	ns[nameParts[nameParts.length - 1]] = type;
};

ss.__genericCache = {};

ss._makeGenericTypeName = function#? DEBUG ss$_makeGenericTypeName##(genericType, typeArguments) {
	var result = genericType.__typeName;
	for (var i = 0; i < typeArguments.length; i++)
		result += (i === 0 ? '[' : ',') + ss.getTypeFullName(typeArguments[i]);
	result += ']';
	return result;
};

ss.makeGenericType = function#? DEBUG ss$makeGenericType##(genericType, typeArguments) {
	var name = ss._makeGenericTypeName(genericType, typeArguments);
	return ss.__genericCache[name] || genericType.apply(null, typeArguments);
};

ss.registerGenericClassInstance = function#? DEBUG ss$registerGenericInstance##(instance, genericType, typeArguments, baseType, interfaceTypes) {
	var name = ss._makeGenericTypeName(genericType, typeArguments);
	ss.__genericCache[name] = instance;
	instance.__genericTypeDefinition = genericType;
	instance.__typeArguments = typeArguments;
	ss.registerClass(null, name, instance, baseType(), interfaceTypes());
};

ss.registerGenericInterfaceInstance = function#? DEBUG ss$registerGenericInstance##(instance, genericType, typeArguments, baseInterfaces) {
	var name = ss._makeGenericTypeName(genericType, typeArguments);
	ss.__genericCache[name] = instance;
	instance.__genericTypeDefinition = genericType;
	instance.__typeArguments = typeArguments;
	ss.registerInterface(null, name, instance, baseInterfaces());
};

ss.isGenericTypeDefinition = function#? DEBUG ss$isGenericTypeDefinition##(type) {
	return type.__isGenericTypeDefinition || false;
};

ss.getGenericTypeDefinition = function#? DEBUG ss$getGenericTypeDefinition##(type) {
	return type.__genericTypeDefinition || null;
};

ss.getGenericParameterCount = function#? DEBUG ss$getGenericParameterCount##(type) {
	return type.__typeArgumentCount || 0;
};

ss.getGenericArguments = function#? DEBUG ss$getGenericArguments##(type) {
	return type.__typeArguments || null;
};

ss.registerClass = function#? DEBUG ss$registerClass##(root, name, ctor, baseType, interfaceType) {
	if (root)
		ss.registerType(root, name, ctor);

	ctor.prototype.constructor = ctor;
	ctor.__typeName = name;
	ctor.__class = true;
	ctor.__baseType = baseType || Object;
	if (baseType) {
		ss.setupBase(ctor);
	}

	if (interfaceType instanceof Array) {
		ctor.__interfaces = interfaceType;
	}
	else if (interfaceType) {
		ctor.__interfaces = [];
		for (var i = 4; i < arguments.length; i++) {
			interfaceType = arguments[i];
			ss.add(ctor.__interfaces, interfaceType);
		}
	}
};

ss.registerGenericClass = function#? DEBUG ss$registerGenericClass##(root, name, ctor, typeArgumentCount) {
	if (root)
		ss.registerType(root, name, ctor);

	ctor.prototype.constructor = ctor;
	ctor.__typeName = name;
	ctor.__class = true;
	ctor.__typeArgumentCount = typeArgumentCount;
	ctor.__isGenericTypeDefinition = true;
	ctor.__baseType = Object;
};

ss.registerInterface = function#? DEBUG ss$createInterface##(root, name, ctor, baseInterface) {
	if (root)
		ss.registerType(root, name, ctor);

	ctor.__typeName = name;
	ctor.__interface = true;
	if (baseInterface instanceof Array) {
		ctor.__interfaces = baseInterface;
	}
	else if (baseInterface) {
		ctor.__interfaces = [];
		for (var i = 3; i < arguments.length; i++) {
			ss.add(ctor.__interfaces, arguments[i]);
		}
	}
};

ss.registerGenericInterface = function#? DEBUG ss$registerGenericClass##(root, name, ctor, typeArgumentCount) {
	if (root)
		ss.registerType(root, name, ctor);

	ctor.prototype.constructor = ctor;
	ctor.__typeName = name;
	ctor.__interface = true;;
	ctor.__typeArgumentCount = typeArgumentCount;
	ctor.__isGenericTypeDefinition = true;
};

ss.registerEnum = function#? DEBUG ss$registerEnum##(root, name, ctor, flags) {
	if (root)
		ss.registerType(root, name, ctor);

	for (var field in ctor.prototype) {
		ctor[field] = ctor.prototype[field];
	}

	ctor.__typeName = name;
	ctor.__enum = true;
	if (flags) {
		ctor.__flags = true;
	}
	ctor.getDefaultValue = ctor.createInstance = function() { return 0; };
	ctor.isInstanceOfType = function(instance) { return typeof(instance) == 'number'; };
};

ss.setupBase = function#? DEBUG Type$setupBase##(type) {
	var baseType = type.__baseType;

	for (var memberName in baseType.prototype) {
		var memberValue = baseType.prototype[memberName];
		if (!type.prototype[memberName]) {
			type.prototype[memberName] = memberValue;
		}
	}
};

ss.getBaseType = function#? DEBUG ss$getBaseType##(type) {
	return type.__baseType || (type === Object ? null : Object);
};

ss.getTypeFullName = function#? DEBUG ss$getTypeFullName##(type) {
	if (type === Array) return 'Array';
	if (type === Boolean) return 'Boolean';
	if (type === Date) return 'Date';
	if (type === Error) return 'Error';
	if (type === Function) return 'Function';
	if (type === Number) return 'Number';
	if (type === Object) return 'Object';
	if (type === RegExp) return 'RegExp';
	if (type === String) return 'String';
	return type.__typeName || 'Object';
};

ss.getTypeName = function#? DEBUG ss$getTypeName##(type) {
	var fullName = ss.getTypeFullName(type);
	var bIndex = fullName.indexOf('[');
	var nsIndex = fullName.lastIndexOf('.', bIndex >= 0 ? bIndex : fullName.length);
	return nsIndex > 0 ? fullName.substr(nsIndex + 1) : fullName;
};

ss.getInterfaces = function#? DEBUG ss$getInterfaces##(type) {
	if (type === Array)
		return [ ss_IEnumerable, ss_ICollection, ss_IList ];
	else if (type === Boolean || type === Date || type === Number || type === String)
		return [ ss_IEquatable, ss_IComparable ];
	else
		return type.__interfaces || [];
};

ss.isInstanceOfType = function#? DEBUG ss$isInstanceOfType##(instance, type) {
	if (ss.isNullOrUndefined(instance))
		return false;

	if (typeof(type.isInstanceOfType) === 'function')
		return type.isInstanceOfType(instance);

	if ((type == Object) || (instance instanceof type)) {
		return true;
	}

	return ss.isAssignableFrom(type, ss.getInstanceType(instance));
};

ss.isAssignableFrom = function#? DEBUG ss$isAssignableFrom##(target, type) {
	if ((target == Object) || (target == type)) {
		return true;
	}
	if (target.__class) {
		var baseType = type.__baseType;
		while (baseType) {
			if (target == baseType) {
				return true;
			}
			baseType = baseType.__baseType;
		}
	}
	else if (target.__interface) {
		var interfaces = ss.getInterfaces(type);
		if (interfaces && ss.contains(interfaces, target)) {
			return true;
		}

		var baseType = ss.getBaseType(type);
		while (baseType) {
			interfaces = ss.getInterfaces(baseType);
			if (interfaces && ss.contains(interfaces, target)) {
				return true;
			}
			baseType = ss.getBaseType(baseType);
		}
	}
	return false;
};

ss.hasProperty = function#? DEBUG ss$hasProperty##(instance, name) {
	return typeof(instance['get_' + name]) === 'function' || typeof(instance['set_' + name]) === 'function';
};

ss.isClass = function#? DEBUG Type$isClass##(type) {
	return (type.__class == true || type === Array || type === Function || type === RegExp || type === String || type === Error || type === Object);
};

ss.isEnum = function#? DEBUG Type$isEnum##(type) {
	return (type.__enum == true);
};

ss.isFlags = function#? DEBUG Type$isFlags##(type) {
	return ((type.__enum == true) && (type.__flags == true));
};

ss.isInterface = function#? DEBUG Type$isInterface##(type) {
	return (type.__interface == true);
};

ss.safeCast = function#? DEBUG ss$safeCast##(instance, type) {
	return ss.isInstanceOfType(instance, type) ? instance : null;
};

ss.cast = function#? DEBUG ss$cast##(instance, type) {
	if (instance === null)
		return null;
	else if (typeof(instance) === "undefined" || ss.isInstanceOfType(instance, type)) {
		return instance;
	}
	throw 'Cannot cast object to type ' + ss.getTypeFullName(type);
};

ss.getInstanceType = function#? DEBUG ss$getInstanceType##(instance) {
	if (instance === null)
		throw 'Cannot get type of null';
	if (typeof(instance) === "undefined")
		throw 'Cannot get type of undefined';

	var ctor = null;

	// NOTE: We have to catch exceptions because the constructor
	//       cannot be looked up on native COM objects
	try {
		ctor = instance.constructor;
	}
	catch (ex) {
	}
	return ctor || Object;
};

ss.getType = function#? DEBUG ss$getType##(typeName) {
	if (!typeName) {
		return null;
	}

	if (!ss.__typeCache) {
		ss.__typeCache = {};
	}

	var type = ss.__typeCache[typeName];
	if (!type) {
		var arr = typeName.split(',');
		var type = (arr.length > 1 ? require(arr[1].trim) : global);

		var parts = arr[0].trim().split('.');
		for (var i = 0; i < parts.length; i++) {
			type = type[parts[i]];
			if (!type)
				break;
		}

		ss.__typeCache[typeName] = type || null;
	}
	return type;
};

ss.getDefaultValue = function#? DEBUG ss$getDefaultValue##(type) {
	if (typeof(type.getDefaultValue) === 'function')
		return type.getDefaultValue();
	else if (type === Boolean)
		return false;
	else if (type === Date)
		return new Date(0);
	else if (type === Number)
		return 0;
	return null;
};

ss.createInstance = function#? DEBUG ss$createInstance##(type) {
	if (typeof(type.createInstance) === 'function')
		return type.createInstance();
	else if (type === Boolean)
		return false;
	else if (type === Date)
		return new Date(0);
	else if (type === Number)
		return 0;
	else if (type === String)
		return '';
	else
		return new type();
};
