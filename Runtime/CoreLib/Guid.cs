
using System.Runtime.CompilerServices;

namespace System {

	/// <summary>
	/// The Guid data type which is mapped to the string type in Javascript.
	/// </summary>
	[ScriptNamespace("ss")]
    [ScriptName("Guid")]
	[Imported(ObeysTypeSystem = true)]
    public struct Guid : IEquatable<Guid>, IFormattable, IComparable<Guid>
    {
        [InlineCode("new {$System.Guid}()")]
        public Guid(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor _)
        {
		}

        [InlineCode("{$System.Guid}.parse({uuid})")]
        public Guid(string uuid) { 
        } 


        public static readonly Guid Empty = new Guid();
 
        [InlineCode("{$System.Guid}.newGuid()")]
        public static Guid NewGuid()
        {
            return default(Guid);
        }

        [InlineCode("{$System.Guid}.parse({uuid})")]
        public static Guid Parse(string uuid)
        {
            return Empty;
        }

        [InlineCode("{$System.Guid}.equalsT({this}, {other})")]
        public bool Equals(Guid other)
        {
            return false;
        }

        [InlineCode("{$System.Guid}.uuid)")]
        public string ToString(string format)
        {
            return null;
        }

        [InlineCode("{$System.Script}.compare({this}, {other})")]
        public int CompareTo(Guid other)
        {
            return 0;
        }
    }

}
