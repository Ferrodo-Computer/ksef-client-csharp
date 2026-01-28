using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Models.Lighthouse;

namespace KSeF.Client.Tests.Core.E2E.Lighthouse;

public sealed class LighthouseClientE2ETests : TestBase
{
    private ILighthouseClient LighthouseClient => Get<ILighthouseClient>();

    [Fact]
    public async Task GetStatusAsyncShouldReturnStatus()
    {
        KsefStatusResponse response = await LighthouseClient.GetStatusAsync(CancellationToken);

        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.Status));

        Assert.Contains(response.Status,
            new[] { KsefStatus.Available, KsefStatus.Maintenance, KsefStatus.Failure, KsefStatus.TotalFailure });

        if (response.Messages is not null)
        {
            Assert.All(response.Messages, msg =>
            {
                Assert.False(string.IsNullOrWhiteSpace(msg.Id));
                Assert.False(string.IsNullOrWhiteSpace(msg.Type));
                Assert.Contains(msg.Type, 
                    new[] { MessageType.FailureStart, MessageType.FailureEnd, MessageType.MaintenanceAnnouncement });
                Assert.False(string.IsNullOrWhiteSpace(msg.Category));
                Assert.Contains(msg.Category,
                    new[] { MessageCategory.Failure, MessageCategory.TotalFailure, MessageCategory.Maintenance });
                Assert.False(string.IsNullOrWhiteSpace(msg.Title));
                Assert.False(string.IsNullOrWhiteSpace(msg.Text));
                Assert.NotEqual(default, msg.Start);
            });
        }
    }

    [Fact]
    public async Task GetMessagesAsyncShouldReturnMessagesList()
    {
        KsefMessagesResponse response = await LighthouseClient.GetMessagesAsync(CancellationToken);

        Assert.NotNull(response);

        foreach (Message msg in response)
        {
            Assert.False(string.IsNullOrWhiteSpace(msg.Id));
            Assert.False(string.IsNullOrWhiteSpace(msg.Type));
            Assert.Contains(msg.Type,
                new[] { MessageType.FailureStart, MessageType.FailureEnd, MessageType.MaintenanceAnnouncement });
            Assert.False(string.IsNullOrWhiteSpace(msg.Category));
            Assert.Contains(msg.Category,
                new[] { MessageCategory.Failure, MessageCategory.TotalFailure, MessageCategory.Maintenance });
            Assert.False(string.IsNullOrWhiteSpace(msg.Title));
            Assert.False(string.IsNullOrWhiteSpace(msg.Text));
            Assert.NotEqual(default, msg.Start);
        }
    }
}
