// SaltarelleCompiler Runtime (http://www.saltarelle-compiler.com)
// Modified version of Script# Core Runtime (http://projects.nikhilk.net/ScriptSharp)

if (typeof(global) === "undefined") {
	if (typeof(window) !== "undefined")
		global = window;
	else if (typeof(self) !== "undefined")
		global = self;
}
(function(global) {
"use strict";

var ss = { __assemblies: {} };

ss.initAssembly = function#? DEBUG assembly##(obj, name, res) {
	res = res || {};
	obj.name = name;
	obj.toString = function() { return this.name; };
	obj.__types = {};
	obj.getResourceNames = function() { return Object.keys(res); };
	obj.getResourceDataBase64 = function(name) { return res[name] || null; };
	obj.getResourceData = function(name) { var r = res[name]; return r ? ss.dec64(r) : null; };
	ss.__assemblies[name] = obj;
};
ss.initAssembly(ss, 'mscorlib');

ss.load = function#? DEBUG ss$load##(name) {
	return ss.__assemblies[name] || require(name);
};

var enc = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/', dec;
ss.enc64 = function#? Debug ss$enc64##(a, b) {
	var s = '', i;
	for (i = 0; i < a.length; i += 3) {
		var c1 = a[i], c2 = a[i+1], c3 = a[i+2];
		s += (b && i && !(i%57) ? '\n' : '') + enc[c1 >> 2] + enc[((c1 & 3) << 4) | (c2 >> 4)] + (i < a.length - 1 ? enc[((c2 & 15) << 2) | (c3 >> 6)] : '=') + (i < a.length - 2 ? enc[c3 & 63] : '=');
	}
	return s;
};

ss.dec64 = function#? Debug ss$dec64##(s) {
	s = s.replace(/\s/g, '');
	dec = dec || (function() { var o = {'=':-1}; for (var i = 0; i < 64; i++) o[enc[i]] = i; return o; })();
	var a = Array(Math.max(s.length * 3 / 4 - 2, 0)), i;
	for (i = 0; i < s.length; i += 4) {
		var j = i * 3 / 4, c1 = dec[s[i]], c2 = dec[s[i+1]], c3 = dec[s[i+2]], c4 = dec[s[i+3]];
		a[j] = (c1 << 2) | (c2 >> 4);
		if (c3 >= 0) a[j+1] = ((c2 & 15) << 4) | (c3 >> 2);
		if (c4 >= 0) a[j+2] = ((c3 & 3) << 6) | c4;
	}
	return a;
};

ss.getAssemblies = function#? DEBUG ss$getAssemblies##() {
	return Object.keys(ss.__assemblies).map(function(n) { return ss.__assemblies[n]; });
};

ss.isNullOrUndefined = function#? DEBUG ss$isNullOrUndefined##(o) {
	return (o === null) || (o === undefined);
};

ss.isValue = function#? DEBUG ss$isValue##(o) {
	return (o !== null) && (o !== undefined);
};

ss.referenceEquals = function#? DEBUG ss$referenceEquals##(a, b) {
	return ss.isValue(a) ? a === b : !ss.isValue(b);
};

ss.mkdict = function#? DEBUG ss$mkdict##() {
	var a = (arguments.length != 1 ? arguments : arguments[0]);
	var r = {};
	for (var i = 0; i < a.length; i += 2) {
		r[a[i]] = a[i + 1];
	}
	return r;
};

ss.clone = function#? DEBUG ss$clone##(t, o) {
	return o ? t.$clone(o) : o;
}

ss.coalesce = function#? DEBUG ss$coalesce##(a, b) {
	return ss.isValue(a) ? a : b;
};

ss.isDate = function#? DEBUG ss$isDate##(obj) {
	return Object.prototype.toString.call(obj) === '[object Date]';
};

ss.isArray = function#? DEBUG ss$isArray##(obj) {
	return Object.prototype.toString.call(obj) === '[object Array]';
};

