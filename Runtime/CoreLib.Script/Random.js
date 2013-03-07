///////////////////////////////////////////////////////////////////////////////
// Random

var ss_Random = function#? DEBUG Random$##(seed) {
	var _seed = (seed === undefined) ? parseInt(Date.now() % 2147483647) : parseInt(Math.abs(seed));
	this.inext = 0;
	this.inextp = 21;
	this.seedArray = new Array(56);
	for(var i = 0; i < 56; i++)
		this.seedArray[i] = 0;

	var _seed = 161803398 - _seed;
	this.seedArray[55] = _seed;
	var mk = 1;
	for (var i = 1; i < 55; i++) {
		var ii = (21 * i) % 55;
		this.seedArray[ii] = mk;
		mk = _seed - mk;
		if (mk < 0)
			mk += 2147483647;

		_seed = this.seedArray[ii];
	}
	for (var j = 1; j < 5; j++) {
		for (var k = 1; k < 56; k++) {
			this.seedArray[k] -= this.seedArray[1 + (k + 30) % 55];
			if (this.seedArray[k] < 0)
				this.seedArray[k] += 2147483647;
		}
	}
};

ss_Random.prototype = {
	next: function#? DEBUG Random$next##() {
		return this.sample() * 2147483647 | 0;
	},
	nextMax: function#? DEBUG Random$nextMax##(max) {
		return this.sample() * max | 0;
	},
	nextMinMax: function#? DEBUG Random$nextMinMax##(min, max) {
		return (this.sample() * (max - min) + min) | 0;
	},
	nextBytes: function#? DEBUG Random$nextBytes##(bytes) {
		for (var i = 0; i < bytes.length; i++)
			bytes[i] = (this.sample() * 255) | 0;
	},
	nextDouble: function#? DEBUG Random$nextDouble##() {
		return this.sample();
	},
	sample: function#? DEBUG Random$sample##() {
		if (++this.inext >= 56)
			this.inext = 1;
		if (++this.inextp >= 56)
			this.inextp = 1;

		var retVal =  this.seedArray[this.inext] - this.seedArray[this.inextp];

		if (retVal < 0)
			retVal += 2147483647;

		this.seedArray[this.inext] = retVal;

		return retVal * (1.0 / 2147483647);
	}
};

ss.registerClass(global, 'ss.Random', ss_Random);
