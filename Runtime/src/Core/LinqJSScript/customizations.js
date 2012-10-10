    if (typeof(global) === "undefined")
      global = window;

    Enumerable.from = (function(old) {
        return function(obj) {
            var ienum = Type.safeCast(obj, ss.IEnumerable);
            if (ienum) {
                var enumerator;
                return new Enumerable(function () {
                    return new IEnumerator(
                        function () { enumerator = ienum.getEnumerator(); },
                        function () {
                            var ok = enumerator.moveNext();
                            return ok ? this.yieldReturn(enumerator.get_current()) : false;
                        },
                        function () {
                            var disposable = Type.safeCast(enumerator, ss.IDisposable);
                            if (disposable) {
                                disposable.dispose();
                            }
                        }
                    );
                });
            }
            else {
                return old(obj);
            }
        };
    })(Enumerable.from);

    Enumerable.prototype.toDictionary = (function (old) {
        return function (defaultValue, keySelector, elementSelector, compareSelector) {
            var d = old.call(this, keySelector, elementSelector, compareSelector);
            d.defaultValue = defaultValue;
            return d;
        };
    })(Enumerable.prototype.toDictionary);

    Enumerable.prototype.ofType = function (type) {
        var source = this;

        return new Enumerable(function () {
            var enumerator;

            return new IEnumerator(
                function () { enumerator = source.getEnumerator(); },
                function () {
                    while (enumerator.moveNext()) {
                        var v = Type.safeCast(enumerator.current(), type);
                        if (ss.isValue(v)) {
                            return this.yieldReturn(v);
                        }
                    }
                    return false;
                },
                function () { Utils.dispose(enumerator); });
        });
    };

    ArrayEnumerable.prototype.getEnumerator = (function (old) {
        return function () {
            var result = old.apply(this);
            result.get_current = result.current;
            return result;
        };
    })(ArrayEnumerable.prototype.getEnumerator);

    Type.registerClass(global, 'Enumerable', Enumerable, null, ss.IEnumerable);

    Grouping.prototype.get_current = function () { return this.current(); };
    Type.registerClass(null, '$Grouping', Grouping, null, ss.IEnumerable);

    IEnumerator.prototype.get_current = function () { return this.current(); };
    IEnumerator.prototype.reset = function () { throw new Error('Reset is not supported'); };
    Type.registerClass(null, '$IEnumerator', IEnumerator, null, ss.IDisposable);

    Lookup.prototype.getEnumerator = function () { return this.toEnumerable().getEnumerator(); };
    Type.registerClass(null, '$Lookup', Lookup, null, ss.IEnumerable);

    Dictionary.prototype.get_item = function (key) { if (!this.contains(key)) throw new Error('Key ' + key + ' does not exist.'); return this.get(key); };
    Dictionary.prototype.set_item = (function (add) { return function (key, value) { add.call(this, key, value); }; })(Dictionary.prototype.add);
    Dictionary.prototype.add = (function (old) { return function (key, value) { if (this.contains(key)) throw new Error('Key ' + key + ' already exists.'); old.call(this, key, value); }; })(Dictionary.prototype.add);
    Dictionary.prototype.remove = (function (old) { return function (key) { var r = this.contains(key); old.call(this, key); return r; }; })(Dictionary.prototype.remove);
    Dictionary.prototype.get_count = function () { return this.count(); };
    Dictionary.prototype.containsKey = function (key) { return this.contains(key); };
    Dictionary.prototype.get_keys = function () { return this.toEnumerable().select(function (x) { return x.key; }); };
    Dictionary.prototype.get_values = function () { return this.toEnumerable().select(function (x) { return x.value; }); };
    Dictionary.prototype.getEnumerator = function () { return this.toEnumerable().getEnumerator(); };
    Dictionary.prototype.tryGetValue = function (key, value) { if (this.containsKey(key)) { value.$ = this.get(key); return true; } else { value.$ = this.defaultValue; return false; } };
    Type.registerClass(null, '$LinqJSDictionary', Dictionary, null, ss.IEnumerable);
