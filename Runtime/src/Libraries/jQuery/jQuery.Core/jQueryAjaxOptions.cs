// jQueryAjaxOptions.cs
// Script#/Libraries/jQuery/Core
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System;
using System.Collections.Generic;
using System.Html;
using System.Runtime.CompilerServices;

namespace jQueryApi {

    /// <summary>
    /// Represents Ajax request settings or options.
    /// </summary>
    [Imported]
	[Serializable]
    public sealed class jQueryAjaxOptions {

        /// <summary>
        /// Initializes an empty instance of a jQueryAjaxOptions object.
        /// </summary>
        public jQueryAjaxOptions() {
        }

        /// <summary>
        /// Gets or sets whether the request is async.
        /// </summary>
        public bool Async {
            get {
                return false;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets the callback to invoke before the request is sent.
        /// </summary>
        public AjaxSendingCallback BeforeSend {
            get {
                return null;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets whether the request can be cached.
        /// </summary>
        public bool Cache {
            get {
                return false;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets the callback invoked after the request is completed
        /// and success or error callbacks have been invoked.
        /// </summary>
        public AjaxCompletedCallback Complete {
            get {
                return null;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets the content type of the data sent to the server.
        /// </summary>
        public string ContentType {
            get {
                return null;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets the element that will be the context for the request.
        /// </summary>
        public object Context {
            get {
                return null;
            }
            set {
            }
        }

		/// <summary>
		/// A map of dataType-to-dataType converters. Each converter's value is a function that returns the transformed value of the response.
		/// </summary>
		public JsDictionary<string, Func<string, object>> Converters {
			get;
			set;
		}

        /// <summary>
        /// Gets or sets the data to be sent to the server.
        /// </summary>
        public TypeOption<JsDictionary<string, object>, Array> Data {
            get {
                return null;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets the data type expected in response from the server.
        /// </summary>
        public string DataType {
            get {
                return null;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets the callback to be invoked if the request fails.
        /// </summary>
        public AjaxErrorCallback Error {
            get {
                return null;
            }
            set {
            }
        }

		/// <summary>
		/// Gets or sets the function to be used to handle the raw response data of XMLHttpRequest.
		/// </summary>
		public AjaxDataFilterCallback DataFilter {
			get {
				return null;
			}
			set {
			}
		}

        /// <summary>
        /// Gets or sets whether to trigger global event handlers for this Ajax request.
        /// </summary>
        public bool Global {
            get {
                return false;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets whether the request is successful only if its been modified since
        /// the last request.
        /// </summary>
        public bool IfModified {
            get {
                return false;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets whether the current environment should be treated as a local
        /// environment (eg. when the page is loaded using file://).
        /// </summary>
        public bool IsLocal {
            get {
                return false;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets the callback parameter name to use for JSONP requests.
        /// </summary>
        public string Jsonp {
            get {
                return null;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets the callback name to use for JSONP requests.
        /// </summary>
        public string JsonpCallback {
            get {
                return null;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets the mime type of the request.
        /// </summary>
        public string MimeType {
            get {
                return null;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets the password to be used for an HTTP authentication request.
        /// </summary>
        public string Password {
            get {
                return null;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets whether the data passed in will be processed.
        /// </summary>
        public bool ProcessData {
            get {
                return false;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets how to handle character sets for script and JSONP requests.
        /// </summary>
        public string ScriptCharset {
            get {
                return null;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets the function to invoke upon successful completion of the request.
        /// </summary>
        public AjaxRequestCallback Success {
            get {
                return null;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets the timeout in milliseconds for the request.
        /// </summary>
        public int Timeout {
            get {
                return 0;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets if you want to use traditional parameter serialization.
        /// </summary>
        public bool Traditional {
            get {
                return false;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets the type or HTTP verb associated with the request.
        /// </summary>
        public string Type {
            get {
                return null;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets the URL to be requested.
        /// </summary>
        public string Url {
            get {
                return null;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets the name of the user to use in a HTTP authentication request.
        /// </summary>
        public string Username {
            get {
                return null;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets the function creating the XmlHttpRequest instance.
        /// </summary>
        [ScriptName("xhr")]
        public XmlHttpRequestCreator XmlHttpRequestCreator {
            get {
                return null;
            }
            set {
            }
        }

        /// <summary>
        /// Gets or sets a set of additional name/value pairs set of the XmlHttpRequest
        /// object.
        /// </summary>
        public JsDictionary<string, object> XhrFields {
            get {
                return null;
            }
            set {
            }
        }
    }
}
