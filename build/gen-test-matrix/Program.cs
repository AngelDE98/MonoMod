using GenTestMatrix;
using GenTestMatrix.Models;

if (args is not [{ } githubOutputFile, ..var matrixOutNames] || matrixOutNames.Length < 1)
{
    await StdErr.WriteLineAsync("Takes 2+ arguments: GITHUB_OUTPUT, matrix output names");
    return 1;
}

await using var jobs = new JobsWriter(File.Open(githubOutputFile, FileMode.Append, FileAccess.Write), matrixOutNames);

foreach (var os in OS.OperatingSystems)
{
    if (!os.Enabled) continue;

    if (os.HasSystemMono && os.Arch.Any(a => a.IsRunnerArch && a.Enabled))
    {
        // this OS has a system Mono, emit a job for that
        await jobs.AddJob(new()
        {
            Title = $"System Mono on {os.Name}",
            OS = os,
            Arch = os.Arch.First(a => a.IsRunnerArch).RidName,
            Dotnet = new()
            {
                Name = "Mono",
                Id = "sysmono",
                NeedsRestore = true, // Monos always need restore
                IsMono = true,
                IsSystemMono = true,
                TFM = Constants.Mono.NonCoreTFM,
            }
        });
    }

    foreach (var arch in os.Arch)
    {
        if (!arch.Enabled) continue;
        var rid = $"{os.RidName}-{arch.RidName}";

        foreach (var dotnet in Dotnet.Versions)
        {
            if (!dotnet.Enabled) continue;

            // skip frameworks if the OS doesn't support framework
            if (dotnet.IsFramework && !os.HasFramework) continue;

            // skip runtime if it doesn't support the current RID
            if (!dotnet.RIDs.Contains(rid)) continue;

            var title = $"{dotnet.Name} {arch.RidName} on {os.Name}";
            var jobDotnet = dotnet with { MonoPackageSource = null, MonoPackageVersion = null }; // make sure we don't accidentally serialize these for non-Mono jobs
            if (dotnet.HasPGO)
            {
                // this runtime supports PGO, generate 2 jobs: one with it enabled, and one without
                await jobs.AddJob(new()
                {
                    Title = title + " (PGO Off)",
                    OS = os,
                    Dotnet = jobDotnet,
                    Arch = arch.RidName,
                    UsePGO = false,
                });
                await jobs.AddJob(new()
                {
                    Title = title + " (PGO On)",
                    OS = os,
                    Dotnet = jobDotnet,
                    Arch = arch.RidName,
                    UsePGO = true,
                });
            }
            else
            {
                // this runtime doesn't support PGO, only add the one job
                await jobs.AddJob(new()
                {
                    Title = title,
                    OS = os,
                    Dotnet = dotnet,
                    Arch = arch.RidName,
                });
            }

            // if this OS specifies a .NET Mono package, add a job for it
            if (dotnet is { MonoPackageSource: not null, MonoPackageVersion: not null } && false) // TODO: We currently have a lot of problems on .NET Mono, they need to be fixed
            {
                var fillDict = new Dictionary<string, string>()
                {
                    [Constants.Tmpl.RID] = rid,
                    [Constants.Tmpl.TFM] = dotnet.TFM,
                    [Constants.Tmpl.DllPre] = os.DllPrefix,
                    [Constants.Tmpl.DllPost] = os.DllSuffix,
                };

                var packageName = Template.Fill(Constants.Mono.Package.NameTmpl, fillDict);
                var libPath = Template.Fill(Constants.Mono.Package.LibPathTmpl, fillDict);
                var dllPath = Template.Fill(Constants.Mono.Package.DllPathTmpl, fillDict);

                var monoDotnet = dotnet with
                {
                    Name = $".NET Mono {dotnet.Sdk}",
                    Sdk = null,
                    Id = $"netmono{dotnet.MonoPackageVersion}",
                    HasPGO = false,
                    IsMono = true,
                    NeedsRestore = true, // Mono always NeedsRestore
                    MonoPackageName = packageName,
                    MonoLibPath = libPath,
                    MonoDllPath = dllPath,
                };

                await jobs.AddJob(new()
                {
                    Title = $"{monoDotnet.Name} {arch.RidName} on {os.Name}",
                    OS = os,
                    Arch = arch.RidName,
                    Dotnet = monoDotnet,
                });
            }
        }

        // TODO: Unity Mono
    }
}

return 0;