ss.isTypedArrayType = function#? DEBUG ss$isTypedArrayType##(type) {
	return ['Float32Array', 'Float64Array', 'Int8Array', 'Int16Array', 'Int32Array', 'Uint8Array', 'Uint16Array', 'Uint32Array', 'Uint8ClampedArray'].indexOf(ss.getTypeFullName(type)) >= 0;
};

ss.isArrayOrTypedArray = function#? DEBUG ss$isArray##(obj) {
	return ss.isArray(obj) || ss.isTypedArrayType(ss.getInstanceType(obj));
};

ss.getHashCode = function#? DEBUG ss$getHashCode##(obj) {
	if (!ss.isValue(obj))
		throw new ss_NullReferenceException('Cannot get hash code of null');
	else if (typeof(obj.getHashCode) === 'function')
		return obj.getHashCode();
	else if (typeof(obj) === 'boolean') {
		return obj ? 1 : 0;
	}
	else if (typeof(obj) === 'number') {
		var s = obj.toExponential();
		s = s.substr(0, s.indexOf('e'));
		return parseInt(s.replace('.', ''), 10) & 0xffffffff;
	}
	else if (typeof(obj) === 'string') {
		var res = 0;
		for (var i = 0; i < obj.length; i++)
			res = (res * 31 + obj.charCodeAt(i)) & 0xffffffff;
		return res;
	}
	else if (ss.isDate(obj)) {
		return obj.valueOf() & 0xffffffff;
	}
	else {
		return ss.defaultHashCode(obj);
	}
};

ss.defaultHashCode = function#? DEBUG ss$defaultHashCode##(obj) {
	return obj.$__hashCode__ || (obj.$__hashCode__ = (Math.random() * 0x100000000) | 0);
};

ss.equals = function#? DEBUG ss$equals##(a, b) {
	if (!ss.isValue(a))
		throw new ss_NullReferenceException('Object is null');
	else if (a !== ss && typeof(a.equals) === 'function')
		return a.equals(b);
	if (ss.isDate(a) && ss.isDate(b))
		return a.valueOf() === b.valueOf();
	else if (typeof(a) === 'function' && typeof(b) === 'function')
		return ss.delegateEquals(a, b);
	else if (ss.isNullOrUndefined(a) && ss.isNullOrUndefined(b))
		return true;
	else
		return a === b;
};

ss.compare = function#? DEBUG ss$compare##(a, b) {
	if (!ss.isValue(a))
		throw new ss_NullReferenceException('Object is null');
	else if (typeof(a) === 'number' || typeof(a) === 'string' || typeof(a) === 'boolean')
		return a < b ? -1 : (a > b ? 1 : 0);
	else if (ss.isDate(a))
		return ss.compare(a.valueOf(), b.valueOf());
	else
		return a.compareTo(b);
};

ss.equalsT = function#? DEBUG ss$equalsT##(a, b) {
	if (!ss.isValue(a))
		throw new ss_NullReferenceException('Object is null');
	else if (typeof(a) === 'number' || typeof(a) === 'string' || typeof(a) === 'boolean')
		return a === b;
	else if (ss.isDate(a))
		return a.valueOf() === b.valueOf();
	else
		return a.equalsT(b);
};

ss.staticEquals = function#? DEBUG ss$staticEquals##(a, b) {
	if (!ss.isValue(a))
		return !ss.isValue(b);
	else
		return ss.isValue(b) ? ss.equals(a, b) : false;
};

ss.shallowCopy = (function() { try { var x = Object.getOwnPropertyDescriptor({ a: 0 }, 'a').value; return true; } catch(ex) { return false; }})() ?
function#? DEBUG ss$shallowCopy##(source, target) {
	var keys = Object.keys(source);
	for (var i = 0, l = keys.length; i < l; i++) {
		Object.defineProperty(target, keys[i], Object.getOwnPropertyDescriptor(source, keys[i]));
	}
} :
function#? DEBUG ss$shallowCopyCompat##(source, target) {
	var keys = Object.keys(source);
	for (var i = 0, l = keys.length; i < l; i++) {
		target[keys[i]] = source[keys[i]];
	}
};

ss.isLower = function#? DEBUG ss$isLower##(c) {
	var s = String.fromCharCode(c);
	return s === s.toLowerCase() && s !== s.toUpperCase();
};

