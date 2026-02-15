using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Leap.Forward.Idle.CodeGen
{
    /// <summary>
    /// Generates partial view model classes for types annotated with [IdleViewModelAttribute], enabling property
    /// binding and UI event handling in Unity UI Toolkit applications.
    /// </summary>
    /// <remarks>This generator creates properties for fields marked with [IdlePropertyAttribute], ensuring
    /// proper encapsulation and property change notification. It also wires up methods annotated with
    /// [ClickHandlerAttribute] to UI button click events and supports property update dependencies via
    /// [PropertyUpdaterAttribute]. Use this generator to simplify data binding and event handling in Unity UI
    /// workflows. Generated classes are intended for use with Unity's UIDocument and VisualElement data binding
    /// features.</remarks>
    [Generator]
    public class IdleViewModelGenerator : IIncrementalGenerator
    {
        private static readonly DiagnosticDescriptor PrivateFieldRule = new DiagnosticDescriptor(
                id: "LFIDLE001",
                title: "Field must be private or protected",
                messageFormat: "The field '{0}' must be private or protected to use [IdleProperty]",
                category: "Design",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor PropertyNameCollisionRule = new DiagnosticDescriptor(
                id: "LFIDLE002",
                title: "Property name conflict",
                messageFormat: "Multiple fields are generating the same property name '{0}'",
                category: "Design",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Filter for classes that might have our attribute
            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (s, _) => s is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                    transform: (ctx, _) => GetSemanticTargetForGeneration(ctx))
                .Where(t => t is not null);

            context.RegisterSourceOutput(classDeclarations, (spc, source) => Execute(spc, source!));
        }

        private static INamedTypeSymbol? GetSemanticTargetForGeneration(GeneratorSyntaxContext ctx)
        {
            var classDeclaration = (ClassDeclarationSyntax)ctx.Node;
            var symbol = ctx.SemanticModel.GetDeclaredSymbol(classDeclaration);

            return symbol?.GetAttributes().Any(a =>
                a.AttributeClass?.ToDisplayString() == "Leap.Forward.IdleHelpers.IdleViewModelAttribute") == true
                ? symbol as INamedTypeSymbol : null;
        }

        private static void Execute(SourceProductionContext spc, INamedTypeSymbol classSymbol)
        {
            var className = classSymbol.Name;
            var ns = classSymbol.ContainingNamespace.ToDisplayString();
            var hasNS = !classSymbol.ContainingNamespace.IsGlobalNamespace;

            var sourceBuilder = new StringBuilder();
            if (hasNS)
            {
                sourceBuilder.AppendLine($"namespace {ns}");
                sourceBuilder.AppendLine("{");
            }
            sourceBuilder.AppendLine($"    public partial class {className}");
            sourceBuilder.AppendLine("    {");

            var idleProperties = new Dictionary<string, IFieldSymbol>();
            var idlePropertyFields = new Dictionary<string, string>();

            foreach (var field in classSymbol.GetMembers().OfType<IFieldSymbol>())
            {
                if (field.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "Leap.Forward.IdleHelpers.IdlePropertyAttribute"))
                {
                    if (field.DeclaredAccessibility == Accessibility.Public)
                    {
                        spc.ReportDiagnostic(Diagnostic.Create(PrivateFieldRule, field.Locations[0], field.Name));
                        continue;
                    }

                    string propertyName = Regex.Replace(field.Name, @"^(_|m_)", "");
                    propertyName = char.ToUpper(propertyName[0]) + propertyName.Substring(1);

                    if (idleProperties.ContainsKey(propertyName))
                    {
                        spc.ReportDiagnostic(Diagnostic.Create(PropertyNameCollisionRule, field.Locations[0], propertyName));
                        continue;
                    };
                    idleProperties.Add(propertyName, field);
                    idlePropertyFields.Add(field.Name, propertyName);
                }
            }

            var dependencyMap = new Dictionary<string, List<string>>(); // PropertyName -> List of MethodNames
            foreach (var method in classSymbol.GetMembers().OfType<IMethodSymbol>())
            {
                if (method.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "Leap.Forward.IdleHelpers.PropertyUpdaterAttribute"))
                {
                    // Get the syntax of the method body
                    var syntax = method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;
                    if (syntax?.Body == null) continue;

                    // Find all identifiers used in the body that match our property names OR field names
                    var accessedIdentifiers = syntax.Body.DescendantNodes()
                        .OfType<IdentifierNameSyntax>()
                        .Select(id => id.Identifier.ValueText)
                        .Distinct();

                    foreach (var id in accessedIdentifiers)
                    {
                        // If they accessed the field or the property, they depend on that property
                        string? targetProp = null;
                        if (idleProperties.ContainsKey(id)) targetProp = id;
                        else if (idlePropertyFields.ContainsKey(id)) targetProp = idlePropertyFields[id];

                        if (targetProp != null)
                        {
                            if (!dependencyMap.ContainsKey(targetProp)) dependencyMap[targetProp] = new List<string>();
                            dependencyMap[targetProp].Add(method.Name);
                        }
                    }
                }
            }

            foreach (var kv in idleProperties)
            {
                var propertyName = kv.Key;
                var field = kv.Value;

                string typeName = field.Type.ToDisplayString();
                List<string> updateMethods;
                var methodRemarks = dependencyMap.TryGetValue(propertyName, out updateMethods) && updateMethods.Count > 0
                    ? $"Setting this property will also trigger updates to: {string.Join(", ", updateMethods)}."
                    : "No dependent updates.";

                sourceBuilder.AppendLine($@"
        /// <summary>
        /// Wrapper property for the field '{field.Name}' marked with [IdleProperty]. This property raises
        /// PropertyChanged events and triggers any dependent update methods when set.
        /// </summary>
        /// <remarks>
        /// {methodRemarks}
        /// </remarks>
        [Unity.Properties.CreateProperty]
        public {typeName} {propertyName}
        {{
            get => {field.Name};
            set
            {{
                if (!System.Collections.Generic.EqualityComparer<{typeName}>.Default.Equals({field.Name}, value))
                {{
                    {field.Name} = value;
                    OnPropertyChanged(nameof({propertyName}));");
                if (dependencyMap.TryGetValue(propertyName, out updateMethods))
                {
                    foreach (var method in updateMethods)
                    {
                        sourceBuilder.AppendLine($"                    {method}();");
                    }
                }
                    sourceBuilder.AppendLine($@"
                }}
            }}
        }}");
            }

           
            sourceBuilder.AppendLine("        public void BindTo(UnityEngine.UIElements.UIDocument document)");
            sourceBuilder.AppendLine("        {");
            sourceBuilder.AppendLine("            var root = document.rootVisualElement;");
            sourceBuilder.AppendLine("            if (root == null) return;");
            sourceBuilder.AppendLine("            root.dataSource = this;");

            // Find all methods with [ClickHandler("Name")]
            foreach (var method in classSymbol.GetMembers().OfType<IMethodSymbol>())
            {
                var clickAttr = method.GetAttributes().FirstOrDefault(a =>
                    a.AttributeClass?.ToDisplayString() == "Leap.Forward.IdleHelpers.ClickHandlerAttribute");

                if (clickAttr != null && clickAttr.ConstructorArguments.Length > 0)
                {
                    string elementName = clickAttr.ConstructorArguments[0].Value?.ToString() ?? "";

                    sourceBuilder.AppendLine($@"
            var el_{method.Name} = UnityEngine.UIElements.UQueryExtensions.Q<UnityEngine.UIElements.Button>(root, ""{elementName}"");
            if (el_{method.Name} != null) 
                el_{method.Name}.clicked += {method.Name};
            else 
                UnityEngine.Debug.LogError($""[IdleGenerator] Could not find Button with name '{elementName}' in {{document.name}}"");");
                }
            }

            sourceBuilder.AppendLine("        }");
            sourceBuilder.AppendLine("    }");
            if (hasNS)
                sourceBuilder.AppendLine("}");

            spc.AddSource($"{className}_Binding.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }
    }
}
