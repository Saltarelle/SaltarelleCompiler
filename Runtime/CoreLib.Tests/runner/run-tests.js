var testrunner = require('qunit');
testrunner.setup({
	log: {
		json: true
	}
});
testrunner.run({
	deps: ['../mscorlib.js', '../contractsruntime.js', '../SimplePromise.js'],
	code: '../CoreLib.TestScript.js',
	tests: process.argv[2],
});
