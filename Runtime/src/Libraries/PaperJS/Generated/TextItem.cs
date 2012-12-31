using System;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// The TextItem type allows you to create typography. Its functionality is inherited by different text item types such as PointText, and AreaText (coming soon). They each add a layer of functionality that is unique to their type, but share the underlying properties and functions that they inherit from TextItem.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class TextItem : Item
    {
        #region Properties
        
        
        /// <summary>
        /// The text contents of the text item.
        /// </summary>
        public string Content;
        
        /// <summary>
        /// The character style of the text item.
        /// </summary>
        public CharacterStyle CharacterStyle;
        
        /// <summary>
        /// The paragraph style of the text item.
        /// </summary>
        public ParagraphStyle ParagraphStyle;
        
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