using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class PostprocessBuild : IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        var parentDirectory = Directory.GetParent(report.summary.outputPath);
        if (!parentDirectory.Exists) return;
        
        CopyCertificatesAtPath("Assets", parentDirectory.FullName);
    }

    private void CopyCertificatesAtPath(string certificatesPath, string buildPath)
    {
        var certificates = Directory.GetFiles(certificatesPath, "*.pfx", SearchOption.AllDirectories);

        if (certificates.Length <= 0) return;
        
        var buildPluginsDirectory = $"{buildPath}/Plugins/";
        if (!Directory.Exists(buildPluginsDirectory)) Directory.CreateDirectory(buildPluginsDirectory);

        foreach (var certificate in certificates)
        {
            var filename = Path.GetFileName(certificate);
            FileUtil.CopyFileOrDirectory(certificate, buildPluginsDirectory + $"{filename}");
        }
    }
}
