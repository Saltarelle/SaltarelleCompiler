using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    /// <summary>
    ///
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class PaperScript
    {
        #region Static Methods
        
        /// <summary>
        /// Compiles PaperScript code into JavaScript code.
        /// </summary>
        /// <param name="code">The PaperScript code.</param>
        /// <returns>The compiled PaperScript as JavaScript code.</returns>
        public static string Compile(string code) { return default(string); }
        
        /// <summary>
        /// Evaluates parsed PaperScript code in the passed PaperScope object. It also installs handlers automatically for us.
        /// </summary>
        /// <param name="code">The PaperScript code.</param>
        /// <param name="scope">The scope in which the code is executed.</param>
        /// <returns>The result of the code evaluation.</returns>
        public static object Evaluate(string code, PaperScript scope) { return default(object); }
        
        #endregion
    }
}