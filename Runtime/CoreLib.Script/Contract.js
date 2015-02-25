///////////////////////////////////////////////////////////////////////////////
// Contract

var ss_Contract = function#? DEBUG Contract$##() {
};
ss_Contract.__typeName = 'ss.Contract';
ss.Contract = ss_Contract;
ss.initClass(ss_Contract, ss, {});

ss_Contract.ReportFailure = function#? DEBUG Contract$ReportFailure##(failureKind, userMessage, condition, innerException, TException) {
    var conditionText = condition.toString();
    conditionText = conditionText.substring(conditionText.indexOf("return") + 7);
    conditionText = conditionText.substr(0, conditionText.lastIndexOf(";"));

    var failureMessage = (conditionText) ? "Contract '" + conditionText + "' failed" : "Contract failed";
    var displayMessage = (userMessage) ? failureMessage + ": " + userMessage : failureMessage;

    if (TException) {
        throw new TException(conditionText, userMessage);
    }
    else {
        //throw new ContractException(failureKind, displayMessage, userMessage, conditionText, innerException);   
        throw new Error(displayMessage);
    }
};

ss_Contract.Assert = function#? DEBUG Contract$Assert##(failureKind, condition, message) {
    if (!condition()) {
        ss.Contract.ReportFailure(failureKind, message, condition, null);
    }
};

ss_Contract.Requires = function#? DEBUG Contract$Requires##(TException, condition, message) {
    if (!condition()) {
        ss.Contract.ReportFailure(0, message, condition, null, TException);
    }
};

ss_Contract.ForAll = function#? DEBUG Contract$ForAll##(fromInclusive, toExclusive, predicate) {
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

ss_Contract.ForAll$1 = function#? DEBUG Contract$ForAll$1##(collection, predicate) {
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

ss_Contract.Exists = function#? DEBUG Contract$Exists##(fromInclusive, toExclusive, predicate) {
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

ss_Contract.Exists$1 = function#? DEBUG Contract$Exists$1##(collection, predicate) {
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