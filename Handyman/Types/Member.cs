using Microsoft.CodeAnalysis;
using Handyman.Generators;

namespace Handyman.Types
{
    public class Member
    {
        public Member(string name, ITypeSymbol type, string documentation = "", bool canWrite = false)
        {
            this.Name = name;
            this.Type = type;
            this.Documentation = documentation;
            this.CanWrite = canWrite;
        }

        public string Name { get; private set; }

        // TODO extract these styling string outside of this class
        public string NameCamelCase => StringCasing.ToFirstCharUpper(this.Name);
        public string NamePascalCase => StringCasing.ToFirstCharLower(this.Name);
        public string GetSetterDocumentation => $"{(this.CanWrite ? "Gets or sets" : "Gets")} {this.NamePascalCase}.";

        public string TypeToken => this.Type.ToDisplayString();

        public ITypeSymbol Type { get; private set; }

        public string Documentation { get; private set; }

        public bool CanWrite { get; private set; }
    }
}
