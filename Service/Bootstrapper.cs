// <copyright file="Bootstrapper.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Ensage.SDK.Service
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;

    using Ensage.SDK.Helpers;
    using Ensage.SDK.Service.Metadata;

    using log4net;

    using PlaySharp.Toolkit.AppDomain.Loader;
    using PlaySharp.Toolkit.Helper;
    using PlaySharp.Toolkit.Logging;

    public class Bootstrapper : AssemblyEntryPoint
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SDKConfig Config { get; private set; }

        public ContextContainer<IServiceContext> Default { get; private set; }

        public List<PluginContainer> PluginContainer { get; } = new List<PluginContainer>();

        public IEnumerable<Lazy<IPluginLoader, IPluginLoaderMetadata>> Plugins
        {
            get
            {
                return this.Default?.GetAll<IPluginLoader, IPluginLoaderMetadata>();
            }
        }

        public void BuildUp(object instance)
        {
            this.Default.BuildUp(instance);
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return this.Default.GetAllInstances(serviceType);
        }

        public object GetInstance(Type serviceType, string key)
        {
            return this.Default.GetInstance(serviceType, key);
        }

        protected override void OnActivated()
        {
            UpdateManager.Subscribe(this.OnLoad);
        }

        protected override void OnDeactivated()
        {
            try
            {
                this.DeactivatePlugins();
                this.Default?.Dispose();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void ActivatePlugins()
        {
            foreach (var plugin in this.PluginContainer)
            {
                if (plugin.Mode == StartupMode.Manual && plugin.ActiveItem)
                {
                    plugin.Activate();
                }

                if (plugin.Mode == StartupMode.Auto && plugin.ActiveItem)
                {
                    plugin.Activate();
                }
            }
        }

        private void DeactivatePlugins()
        {
            foreach (var plugin in this.PluginContainer)
            {
                plugin.Deactivate();
            }
        }

        private void DiscoverPlugins()
        {
            foreach (var assembly in this.Plugins)
            {
                if (assembly.Metadata.Units == null)
                {
                    Log.Debug($"Found [{assembly.Metadata.Mode}] {assembly.Metadata.Name} | {assembly.Metadata.Author} | {assembly.Metadata.Version}");
                }
                else
                {
                    Log.Debug(
                        $"Found [{assembly.Metadata.Mode}] {assembly.Metadata.Name} | {assembly.Metadata.Author} | {assembly.Metadata.Version} | {string.Join(", ", assembly.Metadata.Units)}");
                }

                this.PluginContainer.Add(new PluginContainer(this.Config.Plugins.Factory, assembly));
            }
        }

        private void OnLoad()
        {
            if (ObjectManager.LocalHero == null)
            {
                return;
            }

            UpdateManager.Unsubscribe(this.OnLoad);

            try
            {
                var sw = Stopwatch.StartNew();

                Log.Debug("====================================================");
                Log.Debug($">> Ensage.SDK Bootstrap started");
                Log.Debug("====================================================");

                Log.Debug($">> Building Menu");
                this.Config = new SDKConfig();

                Log.Debug($">> Building Container for LocalHero");
                this.Default = ContainerFactory.CreateContainer(ObjectManager.LocalHero);
                this.Default.RegisterValue(this.Config);

                Log.Debug($">> Initializing Services");
                IoC.Initialize(this.BuildUp, this.GetInstance, this.GetAllInstances);

                Log.Debug($">> Searching for Plugins");
                this.DiscoverPlugins();

                Log.Debug($">> Activating Plugins");
                this.ActivatePlugins();

                sw.Stop();

                Log.Debug("====================================================");
                Log.Debug($">> Bootstrap completed in {sw.Elapsed}");
                Log.Debug("====================================================");
            }
            catch (ReflectionTypeLoadException e)
            {
                foreach (var exception in e.LoaderExceptions)
                {
                    Log.Error(exception);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}