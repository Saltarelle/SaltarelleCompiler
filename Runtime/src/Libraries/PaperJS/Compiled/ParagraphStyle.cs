using System;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// The ParagraphStyle object represents the paragraph style of a text item (textItem.paragraphStyle). Currently, the ParagraphStyle object may seem a bit empty, with just the justification property. Yet, we have lots in store for Paper.js when it comes to typography. Please stay tuned.  Example   var text = new PointText(new Point(0,0)); text.fillColor = 'black'; text.content = 'Hello world.'; text.paragraphStyle.justification = 'center';
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class ParagraphStyle
    {
        #region Properties
        
        
        /// <summary>
        /// The justification of the paragraph.
        /// </summary>
        public ParagraphStyleJustification Justification;
        
        #endregion
    }
}