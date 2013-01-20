///////////////////////////////////////////////////////////////////////////////
// Function Extensions

Function.__typeName = 'Function';
Function.__baseType = Object;
Function.__class = true;

Function.empty = function () { };

Function._contains = function#? DEBUG Function$_contains##(targets, object, method) {
    for (var i = 0; i < targets.length; i += 2) {
        if (targets[i] === object && targets[i + 1] === method) {
            return true;
        }
    }
    return false;
};

Function._mkdel = function#? DEBUG Function$_mkdel##(targets) {
    var delegate = function() {
        if (targets.length == 2) {
            return targets[1].apply(targets[0], arguments);
        }
        else {
            var clone = targets.clone();
            for (var i = 0; i < clone.length; i += 2) {
                if (Function._contains(targets, clone[i], clone[i + 1])) {
                    clone[i + 1].apply(clone[i], arguments);
                }
            }
            return null;
        }
    };
    delegate._targets = targets;

    return delegate;
};

Function.mkdel = function#? DEBUG Function$mkdel##(object, method) {
    if (!object) {
        return method;
    }
    return Function._mkdel([object, method]);
};

Function.combine = function#? DEBUG Function$combine##(delegate1, delegate2) {
    if (!delegate1) {
        if (!delegate2._targets) {
            return Function.mkdel(null, delegate2);
        }
        return delegate2;
    }
    if (!delegate2) {
        if (!delegate1._targets) {
            return Function.mkdel(null, delegate1);
        }
        return delegate1;
    }

    var targets1 = delegate1._targets ? delegate1._targets : [null, delegate1];
    var targets2 = delegate2._targets ? delegate2._targets : [null, delegate2];

    return Function._mkdel(targets1.concat(targets2));
};

Function.remove = function#? DEBUG Function$remove##(delegate1, delegate2) {
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
            targets.splice(i, 2);
            return Function._mkdel(targets);
        }
    }

    return delegate1;
};

Function.clone = function#? DEBUG Function$clone##(source) {
	return source._targets ? Function._mkdel(source._targets) : function() { return source.apply(this, arguments); };
};

Function.thisFix = function#? DEBUG Function$thisFix##(source) {
    return function() {
        var x = [this];
        for(var i = 0; i < arguments.length; i++)
            x.push(arguments[i]);
        return source.apply(source, x);
    };
};
