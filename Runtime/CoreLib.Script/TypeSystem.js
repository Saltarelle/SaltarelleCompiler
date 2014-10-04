///////////////////////////////////////////////////////////////////////////////
// Type System Implementation

ss.__genericCache = {};

ss._makeGenericTypeName = function#? DEBUG ss$_makeGenericTypeName##(genericType, typeArguments) {
	var result = genericType.__typeName;
	for (var i = 0; i < typeArguments.length; i++)
		result += (i === 0 ? '[' : ',') + '[' + ss.getTypeQName(typeArguments[i]) + ']';
	result += ']';
	return result;
};

ss.makeGenericType = function#? DEBUG ss$makeGenericType##(genericType, typeArguments) {
	var name = ss._makeGenericTypeName(genericType, typeArguments);
	return ss.__genericCache[name] || genericType.apply(null, typeArguments);
};

ss.registerGenericClassInstance = function#? DEBUG ss$registerGenericClassInstance##(instance, genericType, typeArguments, members, baseType, interfaceTypes) {
	var name = ss._makeGenericTypeName(genericType, typeArguments);
	ss.__genericCache[name] = instance;
	instance.__typeName = name;
	instance.__genericTypeDefinition = genericType;
	instance.__typeArguments = typeArguments;
	ss.initClass(instance, genericType.__assembly, members, baseType(), interfaceTypes());
};

ss.registerGenericInterfaceInstance = function#? DEBUG ss$registerGenericInterfaceInstance##(instance, genericType, typeArguments, members, baseInterfaces) {
	var name = ss._makeGenericTypeName(genericType, typeArguments);
	ss.__genericCache[name] = instance;
	instance.__typeName = name;
	instance.__genericTypeDefinition = genericType;
	instance.__typeArguments = typeArguments;
	ss.initInterface(instance, genericType.__assembly, members, baseInterfaces());
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

ss.__anonymousCache = {};
ss.anonymousType = function#? DEBUG ss$anonymousType##() {
	var members = Array.prototype.slice.call(arguments);
	var name = 'Anonymous<' + members.map(function(m) { return m[1] + ':' + ss.getTypeFullName(m[0]); }).join(',') + '>';
	var type = ss.__anonymousCache[name];
	if (!type) {
		var type = new Function(members.map(function(m) { return m[1]; }).join(','), members.map(function(m) { return 'this.' + m[1] + '=' + m[1] + ';'; }).join(''));
		type.__typeName = name;
		var infos = members.map(function(m) { return { name: m[1], typeDef: type, type: 16, returnType: m[0], getter: { name: 'get_' + m[1], typeDef: type, params: [], returnType: m[0], fget: m[1] } }; });
		infos.push({ name: '.ctor', typeDef: type, type: 1, params: members.map(function(m) { return m[0]; }) });
		type.__metadata = { members: infos };
		ss.__anonymousCache[name] = type;
	}
	return type;
}

ss.setMetadata = function#? DEBUG ss$_setMetadata##(type, metadata) {
	if (metadata.members) {
		for (var i = 0; i < metadata.members.length; i++) {
			var m = metadata.members[i];
			m.typeDef = type;
			if (m.adder) m.adder.typeDef = type;
			if (m.remover) m.remover.typeDef = type;
			if (m.getter) m.getter.typeDef = type;
			if (m.setter) m.setter.typeDef = type;
		}
	}
	type.__metadata = metadata;
	if (metadata.variance) {
		type.isAssignableFrom = function(source) {
			var check = function(target, type) {
				if (type.__genericTypeDefinition === target.__genericTypeDefinition && type.__typeArguments.length == target.__typeArguments.length) {
					for (var i = 0; i < target.__typeArguments.length; i++) {
						var v = target.__metadata.variance[i], t = target.__typeArguments[i], s = type.__typeArguments[i];
						switch (v) {
							case 1: if (!ss.isAssignableFrom(t, s)) return false; break;
							case 2: if (!ss.isAssignableFrom(s, t)) return false; break;
							default: if (s !== t) return false;
						}
					}
					return true;
				}
				return false;
			};

			if (source.__interface && check(this, source))
				return true;
			var ifs = ss.getInterfaces(source);
			for (var i = 0; i < ifs.length; i++) {
				if (ifs[i] === this || check(this, ifs[i]))
					return true;
			}
			return false;
		};
	}
}

