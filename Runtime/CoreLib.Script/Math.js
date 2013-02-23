///////////////////////////////////////////////////////////////////////////////
// Math Extensions

ss.divRem = function#? DEBUG ss$divRem##(a, b, result) {
	var remainder = a % b;
	result.$ = remainder;
	return (a - remainder) / b;
};

ss.round = function#? DEBUG ss$round##(n, d, rounding) {
	var m = Math.pow(10, d || 0);
	n *= m;
	var sign = (n > 0) | -(n < 0);
	if (n % 1 === 0.5 * sign) {
		var f = Math.floor(n);
		return (f + (rounding ? (sign > 0) : (f % 2 * sign))) / m;
	}

	return Math.round(n) / m;
};
