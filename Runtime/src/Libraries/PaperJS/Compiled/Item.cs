using System;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// The Item type allows you to access and modify the items in Paper.js projects. Its functionality is inherited by different project item types such as Path, CompoundPath, Group, Layer and Raster. They each add a layer of functionality that is unique to their type, but share the underlying properties and functions that they inherit from Item.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class Item
    {
        #region Properties
        
        
        /// <summary>
        /// The unique id of the item.
        /// </summary>
        public int Id;
        
        /// <summary>
        /// The name of the item. If the item has a name, it can be accessed by name through its parent's children list.
        /// </summary>
        public string Name;
        
        /// <summary>
        /// The item's position within the project. This is the rectangle.center of the item's bounds rectangle.
        /// </summary>
        public Point Position;
        
        /// <summary>
        /// The path style of the item.
        /// </summary>
        public PathStyle Style;
        
        /// <summary>
        /// Specifies whether the item is visible. When set to false, the item won't be drawn.
        /// </summary>
        public bool Visible;
        
        /// <summary>
        /// The blend mode of the item.
        /// </summary>
        public ItemBlendMode BlendMode;
        
        /// <summary>
        /// The opacity of the item as a value between 0 and 1.
        /// </summary>
        public double Opacity;
        
        /// <summary>
        /// Specifies whether the item functions as a guide. When set to true, the item will be drawn at the end as a guide.
        /// </summary>
        public double Guide;
        
        /// <summary>
        /// Specifies whether an item is selected and will also return true if the item is partially selected (groups with some selected or partially selected paths). Paper.js draws the visual outlines of selected items on top of your project. This can be useful for debugging, as it allows you to see the construction of paths, position of path curves, individual segment points and bounding boxes of symbol and raster items.
        /// </summary>
        public bool Selected;
        
        /// <summary>
        /// Specifies whether the item defines a clip mask. This can only be set on paths, compound paths, and text frame objects, and only if the item is already contained within a clipping group.
        /// </summary>
        public bool ClipMask;
        
        /// <summary>
        /// The project that this item belongs to.
        /// </summary>
        public Project Project;
        
        /// <summary>
        /// The layer that this item is contained within.
        /// </summary>
        public Layer Layer;
        
        /// <summary>
        /// The item that this item is contained within.
        /// </summary>
        public Item Parent;
        
        /// <summary>
        /// The children items contained within this item. Items that define a name can also be accessed by name. Please note: The children array should not be modified directly using array functions. To remove single items from the children list, use item.remove(), to remove all items from the children list, use item.removeChildren(). To add items to the children list, use item.addChild(item) or item.insertChild(index, item).
        /// </summary>
        public Item[] Children;
        
        /// <summary>
        /// The first item contained within this item. This is a shortcut for accessing item.children[0].
        /// </summary>
        public Item FirstChild;
        
        /// <summary>
        /// The last item contained within this item.This is a shortcut for accessing item.children[item.children.length - 1].
        /// </summary>
        public Item LastChild;
        
        /// <summary>
        /// The next item on the same level as this item.
        /// </summary>
        public Item NextSibling;
        
        /// <summary>
        /// The previous item on the same level as this item.
        /// </summary>
        public Item PreviousSibling;
        
        /// <summary>
        /// The index of this item within the list of its parent's children.
        /// </summary>
        public int Index;
        
        /// <summary>
        /// The bounding rectangle of the item excluding stroke width.
        /// </summary>
        public Rectangle Bounds;
        
        /// <summary>
        /// The bounding rectangle of the item including stroke width.
        /// </summary>
        public Rectangle StrokeBounds;
        
        /// <summary>
        /// The bounding rectangle of the item including handles.
        /// </summary>
        public Rectangle HandleBounds;
        
        /// <summary>
        /// The color of the stroke.
        /// </summary>
        public Color StrokeColor;
        
        /// <summary>
        /// The width of the stroke.
        /// </summary>
        public double StrokeWidth;
        
        /// <summary>
        /// The shape to be used at the end of open Path items, when they have a stroke.
        /// </summary>
        public StrokeCap StrokeCap;
        
        /// <summary>
        /// The shape to be used at the corners of paths when they have a stroke.
        /// </summary>
        public string StrokeJoin;
        
        /// <summary>
        /// The dash offset of the stroke.
        /// </summary>
        public double DashOffset;
        
        /// <summary>
        /// Specifies an array containing the dash and gap lengths of the stroke.
        /// </summary>
        public Array DashArray;
        
        /// <summary>
        /// The miter limit of the stroke. When two line segments meet at a sharp angle and miter joins have been specified for item.strokeJoin, it is possible for the miter to extend far beyond the item.strokeWidth of the path. The miterLimit imposes a limit on the ratio of the miter length to the item.strokeWidth.
        /// </summary>
        public double MiterLimit;
        
        /// <summary>
        /// The fill color of the item.
        /// </summary>
        public Color FillColor;
        
        #endregion
        
        #region Methods
        
        /// <summary>
        /// Clones the item within the same project and places the copy above the item.
        /// </summary>
        /// <returns>the newly cloned item</returns>
        public Item Clone() { return default(Item); }
        
        /// <summary>
        /// When passed a project, copies the item to the project, or duplicates it within the same project. When passed an item, copies the item into the specified item.
        /// </summary>
        /// <param name="item">the item or project to copy the item to</param>
        /// <returns>the new copy of the item</returns>
        public Item CopyTo(Project item) { return default(Item); }
        
        /// <summary>
        /// When passed a project, copies the item to the project, or duplicates it within the same project. When passed an item, copies the item into the specified item.
        /// </summary>
        /// <param name="item">the item or project to copy the item to</param>
        /// <returns>the new copy of the item</returns>
        public Item CopyTo(Layer item) { return default(Item); }
        
        /// <summary>
        /// When passed a project, copies the item to the project, or duplicates it within the same project. When passed an item, copies the item into the specified item.
        /// </summary>
        /// <param name="item">the item or project to copy the item to</param>
        /// <returns>the new copy of the item</returns>
        public Item CopyTo(Group item) { return default(Item); }
        
        /// <summary>
        /// When passed a project, copies the item to the project, or duplicates it within the same project. When passed an item, copies the item into the specified item.
        /// </summary>
        /// <param name="item">the item or project to copy the item to</param>
        /// <returns>the new copy of the item</returns>
        public Item CopyTo(CompoundPath item) { return default(Item); }
        
        /// <summary>
        /// Rasterizes the item into a newly created Raster object. The item itself is not removed after rasterization.
        /// </summary>
        /// <param name="resolution">the resolution of the raster in dpi - optional, default: 72</param>
        /// <returns>the newly created raster item</returns>
        public Raster Rasterize(double resolution) { return default(Raster); }
        
        /// <summary>
        /// Rasterizes the item into a newly created Raster object. The item itself is not removed after rasterization.
        /// </summary>
        /// <returns>the newly created raster item</returns>
        public Raster Rasterize() { return default(Raster); }
        
        /// <summary>
        /// Perform a hit test on the item (and its children, if it is a Group or Layer) at the location of the specified point. The optional options object allows you to control the specifics of the hit test and may contain a combination of the following values: tolerance: Number - The tolerance of the hit test in points. options.type: Only hit test again a certain item type: PathItem, Raster, TextItem, etc. options.fill: Boolean - Hit test the fill of items. options.stroke: Boolean - Hit test the curves of path items, taking into account stroke width. options.segment: Boolean - Hit test for segment.point of Path items. options.handles: Boolean - Hit test for the handles (segment.handleIn / segment.handleOut) of path segments. options.ends: Boolean - Only hit test for the first or last segment points of open path items. options.bounds: Boolean - Hit test the corners and side-centers of the bounding rectangle of items (item.bounds). options.center: Boolean - Hit test the rectangle.center of the bounding rectangle of items (item.bounds). options.guide: Boolean - Hit test items that have item.guide set to true. options.selected: Boolean - Only hit selected items.
        /// </summary>
        /// <param name="point">The point where the hit test should be performed</param>
        /// <param name="options">optional, default: { fill: true, stroke: true, segments: true, tolerance: 2 }</param>
        /// <returns>A hit result object that contains more information about what exactly was hit or null if nothing was hit.</returns>
        public HitResult HitTest(Point point, object options) { return default(HitResult); }
        
        /// <summary>
        /// Perform a hit test on the item (and its children, if it is a Group or Layer) at the location of the specified point. The optional options object allows you to control the specifics of the hit test and may contain a combination of the following values: tolerance: Number - The tolerance of the hit test in points. options.type: Only hit test again a certain item type: PathItem, Raster, TextItem, etc. options.fill: Boolean - Hit test the fill of items. options.stroke: Boolean - Hit test the curves of path items, taking into account stroke width. options.segment: Boolean - Hit test for segment.point of Path items. options.handles: Boolean - Hit test for the handles (segment.handleIn / segment.handleOut) of path segments. options.ends: Boolean - Only hit test for the first or last segment points of open path items. options.bounds: Boolean - Hit test the corners and side-centers of the bounding rectangle of items (item.bounds). options.center: Boolean - Hit test the rectangle.center of the bounding rectangle of items (item.bounds). options.guide: Boolean - Hit test items that have item.guide set to true. options.selected: Boolean - Only hit selected items.
        /// </summary>
        /// <param name="point">The point where the hit test should be performed</param>
        /// <returns>A hit result object that contains more information about what exactly was hit or null if nothing was hit.</returns>
        public HitResult HitTest(Point point) { return default(HitResult); }
        
        /// <summary>
        /// Adds the specified item as a child of this item at the end of the its children list. You can use this function for groups, compound paths and layers.
        /// </summary>
        /// <param name="item">The item to be added as a child</param>
        public void AddChild(Item item) { }
        
        /// <summary>
        /// Inserts the specified item as a child of this item at the specified index in its children list. You can use this function for groups, compound paths and layers.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item">The item to be appended as a child</param>
        public void InsertChild(int index, Item item) { }
        
        /// <summary>
        /// Adds the specified items as children of this item at the end of the its children list. You can use this function for groups, compound paths and layers.
        /// </summary>
        /// <param name="items">The items to be added as children</param>
        public void AddChildren(Item items) { }
        
        /// <summary>
        /// Inserts the specified items as children of this item at the specified index in its children list. You can use this function for groups, compound paths and layers.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="items">The items to be appended as children</param>
        public void InsertChildren(int index, Item[] items) { }
        
        /// <summary>
        /// Inserts this item above the specified item.
        /// </summary>
        /// <param name="item">The item above which it should be moved</param>
        /// <returns>true it was inserted, false otherwise</returns>
        public bool InsertAbove(Item item) { return default(bool); }
        
        /// <summary>
        /// Inserts this item below the specified item.
        /// </summary>
        /// <param name="item">The item above which it should be moved</param>
        /// <returns>true it was inserted, false otherwise</returns>
        public bool InsertBelow(Item item) { return default(bool); }
        
        /// <summary>
        /// Removes the item from the project. If the item has children, they are also removed.
        /// </summary>
        /// <returns>true the item was removed, false otherwise</returns>
        public bool Remove() { return default(bool); }
        
        /// <summary>
        /// Removes all of the item's children (if any).
        /// </summary>
        /// <returns>an array containing the removed items</returns>
        public Item[] RemoveChildren() { return default(Item[]); }
        
        /// <summary>
        /// Removes the children from the specified from index to the to index from the parent's children array.
        /// </summary>
        /// <param name="from">the beginning index, inclusive</param>
        /// <param name="to">the ending index, exclusive - optional, default: children.length</param>
        /// <returns>an array containing the removed items</returns>
        public Item[] RemoveChildren(double from, double to) { return default(Item[]); }
        
        /// <summary>
        /// Removes the children from the specified from index to the to index from the parent's children array.
        /// </summary>
        /// <param name="from">the beginning index, inclusive</param>
        /// <returns>an array containing the removed items</returns>
        public Item[] RemoveChildren(double from) { return default(Item[]); }
        
        /// <summary>
        ///
        /// </summary>
        public void ReverseChildren() { }
        
        /// <summary>
        /// Checks if the item contains any children items.
        /// </summary>
        /// <returns>true it has one or more children, false otherwise</returns>
        public bool HasChildren() { return default(bool); }
        
        /// <summary>
        /// Checks if this item is above the specified item in the stacking order of the project.
        /// </summary>
        /// <param name="item">The item to check against</param>
        /// <returns>true if it is above the specified item, false otherwise</returns>
        public bool IsAbove(Item item) { return default(bool); }
        
        /// <summary>
        /// Checks if the item is below the specified item in the stacking order of the project.
        /// </summary>
        /// <param name="item">The item to check against</param>
        /// <returns>true if it is below the specified item, false otherwise</returns>
        public bool IsBelow(Item item) { return default(bool); }
        
        /// <summary>
        /// Checks whether the specified item is the parent of the item.
        /// </summary>
        /// <param name="item">The item to check against</param>
        /// <returns>true if it is the parent of the item, false otherwise</returns>
        public bool IsParent(Item item) { return default(bool); }
        
        /// <summary>
        /// Checks whether the specified item is a child of the item.
        /// </summary>
        /// <param name="item">The item to check against</param>
        /// <returns>true it is a child of the item, false otherwise</returns>
        public bool IsChild(Item item) { return default(bool); }
        
        /// <summary>
        /// Checks if the item is contained within the specified item.
        /// </summary>
        /// <param name="item">The item to check against</param>
        /// <returns>true if it is inside the specified item, false otherwise</returns>
        public bool IsDescendant(Item item) { return default(bool); }
        
        /// <summary>
        /// Checks if the item is an ancestor of the specified item.
        /// </summary>
        /// <param name="item">the item to check against</param>
        /// <returns>true if the item is an ancestor of the specified item, false otherwise</returns>
        public bool IsAncestor(Item item) { return default(bool); }
        
        /// <summary>
        /// Checks whether the item is grouped with the specified item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true if the items are grouped together, false otherwise</returns>
        public bool IsGroupedWith(Item item) { return default(bool); }
        
        /// <summary>
        /// Scales the item by the given value from its center point, or optionally from a supplied point.
        /// </summary>
        /// <param name="scale">the scale factor</param>
        /// <param name="center">optional, default: item.position</param>
        public void Scale(double scale, Point center) { }
        
        /// <summary>
        /// Scales the item by the given value from its center point, or optionally from a supplied point.
        /// </summary>
        /// <param name="scale">the scale factor</param>
        public void Scale(double scale) { }
        
        /// <summary>
        /// Scales the item by the given values from its center point, or optionally from a supplied point.
        /// </summary>
        /// <param name="hor">the horizontal scale factor</param>
        /// <param name="ver">the vertical scale factor</param>
        /// <param name="center">optional, default: item.position</param>
        public void Scale(double hor, double ver, Point center) { }
        
        /// <summary>
        /// Scales the item by the given values from its center point, or optionally from a supplied point.
        /// </summary>
        /// <param name="hor">the horizontal scale factor</param>
        /// <param name="ver">the vertical scale factor</param>
        public void Scale(double hor, double ver) { }
        
        /// <summary>
        /// Translates (moves) the item by the given offset point.
        /// </summary>
        /// <param name="delta">the offset to translate the item by</param>
        public void Translate(Point delta) { }
        
        /// <summary>
        /// Rotates the item by a given angle around the given point. Angles are oriented clockwise and measured in degrees.
        /// </summary>
        /// <param name="angle">the rotation angle</param>
        /// <param name="center">optional, default: item.position</param>
        public void Rotate(double angle, Point center) { }
        
        /// <summary>
        /// Rotates the item by a given angle around the given point. Angles are oriented clockwise and measured in degrees.
        /// </summary>
        /// <param name="angle">the rotation angle</param>
        public void Rotate(double angle) { }
        
        /// <summary>
        /// Shears the item by the given value from its center point, or optionally by a supplied point.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="center">optional, default: item.position</param>
        public void Shear(Point point, Point center) { }
        
        /// <summary>
        /// Shears the item by the given value from its center point, or optionally by a supplied point.
        /// </summary>
        /// <param name="point"></param>
        public void Shear(Point point) { }
        
        /// <summary>
        /// Shears the item by the given values from its center point, or optionally by a supplied point.
        /// </summary>
        /// <param name="hor">the horizontal shear factor.</param>
        /// <param name="ver">the vertical shear factor.</param>
        /// <param name="center">optional, default: item.position</param>
        public void Shear(double hor, double ver, Point center) { }
        
        /// <summary>
        /// Shears the item by the given values from its center point, or optionally by a supplied point.
        /// </summary>
        /// <param name="hor">the horizontal shear factor.</param>
        /// <param name="ver">the vertical shear factor.</param>
        public void Shear(double hor, double ver) { }
        
        /// <summary>
        /// Transform the item.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="flags"></param>
        public void Transform(Matrix matrix, object flags) { }
        
        /// <summary>
        /// Transform the item so that its bounds fit within the specified rectangle, without changing its aspect ratio.
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="fill">optional, default: false</param>
        public void FitBounds(Rectangle rectangle, bool fill) { }
        
        /// <summary>
        /// Transform the item so that its bounds fit within the specified rectangle, without changing its aspect ratio.
        /// </summary>
        /// <param name="rectangle"></param>
        public void FitBounds(Rectangle rectangle) { }
        
        /// <summary>
        /// Removes the item when the events specified in the passed object literal occur. The object literal can contain the following values: Remove the item when the next tool.onMouseMove event is fired: object.move = true Remove the item when the next tool.onMouseDrag event is fired: object.drag = true Remove the item when the next tool.onMouseDown event is fired: object.down = true Remove the item when the next tool.onMouseUp event is fired: object.up = true
        /// </summary>
        /// <param name="_object"></param>
        public void RemoveOn(object _object) { }
        
        /// <summary>
        ///
        /// </summary>
        public void RemoveOnMove() { }
        
        /// <summary>
        ///
        /// </summary>
        public void RemoveOnDown() { }
        
        /// <summary>
        ///
        /// </summary>
        public void RemoveOnDrag() { }
        
        /// <summary>
        ///
        /// </summary>
        public void RemoveOnUp() { }
        
        #endregion
        
    }
}