ss.initClass = function#? DEBUG ss$initClass##(ctor, asm, members, baseType, interfaces) {
	ctor.__class = true;
	ctor.__assembly = asm;
	if (!ctor.__typeArguments)
		asm.__types[ctor.__typeName] = ctor;
	if (baseType && baseType !== Object) {
		var f = function(){};
		f.prototype = baseType.prototype;
		ctor.prototype = new f();
		ctor.prototype.constructor = ctor;
	}
	ss.shallowCopy(members, ctor.prototype);
	if (interfaces)
		ctor.__interfaces = interfaces;
};

ss.initGenericClass = function#? DEBUG ss$initGenericClass##(ctor, asm, typeArgumentCount) {
	ctor.__class = true;
	ctor.__assembly = asm;
	asm.__types[ctor.__typeName] = ctor;
	ctor.__typeArgumentCount = typeArgumentCount;
	ctor.__isGenericTypeDefinition = true;
};

ss.initInterface = function#? DEBUG ss$initInterface##(ctor, asm, members, baseInterfaces) {
	ctor.__interface = true;
	ctor.__assembly = asm;
	if (!ctor.__typeArguments)
		asm.__types[ctor.__typeName] = ctor;
	if (baseInterfaces)
		ctor.__interfaces = baseInterfaces;
	ss.shallowCopy(members, ctor.prototype);
	ctor.isAssignableFrom = function(type) { return ss.contains(ss.getInterfaces(type), this); };
};

ss.initGenericInterface = function#? DEBUG ss$initGenericClass##(ctor, asm, typeArgumentCount) {
	ctor.__interface = true;
	ctor.__assembly = asm;
	asm.__types[ctor.__typeName] = ctor;
	ctor.__typeArgumentCount = typeArgumentCount;
	ctor.__isGenericTypeDefinition = true;
};

ss.initEnum = function#? DEBUG ss$initEnum##(ctor, asm, members, namedValues) {
	ctor.__enum = true;
	ctor.__assembly = asm;
	asm.__types[ctor.__typeName] = ctor;
	ss.shallowCopy(members, ctor.prototype);
	ctor.getDefaultValue = ctor.createInstance = function() { return namedValues ? null : 0; };
	ctor.isInstanceOfType = function(instance) { return typeof(instance) == (namedValues ? 'string' : 'number'); };
};

ss.getBaseType = function#? DEBUG ss$getBaseType##(type) {
	if (type === Object || type.__interface) {
		return null;
	}
	else if (Object.getPrototypeOf) {
		return Object.getPrototypeOf(type.prototype).constructor;
	}
	else {
		var p = type.prototype;
		if (Object.prototype.hasOwnProperty.call(p, 'constructor')) {
			try {
				var ownValue = p.constructor;
				delete p.constructor;
				return p.constructor;
			}
			finally {
				p.constructor = ownValue;
			}
		}
		return p.constructor;
	}
};

ss.getTypeFullName = function#? DEBUG ss$getTypeFullName##(type) {
	return type.__typeName || type.name || (type.toString().match(/^\s*function\s*([^\s(]+)/) || [])[1] || 'Object';
};

ss.getTypeQName = function#? DEBUG ss$getTypeFullName##(type) {
	return ss.getTypeFullName(type) + (type.__assembly ? ', ' + type.__assembly.name : '');
};

ss.getTypeName = function#? DEBUG ss$getTypeName##(type) {
	var fullName = ss.getTypeFullName(type);
	var bIndex = fullName.indexOf('[');
	var nsIndex = fullName.lastIndexOf('.', bIndex >= 0 ? bIndex : fullName.length);
	return nsIndex > 0 ? fullName.substr(nsIndex + 1) : fullName;
};

ss.getTypeNamespace = function#? DEBUG ss$getTypeNamespace##(type) {
	var fullName = ss.getTypeFullName(type);
	var bIndex = fullName.indexOf('[');
	var nsIndex = fullName.lastIndexOf('.', bIndex >= 0 ? bIndex : fullName.length);
	return nsIndex > 0 ? fullName.substr(0, nsIndex) : '';
};

ss.getTypeAssembly = function#? DEBUG ss$getTypeAssembly##(type) {
	if (ss.contains([Date, Number, Boolean, String, Function, Array], type))
		return ss;
	else
		return type.__assembly || null;
};

ss.getAssemblyType = function#? DEBUG ss$getAssemblyTypes##(asm, name) {
	var result = [];
	if (asm.__types) {
		return asm.__types[name] || null;
	}
	else {
		var a = name.split('.');
		for (var i = 0; i < a.length; i++) {
			asm = asm[a[i]];
			if (!ss.isValue(asm))
				return null;
		}
		if (typeof asm !== 'function')
			return null;
		return asm;
	}
};

