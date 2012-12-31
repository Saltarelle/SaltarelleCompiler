using System;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// A PointText item represents a piece of typography in your Paper.js project which starts from a certain point and extends by the amount of characters contained in it.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class PointText : TextItem
    {
        #region Properties
        
        
        /// <summary>
        /// The PointText's anchor point
        /// </summary>
        public Point Point;
        
        #endregion
        
        #region Constructors

        /// <summary>
        /// Constructor for enable inheritance
        /// </summary>
        protected PointText(){ }
        
        /// <summary>
        /// Creates a point text item
        /// </summary>
        /// <param name="point">the position where the text will start</param>
        [ScriptName("")]
        public PointText(Point point){ }

        #endregion
    }
}