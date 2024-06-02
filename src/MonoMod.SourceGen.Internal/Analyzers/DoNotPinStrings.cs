using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace MonoMod.SourceGen.Internal.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotPinStrings : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor PinningStringsIsDangerous = new(
            "MMA001",
            "Do not pin strings, as it may crash some older Mono runtimes",
            "Do not pin strings, as it may crash some older Mono runtimes (see docs/RuntimeIssueNotes.md). Pin a span instead.",
            "Stability",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(PinningStringsIsDangerous);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods",
            Justification = "Roslyn always passes a non-null context")]
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.ReportDiagnostics); // if generated code pins strings, we still want to report it

            context.RegisterOperationAction(ctx =>
            {
                var op = (IVariableDeclaratorOperation)ctx.Operation;

                if (!op.Symbol.IsFixed)
                {
                    // we only care about fixed variables
                    return;
                }

                var initializer = op.GetVariableInitializer()?.Value;
                if (initializer is null)
                {
                    // no initializer, nothing to do
                    return;
                }

                if (initializer.IsImplicit)
                {
                    initializer = initializer.ChildOperations.Any() ? initializer.ChildOperations.First() : null;
                }

                if (initializer is null || initializer.Type is null)
                {
                    // no initializer, nothing to do
                    return;
                }

                if (initializer.Type.SpecialType is SpecialType.System_String)
                {
                    // the initializer of the fixed variable is a string, report it
                    ctx.ReportDiagnostic(Diagnostic.Create(PinningStringsIsDangerous, initializer.Syntax.GetLocation()));
                }

            }, OperationKind.VariableDeclarator);
        }
    }
}