ss.getAssemblyTypes = function#? DEBUG ss$getAssemblyTypes##(asm) {
	var result = [];
	if (asm.__types) {
		for (var t in asm.__types) {
			if (asm.__types.hasOwnProperty(t))
				result.push(asm.__types[t]);
		}
	}
	else {
		var traverse = function(s, n) {
			for (var c in s) {
				if (s.hasOwnProperty(c))
					traverse(s[c], c);
			}
			if (typeof(s) === 'function' && ss.isUpper(n.charCodeAt(0)))
				result.push(s);
		};
		traverse(asm, '');
	}
	return result;
};

ss.createAssemblyInstance = function#? DEBUG ss$createAssemblyInstance##(asm, typeName) {
	var t = ss.getAssemblyType(asm, typeName);
	return t ? ss.createInstance(t) : null;
};

ss.getInterfaces = function#? DEBUG ss$getInterfaces##(type) {
	if (type.__interfaces)
		return type.__interfaces;
	else if (type === Date || type === Number)
		return [ ss_IEquatable, ss_IComparable, ss_IFormattable ];
	else if (type === Boolean || type === String)
		return [ ss_IEquatable, ss_IComparable ];
	else if (type === Array || ss.isTypedArrayType(type))
		return [ ss_IEnumerable, ss_ICollection, ss_IList, ss_IReadOnlyCollection, ss_IReadOnlyList ];
	else
		return [];
};

ss.isInstanceOfType = function#? DEBUG ss$isInstanceOfType##(instance, type) {
	if (ss.isNullOrUndefined(instance))
		return false;

	if (typeof(type.isInstanceOfType) === 'function')
		return type.isInstanceOfType(instance);

	return ss.isAssignableFrom(type, ss.getInstanceType(instance));
};

ss.isAssignableFrom = function#? DEBUG ss$isAssignableFrom##(target, type) {
	return target === type || (typeof(target.isAssignableFrom) === 'function' && target.isAssignableFrom(type)) || type.prototype instanceof target;
};

ss.isClass = function#? DEBUG Type$isClass##(type) {
	return (type.__class == true || type === Array || type === Function || type === RegExp || type === String || type === Error || type === Object);
};

ss.isEnum = function#? DEBUG Type$isEnum##(type) {
	return !!type.__enum;
};

ss.isFlags = function#? DEBUG Type$isFlags##(type) {
	return type.__metadata && type.__metadata.enumFlags || false;
};

ss.isInterface = function#? DEBUG Type$isInterface##(type) {
	return !!type.__interface;
};

ss.safeCast = function#? DEBUG ss$safeCast##(instance, type) {
	if (type === true)
		return instance;
	else if (type === false)
		return null;
	else
		return ss.isInstanceOfType(instance, type) ? instance : null;
};

ss.cast = function#? DEBUG ss$cast##(instance, type) {
	if (instance === null || typeof(instance) === 'undefined')
		return instance;
	else if (type === true || (type !== false && ss.isInstanceOfType(instance, type)))
		return instance;
	throw new ss_InvalidCastException('Cannot cast object to type ' + ss.getTypeFullName(type));
};

ss.getInstanceType = function#? DEBUG ss$getInstanceType##(instance) {
	if (!ss.isValue(instance))
		throw new ss_NullReferenceException('Cannot get type of null');

	// NOTE: We have to catch exceptions because the constructor
	//       cannot be looked up on native COM objects
	try {
		return instance.constructor;
	}
	catch (ex) {
		return Object;
	}
};

ss.getType = function#? DEBUG ss$getType##(typeName) {
	if (!typeName)
		return null;

	var arr = typeName.split(',');
	var module = (arr.length > 1 ? ss.__assemblies[arr[1].trim()] : global);
	return module ? ss.getAssemblyType(module, arr[0].trim()) : null;
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

ss.applyConstructor = function#? DEBUG ss$applyConstructor##(constructor, args) {
	var f = function() {
		constructor.apply(this, args);
	};
	f.prototype = constructor.prototype;
	return new f();
};

ss.getAttributes = function#? DEBUG ss$getAttributes##(type, attrType, inherit) {
	var result = [];
	if (inherit) {
		var b = ss.getBaseType(type);
		if (b) {
			var a = ss.getAttributes(b, attrType, true);
			for (var i = 0; i < a.length; i++) {
				var t = ss.getInstanceType(a[i]);
				if (!t.__metadata || !t.__metadata.attrNoInherit)
					result.push(a[i]);
			}
		}
	}
	if (type.__metadata && type.__metadata.attr) {
		for (var i = 0; i < type.__metadata.attr.length; i++) {
			var a = type.__metadata.attr[i];
			if (attrType == null || ss.isInstanceOfType(a, attrType)) {
				var t = ss.getInstanceType(a);
				if (!t.__metadata || !t.__metadata.attrAllowMultiple) {
					for (var j = result.length - 1; j >= 0; j--) {
						if (ss.isInstanceOfType(result[j], t))
							result.splice(j, 1);
					}
				}
				result.push(a);
			}
		}
	}
	return result;
};

