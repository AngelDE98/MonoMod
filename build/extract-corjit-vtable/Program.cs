using ClangSharp;
using ClangSharp.Interop;

if (args is not [{ } dotnetRepoRoot, ..])
{
    await StdErr.WriteLineAsync("takes 1 arguments: dotnetRepoRoot");
    return 1;
}

using var index = CXIndex.Create();
var coreclrInc = Path.Combine(dotnetRepoRoot, "src", "coreclr", "inc");
var corjitScanH = Path.Combine(coreclrInc, "corjit-scan.h");
var cxtu = CXTranslationUnit.CreateFromSourceFile(index, corjitScanH,
    ["-x", "c++", "-std=c++17"],
    [CXUnsavedFile.Create(corjitScanH, """
    #include "corinfo.h"
    #include "corjit.h"
    """)]);
using var tu = TranslationUnit.GetOrCreate(cxtu);

foreach (var decl in tu.TranslationUnitDecl.Decls)
{
    if (decl.Kind is not CX_DeclKind.CX_DeclKind_CXXRecord) continue;
    var record = (CXXRecordDecl)decl.MostRecentDecl;
    if (record.Name is not "ICorJitInfo") continue;

    record.Extent.Start.GetSpellingLocation(out var file, out var line, out var column, out _);
    await StdOut.WriteLineAsync($"Found ICorJitInfo at {file.Name}:{line}:{column}");

    // we're now looking at ICorJitInfo, lets try to extract its vtable

    // need to collect its bases so we can enumerate them in order
    var toProcess = new Queue<CXXRecordDecl>();
    var baseStack = new Stack<CXXRecordDecl>();
    toProcess.Enqueue(record);
    while (toProcess.TryDequeue(out var cur))
    {
        baseStack.Push(cur);
        for (var i = cur.Bases.Count - 1; i >= 0; i--)
        {
            var @base = cur.Bases[i];
            toProcess.Enqueue((CXXRecordDecl)@base.Referenced.MostRecentDecl);
        }
    }

    // now that we have all bases in order, lets go through and enumerate them
    var vtblIdx = 0;
    while (baseStack.TryPop(out record))
    {
        record.Extent.Start.GetSpellingLocation(out file, out line, out column, out _);
        await StdOut.WriteLineAsync("//");
        await StdOut.WriteLineAsync($"// {Path.GetRelativePath(dotnetRepoRoot, file.Name.CString)}:{line}");
        await StdOut.WriteLineAsync($"// class {record.Name}");

        foreach (var method in record.Methods)
        {
            if (!method.IsVirtual) continue;

            var methodStr = $"{method.ReturnType.ToString()} {method.Name}(";
            foreach (var param in method.Parameters)
            {
                if (methodStr[^1] is not '(')
                {
                    methodStr += ", ";
                }
                methodStr += param.Type.ToString();
            }

            await StdOut.WriteLineAsync($"// {vtblIdx++:X2}: {methodStr})");
            if (method.Name is "allocMem")
            {
                await StdOut.WriteLineAsync($"public const int AllocMemIndex = 0x{vtblIdx - 1:X};");
            }
        }
    }

    await StdOut.WriteLineAsync("");
    await StdOut.WriteLineAsync($"public const int TotalVtableCount = 0x{vtblIdx:X};");


    break;
}

return 0;