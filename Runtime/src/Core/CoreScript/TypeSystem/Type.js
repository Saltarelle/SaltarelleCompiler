///////////////////////////////////////////////////////////////////////////////
// Type System Implementation

global.Type = Function;

Type.registerType = function#? DEBUG Type$registerType##(root, typeName, type) {
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

Type.__genericCache = {};

Type._makeGenericTypeName = function#? DEBUG Type$_makeGenericTypeName##(genericType, typeArguments) {
	var result = genericType.__typeName;
	for (var i = 0; i < typeArguments.length; i++)
		result += (i === 0 ? '[' : ',') + typeArguments[i].__typeName;
	result += ']';
	return result;
};

Type.makeGenericType = function#? DEBUG Type$makeGenericType##(genericType, typeArguments) {
	var name = Type._makeGenericTypeName(genericType, typeArguments);
	return Type.__genericCache[name] || genericType.apply(null, typeArguments);
};

Type.prototype.registerGenericClassInstance = function#? DEBUG Type$registerGenericInstance##(instance, genericType, typeArguments, baseType, interfaceTypes) {
	var name = Type._makeGenericTypeName(genericType, typeArguments);
	Type.__genericCache[name] = instance;
	instance.__genericTypeDefinition = genericType;
	instance.__typeArguments = typeArguments;
	Type.registerClass(null, name, instance, baseType(), interfaceTypes());
};

Type.registerGenericInterfaceInstance = function#? DEBUG Type$registerGenericInstance##(instance, genericType, typeArguments, baseInterfaces) {
	var name = Type._makeGenericTypeName(genericType, typeArguments);
	Type.__genericCache[name] = instance;
	instance.__genericTypeDefinition = genericType;
	instance.__typeArguments = typeArguments;
	Type.registerInterface(null, name, instance, baseInterfaces());
};

Type.prototype.get_isGenericTypeDefinition = function#? DEBUG Type$get_isGenericTypeDefinition##() {
	return this.__isGenericTypeDefinition || false;
};

Type.prototype.getGenericTypeDefinition = function#? DEBUG Type$getGenericTypeDefinition##() {
	return this.__genericTypeDefinition || null;
};

Type.prototype.get_genericParameterCount = function#? DEBUG Type$get_genericParameterCount##() {
	return this.__typeArgumentCount || 0;
};

Type.prototype.getGenericArguments = function#? DEBUG Type$getGenericArguments##() {
    return this.__typeArguments || null;
};

Type.registerClass = function#? DEBUG Type$registerClass##(root, name, ctor, baseType, interfaceType) {
	if (root)
		Type.registerType(root, name, ctor);

    ctor.prototype.constructor = ctor;
    ctor.__typeName = name;
    ctor.__class = true;
    ctor.__baseType = baseType || Object;
    if (baseType) {
        ctor.setupBase(baseType);
    }

	if (interfaceType instanceof Array) {
		ctor.__interfaces = interfaceType;
	}
	else if (interfaceType) {
        ctor.__interfaces = [];
        for (var i = 4; i < arguments.length; i++) {
            interfaceType = arguments[i];
            ctor.__interfaces.add(interfaceType);
        }
    }
};

Type.registerGenericClass = function#? DEBUG Type$registerGenericClass##(root, name, ctor, typeArgumentCount) {
	if (root)
		Type.registerType(root, name, ctor);

    ctor.prototype.constructor = ctor;
    ctor.__typeName = name;
    ctor.__class = true;
	ctor.__typeArgumentCount = typeArgumentCount;
	ctor.__isGenericTypeDefinition = true;
    ctor.__baseType = Object;
};

Type.registerInterface = function#? DEBUG Type$createInterface##(root, name, ctor, baseInterface) {
	if (root)
		Type.registerType(root, name, ctor);

    ctor.__typeName = name;
    ctor.__interface = true;
	if (baseInterface instanceof Array) {
		ctor.__interfaces = baseInterface;
	}
	else if (baseInterface) {
        ctor.__interfaces = [];
        for (var i = 3; i < arguments.length; i++) {
            ctor.__interfaces.add(arguments[i]);
        }
    }
};

Type.registerGenericInterface = function#? DEBUG Type$registerGenericClass##(root, name, ctor, typeArgumentCount) {
	if (root)
		Type.registerType(root, name, ctor);

    ctor.prototype.constructor = ctor;
    ctor.__typeName = name;
    ctor.__interface = true;;
	ctor.__typeArgumentCount = typeArgumentCount;
	ctor.__isGenericTypeDefinition = true;
};

Type.prototype.registerEnum = function#? DEBUG Type$createEnum##(root, name, ctor, flags) {
	if (root)
		Type.registerType(root, name, ctor);

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

