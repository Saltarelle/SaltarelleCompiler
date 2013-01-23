///////////////////////////////////////////////////////////////////////////////
// Nullable

var ss_Nullable = function#? DEBUG Nullable$##() {
};

ss.registerClass(global, 'ss.Nullable', ss_Nullable);

ss_Nullable.unbox = function#? DEBUG Nullable$unbox##(instance) {
	if (!ss.isValue(instance))
		throw 'Instance is null';
	return instance;
};

ss_Nullable.eq = function#? DEBUG Nullable$eq##(a, b) {
	return !ss.isValue(a) ? !ss.isValue(b) : (a === b);
};

ss_Nullable.ne = function#? DEBUG Nullable$eq##(a, b) {
	return !ss.isValue(a) ? ss.isValue(b) : (a !== b);
};

ss_Nullable.le = function#? DEBUG Nullable$le##(a, b) {
	return ss.isValue(a) && ss.isValue(b) && a <= b;
};

ss_Nullable.ge = function#? DEBUG Nullable$ge##(a, b) {
	return ss.isValue(a) && ss.isValue(b) && a >= b;
};

ss_Nullable.lt = function#? DEBUG Nullable$lt##(a, b) {
	return ss.isValue(a) && ss.isValue(b) && a < b;
};

ss_Nullable.gt = function#? DEBUG Nullable$gt##(a, b) {
	return ss.isValue(a) && ss.isValue(b) && a > b;
};

ss_Nullable.sub = function#? DEBUG Nullable$sub##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a - b : null;
};

ss_Nullable.add = function#? DEBUG Nullable$add##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a + b : null;
};

ss_Nullable.mod = function#? DEBUG Nullable$mod##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a % b : null;
};

ss_Nullable.div = function#? DEBUG Nullable$divf##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a / b : null;
};

ss_Nullable.mul = function#? DEBUG Nullable$mul##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a * b : null;
};

ss_Nullable.band = function#? DEBUG Nullable$band##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a & b : null;
};

ss_Nullable.bor = function#? DEBUG Nullable$bor##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a | b : null;
};

ss_Nullable.xor = function#? DEBUG Nullable$xor##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a ^ b : null;
};

ss_Nullable.shl = function#? DEBUG Nullable$shl##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a << b : null;
};

ss_Nullable.srs = function#? DEBUG Nullable$srs##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a >> b : null;
};

ss_Nullable.sru = function#? DEBUG Nullable$sru##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? a >>> b : null;
};

ss_Nullable.and = function#? DEBUG Nullable$and##(a, b) {
	if (a === true && b === true)
		return true;
	else if (a === false || b === false)
		return false;
	else
		return null;
};

ss_Nullable.or = function#? DEBUG Nullable$or##(a, b) {
	if (a === true || b === true)
		return true;
	else if (a === false && b === false)
		return false;
	else
		return null;
};

ss_Nullable.not = function#? DEBUG Nullable$not##(a) {
	return ss.isValue(a) ? !a : null;
};

ss_Nullable.neg = function#? DEBUG Nullable$neg##(a) {
	return ss.isValue(a) ? -a : null;
};

ss_Nullable.pos = function#? DEBUG Nullable$pos##(a) {
	return ss.isValue(a) ? +a : null;
};

ss_Nullable.cpl = function#? DEBUG Nullable$cpl##(a) {
	return ss.isValue(a) ? ~a : null;
};
