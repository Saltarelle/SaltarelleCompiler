///////////////////////////////////////////////////////////////////////////////
// Math Extensions

ss.divRem = function#? DEBUG ss$divRem##(a, b, result) {
    var remainder = a % b;
    result.$ = remainder;
    return (a - remainder) / b;
};

ss.roundWithDigits = function#? DEBUG ss$roundWithDigits##(n, d) {
    var digitMultiplier = Math.pow(10, d);
    return Math.round(n * digitMultiplier) / digitMultiplier;
};

ss.roundWithDigitsAndMidpoint = function#? DEBUG ss$roundWithDigitsAndMidpoint##(n, d, rounding) {
	// http://phpjs.org/functions/round/
    d |= 0;
    var m = Math.pow(10, d);
    n *= m;
    var sign = (n > 0) | -(n < 0);
    var isHalf = n % 1 === 0.5 * sign;
    var f = Math.floor(n);

    if (isHalf) {
        switch (rounding) {
            case 1:
                // 'AwayFromZero': rounds .5 away from zero
                n = f + (sign > 0);
                break;
            case 0: 
                // 'ToEven': rouds .5 towards the next even integer
                n = f + (f % 2 * sign);
                break;
            default:
                // rounds .5 toward zero
                n = f + (sign < 0);
        }
    }

    return (isHalf ? n : Math.round(n)) / m;
};