using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handyman.Types
{
    /// <summary>
    /// The base of a type containing <see cref="Member"/>s.
    /// </summary>
    public abstract class MemberedBaseType
    {
        public MemberedBaseType(string name, string baseClassName, IEnumerable<Member> members, string documentation)
        {
            this.Name = name;
            this.Members = new List<Member>(members);
            this.BaseClassName = baseClassName;
            this.Documentation = documentation;
            this.Namespace = string.Empty;
        }

        public string Name { get; set; }

        public IList<Member> Members { get; private set; }

        public string Documentation { get; private set; }

        public string BaseClassName { get; private set; }

        public string Namespace { get; set; }

        public bool HasNamespace => !string.IsNullOrWhiteSpace(this.Namespace);

        // TODO extract these styling string outside of this class
        public string ConstructorArguments => string.Join(", ", this.Members.Select(m => $"{m.TypeToken} {m.NamePascalCase}" ));
    }
}
