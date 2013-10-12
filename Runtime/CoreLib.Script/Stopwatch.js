///////////////////////////////////////////////////////////////////////////////
// Stopwatch

var ss_Stopwatch = function Stopwatch$() {
	this._stopTime = 0;
	this._startTime = 0;
	this.isRunning = false;
};

ss_Stopwatch.startNew = function #? DEBUG Stopwatch$startNew##() {
	var s = new ss_Stopwatch();
	s.start();
	return s;
};

if (typeof(window) !== 'undefined' && window.performance && window.performance.now) {
	ss_Stopwatch.frequency = 1e6;
	ss_Stopwatch.isHighResolution = true;
	ss_Stopwatch.getTimestamp = function() { return Math.round(window.performance.now() * 1000); };
}
else if (typeof(process) !== 'undefined' && process.hrtime) {
	ss_Stopwatch.frequency = 1e9;
	ss_Stopwatch.isHighResolution = true;
	ss_Stopwatch.getTimestamp = function() { var hr = process.hrtime(); return hr[0] * 1e9 + hr[1]; };
}
else {
	ss_Stopwatch.frequency = 1e3;
	ss_Stopwatch.isHighResolution = false;
	ss_Stopwatch.getTimestamp = function() { return new Date().valueOf(); };
}

ss_Stopwatch.__typeName = 'ss.Stopwatch';
ss.Stopwatch = ss_Stopwatch;
ss.initClass(ss_Stopwatch, ss, {
	reset: function#? DEBUG Stopwatch$reset##() {
		this._stopTime = this._startTime = ss_Stopwatch.getTimestamp();
		this.isRunning = false;
	},

	ticks: function#? DEBUG Stopwatch$ticks##() {
		return (this.isRunning ? ss_Stopwatch.getTimestamp() : this._stopTime) - this._startTime;
	},

	milliseconds: function#? DEBUG Stopwatch$milliseconds##() {
		return Math.round(this.ticks() / ss_Stopwatch.frequency * 1000);
	},

	timeSpan: function#? DEBUG Stopwatch$timeSpan##() {
		return new ss_TimeSpan(this.milliseconds() * 10000);
	},

	start: function#? DEBUG Stopwatch$start##() {
		if (this.isRunning)
			return;
		this._startTime = ss_Stopwatch.getTimestamp();
		this.isRunning = true;
	},

	stop: function#? DEBUG Stopwatch$stop##() {
		if (!this.isRunning)
			return;
		this._stopTime = ss_Stopwatch.getTimestamp();
		this.isRunning = false;
	},

	restart: function#? DEBUG Stopwatch$restart##() {
		this.isRunning = false;
		this.start();
	}
});
