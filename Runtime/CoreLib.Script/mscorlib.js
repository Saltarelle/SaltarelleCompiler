//! Script# Core Runtime
//! More information at http://projects.nikhilk.net/ScriptSharp
//!
if (typeof(global) === "undefined")
	global = window;

var ss = {};

ss.isUndefined = function#? DEBUG ss$isUndefined##(o) {
	return (o === undefined);
};

ss.isNull = function#? DEBUG ss$isNull##(o) {
	return (o === null);
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
	else if (typeof(a.equals) === 'function')
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

ss.shallowCopy = function#? DEBUG ss$shallowCopy##(source, target) {
	for (var p in source) {
		if (source.hasOwnProperty(p))
			target[p] = source[p];
	}
};

if (typeof(window) == 'object') {
	// Browser-specific stuff that could go into the Web assembly, but that assembly does not have an associated JS file.
	if (!window.Element) {
		// IE does not have an Element constructor. This implementation should make casting to elements work.
		window.Element = function() {};
		window.Element.isInstanceOfType = function(instance) { return instance && typeof instance.constructor === 'undefined' && typeof instance.tagName === 'string'; };
	}
	window.Element.__typeName = 'Element';
	window.Element.__baseType = Object;
	
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

#include "TypeSystem.js"

#include "IFormattable.js"

#include "IComparable.js"

#include "IEquatable.js"

#include "Object.js"

#include "Number.js"

#include "String.js"

#include "Math.js"

#include "Array.js"

#include "Date.js"

#include "Function.js"

#include "Debug.js"

#include "Enum.js"

#include "CultureInfo.js"

#include "IEnumerator.js"

#include "IEnumerable.js"

#include "ICollection.js"

#include "TimeSpan.js"

#include "IEqualityComparer.js"

#include "IComparer.js"

#include "Nullable.js"

#include "IList.js"

#include "IDictionary.js"

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

#include "NotSupportedException.js"

#include "AggregateException.js"

#include "PromiseException.js"

#include "JsErrorException.js"

#include "ArgumentException.js"

#include "ArgumentNullException.js"

#include "ArgumentOutOfRangeException.js"

#include "FormatException.js"

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

if (global.ss) {
	for (var n in ss) {
		if (ss.hasOwnProperty(n))
			global.ss[n] = ss[n];
	}
}
else {
	global.ss = ss;
}
