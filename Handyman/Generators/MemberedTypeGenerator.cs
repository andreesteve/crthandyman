using System.Collections.Generic;
using Handyman.Types;
using Stubble.Core.Builders;

namespace Handyman.Generators
{
    public class MemberedTypeGenerator
    {
        public string GenerateSyntax(MemberedBaseType memberedType)
        {
            var stubble = new StubbleBuilder().Build();
            const string Template = @"
{{#HasNamespace}}
namespace {{ Namespace }}
{
{{/HasNamespace}}
    /// <summary>
    /// {{ Documentation }}
    /// </summary>
    public class {{ Name }} : {{ BaseClassName }}
    {
        /// <summary>
        /// Initializes a new instance of the <see cref=""{{ Name }}""/> class.
        /// </summary>
{{ #Members }}
        /// <param name=""{{ Name }}"">{{ Documentation }}</param>
{{ /Members }}
        public {{ Name }}({{ ConstructorArguments }})
        {
{{ #Members }}
            this.{{ NameCamelCase }} = {{ NamePascalCase }};
{{ /Members }}
        }

{{ #Members }}
        /// <summary>
        /// {{ GetSetterDocumentation }}
        /// </summary>
        public {{ TypeToken }} {{ NameCamelCase }} { get; private set; }
{{ /Members }}
    }
{{#HasNamespace}}
}
{{/HasNamespace}}
";

            return stubble.Render(Template, memberedType);
        }
    }
}