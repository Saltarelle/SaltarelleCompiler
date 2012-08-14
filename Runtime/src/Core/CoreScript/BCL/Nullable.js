///////////////////////////////////////////////////////////////////////////////
// Nullable

ss.Nullable = function#? DEBUG Nullable$##() {
};

ss.Nullable.registerClass('ss.Nullable');

ss.Nullable.unbox = function#? DEBUG Nullable$unbox##(instance) {
	if (!ss.isValue(instance))
		throw 'Instance is null';
	return instance;
}

ss.Nullable.eq = function#? DEBUG Nullable$eq##(a, b) {
	return !ss.isValue(a) ? !ss.isValue(b) : (a === b);
}

ss.Nullable.ne = function#? DEBUG Nullable$eq##(a, b) {
	return !ss.isValue(a) ? ss.isValue(b) : (a !== b);
}

ss.Nullable.le = function#? DEBUG Nullable$le##(a, b) {
	return ss.isValue(a) && ss.isValue(b) && a <= b;
}

ss.Nullable.ge = function#? DEBUG Nullable$ge##(a, b) {
	return ss.isValue(a) && ss.isValue(b) && a >= b;
}

ss.Nullable.lt = function#? DEBUG Nullable$lt##(a, b) {
	return ss.isValue(a) && ss.isValue(b) && a < b;
}

ss.Nullable.gt = function#? DEBUG Nullable$gt##(a, b) {
	return ss.isValue(a) && ss.isValue(b) && a > b;
}

ss.Nullable.sub = function#? DEBUG Nullable$sub##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a - b : null;
}

ss.Nullable.add = function#? DEBUG Nullable$add##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a + b : null;
}

ss.Nullable.mod = function#? DEBUG Nullable$mod##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a % b : null;
}

ss.Nullable.div = function#? DEBUG Nullable$divf##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a / b : null;
}

ss.Nullable.mul = function#? DEBUG Nullable$mul##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a * b : null;
}

ss.Nullable.band = function#? DEBUG Nullable$band##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a & b : null;
}

ss.Nullable.bor = function#? DEBUG Nullable$bor##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a | b : null;
}

ss.Nullable.xor = function#? DEBUG Nullable$xor##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a ^ b : null;
}

ss.Nullable.shl = function#? DEBUG Nullable$shl##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a << b : null;
}

ss.Nullable.srs = function#? DEBUG Nullable$srs##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a >> b : null;
}

ss.Nullable.sru = function#? DEBUG Nullable$sru##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a >>> b : null;
}

ss.Nullable.and = function#? DEBUG Nullable$and##(a, b) {
	if (a === true && b === true)
		return true;
	else if (a === false || b === false)
		return false;
	else
		return null;
}

ss.Nullable.or = function#? DEBUG Nullable$or##(a, b) {
	if (a === true || b === true)
		return true;
	else if (a === false && b === false)
		return false;
	else
		return null;
}

ss.Nullable.not = function#? DEBUG Nullable$not##(a) {
	return ss.isValue(a) ? !a : null;
}

ss.Nullable.neg = function#? DEBUG Nullable$neg##(a) {
	return ss.isValue(a) ? -a : null;
}

ss.Nullable.pos = function#? DEBUG Nullable$pos##(a) {
	return ss.isValue(a) ? +a : null;
}

ss.Nullable.cpl = function#? DEBUG Nullable$cpl##(a) {
	return ss.isValue(a) ? ~a : null;
}
