using System;
using System.Html;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// The PaperScope class represents the scope associated with a Paper context. When working with PaperScript, these scopes are automatically created for us, and through clever scoping the properties and methods of the active scope seem to become part of the global scope. When working with normal JavaScript code, PaperScope objects need to be manually created and handled. Paper classes can only be accessed through PaperScope objects. Thus in PaperScript they are global, while in JavaScript, they are available on the global paper object. For JavaScript you can use paperScope.install(scope) to install the Paper classes and objects on the global scope. Note that when working with more than one scope, this still works for classes, but not for objects like paperScope.project, since they are not updated in the injected scope if scopes are switched. The global paper object is simply a reference to the currently active PaperScope.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class PaperScope
    {
        #region Properties
        
        
        /// <summary>
        /// The version of Paper.js, as a float number.
        /// </summary>
        public double Version;
        
        /// <summary>
        /// The currently active project.
        /// </summary>
        public Project Project;
        
        /// <summary>
        /// The list of all open projects within the current Paper.js context.
        /// </summary>
        public Project[] Projects;
        
        /// <summary>
        /// The active view of the active project.
        /// </summary>
        public View View;
        
        /// <summary>
        /// The list of view of the active project.
        /// </summary>
        public View[] Views;
        
        /// <summary>
        /// The reference to the active tool.
        /// </summary>
        public Tool Tool;
        
        /// <summary>
        /// The list of available tools.
        /// </summary>
        public Tool[] Tools;
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        ///
        /// </summary>
        [ScriptName("")]
        public PaperScope(){ }

        #endregion
        
        #region Methods
        
        /// <summary>
        /// Injects the paper scope into any other given scope. Can be used for examle to inject the currently active PaperScope into the window's global scope, to emulate PaperScript-style globally accessible Paper classes and objects:
        /// </summary>
        /// <param name="scope"></param>
        public void Install(object scope) { }
        
        /// <summary>
        /// Sets up an empty project for us. If a canvas is provided, it also creates a View for it, both linked to this scope.
        /// </summary>
        /// <param name="canvas">The canvas this scope should be associated with.</param>
        public void Setup(CanvasElement canvas) { }
        
        #endregion
        
        #region Static Methods
        
        /// <summary>
        /// Retrieves a PaperScope object with the given id or associated with the passed canvas element.
        /// </summary>
        /// <param name="id"></param>
        public static void Get(object id) { }
        
        /// <summary>
        /// Iterates over all active scopes and calls the passed iterator function for each of them.
        /// </summary>
        /// <param name="iter">the iterator function.</param>
        public static void Each(object iter) { }
        
        #endregion
    }
}