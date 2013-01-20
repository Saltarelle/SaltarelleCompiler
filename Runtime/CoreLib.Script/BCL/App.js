///////////////////////////////////////////////////////////////////////////////
// Interfaces

var ss_IApplication = function() { };
Type.registerInterface(global, 'ss.IApplication', ss_IApplication);

var ss_IContainer = function () { };
Type.registerInterface(global, 'ss.IContainer', ss_IContainer);

var ss_IObjectFactory = function () { };
Type.registerInterface(global, 'ss.IObjectFactory', ss_IObjectFactory);

var ss_IEventManager = function () { };
Type.registerInterface(global, 'ss.IEventManager', ss_IEventManager);

var ss_IInitializable = function () { };
Type.registerInterface(global, 'ss.IInitializable', ss_IInitializable);
