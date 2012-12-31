using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// Modifiers of a Tool Event
    /// </summary>
    [Imported, IgnoreNamespace]
    public class EventModifiers
    {
        /// <summary>
        /// True if the Shift key was pressed when the event happened, false otherwise
        /// </summary>
        public bool Shift;

        /// <summary>
        /// True if the Control key was pressed when the event happened, false otherwise
        /// </summary>
        public bool Control;

        /// <summary>
        /// True if the Option (Mac) key was pressed when the event happened, false otherwise
        /// </summary>
        public bool Option;

        /// <summary>
        /// True if the Command (Mac) key was pressed when the event happened, false otherwise
        /// </summary>
        public bool Command;

        /// <summary>
        /// True if the CapsLock key was pressed when the event happened, false otherwise
        /// </summary>
        public bool CapsLock;
    }
}