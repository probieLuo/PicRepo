using PicRepo.Client.Models;
using SharpYaml;
using SharpYaml.Serialization;
using System.IO;
using System.Text;
using System.Windows;

namespace PicRepo.Client.Helper;

public class AppSettings : IAppSettings
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    private static string _path => Path.GetFullPath("config.yaml");

    public string Theme { get; set; } = "System";
    public bool MinimizeToTrayOnClose { get; set; } = true;
    public bool MiniWindowingOnClose { get; set; } = true;
    public List<IPicRepoConfig> PicRepoConfigs { get; set; } = [];
    public CopyType? CopyType { get; set; } = Models.CopyType.URL;

    public void LoadConfig()
    {
        try
        {
            if (File.Exists(_path))
            {
                var options = new YamlSerializerOptions();
                var json = File.ReadAllText(_path, Encoding.UTF8);
                var cfg = YamlSerializer.Deserialize<AppSettings>(json, options);
                ObjectHelper.DeepCopy(cfg, this);
            }

            SaveConfig();
        }
        catch (Exception ex)
        {
            _logger.Error(ex);
        }
    }

    public void SaveConfig()
    {
        try
        {
            var options = new YamlSerializerOptions();
            var json = YamlSerializer.Serialize(this, options);
            File.WriteAllText(_path, json, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            _logger.Error(ex);
        }
    }
}

public interface IAppSettings
{
    string Theme { get; set; }
    bool MinimizeToTrayOnClose { get; set; }
    bool MiniWindowingOnClose { get; set; }
    List<IPicRepoConfig> PicRepoConfigs { get; set; }
    CopyType? CopyType { get; set; }

    void LoadConfig();

    void SaveConfig();
}

[YamlPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[YamlDerivedType(typeof(GitHubPicRepoConfig), "GitHubPicRepoConfig")]
[YamlDerivedType(typeof(GiteePicRepoConfig), "GiteePicRepoConfig")]
public interface IPicRepoConfig
{
    PicRepoType PicRepoType { get; set; }
    string Name { get; set; }
    bool IsDefault { get; set; }
}
public enum PicRepoType
{
    GitHub = 0,
    Gitee = 1,
}

public class GitHubPicRepoConfig : BindableBase, IPicRepoConfig
{
    public PicRepoType PicRepoType { get; set; } = PicRepoType.GitHub;
    public bool IsDefault { get; set; }
    public string Name { get; set; } = "NewGitHubConfig";
    public string Token { get; set; } = string.Empty;
    public string ProductHeader { get; set; } = "PicRepo";
    public string Owner { get; set; } = string.Empty;
    public string Repo { get; set; } = string.Empty;
    public string Branch { get; set; } = "main";
}

public class GiteePicRepoConfig : BindableBase, IPicRepoConfig
{
    public PicRepoType PicRepoType { get; set; } = PicRepoType.Gitee;
    public bool IsDefault { get; set; }
    public string Name { get; set; } = "NewGiteeConfig";
    public string Token { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Repo { get; set; } = string.Empty;
    public string Branch { get; set; } = "main";
}