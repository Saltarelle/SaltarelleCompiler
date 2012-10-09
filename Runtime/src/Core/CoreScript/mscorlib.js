//! Script# Core Runtime
//! More information at http://projects.nikhilk.net/ScriptSharp
//!
if (typeof(global) === "undefined")
  global = window;

var ss = {};

ss.isUndefined = function (o) {
  return (o === undefined);
};

ss.isNull = function (o) {
  return (o === null);
};

ss.isNullOrUndefined = function (o) {
  return (o === null) || (o === undefined);
};

ss.isValue = function (o) {
  return (o !== null) && (o !== undefined);
};

ss.referenceEquals = function (a, b) {
  return ss.isValue(a) ? a === b : !ss.isValue(b);
};

ss.mkdict = function (a) {
  a = (arguments.length != 1 ? arguments : arguments[0]);
  var r = {};
  for (var i = 0; i < a.length; i += 2) {
    r[a[i]] = a[i + 1];
  }
  return r;
};

ss.coalesce = function (a, b) {
  return ss.isValue(a) ? a : b;
};

if (typeof(window) == 'object') {
  // Browser-specific stuff that could go into the Web assembly, but that assembly does not have an associated JS file.
  if (!window.Element) {
  // IE does not have an Element constructor. This implementation should make casting to elements work.
    window.Element = function() {
    };
    window.Element.isInstanceOfType = function(instance) { return instance && typeof instance.constructor === 'undefined' && typeof instance.tagName === 'string'; };
  }

  if (!window.XMLHttpRequest) {
    window.XMLHttpRequest = function() {
      var progIDs = [ 'Msxml2.XMLHTTP', 'Microsoft.XMLHTTP' ];
  
      for (var i = 0; i < progIDs.length; i++) {
        try {
          var xmlHttp = new ActiveXObject(progIDs[i]);
          return xmlHttp;
        }
        catch (ex) {
        }
      }
  
      return null;
    };
  }

  ss.parseXml = function(markup) {
    try {
      if (DOMParser) {
        var domParser = new DOMParser();
        return domParser.parseFromString(markup, 'text/xml');
      }
      else {
        var progIDs = [ 'Msxml2.DOMDocument.3.0', 'Msxml2.DOMDocument' ];
          
        for (var i = 0; i < progIDs.length; i++) {
          var xmlDOM = new ActiveXObject(progIDs[i]);
          xmlDOM.async = false;
          xmlDOM.loadXML(markup);
          xmlDOM.setProperty('SelectionLanguage', 'XPath');
                  
          return xmlDOM;
        }
      }
    }
    catch (ex) {
    }
  
    return null;
  };
}

#include "TypeSystem\Type.js"

#include "Extensions\Object.js"

#include "Extensions\Boolean.js"

#include "Extensions\Number.js"

#include "Extensions\String.js"

#include "Extensions\Array.js"

#include "Extensions\RegExp.js"

#include "Extensions\Date.js"

#include "Extensions\Error.js"

#include "Extensions\Function.js"

#include "BCL\Debug.js"

#include "BCL\Enum.js"

#include "BCL\CultureInfo.js"

#include "BCL\IEnumerator.js"

#include "BCL\IEnumerable.js"

#include "BCL\ICollection.js"

#include "BCL\Nullable.js"

#include "BCL\IList.js"

#include "BCL\IDictionary.js"

#include "BCL\Int32.js"

#include "BCL\JsDate.js"

#include "BCL\ArrayEnumerator.js"

#include "BCL\ObjectEnumerator.js"

#include "BCL\Dictionary.js"

#include "BCL\IDisposable.js"

#include "BCL\StringBuilder.js"

#include "BCL\EventArgs.js"

#include "BCL\Exception.js"

#include "BCL\NotSupportedException.js"

#include "BCL\AggregateException.js"

#include "BCL\PromiseException.js"

#include "BCL\JsErrorException.js"

#include "BCL\IteratorBlockEnumerable.js"

#include "BCL\IteratorBlockEnumerator.js"

#include "BCL\Task.js"

#include "BCL\TaskCompletionSource.js"

#include "BCL\CancelEventArgs.js"

#include "BCL\App.js"

if (global.ss) {
  for (var n in ss) {
    if (ss.hasOwnProperty(n))
      global.ss[n] = ss[n];
  }
}
else {
  global.ss = ss;
}
