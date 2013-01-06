using System;
using System.Html;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// The CharacterStyle object represents the character style of a text item (textItem.characterStyle)  Example   var text = new PointText(new Point(50, 50)); text.content = 'Hello world.'; text.characterStyle = { fontSize: 50, fillColor: 'black', };
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class CharacterStyle : PathStyle
    {
        #region Properties
        
        
        /// <summary>
        /// The font of the character style.
        /// </summary>
        public string Font;
        
        /// <summary>
        /// The font size of the character style in points.
        /// </summary>
        public double FontSize;
        
        #endregion
    }
}