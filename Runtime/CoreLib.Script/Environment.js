///////////////////////////////////////////////////////////////////////////////
// Environment

var ss_Environment = function#? DEBUG Environment##(seed) {
}

ss_Environment.get_tickCount = function#? DEBUG Environment$get_tickCount##() {
	return (new Date().getTime()) * 10000;
};

ss.Environment = ss_Environment;
ss.initClass(ss_Environment);