ss.getMembers = function#? DEBUG ss$getMembers##(type, memberTypes, bindingAttr, name, params) {
	var result = [];
	if ((bindingAttr & 72) == 72 || (bindingAttr & 6) == 4) {
		var b = ss.getBaseType(type);
		if (b)
			result = ss.getMembers(b, memberTypes & ~1, bindingAttr & (bindingAttr & 64 ? 255 : 247) & (bindingAttr & 2 ? 251 : 255), name, params);
	}

	var f = function(m) {
		if ((memberTypes & m.type) && (((bindingAttr & 4) && !m.isStatic) || ((bindingAttr & 8) && m.isStatic)) && (!name || m.name === name)) {
			if (params) {
				if ((m.params || []).length !== params.length)
					return;
				for (var i = 0; i < params.length; i++) {
					if (params[i] !== m.params[i])
						return;
				}
			}
			result.push(m);
		}
	};

	if (type.__metadata && type.__metadata.members) {
		for (var i = 0; i < type.__metadata.members.length; i++) {
			var m = type.__metadata.members[i];
			f(m);
			for (var j = 0; j < 4; j++) {
				var a = ['getter','setter','adder','remover'][j];
				if (m[a])
					f(m[a]);
			}
		}
	}

	if (bindingAttr & 256) {
		while (type) {
			var r = [];
			for (var i = 0; i < result.length; i++) {
				if (result[i].typeDef === type)
					r.push(result[i]);
			}
			if (r.length > 1)
				throw new ss_AmbiguousMatchException('Ambiguous match');
			else if (r.length === 1)
				return r[0];
			type = ss.getBaseType(type);
		}
		return null;
	}

	return result;
};

ss.midel = function#? DEBUG ss$midel##(mi, target, typeArguments) {
	if (mi.isStatic && !!target)
		throw new ss_ArgumentException('Cannot specify target for static method');
	else if (!mi.isStatic && !target)
		throw new ss_ArgumentException('Must specify target for instance method');

	var method;
	if (mi.fget) {
		method = function() { return (mi.isStatic ? mi.typeDef : this)[mi.fget]; };
	}
	else if (mi.fset) {
		method = function(v) { (mi.isStatic ? mi.typeDef : this)[mi.fset] = v; };
	}
	else {
		method = mi.def || (mi.isStatic || mi.sm ? mi.typeDef[mi.sname] : target[mi.sname]);

		if (mi.tpcount) {
			if (!typeArguments || typeArguments.length !== mi.tpcount)
				throw new ss_ArgumentException('Wrong number of type arguments');
			method = method.apply(null, typeArguments);
		}
		else {
			if (typeArguments && typeArguments.length)
				throw new ss_ArgumentException('Cannot specify type arguments for non-generic method');
		}
		if (mi.exp) {
			var _m1 = method;
			method = function () { return _m1.apply(this, Array.prototype.slice.call(arguments, 0, arguments.length - 1).concat(arguments[arguments.length - 1])); };
		}
		if (mi.sm) {
			var _m2 = method;
			method = function() { return _m2.apply(null, [this].concat(Array.prototype.slice.call(arguments))); };
		}
	}
	return ss.mkdel(target, method);
};

ss.invokeCI = function#? DEBUG ss$invokeCI##(ci, args) {
	if (ci.exp)
		args = args.slice(0, args.length - 1).concat(args[args.length - 1]);

	if (ci.def)
		return ci.def.apply(null, args);
	else if (ci.sm)
		return ci.typeDef[ci.sname].apply(null, args);
	else
		return ss.applyConstructor(ci.sname ? ci.typeDef[ci.sname] : ci.typeDef, args);
};

ss.fieldAccess = function#? DEBUG ss$fieldAccess##(fi, obj) {
	if (fi.isStatic && !!obj)
		throw new ss_ArgumentException('Cannot specify target for static field');
	else if (!fi.isStatic && !obj)
		throw new ss_ArgumentException('Must specify target for instance field');
	obj = fi.isStatic ? fi.typeDef : obj;
	if (arguments.length === 3)
		obj[fi.sname] = arguments[2];
	else
		return obj[fi.sname];
};
