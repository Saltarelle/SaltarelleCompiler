using System;
using System.Html;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// A Project object in Paper.js is what usually is referred to as the document: The top level object that holds all the items contained in the scene graph. As the term document is already taken in the browser context, it is called Project. Projects allow the manipluation of the styles that are applied to all newly created items, give access to the selected items, and will in future versions offer ways to query for items in the scene graph defining specific requirements, and means to persist and load from different formats, such as SVG and PDF. The currently active project can be accessed through the paperScope.project variable. An array of all open projects is accessible through the paperScope.projects variable.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class Project
    {
        #region Properties
        
        
        /// <summary>
        /// The currently active path style. All selected items and newly created items will be styled with this style.
        /// </summary>
        public PathStyle CurrentStyle;
        
        /// <summary>
        /// The index of the project in the paperScope.projects list.
        /// </summary>
        public int Index;
        
        /// <summary>
        /// The selected items contained within the project.
        /// </summary>
        public Item[] SelectedItems;
        
        /// <summary>
        /// The layers contained within the project.
        /// </summary>
        public Layer[] Layers;
        
        /// <summary>
        /// The layer which is currently active. New items will be created on this layer by default.
        /// </summary>
        public Layer ActiveLayer;
        
        /// <summary>
        /// The symbols contained within the project.
        /// </summary>
        public Symbol[] Symbols;
        
        /// <summary>
        /// The views contained within the project.
        /// </summary>
        public View[] Views;
        
        /// <summary>
        /// The view which is currently active.
        /// </summary>
        public View ActiveView;
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        ///
        /// </summary>
        [ScriptName("")]
        public Project(){ }

        #endregion
        
        #region Methods
        
        /// <summary>
        ///
        /// </summary>
        public void Activate() { }
        
        /// <summary>
        ///
        /// </summary>
        public void Remove() { }
        
        /// <summary>
        ///
        /// </summary>
        public void SelectAll() { }
        
        /// <summary>
        ///
        /// </summary>
        public void DeselectAll() { }
        
        /// <summary>
        /// Perform a hit test on the items contained within the project at the location of the specified point. The optional options object allows you to control the specifics of the hit test and may contain a combination of the following values: options.tolerance: Number - The tolerance of the hit test in points. options.type: Only hit test again a certain item type: PathItem, Raster, TextItem, etc. options.fill: Boolean - Hit test the fill of items. options.stroke: Boolean - Hit test the curves of path items, taking into account stroke width. options.segments: Boolean - Hit test for segment.point of Path items. options.handles: Boolean - Hit test for the handles (segment.handleIn / segment.handleOut) of path segments. options.ends: Boolean - Only hit test for the first or last segment points of open path items. options.bounds: Boolean - Hit test the corners and side-centers of the bounding rectangle of items (item.bounds). options.center: Boolean - Hit test the rectangle.center of the bounding rectangle of items (item.bounds). options.guide: Boolean - Hit test items that have item.guide set to true. options.selected: Boolean - Only hit selected items.
        /// </summary>
        /// <param name="point">The point where the hit test should be performed</param>
        /// <param name="options">optional, default: { fill: true, stroke: true, segments: true, tolerance: true }</param>
        /// <returns>A hit result object that contains more information about what exactly was hit or null if nothing was hit.</returns>
        public HitResult HitTest(Point point, object options) { return default(HitResult); }
        
        /// <summary>
        /// Perform a hit test on the items contained within the project at the location of the specified point. The optional options object allows you to control the specifics of the hit test and may contain a combination of the following values: options.tolerance: Number - The tolerance of the hit test in points. options.type: Only hit test again a certain item type: PathItem, Raster, TextItem, etc. options.fill: Boolean - Hit test the fill of items. options.stroke: Boolean - Hit test the curves of path items, taking into account stroke width. options.segments: Boolean - Hit test for segment.point of Path items. options.handles: Boolean - Hit test for the handles (segment.handleIn / segment.handleOut) of path segments. options.ends: Boolean - Only hit test for the first or last segment points of open path items. options.bounds: Boolean - Hit test the corners and side-centers of the bounding rectangle of items (item.bounds). options.center: Boolean - Hit test the rectangle.center of the bounding rectangle of items (item.bounds). options.guide: Boolean - Hit test items that have item.guide set to true. options.selected: Boolean - Only hit selected items.
        /// </summary>
        /// <param name="point">The point where the hit test should be performed</param>
        /// <returns>A hit result object that contains more information about what exactly was hit or null if nothing was hit.</returns>
        public HitResult HitTest(Point point) { return default(HitResult); }
        
        #endregion
        
    }
}