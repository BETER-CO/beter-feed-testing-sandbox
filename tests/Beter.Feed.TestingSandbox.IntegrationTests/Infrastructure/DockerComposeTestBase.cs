using Ductus.FluentDocker.Services;

namespace Beter.Feed.TestingSandbox.IntegrationTests.Infrastructure
{
    public abstract class DockerComposeTestBase : IDisposable
    {
        protected IHostService DockerHost;
        protected ICompositeService CompositeService;

        public DockerComposeTestBase()
        {
            EnsureDockerHost();

            CompositeService = Build();
            try
            {
                CompositeService.Start();
            }
            catch
            {
                CompositeService.Dispose();
                throw;
            }

            OnContainerInitialized();
        }

        public void Dispose()
        {
            OnContainerTearDown();

            var compositeService = CompositeService;
            CompositeService = null!;

            try
            {
                compositeService?.Dispose();
            }
            catch
            {
            }
        }

        protected abstract ICompositeService Build();

        protected virtual void OnContainerTearDown()
        {
        }

        protected virtual void OnContainerInitialized()
        {
        }

        private void EnsureDockerHost()
        {
            if (DockerHost?.State == ServiceRunningState.Running) return;

            var hosts = new Hosts().Discover();
            DockerHost = hosts.FirstOrDefault(x => x.IsNative) ?? hosts.FirstOrDefault(x => x.Name == "default");

            if (null != DockerHost)
            {
                if (DockerHost.State != ServiceRunningState.Running) DockerHost.Start();

                return;
            }

            if (hosts.Count > 0) DockerHost = hosts.First();

            if (null != DockerHost) return;

            EnsureDockerHost();
        }
    }
}
