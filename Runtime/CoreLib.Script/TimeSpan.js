///////////////////////////////////////////////////////////////////////////////
// TimeSpan

var ss_TimeSpan = function#? DEBUG TimeSpan$##(ticks) {
	this.ticks = ticks || 0;
};

ss_TimeSpan.getDefaultValue = ss_TimeSpan.createInstance = function#? DEBUG TimeSpan$default##() {
	return new ss_TimeSpan(0);
};

ss_TimeSpan.__typeName = 'ss.TimeSpan';
ss.TimeSpan = ss_TimeSpan;
ss.initClass(ss_TimeSpan, ss, {
	compareTo: function#? DEBUG TimeSpan$compareTo##(other) {
		return this.ticks < other.ticks ? -1 : (this.ticks > other.ticks ? 1 : 0);
	},
	equals: function#? DEBUG TimeSpan$equals##(other) {
		return ss.isInstanceOfType(other, ss_TimeSpan) && other.ticks === this.ticks;
	},
	equalsT: function#? DEBUG TimeSpan$equalsT##(other) {
		return other.ticks === this.ticks;
	},
	toString: function#? DEBUG TimeSpan$toString##() {
		var d = function(s, n) { return ss.padLeftString(s + '', n || 2, 48); };

		var ticks = this.ticks;
		var result = '';
		if (Math.abs(ticks) >= 864000000000) {
			result += d((ticks / 864000000000) | 0) + '.';
			ticks %= 864000000000;
		}
		result += d(ticks / 36000000000 | 0) + ':';
		ticks %= 36000000000;
		result += d(ticks / 600000000 | 0) + ':';
		ticks %= 600000000;
		result += d(ticks / 10000000 | 0);
		ticks %= 10000000;
		if (ticks > 0)
			result += '.' + d(ticks, 7);
		return result;
	}
}, null, [ss_IComparable, ss_IEquatable]);
ss_TimeSpan.__class = false;
