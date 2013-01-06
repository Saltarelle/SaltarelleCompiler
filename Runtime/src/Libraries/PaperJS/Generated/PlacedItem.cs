using System;
using System.Html;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// The PlacedItem class is the base for any items that have a matrix associated with them, describing their placement in the project, such as Raster and PlacedSymbol.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class PlacedItem : Item
    {
        #region Properties
        
        
        /// <summary>
        /// The item's transformation matrix, defining position and dimensions in the document.
        /// </summary>
        public Matrix Matrix;
        
        #endregion
        
        #region Methods
        
        /// <summary>
        /// Inserts the specified item as a child of this item by appending it to the list of children and moving it above all other children. You can use this function for groups, compound paths and layers.
        /// </summary>
        /// <param name="item">The item to be appended as a child</param>
        public void AppendTop(Item item) { }
        
        /// <summary>
        /// Inserts the specified item as a child of this item by appending it to the list of children and moving it below all other children. You can use this function for groups, compound paths and layers.
        /// </summary>
        /// <param name="item">The item to be appended as a child</param>
        public void AppendBottom(Item item) { }
        
        /// <summary>
        /// Moves this item above the specified item.
        /// </summary>
        /// <param name="item">The item above which it should be moved</param>
        /// <returns>true it was moved, false otherwise</returns>
        public bool MoveAbove(Item item) { return default(bool); }
        
        /// <summary>
        /// Moves the item below the specified item.
        /// </summary>
        /// <param name="item">the item below which it should be moved</param>
        /// <returns>true it was moved, false otherwise</returns>
        public bool MoveBelow(Item item) { return default(bool); }
        
        #endregion
        
    }
}