using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mobile.Secrets.SecretCollectorStrategies;
using Task = Microsoft.Build.Utilities.Task;

namespace Mobile.Secrets
{

    public class SecretTask : Task
    {
        public string SettingsPath { get; set; } = "";

        public string OutputPath { get; set; } = "";

        public string NamespaceName { get; set; } = "";

        [Output] public ITaskItem[] GeneratedFiles { get; private set; } = Array.Empty<ITaskItem>();

        public override bool Execute()
        {
            var className = "Settings";

            var items = RunCollectors();

            if (items.Count <= 0)
                throw new Exception($"No settings were found. {SettingsPath} was used as settings path, Secret_ was used as prefix to find settings in your environment.");
            
            var secrets = BuildSecretsClass(items, className, NamespaceName);

            var outputPath = Path.Combine(OutputPath, $"{className}.Generated.cs");

            var taskItem = new TaskItem(outputPath);
            taskItem.SetMetadata("Visible", bool.TrueString);
            taskItem.SetMetadata("Link", Path.Combine(NamespaceName, $"{className}.Generated.cs"));

            var output = new List<ITaskItem>
            {
                taskItem
            };
            GeneratedFiles = output.ToArray();

            File.WriteAllText(outputPath, secrets);

            return true;
        }

        private Dictionary<string, object> RunCollectors()
        {
            var collectors = new List<ICollectorStrategy>
            {
                new AppSettingsCollectorStrategy(SettingsPath),
                new EnvironmentCollectorStrategy("Secret_")
            };

            foreach (var collectorStrategy in collectors)
            {
                try
                {
                    return collectorStrategy.CollectAndMapSecrets();
                }
                catch
                {
                }
            }

            return new Dictionary<string, object>();
        }

        private string BuildSecretsClass(Dictionary<string, object> items, string className, string namespaceName)
        {
            var codeBuilder = new CodeBuilder(className, namespaceName);

            foreach (var secret in items)
                codeBuilder.AddProperty(secret.Key, secret.Value);

            return codeBuilder.BuildString();
        }
    }
}