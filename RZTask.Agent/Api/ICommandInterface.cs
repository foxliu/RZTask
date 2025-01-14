using RZTask.Common.Protos;

namespace RZTask.Agent.Api
{
    public interface ICommandInterface
    {
        public int ReturnCode { get; }

        public event Action<string>? OnOutputReceived;

        public void InitializationCmd(TaskRequest request);

        public Task Start(CancellationToken cancellationToken);
    }
}
