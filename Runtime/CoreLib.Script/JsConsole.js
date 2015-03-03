// Polyfills for javascript console
(function (noop, undef) {
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
		if (typeof console.count !== 'function') {
			console.count = function count(label) {
				var cache;

				if (label === undef) {
					if (typeof count.undef === 'number') {
						console.log(': ' + (++count.undef));
					} else {
						console.log(': ' + (count.undef = 1));
					}
				} else {
					cache = count.cache || (count.cache = {});
					if (typeof cache[label] === 'number') {
						console.log(label + ': ' + (++cache[label]));
					} else {
						console.log(label + ': ' + (cache[label] = 1));
					}
				}
			}
		}

		// Emulate time/timeEnd (no sub-millisecond precision, but that is likely 
		// not possible on a browser that lacks these methods to begin with)
		if (typeof console.time !== 'function') {
			console.time = function time(label) {
				var cache;

				if (label !== undef) {
					cache = console.time.cache || (console.time.cache = {});
					if (typeof cache[label] !== 'number') {
						cache[label] = +new Date;
					}
				}
			}

			console.timeEnd = function timeEnd(label) {
				var cache;

				if (label !== undef) {
					cache = console.time.cache || (console.time.cache = {});
					if (typeof cache[label] === 'number') {
						console.log(label + ': ' + (new Date - cache[label]) + 'ms');
						delete cache[label];
					}
				}
			}
		}
	} else {
		// Make sure we have a console defined
		console = console || {};

		// If we have no console log, just make all the methods noops to prevent runtime errors
		polyfillMethods([
			'log', 'dir', 'info', 'warn', 'error', 'trace',
			'group', 'groupEnd', 'time', 'timeEnd', 'count'
		], noop);
	}
})(function () { });