// Polyfills for javascript console
(function (noop, undef) {
	var console = global.console;

	// Takes an array of method console method names and assigns a polyfill implementation 
	// to any which are not natively present
	function polyfillMethods(methods, implementation) {
		var i = methods.length,
			m;

		while (m = methods[--i]) {
			if (typeof console[m] !== 'function') {
				console[m] = implementation;
			}
		}
	}

	// Polyfill console.log for Opera <10.5
	if (!console) {
		if (global.opera && global.opera.postError) {
			console = global.console = {};
			console.log = function () {
				// Make sure we join all args together as postError only accepts a single arg
				global.opera.postError(Array.prototype.join.call(arguments, '\t'));
			}
		}
	}

	if (console && console.log) {
		// All of the 'augmented logging' methods fall back to 'log'
		polyfillMethods(['dir', 'info', 'warn', 'error'], console.log);

		// Attempt to mimic trace by throwing and catching an error to get a stack trace
		if (typeof console.trace !== 'function') {
			console.trace = function() {
				try {
					throw new Error('console.trace()');
				} catch (ex) {
					if (ex && ex.stack) {
						console.log(ex.stack);
					} else {
						console.log('Unable to capture stack trace.');
					}
				}
			};
		}

		// We can't emulate group, so just make them noops
		polyfillMethods(['group', 'groupCollapsed', 'groupEnd'], noop);

		// Emulate count
		var countCache = {},
			countUndefinedCache;
		if (typeof console.count !== 'function') {
			console.count = function count(label) {
				if (label === undef) {
					if (typeof countUndefinedCache === 'number') {
						console.log(': ' + (++countUndefinedCache));
					} else {
						console.log(': ' + (countUndefinedCache = 1));
					}
				} else {
					if (typeof countCache[label] === 'number') {
						console.log(label + ': ' + (++countCache[label]));
					} else {
						console.log(label + ': ' + (countCache[label] = 1));
					}
				}
			}
		}

		// Emulate time/timeEnd (no sub-millisecond precision, but that is likely 
		// not possible on a browser that lacks these methods to begin with)
		var timeCache = {};
		if (typeof console.time !== 'function') {
			console.time = function time(label) {
				// console.time ignores calls with no arguments/undefined
				if (label !== undef) {
					if (typeof timeCache[label] !== 'number') {
						timeCache[label] = +new Date;
					}
				}
			}

			console.timeEnd = function timeEnd(label) {
				if (label !== undef) {
					if (typeof timeCache[label] === 'number') {
						console.log(label + ': ' + (new Date - timeCache[label]) + 'ms');
						delete timeCache[label];
					}
				}
			}
		}
	} else {
		// Make sure we have a console defined (not just a missing log method)
		console = global.console = console || {};

		// If we have no console log, just make all the methods noops to prevent runtime errors
		polyfillMethods([
			'log', 'dir', 'info', 'warn', 'error', 'trace',
			'group', 'groupEnd', 'groupCollapsed', 'time', 'timeEnd', 'count'
		], noop);
	}
})(function () { });