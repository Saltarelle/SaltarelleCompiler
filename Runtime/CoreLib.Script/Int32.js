///////////////////////////////////////////////////////////////////////////////
// Int32
var intRE = /^\s*[+-]?[0-9]+\s*$/;

var defInt = function(name, min, max) {
	var type = ss[name] = ss.mkType(ss, 'ss.' + name,
		function() {
		},
		null,
		{
			isInstanceOfType: function(instance) {
				return typeof(instance) === 'number' && Math.round(instance, 0) == instance && instance >= min && instance <= max;
			},
			createInstance: function() {
				return 0;
			},
			parse: function(s) {
				var r = {};
				if (!type.tryParse(s, r))
					throw ss.isValue(s) ? intRE.test(s) ? new ss_OverflowException() : new ss_FormatException() : new ss_ArgumentNullException('s');
				return r.$;
			},
			tryParse: function(s, result) {
				result.$ = 0;
				if (!intRE.test(s))
					return false;
				var n = parseInt(s, 10);
				if (n < min || n > max)
					return false;
				result.$ = n;
				return true;
			}
		}
	);
	ss.initStruct(type, [ ss_IEquatable, ss_IComparable, ss_IFormattable ]);
	return type;
};

var ss_Byte = defInt('Byte', 0, 255);
var ss_SByte = defInt('SByte', -128, 127);
var ss_Int16 = defInt('Int16', -32768, 32767);
var ss_UInt16 = defInt('UInt16', 0, 65535);
var ss_Int32 = defInt('Int32', -2147483648, 2147483647);
var ss_UInt32 = defInt('UInt32', 0, 4294967295);
var ss_Int64 = defInt('Int64', -9223372036854775808, 9223372036854775807);
var ss_UInt64 = defInt('UInt64', 0, 18446744073709551615);
var ss_Char = defInt('Char', 0, 65535);

ss_Char.tryParse = function#? DEBUG Char$tryParse##(s, result) {
	var b = s && s.length === 1;
	result.$ = b ? s.charCodeAt(0) : 0;
	return b;
};

ss_Char.parse = function#? DEBUG Char$parse##(s, result) {
	if (!ss.isValue(s))
		throw new ss_ArgumentNullException('s');
	if (s.length !== 1)
		throw new ss_FormatException();
	return s.charCodeAt(0);
};

ss.sxb = function#? DEBUG ss$sxb##(x) {
	return x | (x & 0x80 ? 0xffffff00 : 0);
};

ss.sxs = function#? DEBUG ss$sxs##(x) {
	return x | (x & 0x8000 ? 0xffff0000 : 0);
};

ss.clip8 = function#? DEBUG ss$clip8##(x) {
	return ss.isValue(x) ? ss.sxb(x & 0xff) : null;
};

ss.clipu8 = function#? DEBUG ss$clipu8##(x) {
	return ss.isValue(x) ? x & 0xff : null;
};

ss.clip16 = function#? DEBUG ss$clip16##(x) {
	return ss.isValue(x) ? ss.sxs(x & 0xffff) : null;
};

ss.clipu16 = function#? DEBUG ss$clipu16##(x) {
	return ss.isValue(x) ? x & 0xffff : null;
};

ss.clip32 = function#? DEBUG ss$clip32##(x) {
	return ss.isValue(x) ? x | 0 : null;
};

ss.clipu32 = function#? DEBUG ss$clipu32##(x) {
	return ss.isValue(x) ? x >>> 0 : null;
};

ss.clip64 = function#? DEBUG ss$clip64##(x) {
	return ss.isValue(x) ? (Math.floor(x / 0x100000000) | 0) * 0x100000000 + (x >>> 0) : null;
};

ss.clipu64 = function#? DEBUG ss$clipu64##(x) {
	return ss.isValue(x) ? (Math.floor(x / 0x100000000) >>> 0) * 0x100000000 + (x >>> 0) : null;
};

ss.ck = function#? DEBUG ss$ck##(x, tp) {
	if (ss.isValue(x) && !tp.isInstanceOfType(x))
		throw new ss_OverflowException();
	return x;
};

ss.trunc = function#? DEBUG ss$trunc##(n) {
	return ss.isValue(n) ? (n > 0 ? Math.floor(n) : Math.ceil(n)) : null;
};

ss.idiv = function#? DEBUG ss$idiv##(a, b) {
	if (!ss.isValue(a) || !ss.isValue(b)) return null;
	if (!b) throw new ss_DivideByZeroException();
	return ss.trunc(a / b);
};