Type.prototype.setupBase = function#? DEBUG Type$setupBase##() {
	var baseType = this.__baseType;

	for (var memberName in baseType.prototype) {
		var memberValue = baseType.prototype[memberName];
		if (!this.prototype[memberName]) {
			this.prototype[memberName] = memberValue;
		}
	}
};

if (!Type.prototype.resolveInheritance) {
    // This function is not used by Script#; Visual Studio relies on it
    // for JavaScript IntelliSense support of derived types.
    Type.prototype.resolveInheritance = Type.prototype.setupBase;
};

Type.prototype.get_baseType = function#? DEBUG Type$get_baseType##() {
    return this.__baseType || null;
};

Type.prototype.get_fullName = function#? DEBUG Type$get_fullName##() {
    return this.__typeName;
};

Type.prototype.get_name = function#? DEBUG Type$get_name##() {
    var fullName = this.__typeName;
    var nsIndex = fullName.lastIndexOf('.');
    if (nsIndex > 0) {
        return fullName.substr(nsIndex + 1);
    }
    return fullName;
};

Type.prototype.getInterfaces = function#? DEBUG Type$getInterfaces##() {
    return this.__interfaces;
};

Type.prototype.isInstanceOfType = function#? DEBUG Type$isInstanceOfType##(instance) {
    if (ss.isNullOrUndefined(instance)) {
        return false;
    }
    if ((this == Object) || (instance instanceof this)) {
        return true;
    }

    var type = Type.getInstanceType(instance);
    return this.isAssignableFrom(type);
};

Type.isInstanceOfType = function#? DEBUG Type$isInstanceOfTypeStatic##(instance, type) {
    return instance instanceof type || (type !== Function && type.isInstanceOfType && type.isInstanceOfType(instance));
};

Type.prototype.isAssignableFrom = function#? DEBUG Type$isAssignableFrom##(type) {
    if ((this == Object) || (this == type)) {
        return true;
    }
    if (this.__class) {
        var baseType = type.__baseType;
        while (baseType) {
            if (this == baseType) {
                return true;
            }
            baseType = baseType.__baseType;
        }
    }
    else if (this.__interface) {
        var interfaces = type.__interfaces;
        if (interfaces && interfaces.contains(this)) {
            return true;
        }

        var baseType = type.__baseType;
        while (baseType) {
            interfaces = baseType.__interfaces;
            if (interfaces && interfaces.contains(this)) {
                return true;
            }
            baseType = baseType.__baseType;
        }
    }
    return false;
};

Type.hasProperty = function#? DEBUG Type$hasProperty##(instance, name) {
	return typeof(instance['get_' + name]) === 'function' || typeof(instance['set_' + name]) === 'function';
};

Type.prototype.get_isClass = function#? DEBUG Type$get_isClass##() {
    return (this.__class == true);
};

Type.prototype.get_isEnum = function#? DEBUG Type$get_isEnum##() {
    return (this.__enum == true);
};

Type.prototype.get_isFlags = function#? DEBUG Type$get_isFlags##() {
    return ((this.__enum == true) && (this.__flags == true));
};

Type.prototype.get_isInterface = function#? DEBUG Type$get_isInterface##() {
    return (this.__interface == true);
};

Type.canCast = function#? DEBUG Type$canCast##(instance, type) {
    return Type.isInstanceOfType(instance, type);
};

Type.safeCast = function#? DEBUG Type$safeCast##(instance, type) {
    if (Type.isInstanceOfType(instance, type)) {
        return instance;
    }
    return null;
};

Type.cast = function#? DEBUG Type$cast##(instance, type) {
	if (instance === null)
		return null;
    else if (typeof(instance) === "undefined" || Type.isInstanceOfType(instance, type)) {
        return instance;
    }
    throw 'Cannot cast object to type ' + type.__typeName;
};

Type.getInstanceType = function#? DEBUG Type$getInstanceType##(instance) {
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
    if (!ctor || !ctor.__typeName) {
        ctor = Object;
    }
    return ctor;
};

Type.getType = function#? DEBUG Type$getType##(typeName) {
    if (!typeName) {
        return null;
    }

    if (!Type.__typeCache) {
        Type.__typeCache = {};
    }

    var type = Type.__typeCache[typeName];
    if (!type) {
		var arr = typeName.split(',');
		var type = (arr.length > 1 ? require(arr[1].trim) : global);

		var parts = arr[0].trim().split('.');
		for (var i = 0; i < parts.length; i++) {
			type = type[parts[i]];
			if (!type)
				break;
		}

        Type.__typeCache[typeName] = type || null;
    }
    return type;
};

Type.prototype.getDefaultValue = function#? DEBUG Type$getDefaultValue##() {
	return null;
};

Type.prototype.createInstance = function#? DEBUG Type$createInstance##() {
    return new this();
};

Type.parse = function#? DEBUG Type$parse##(typeName) {
    return Type.getType(typeName);
};
