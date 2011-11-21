﻿namespace ComImports.ActiveScript {
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using ComTypes = System.Runtime.InteropServices.ComTypes;

    public abstract class BaseActiveScriptSite : IActiveScriptSite {

        private const int TYPE_E_ELEMENTNOTFOUND = unchecked((int)(0x8002802B));

        private ActiveScriptException _lastException = null;

        /// <summary>
        /// Gets or sets the host-defined document version string.
        /// </summary>
        public string DocumentVersion { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseActiveScriptSite"/> class.
        /// </summary>
        public BaseActiveScriptSite()
            : this(DateTime.UtcNow.ToString("o")) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseActiveScriptSite"/> class.
        /// </summary>
        /// <param name="documentVersion">The host-defined document version string.</param>
        public BaseActiveScriptSite(string documentVersion) {
            DocumentVersion = documentVersion;
        }

        /// <summary>
        /// Retrieves the locale identifier associated with the host's user interface. The scripting
        /// engine uses the identifier to ensure that error strings and other user-interface elements
        /// generated by the engine appear in the appropriate language.
        /// </summary>
        /// <param name="lcid">A variable that receives the locale identifier for user-interface
        /// elements displayed by the scripting engine.</param>
        public void GetLCID(out int lcid) {
            lcid = CultureInfo.CurrentCulture.LCID;
        }

        /// <summary>
        /// Allows the scripting engine to obtain information about an item added with the
        /// IActiveScript.AddNamedItem method.
        /// </summary>
        /// <param name="name">The name associated with the item, as specified in the
        /// IActiveScript.AddNamedItem method.</param>
        /// <param name="returnMask">A bit mask specifying what information about the item should be
        /// returned. The scripting engine should request the minimum amount of information possible
        /// because some of the return parameters (for example, ITypeInfo) can take considerable
        /// time to load or generate.</param>
        /// <param name="item">A variable that receives a pointer to the IUnknown interface associated
        /// with the given item. The scripting engine can use the IUnknown.QueryInterface method to
        /// obtain the IDispatch interface for the item. This parameter receives null if returnMask
        /// does not include the ScriptInfo.IUnknown value. Also, it receives null if there is no
        /// object associated with the item name; this mechanism is used to create a simple class when
        /// the named item was added with the ScriptItem.CodeOnly flag set in the
        /// IActiveScript.AddNamedItem method.</param>
        /// <param name="typeInfo">A variable that receives a pointer to the ITypeInfo interface
        /// associated with the item. This parameter receives null if returnMask does not include the
        /// ScriptInfo.ITypeInfo value, or if type information is not available for this item. If type
        /// information is not available, the object cannot source events, and name binding must be
        /// realized with the IDispatch.GetIDsOfNames method. Note that the ITypeInfo interface
        /// retrieved describes the item's coclass (TKIND_COCLASS) because the object may support
        /// multiple interfaces and event interfaces. If the item supports the IProvideMultipleTypeInfo
        /// interface, the ITypeInfo interface retrieved is the same as the index zero ITypeInfo that
        /// would be obtained using the IProvideMultipleTypeInfo.GetInfoOfIndex method.</param>
        public void GetItemInfo(string name, ScriptInfoFlags returnMask, out object item, out IntPtr typeInfo) {
            if ((returnMask & ScriptInfoFlags.IUnknown) > 0) {
                item = GetItem(name);
                if (item == null) throw new COMException(string.Format("{0} not found.", name), TYPE_E_ELEMENTNOTFOUND);
            } else {
                item = null;
            }

            if ((returnMask & ScriptInfoFlags.ITypeInfo) > 0) {
                typeInfo = GetTypeInfo(name);
                if (typeInfo == null) throw new COMException(string.Format("{0} not found.", name), TYPE_E_ELEMENTNOTFOUND);
            } else {
                typeInfo = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Allows the scripting engine to obtain information about an item added with the
        /// IActiveScript.AddNamedItem method.
        /// </summary>
        /// <param name="name">The name associated with the item, as specified in the
        /// IActiveScript.AddNamedItem method.</param>
        public abstract object GetItem(string name);

        /// <summary>
        /// Allows the scripting engine to obtain information about an item added with the
        /// IActiveScript.AddNamedItem method.  Gets the COM ITypeInfo.
        /// </summary>
        /// <param name="name">The name associated with the item, as specified in the
        /// IActiveScript.AddNamedItem method.</param>
        public abstract IntPtr GetTypeInfo(string name);

        /// <summary>
        /// Retrieves a host-defined string that uniquely identifies the current document version. If
        /// the related document has changed outside the scope of Windows Script (as in the case of an
        /// HTML page being edited with Notepad), the scripting engine can save this along with its
        /// persisted state, forcing a recompile the next time the script is loaded.
        /// </summary>
        /// <param name="versionString">The host-defined document version string.</param>
        public void GetDocVersionString(out string versionString) {
            versionString = DocumentVersion;
        }

        /// <summary>
        /// Informs the host that the script has completed execution.
        /// </summary>
        /// <param name="result">A variable that contains the script result, or null if the script
        /// produced no result.</param>
        /// <param name="exceptionInfo">Contains exception information generated when the script
        /// terminated, or null if no exception was generated.</param>
        public virtual void OnScriptTerminate(object result, ComTypes.EXCEPINFO exceptionInfo) {
        }

        /// <summary>
        /// Informs the host that the scripting engine has changed states.
        /// </summary>
        /// <param name="scriptState">Indicates the new script state.</param>
        public virtual void OnStateChange(ScriptState scriptState) {
        }

        /// <summary>
        /// Informs the host that an execution error occurred while the engine was running the script.
        /// </summary>
        /// <param name="scriptError">A host can use this interface to obtain information about the
        /// execution error.</param>
        public void OnScriptError(IActiveScriptError scriptError) {
            _lastException = ActiveScriptException.Create(scriptError);
            OnScriptError(_lastException);
        }

        /// <summary>
        /// Gets and resets the last exception.  Returns null for none.
        /// </summary>
        protected ActiveScriptException GetAndResetLastException() {
            var temp = _lastException;
            _lastException = null;
            return temp;
        }

        /// <summary>
        /// Informs the host that an execution error occurred while the engine was running the script.
        /// </summary>
        /// <param name="exception">The exception.</param>
        protected virtual void OnScriptError(ActiveScriptException exception) {
        }

        /// <summary>
        /// Informs the host that the scripting engine has begun executing the script code.
        /// </summary>
        public virtual void OnEnterScript() {
        }

        /// <summary>
        /// Informs the host that the scripting engine has returned from executing script code.
        /// </summary>
        public virtual void OnLeaveScript() {
        }
    }
}
