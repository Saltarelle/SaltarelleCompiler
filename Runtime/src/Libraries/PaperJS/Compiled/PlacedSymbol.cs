using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    /// <summary>
    /// A PlacedSymbol represents an instance of a symbol which has been placed in a Paper.js project.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class PlacedSymbol : PlacedItem
    {
        #region Properties
        
        
        /// <summary>
        /// The symbol that the placed symbol refers to.
        /// </summary>
        public Symbol Symbol;
        
        #endregion
        
        #region Constructors

        protected PlacedSymbol(){ }
        
        /// <summary>
        /// Creates a new PlacedSymbol Item.
        /// </summary>
        /// <param name="symbol">the symbol to place</param>
        /// <param name="matrixOrOffset">the center point of the placed symbol or a Matrix transformation to transform the placed symbol with. - optional</param>
        [ScriptName("")]
        public PlacedSymbol(Symbol symbol, Point matrixOrOffset){ }
        
        /// <summary>
        /// Creates a new PlacedSymbol Item.
        /// </summary>
        /// <param name="symbol">the symbol to place</param>
        [ScriptName("")]
        public PlacedSymbol(Symbol symbol){ }
        
        /// <summary>
        /// Creates a new PlacedSymbol Item.
        /// </summary>
        /// <param name="symbol">the symbol to place</param>
        /// <param name="matrixOrOffset">the center point of the placed symbol or a Matrix transformation to transform the placed symbol with. - optional</param>
        [ScriptName("")]
        public PlacedSymbol(Symbol symbol, Matrix matrixOrOffset){ }

        #endregion
    }
}