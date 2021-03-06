using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Pinspaces.Shell.Git
{
    public class GitInterop
    {
        private readonly string repositoryPath;

        public GitInterop(string repositoryPath)
        {
            this.repositoryPath = repositoryPath;
        }

        public static GitStatusCode CharToStatusCode(char value)
        {
            return value switch
            {
                'M' => GitStatusCode.Modified,
                'A' => GitStatusCode.Added,
                'D' => GitStatusCode.Deleted,
                'R' => GitStatusCode.Renamed,
                'C' => GitStatusCode.Copied,
                'U' => GitStatusCode.Updated,
                '?' => GitStatusCode.Untracked,
                _ => GitStatusCode.Unmodified,
            };
        }

        public async Task<bool> IsGitRepositoryAsync()
        {
            return !IsOutputFatal(await Execute("rev-parse"));
        }

        public async Task<IList<GitStatusEntry>> StatusAsync()
        {
            var list = new List<GitStatusEntry>();
            var output = await Execute("status -z");
            if (IsOutputFatal(output))
            {
                return list;
            }
            var lines = output.Split('\0');
            foreach (var line in lines)
            {
                if (line.Length < 3 || (line[1] != ' ' && line[2] != ' '))
                {
                    if (list.Count > 0)
                    {
                        list[^1].FromPath = line;
                    }
                    continue;
                }
                var entryLine = line[2] == ' ' ? line : ' ' + line;
                var entry = new GitStatusEntry
                {
                    IndexStatus = CharToStatusCode(entryLine[0]),
                    ToPath = entryLine[3..],
                    WorkTreeStatus = CharToStatusCode(entryLine[1])
                };
                list.Add(entry);
            }
            return list;
        }

        private static bool IsOutputFatal(string output)
        {
            return output.StartsWith("fatal:");
        }

        private async Task<string> Execute(string commandLine)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                Arguments = commandLine,
                CreateNoWindow = true,
                FileName = "git.exe",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = repositoryPath
            });
            process.WaitForExit();

            return process.ExitCode switch
            {
                0 => await process.StandardOutput.ReadToEndAsync(),
                _ => await process.StandardError.ReadToEndAsync()
            };
        }
    }
}
