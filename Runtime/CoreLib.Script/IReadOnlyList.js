///////////////////////////////////////////////////////////////////////////////
// IReadOnlyList

var ss_IReadOnlyList = ss.IReadOnlyList = ss.mkType(ss, 'ss.IReadOnlyList');

ss.initInterface(ss_IReadOnlyList, [ss_IReadOnlyCollection, ss_IEnumerable]);
