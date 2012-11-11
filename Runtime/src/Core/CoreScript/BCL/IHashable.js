///////////////////////////////////////////////////////////////////////////////
// IHashable

var ss_IHashable = function#? DEBUG IHashable$##() { };
ss_IHashable.prototype = {
    getHashCode: null
};

Type.registerInterface(global, 'ss.IHashable', ss_IHashable, ss_IEquatable);