ss.isUpper = function#? DEBUG ss$isUpper##(c) {
	var s = String.fromCharCode(c);
	return s !== s.toLowerCase() && s === s.toUpperCase();
};

if (typeof(window) == 'object') {
	// Browser-specific stuff that could go into the Web assembly, but that assembly does not have an associated JS file.
	if (!window.Element) {
		// IE does not have an Element constructor. This implementation should make casting to elements work.
		window.Element = function() {};
		window.Element.isInstanceOfType = function(instance) { return instance && typeof instance.constructor === 'undefined' && typeof instance.tagName === 'string'; };
	}
	window.Element.__typeName = 'Element';
	
	if (!window.XMLHttpRequest) {
		window.XMLHttpRequest = function() {
			var progIDs = [ 'Msxml2.XMLHTTP', 'Microsoft.XMLHTTP' ];
	
			for (var i = 0; i < progIDs.length; i++) {
				try {
					var xmlHttp = new ActiveXObject(progIDs[i]);
					return xmlHttp;
				}
				catch (ex) {
				}
			}
	
			return null;
		};
	}

	ss.parseXml = function(markup) {
		try {
			if (DOMParser) {
				var domParser = new DOMParser();
				return domParser.parseFromString(markup, 'text/xml');
			}
			else {
				var progIDs = [ 'Msxml2.DOMDocument.3.0', 'Msxml2.DOMDocument' ];

				for (var i = 0; i < progIDs.length; i++) {
					var xmlDOM = new ActiveXObject(progIDs[i]);
					xmlDOM.async = false;
					xmlDOM.loadXML(markup);
					xmlDOM.setProperty('SelectionLanguage', 'XPath');
					return xmlDOM;
				}
			}
		}
		catch (ex) {
		}

		return null;
	};
}

#include "Object.js"

#include "TypeSystem.js"

#include "IFormattable.js"

#include "IComparable.js"

#include "IEquatable.js"

#include "Number.js"

#include "String.js"

#include "Math.js"

#include "IFormatProvider.js"

#include "NumberFormatInfo.js"

#include "DateTimeFormatInfo.js"

#include "Stopwatch.js"

#include "Array.js"

#include "Date.js"

#include "Function.js"

#include "RegExp.js"

#include "Debug.js"

#include "Enum.js"

#include "CultureInfo.js"

#include "IEnumerator.js"

#include "IEnumerable.js"

#include "ICollection.js"

#include "IReadOnlyCollection.js"

#include "TimeSpan.js"

#include "IEqualityComparer.js"

#include "IComparer.js"

#include "Nullable.js"

#include "IList.js"

#include "IReadonlyList.js"

#include "IDictionary.js"

#include "IReadOnlyDictionary.js"

#include "Int32.js"

#include "JsDate.js"

#include "ArrayEnumerator.js"

#include "ObjectEnumerator.js"

#include "EqualityComparer.js"

#include "Comparer.js"

#include "Dictionary.js"

#include "IDisposable.js"

#include "StringBuilder.js"

#include "Random.js"

#include "EventArgs.js"

#include "Exception.js"

#include "NotImplementedException.js"

#include "NotSupportedException.js"

#include "AggregateException.js"

#include "PromiseException.js"

#include "JsErrorException.js"

#include "ArgumentException.js"

#include "ArgumentNullException.js"

#include "ArgumentOutOfRangeException.js"

#include "FormatException.js"

#include "DivideByZeroException.js"

#include "InvalidCastException.js"

#include "InvalidOperationException.js"

#include "NullReferenceException.js"

#include "KeyNotFoundException.js"

#include "AmbiguousMatchException.js"

#include "IteratorBlockEnumerable.js"

#include "IteratorBlockEnumerator.js"

#include "Lazy.js"

#include "Task.js"

#include "TaskCompletionSource.js"

#include "CancelEventArgs.js"

#include "Guid.js"

if (global.ss) {
	for (var n in ss) {
		if (ss.hasOwnProperty(n))
			global.ss[n] = ss[n];
	}
}
else {
	global.ss = ss;
}
})(global);
