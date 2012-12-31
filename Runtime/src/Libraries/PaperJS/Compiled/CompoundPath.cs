using System;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// A compound path contains two or more paths, holes are drawn where the paths overlap. All the paths in a compound path take on the style of the backmost path and can be accessed through its item.children list.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class CompoundPath : PathItem
    {
    
        #region Constructors
        
        /// <summary>
        /// Creates a new compound path item and places it in the active layer.
        /// </summary>
        /// <param name="paths">the paths to place within the compound path. - optional</param>
        [ScriptName("")]
        public CompoundPath(Path[] paths){ }
        
        /// <summary>
        /// Creates a new compound path item and places it in the active layer.
        /// </summary>
        [ScriptName("")]
        public CompoundPath(){ }

        #endregion
        
        #region Methods
        
        /// <summary>
        /// If this is a compound path with only one path inside, the path is moved outside and the compound path is erased. Otherwise, the compound path is returned unmodified.
        /// </summary>
        /// <returns>- the simplified compound path</returns>
        public CompoundPath Simplify() { return default(CompoundPath); }
        
        #endregion
        
    }
}