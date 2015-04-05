///////////////////////////////////////////////////////////////////////////////
// Contract

var ss_Contract = function#? DEBUG Contract$##() {
};
ss_Contract.__typeName = 'ss.Contract';
ss.Contract = ss_Contract;
ss.initClass(ss_Contract, ss, {});

ss_Contract.reportFailure = function#? DEBUG Contract$reportFailure##(failureKind, userMessage, condition, innerException, TException) {
	var conditionText = condition.toString();
	conditionText = conditionText.substring(conditionText.indexOf("return") + 7);
	conditionText = conditionText.substr(0, conditionText.lastIndexOf(";"));

	var failureMessage = (conditionText) ? "Contract '" + conditionText + "' failed" : "Contract failed";
	var displayMessage = (userMessage) ? failureMessage + ": " + userMessage : failureMessage;

	if (TException) {
		throw new TException(conditionText, userMessage);
	}
	else {
		throw new ss.ContractException(failureKind, displayMessage, userMessage, conditionText, innerException);   
	}
};

ss_Contract.assert = function#? DEBUG Contract$assert##(failureKind, condition, message) {
	if (!condition()) {
		ss.Contract.reportFailure(failureKind, message, condition, null);
	}
};

ss_Contract.requires = function#? DEBUG Contract$requires##(TException, condition, message) {
	if (!condition()) {
		ss.Contract.reportFailure(0, message, condition, null, TException);
	}
};

ss_Contract.forAll = function#? DEBUG Contract$forAll##(fromInclusive, toExclusive, predicate) {
	if (!predicate) {
		throw new ss.ArgumentNullException("predicate");
	}
	for (; fromInclusive < toExclusive; fromInclusive++) {
		if (!predicate(fromInclusive)) {
			return false;
		}
	}
	return true;
};

ss_Contract.forAll$1 = function#? DEBUG Contract$forAll$1##(collection, predicate) {
	if (!collection) {
		throw new ss.ArgumentNullException("collection");
	}
	if (!predicate) {
		throw new ss.ArgumentNullException("predicate");
	}
	var enumerator = ss.getEnumerator(collection);
	try {
		while (enumerator.moveNext()) {
			if (!predicate(enumerator.current())) {
				return false;
			}
		}
		return true;
	} finally {
		enumerator.dispose();
	}
};

ss_Contract.exists = function#? DEBUG Contract$exists##(fromInclusive, toExclusive, predicate) {
	if (!predicate) {
		throw new ss.ArgumentNullException("predicate");
	}
	for (; fromInclusive < toExclusive; fromInclusive++) {
		if (predicate(fromInclusive)) {
			return true;
		}
	}
	return false;
};

ss_Contract.exists$1 = function#? DEBUG Contract$exists$1##(collection, predicate) {
	if (!collection) {
		throw new ss.ArgumentNullException("collection");
	}
	if (!predicate) {
		throw new ss.ArgumentNullException("predicate");
	}
	var enumerator = ss.getEnumerator(collection);
	try {
		while (enumerator.moveNext()) {
			if (predicate(enumerator.current())) {
				return true;
			}
		}
		return false;
	} finally {
		enumerator.dispose();
	}
}

///////////////////////////////////////////////////////////////////////////////
// ContractFailureKind

var ss_ContractFailureKind = function#? DEBUG ContractFailureKind$##() {
};
ss_ContractFailureKind.__typeName = 'ss.ContractFailureKind';
ss.ContractFailureKind = ss_ContractFailureKind;
ss.initEnum(ss_ContractFailureKind, ss, {precondition: 0, postcondition: 1, postconditionOnException: 2, invarian: 3, assert: 4, assume: 5});
