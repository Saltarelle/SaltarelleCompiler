///////////////////////////////////////////////////////////////////////////////
// TimeSpan

var ss_TimeSpan = ss.TimeSpan = ss.mkType(ss, 'ss.TimeSpan',
	function#? DEBUG TimeSpan$##(ticks) {
		this.ticks = ticks || 0;
	},
	{
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
	}
);

ss.initStruct(ss_TimeSpan, [ss_IComparable, ss_IEquatable]);
