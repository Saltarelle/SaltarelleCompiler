///////////////////////////////////////////////////////////////////////////////
// IEnumerator

ss.IEnumerator = function#? DEBUG IEnumerator$##() { };
#if DEBUG
ss.IEnumerator.prototype = {
    get_current: null,
    moveNext: null,
    reset: null
}
#endif // DEBUG

ss.IEnumerator.registerInterface('ss.IEnumerator', ss.IDisposable);
