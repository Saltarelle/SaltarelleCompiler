using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    /// <summary>
    /// The Layer item represents a layer in a Paper.js project. The layer which is currently active can be accessed through project.activeLayer. An array of all layers in a project can be accessed through project.layers.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class Layer : Group
    {
    
        #region Constructors
        
        /// <summary>
        /// Creates a new Layer item and places it at the end of the project.layers array. The newly created layer will be activated, so all newly created items will be placed within it.
        /// </summary>
        /// <param name="children">An array of items that will be added to the newly created layer. - optional</param>
        [ScriptName("")]
        public Layer(Item[] children){ }
        
        /// <summary>
        /// Creates a new Layer item and places it at the end of the project.layers array. The newly created layer will be activated, so all newly created items will be placed within it.
        /// </summary>
        [ScriptName("")]
        public Layer(){ }

        #endregion
        
        #region Methods
        
        /// <summary>
        ///
        /// </summary>
        public void Activate() { }
        
        #endregion
        
    }
}