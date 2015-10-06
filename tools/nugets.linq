<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

void Main()
{
	var toolsDiretory = Path.GetDirectoryName(Util.CurrentQueryPath);
	var docsDirectory = Directory.GetParent(toolsDiretory).FullName;
	var nuget = Path.Combine(toolsDiretory, "nuget.exe");
	var packagesDirectory = Path.Combine(docsDirectory, "packages");

	var solutionFiles = Directory.EnumerateFiles(docsDirectory, "*.sln", SearchOption.AllDirectories);

	Directory.SetCurrentDirectory(docsDirectory);
	try
	{
		Parallel.ForEach(solutionFiles,
		new ParallelOptions() { MaxDegreeOfParallelism = 10 },
		(solutionFile) =>
		{
			Execute(nuget, "restore " + solutionFile + " -packagesDirectory " + packagesDirectory);
			Execute(nuget, "update " + solutionFile + " -safe -repositoryPath " + packagesDirectory);
		}
		);
	}
	catch (Exception ex)
	{
		Debug.WriteLine(ex.InnerException.Message);
	}
}

void Execute(string file, string arguments)
{
	var startInfo = new ProcessStartInfo();
	startInfo.FileName = file;
	startInfo.CreateNoWindow = true;
	startInfo.UseShellExecute = false;
	startInfo.RedirectStandardOutput = true;
	startInfo.RedirectStandardError = true;
	startInfo.Arguments = arguments;
	var process = Process.Start(startInfo);
	process.WaitForExit();
	var error = process.StandardError.ReadToEnd();
	if (!string.IsNullOrWhiteSpace(error))
	{
		throw new Exception(error);
	}
	var result = process.StandardOutput.ReadToEnd();
	Debug.WriteLine(result);
}

