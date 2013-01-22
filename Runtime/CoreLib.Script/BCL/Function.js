///////////////////////////////////////////////////////////////////////////////
// Function Extensions

ss._delegateContains = function#? DEBUG ss$_delegateContains##(targets, object, method) {
	for (var i = 0; i < targets.length; i += 2) {
		if (targets[i] === object && targets[i + 1] === method) {
			return true;
		}
	}
	return false;
};

ss._mkdel = function#? DEBUG ss$_mkdel##(targets) {
	var delegate = function() {
		if (targets.length == 2) {
			return targets[1].apply(targets[0], arguments);
		}
		else {
			var clone = ss.arrayClone(targets);
			for (var i = 0; i < clone.length; i += 2) {
				if (ss._delegateContains(targets, clone[i], clone[i + 1])) {
					clone[i + 1].apply(clone[i], arguments);
				}
			}
			return null;
		}
	};
	delegate._targets = targets;

	return delegate;
};

ss.mkdel = function#? DEBUG ss$mkdel##(object, method) {
	if (!object) {
		return method;
	}
	return ss._mkdel([object, method]);
};

ss.delegateCombine = function#? DEBUG ss$delegateCombine##(delegate1, delegate2) {
	if (!delegate1) {
		if (!delegate2._targets) {
			return ss.mkdel(null, delegate2);
		}
		return delegate2;
	}
	if (!delegate2) {
		if (!delegate1._targets) {
			return ss.mkdel(null, delegate1);
		}
		return delegate1;
	}

	var targets1 = delegate1._targets ? delegate1._targets : [null, delegate1];
	var targets2 = delegate2._targets ? delegate2._targets : [null, delegate2];

	return ss._mkdel(targets1.concat(targets2));
};

ss.delegateRemove = function#? DEBUG ss$delegateRemove##(delegate1, delegate2) {
	if (!delegate1 || (delegate1 === delegate2)) {
		return null;
	}
	if (!delegate2) {
		return delegate1;
	}

	var targets = delegate1._targets;
	var object = null;
	var method;
	if (delegate2._targets) {
		object = delegate2._targets[0];
		method = delegate2._targets[1];
	}
	else {
		method = delegate2;
	}

	for (var i = 0; i < targets.length; i += 2) {
		if ((targets[i] === object) && (targets[i + 1] === method)) {
			if (targets.length == 2) {
				return null;
			}
			var t = ss.arrayClone(targets);
			t.splice(i, 2);
			return ss._mkdel(t);
		}
	}

	return delegate1;
};

ss.delegateEquals = function#? DEBUG ss$delegateEquals##(a, b) {
	if (a === b)
		return true;
	if (!a._targets && !b._targets)
		return false;
	var ta = a._targets || [null, a], tb = b._targets || [null, b];
	if (ta.length != tb.length)
		return false;
	for (var i = 0; i < ta.length; i++) {
		if (ta[i] !== tb[i])
			return false;
	}
	return true;
};

ss.delegateClone = function#? DEBUG ss$delegateClone##(source) {
	return source._targets ? ss._mkdel(source._targets) : function() { return source.apply(this, arguments); };
};

ss.thisFix = function#? DEBUG ss$thisFix##(source) {
	return function() {
		var x = [this];
		for(var i = 0; i < arguments.length; i++)
			x.push(arguments[i]);
		return source.apply(source, x);
	};
};

ss.getInvocationList = function#? DEBUG ss$getInvocationList##(delegate) {
	if (!delegate._targets)
		return [delegate];
	var result = [];
	for (var i = 0; i < delegate._targets.length; i += 2)
		result.push(ss.mkdel(delegate._targets[i], delegate._targets[i + 1]));
	return result;
};