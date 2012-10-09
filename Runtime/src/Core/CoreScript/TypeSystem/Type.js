///////////////////////////////////////////////////////////////////////////////
// Type System Implementation

global.Type = Function;

Type.registerNamespace = function#? DEBUG Type$registerNamespace##(name) {
    var root = (__isModule ? exports : global);
    if (!root.__namespaces) {
        root.__namespaces = {};
    }

    if (root.__namespaces[name]) {
        return;
    }

    var ns = root;
    var nameParts = name.split('.');

    for (var i = 0; i < nameParts.length; i++) {
        var part = nameParts[i];
        var nso = ns[part];
        if (!nso) {
            ns[part] = nso = {};
        }
        ns = nso;
    }

    root.__namespaces[name] = ns;
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
	instance.registerClass(name, baseType(), interfaceTypes());
};

Type.prototype.registerGenericInterfaceInstance = function#? DEBUG Type$registerGenericInstance##(instance, genericType, typeArguments, baseInterfaces) {
	var name = Type._makeGenericTypeName(genericType, typeArguments);
	Type.__genericCache[name] = instance;
	instance.__genericTypeDefinition = genericType;
	instance.__typeArguments = typeArguments;
	instance.registerInterface(name, baseInterfaces());
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

Type.prototype.registerClass = function#? DEBUG Type$registerClass##(name, baseType, interfaceType) {
    this.prototype.constructor = this;
    this.__typeName = name;
    this.__class = true;
    this.__baseType = baseType || Object;
    if (baseType) {
        this.setupBase(baseType);
    }

	if (interfaceType instanceof Array) {
		this.__interfaces = interfaceType;
	}
	else if (interfaceType) {
        this.__interfaces = [];
        for (var i = 2; i < arguments.length; i++) {
            interfaceType = arguments[i];
            this.__interfaces.add(interfaceType);
        }
    }
};

Type.prototype.registerGenericClass = function#? DEBUG Type$registerGenericClass##(name, typeArgumentCount) {
    this.prototype.constructor = this;
    this.__typeName = name;
    this.__class = true;
	this.__typeArgumentCount = typeArgumentCount;
	this.__isGenericTypeDefinition = true;
    this.__baseType = Object;
};

Type.prototype.registerInterface = function#? DEBUG Type$createInterface##(name, baseInterface) {
    this.__typeName = name;
    this.__interface = true;
	if (baseInterface instanceof Array) {
		this.__interfaces = baseInterface;
	}
	else if (baseInterface) {
        this.__interfaces = [];
        for (var i = 1; i < arguments.length; i++) {
            this.__interfaces.add(arguments[i]);
        }
    }
};

Type.prototype.registerGenericInterface = function#? DEBUG Type$registerGenericClass##(name, typeArgumentCount) {
    this.prototype.constructor = this;
    this.__typeName = name;
    this.__interface = true;;
	this.__typeArgumentCount = typeArgumentCount;
	this.__isGenericTypeDefinition = true;
};

Type.prototype.registerEnum = function#? DEBUG Type$createEnum##(name, flags) {
    for (var field in this.prototype) {
         this[field] = this.prototype[field];
    }

    this.__typeName = name;
    this.__enum = true;
    if (flags) {
        this.__flags = true;
    }
    this.getDefaultValue = this.createInstance = function() { return 0; };
    this.isInstanceOfType = function(instance) { return typeof(instance) == 'number'; };
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

Type.prototype.initializeBase = function#? DEBUG Type$initializeBase##(instance, args) {
    if (!args) {
        this.__baseType.apply(instance);
    }
    else {
        this.__baseType.apply(instance, args);
    }
};

Type.prototype.callBaseMethod = function#? DEBUG Type$callBaseMethod##(instance, name, args) {
    var baseMethod = this.__baseType.prototype[name];
    if (!args) {
        return baseMethod.apply(instance);
    }
    else {
        return baseMethod.apply(instance, args);
    }
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
        type = eval(typeName);
        Type.__typeCache[typeName] = type;
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
