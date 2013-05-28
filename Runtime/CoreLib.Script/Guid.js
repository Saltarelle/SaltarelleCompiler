///////////////////////////////////////////////////////////////////////////////
// Guid

var ss_Guid = function (uuid) {
    if (typeof variable === 'undefined')
        this.uuid = UUID.empty;
    else
        this.uuid = UUID.parse(uuid);
};

ss_Guid.__typeName = 'ss.Guid';
ss.Guid = ss_Guid;
ss.initClass(ss_Guid, Object, [ ss_IEquatable, ss_IFormattable ]);
ss_Guid.__class = false;



ss_Guid.prototype = {
    equals: function#? DEBUG  Guid$equals##(other) {
            return ss.isInstanceOfType(other, ss_Guid) && other.valueOf() === this.valueOf();
    },
    equalsT: function#? DEBUG Guid$equalsT##(other) {
                return this.equals(other);
    },
    toString: function#? DEBUG Guid$toString##() {
        return this.valueOf();
   }
};


ss_Guid.isInstanceOfType = function(instance) {
    return typeof(instance) === 'ss_Guid';
};

ss_Guid.getDefaultValue = ss_Guid.createInstance = function() {
    return UUID.empty;
};



ss_Guid.parse = function (uuid) {
    if (typeof variable === 'undefined')
        throw new ss_ArgumentNullException('uuid');
    var guid = new ss_Guid(uuid);
};


ss_Guid.newGuid = function () {
    var uuid = UUID.create();
    return new ss_Guid(uuid);
};

ss_Guid.equalsT = function (that, other) {
    return that.valueOf() === other.valueOf();
};




(function (w) {
    // From http://baagoe.com/en/RandomMusings/javascript/
    // Johannes BaagÃ¸e <baagoe@baagoe.com>, 2010
    function Mash() {
        var n = 0xefc8249d;

        var mash = function (data) {
            data = data.toString();
            for (var i = 0; i < data.length; i++) {
                n += data.charCodeAt(i);
                var h = 0.02519603282416938 * n;
                n = h >>> 0;
                h -= n;
                h *= n;
                n = h >>> 0;
                h -= n;
                n += h * 0x100000000; // 2^32
            }
            return (n >>> 0) * 2.3283064365386963e-10; // 2^-32
        };

        mash.version = 'Mash 0.9';
        return mash;
    }

    // From http://baagoe.com/en/RandomMusings/javascript/
    function Kybos() {
        return (function (args) {
            // Johannes BaagÃ¸e <baagoe@baagoe.com>, 2010
            var s0 = 0;
            var s1 = 0;
            var s2 = 0;
            var c = 1;
            var s = [];
            var k = 0;

            var mash = Mash();
            var s0 = mash(' ');
            var s1 = mash(' ');
            var s2 = mash(' ');
            for (var j = 0; j < 8; j++) {
                s[j] = mash(' ');
            }

            if (args.length == 0) {
                args = [+new Date];
            }
            for (var i = 0; i < args.length; i++) {
                s0 -= mash(args[i]);
                if (s0 < 0) {
                    s0 += 1;
                }
                s1 -= mash(args[i]);
                if (s1 < 0) {
                    s1 += 1;
                }
                s2 -= mash(args[i]);
                if (s2 < 0) {
                    s2 += 1;
                }
                for (var j = 0; j < 8; j++) {
                    s[j] -= mash(args[i]);
                    if (s[j] < 0) {
                        s[j] += 1;
                    }
                }
            }

            var random = function () {
                var a = 2091639;
                k = s[k] * 8 | 0;
                var r = s[k];
                var t = a * s0 + c * 2.3283064365386963e-10; // 2^-32
                s0 = s1;
                s1 = s2;
                s2 = t - (c = t | 0);
                s[k] -= s2;
                if (s[k] < 0) {
                    s[k] += 1;
                }
                return r;
            };
            random.uint32 = function () {
                return random() * 0x100000000; // 2^32
            };
            random.fract53 = function () {
                return random() +
                (random() * 0x200000 | 0) * 1.1102230246251565e-16; // 2^-53
            };
            random.addNoise = function () {
                for (var i = arguments.length - 1; i >= 0; i--) {
                    for (j = 0; j < 8; j++) {
                        s[j] -= mash(arguments[i]);
                        if (s[j] < 0) {
                            s[j] += 1;
                        }
                    }
                }
            };
            random.version = 'Kybos 0.9';
            random.args = args;
            return random;

        }(Array.prototype.slice.call(arguments)));
    };

    var rnd = Kybos();

    // UUID/GUID implementation from http://frugalcoder.us/post/2012/01/13/javascript-guid-uuid-generator.aspx
    var UUID = {
        "empty": "00000000-0000-0000-0000-000000000000"
      , "parse": function (input) {
          var ret = input.toString().trim().toLowerCase().replace(/^[\s\r\n]+|[\{\}]|[\s\r\n]+$/g, "");
          if ((/[a-f0-9]{8}\-[a-f0-9]{4}\-[a-f0-9]{4}\-[a-f0-9]{4}\-[a-f0-9]{12}/).test(ret))
              return ret;
          else
              throw new ss_FormatException("Unable to parse UUID");
      }
      , "createSequential": function () {
          var ret = new Date().valueOf().toString(16).replace("-", "")
          for (; ret.length < 12; ret = "0" + ret);
          ret = ret.substr(ret.length - 12, 12); //only least significant part
          for (; ret.length < 32; ret += Math.floor(rnd() * 0xffffffff).toString(16));
          return [ret.substr(0, 8), ret.substr(8, 4), "4" + ret.substr(12, 3), "89AB"[Math.floor(Math.random() * 4)] + ret.substr(16, 3), ret.substr(20, 12)].join("-");
      }
      , "create": function () {
          var ret = "";
          for (; ret.length < 32; ret += Math.floor(rnd() * 0xffffffff).toString(16));
          return [ret.substr(0, 8), ret.substr(8, 4), "4" + ret.substr(12, 3), "89AB"[Math.floor(Math.random() * 4)] + ret.substr(16, 3), ret.substr(20, 12)].join("-");
      }
      , "random": function () {
          return rnd();
      }
      , "tryParse": function (input) {
          try {
              return UUID.parse(input);
          } catch (ex) {
              return UUID.empty;
          }
      }
    };
    UUID["new"] = UUID.create;

    w.UUID = w.Guid = UUID;
}(window || this));
