using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    /// <summary>
    /// Symbols allow you to place multiple instances of an item in your project. This can save memory, since all instances of a symbol simply refer to the original item and it can speed up moving around complex objects, since internal properties such as segment lists and gradient positions don't need to be updated with every transformation.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class Symbol
    {
        #region Properties
        
        
        /// <summary>
        /// The project that this symbol belongs to.
        /// </summary>
        public Project Project;
        
        /// <summary>
        /// The symbol definition.
        /// </summary>
        public Item Definition;
        
        #endregion
        
        #region Constructors

        protected Symbol(){ }
        
        /// <summary>
        /// Creates a Symbol item.
        /// </summary>
        /// <param name="item">the source item which is copied as the definition of the symbol</param>
        [ScriptName("")]
        public Symbol(Item item){ }

        #endregion
        
        #region Methods
        
        /// <summary>
        /// Places in instance of the symbol in the project.
        /// </summary>
        /// <param name="position">The position of the placed symbol. - optional</param>
        /// <returns></returns>
        public PlacedSymbol Place(Point position) { return default(PlacedSymbol); }
        
        /// <summary>
        /// Places in instance of the symbol in the project.
        /// </summary>
        /// <returns></returns>
        public PlacedSymbol Place() { return default(PlacedSymbol); }
        
        /// <summary>
        /// Returns a copy of the symbol.
        /// </summary>
        /// <returns></returns>
        public Symbol Clone() { return default(Symbol); }
        
        #endregion
        
    }
}