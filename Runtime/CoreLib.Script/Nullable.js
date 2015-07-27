///////////////////////////////////////////////////////////////////////////////
// Nullable

ss.unbox = function#? DEBUG ss$unbox##(instance) {
	if (!ss.isValue(instance))
		throw new ss_InvalidOperationException('Nullable object must have a value.');
	return instance;
};

var ss_Nullable$1 = function#? DEBUG Nullable$1$##(T) {
	var $type = function() {
	};
	$type.isInstanceOfType = function(instance) {
		return ss.isInstanceOfType(instance, T);
	};
	ss.registerGenericClassInstance($type, ss_Nullable$1, [T], {}, function() { return null; }, function() { return []; });
	return $type;
};

ss_Nullable$1.__typeName = 'ss.Nullable$1';
ss.Nullable$1 = ss_Nullable$1;
ss.initGenericClass(ss_Nullable$1, ss, 1);

ss_Nullable$1.eq = function#? DEBUG Nullable$eq##(a, b) {
	return !ss.isValue(a) ? !ss.isValue(b) : (a === b);
};

ss_Nullable$1.ne = function#? DEBUG Nullable$eq##(a, b) {
	return !ss.isValue(a) ? ss.isValue(b) : (a !== b);
};

ss_Nullable$1.le = function#? DEBUG Nullable$le##(a, b) {
	return ss.isValue(a) && ss.isValue(b) && a <= b;
};

ss_Nullable$1.ge = function#? DEBUG Nullable$ge##(a, b) {
	return ss.isValue(a) && ss.isValue(b) && a >= b;
};

ss_Nullable$1.lt = function#? DEBUG Nullable$lt##(a, b) {
	return ss.isValue(a) && ss.isValue(b) && a < b;
};

ss_Nullable$1.gt = function#? DEBUG Nullable$gt##(a, b) {
	return ss.isValue(a) && ss.isValue(b) && a > b;
};

ss_Nullable$1.sub = function#? DEBUG Nullable$sub##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a - b : null;
};

ss_Nullable$1.add = function#? DEBUG Nullable$add##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a + b : null;
};

ss_Nullable$1.mod = function#? DEBUG Nullable$mod##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a % b : null;
};

ss_Nullable$1.div = function#? DEBUG Nullable$divf##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a / b : null;
};

ss_Nullable$1.mul = function#? DEBUG Nullable$mul##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a * b : null;
};

ss_Nullable$1.band = function#? DEBUG Nullable$band##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a & b : null;
};

ss_Nullable$1.bor = function#? DEBUG Nullable$bor##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a | b : null;
};

ss_Nullable$1.bxor = function#? DEBUG Nullable$xor##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a ^ b : null;
};

ss_Nullable$1.shl = function#? DEBUG Nullable$shl##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a << b : null;
};

ss_Nullable$1.srs = function#? DEBUG Nullable$srs##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a >> b : null;
};

ss_Nullable$1.sru = function#? DEBUG Nullable$sru##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a >>> b : null;
};

ss_Nullable$1.and = function#? DEBUG Nullable$and##(a, b) {
	if (a === true && b === true)
		return true;
	else if (a === false || b === false)
		return false;
	else
		return null;
};

ss_Nullable$1.or = function#? DEBUG Nullable$or##(a, b) {
	if (a === true || b === true)
		return true;
	else if (a === false && b === false)
		return false;
	else
		return null;
};

ss_Nullable$1.xor = function#? DEBUG Nullable$xor##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? !!(a ^ b) : null;
};

ss_Nullable$1.not = function#? DEBUG Nullable$not##(a) {
	return ss.isValue(a) ? !a : null;
};

ss_Nullable$1.neg = function#? DEBUG Nullable$neg##(a) {
	return ss.isValue(a) ? -a : null;
};

ss_Nullable$1.pos = function#? DEBUG Nullable$pos##(a) {
	return ss.isValue(a) ? +a : null;
};

ss_Nullable$1.cpl = function#? DEBUG Nullable$cpl##(a) {
	return ss.isValue(a) ? ~a : null;
};

ss_Nullable$1.lift1 = function#? DEBUG Nullable$lift1##(f, o) {
	return ss.isValue(o) ? f(o) : null;
};

ss_Nullable$1.lift2 = function#? DEBUG Nullable$lift2##(f, a, b) {
	return ss.isValue(a) && ss.isValue(b) ? f(a, b) : null;
};

ss_Nullable$1.liftcmp = function#? DEBUG Nullable$liftcmp##(f, a, b) {
	return ss.isValue(a) && ss.isValue(b) ? f(a, b) : false;
};

ss_Nullable$1.lifteq = function#? DEBUG Nullable$lifteq##(f, a, b) {
	var va = ss.isValue(a), vb = ss.isValue(b);
	return (!va && !vb) || (va && vb && f(a, b));
};

ss_Nullable$1.liftne = function#? DEBUG Nullable$liftne##(f, a, b) {
	var va = ss.isValue(a), vb = ss.isValue(b);
	return (va !== vb) || (va && f(a, b));
};
