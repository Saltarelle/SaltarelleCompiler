using System;
using System.Html;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// A Group is a collection of items. When you transform a Group, its children are treated as a single unit without changing their relative positions.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class Group : Item
    {
        #region Properties
        
        
        /// <summary>
        /// Specifies whether the group item is to be clipped. When setting to true, the first child in the group is automatically defined as the clipping mask.
        /// </summary>
        public bool Clipped;
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates a new Group item and places it at the top of the active layer.
        /// </summary>
        /// <param name="children">An array of children that will be added to the newly created group. - optional</param>
        [ScriptName("")]
        public Group(Item[] children){ }
        
        /// <summary>
        /// Creates a new Group item and places it at the top of the active layer.
        /// </summary>
        [ScriptName("")]
        public Group(){ }

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