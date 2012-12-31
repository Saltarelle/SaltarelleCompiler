using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    /// <summary>
    /// KeyEvent The KeyEvent object is received by the Tool's keyboard handlers tool.onKeyDown, tool.onKeyUp, The KeyEvent object is the only parameter passed to these functions and contains information about the keyboard event.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class KeyEvent
    {
        #region Properties
        
        
        /// <summary>
        /// The type of key event.
        /// </summary>
        public KeyEventType Type;
        
        /// <summary>
        /// The string character of the key that caused this key event.
        /// </summary>
        public string Character;
        
        /// <summary>
        /// The key that caused this key event.
        /// </summary>
        public string Key;
        
        /// <summary>
        ///
        /// </summary>
        public EventModifiers Modifiers;
        
        #endregion
        
        #region Methods
        
        /// <summary>
        ///
        /// </summary>
        /// <returns>A string representation of the key event.</returns>
        public string ToString() { return default(string); }
        
        #endregion
        
    }
}