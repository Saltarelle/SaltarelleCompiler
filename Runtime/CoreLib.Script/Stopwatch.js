///////////////////////////////////////////////////////////////////////////////
// Stopwatch

var ss_Stopwatch = function Stopwatch$() {
  this.stopTime = 0;
  this.startTime = 0;
  this.isRunning = false;
}

ss_Stopwatch.prototype = {
    getTimestamp: function Stopwatch$getTimestamp() {
      return new Date().getTime() * ss_Stopwatch.ticksPerMillisecond;
  },

  reset: function Stopwatch$reset() {
    this.stopTime = this.startTime = this.get_elapsedTicks();
    this.isRunning = false;
  },

  get_elapsed: function Stopwatch$elapsed() {
    return new ss_TimeSpan(this.get_elapsedTicks());
  },

  get_elapsedTicks: function Stopwatch$elapsedTicks() {
    return this.isRunning ? (this.getTimestamp() - this.startTime) : (this.stopTime - this.startTime);
  },

  get_elapsedMilliseconds: function Stopwatch$elapsedMilliseconds() {
    return this.get_elapsed().ticks / ss_Stopwatch.ticksPerMillisecond;
  },

  start: function Stopwatch$start() {
    if (this.isRunning)
      return;
    this.startTime = this.getTimestamp();
    this.stopTime = this.startTime;
    this.isRunning = true;
  },

  stop: function Stopwatch$stop() {
    if (!this.isRunning)
      return;
    this.stopTime = this.getTimestamp();
    this.isRunning = false;
  },

  restart: function Stopwatch$restart() {
    this.isRunning = false;
    this.start();
  }
};

ss_Stopwatch.startNew = function Stopwatch$startNew() {
  var s = new ss_Stopwatch();
  s.start();
  return s;
};

ss_Stopwatch.ticksPerMillisecond = 10000;
ss_Stopwatch.frequency = 10000000; // Same as ss_TimeSpan.ticksPerSecond
ss_Stopwatch.isHighResolution = false;

ss_Stopwatch.__typeName = 'ss.Stopwatch';
ss.Stopwatch = ss_Stopwatch;
ss.initClass(ss_Stopwatch);
