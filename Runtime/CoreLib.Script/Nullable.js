///////////////////////////////////////////////////////////////////////////////
// Nullable

ss.unbox = function#? DEBUG ss$unbox##(instance) {
	if (!ss.isValue(instance))
		throw new ss_InvalidOperationException('Nullable object must have a value.');
	return instance;
};

var ss_Nullable$1 = ss.Nullable$1 = ss.mkType(ss, 'ss.Nullable$1',
	function#? DEBUG Nullable$1$##(T) {
		var $type = ss.registerGenericClassInstance(ss_Nullable$1, [T], null, {}, {
			isInstanceOfType: function(instance) {
				return ss.isInstanceOfType(instance, T);
			}
		});
		return $type;
	},
	null,
	{
		eq: function#? DEBUG Nullable$eq##(a, b) {
			return !ss.isValue(a) ? !ss.isValue(b) : (a === b);
		},
		ne: function#? DEBUG Nullable$eq##(a, b) {
			return !ss.isValue(a) ? ss.isValue(b) : (a !== b);
		},
		le: function#? DEBUG Nullable$le##(a, b) {
			return ss.isValue(a) && ss.isValue(b) && a <= b;
		},
		ge: function#? DEBUG Nullable$ge##(a, b) {
			return ss.isValue(a) && ss.isValue(b) && a >= b;
		},
		lt: function#? DEBUG Nullable$lt##(a, b) {
			return ss.isValue(a) && ss.isValue(b) && a < b;
		},
		gt: function#? DEBUG Nullable$gt##(a, b) {
			return ss.isValue(a) && ss.isValue(b) && a > b;
		},
		sub: function#? DEBUG Nullable$sub##(a, b) {
			return ss.isValue(a) && ss.isValue(b) ? a - b : null;
		},
		add: function#? DEBUG Nullable$add##(a, b) {
			return ss.isValue(a) && ss.isValue(b) ? a + b : null;
		},
		mod: function#? DEBUG Nullable$mod##(a, b) {
			return ss.isValue(a) && ss.isValue(b) ? a % b : null;
		},
		div: function#? DEBUG Nullable$divf##(a, b) {
			return ss.isValue(a) && ss.isValue(b) ? a / b : null;
		},
		mul: function#? DEBUG Nullable$mul##(a, b) {
			return ss.isValue(a) && ss.isValue(b) ? a * b : null;
		},
		band: function#? DEBUG Nullable$band##(a, b) {
			return ss.isValue(a) && ss.isValue(b) ? a & b : null;
		},
		bor: function#? DEBUG Nullable$bor##(a, b) {
			return ss.isValue(a) && ss.isValue(b) ? a | b : null;
		},
		xor: function#? DEBUG Nullable$xor##(a, b) {
			return ss.isValue(a) && ss.isValue(b) ? a ^ b : null;
		},
		shl: function#? DEBUG Nullable$shl##(a, b) {
			return ss.isValue(a) && ss.isValue(b) ? a << b : null;
		},
		srs: function#? DEBUG Nullable$srs##(a, b) {
			return ss.isValue(a) && ss.isValue(b) ? a >> b : null;
		},
		sru: function#? DEBUG Nullable$sru##(a, b) {
			return ss.isValue(a) && ss.isValue(b) ? a >>> b : null;
		},
		and: function#? DEBUG Nullable$and##(a, b) {
			if (a === true && b === true)
				return true;
			else if (a === false || b === false)
				return false;
			else
				return null;
		},
		or: function#? DEBUG Nullable$or##(a, b) {
			if (a === true || b === true)
				return true;
			else if (a === false && b === false)
				return false;
			else
				return null;
		},
		not: function#? DEBUG Nullable$not##(a) {
			return ss.isValue(a) ? !a : null;
		},
		neg: function#? DEBUG Nullable$neg##(a) {
			return ss.isValue(a) ? -a : null;
		},
		pos: function#? DEBUG Nullable$pos##(a) {
			return ss.isValue(a) ? +a : null;
		},
		cpl: function#? DEBUG Nullable$cpl##(a) {
			return ss.isValue(a) ? ~a : null;
		},
		lift: function#? DEBUG Nullable$lift##() {
			for (var i = 0; i < arguments.length; i++) {
				if (!ss.isValue(arguments[i]))
					return null;
			}
			return arguments[0].apply(null, Array.prototype.slice.call(arguments, 1));
		}
	}
);

ss.initGenericClass(ss_Nullable$1, 1);

