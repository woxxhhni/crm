using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cls.Tests.Infrastructure;
using FluentAssertions;

namespace Cls.Tests.Regression;

public class BackupRegressionTests(ClsWebApplicationFactory factory) : IClassFixture<ClsWebApplicationFactory>
{
    private const string RunningJobWithWaitMessage =
        "A backup or restore job is already in progress. Please wait for it to complete.";

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateBackup_WhenJobAlreadyPending_Returns409WithExpectedMessage()
    {
        var token = await ApiTestAuth.LoginAdminAsync(_client);

        var first = await ApiTestAuth.PostAsync(_client, token, "/api/v1/backups");
        if (first.StatusCode == HttpStatusCode.Conflict)
        {
            await AssertConflictMessageAsync(first);
            return;
        }

        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await ApiTestAuth.PostAsync(_client, token, "/api/v1/backups");
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await AssertConflictMessageAsync(second);
    }

    [Fact]
    public async Task GetBackup_WhenMissing_Returns404()
    {
        var token = await ApiTestAuth.LoginAdminAsync(_client);
        var response = await ApiTestAuth.GetAsync(_client, token, "/api/v1/backups/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DownloadBackup_WhenJobNotCompleted_Returns400WithMessage()
    {
        var token = await ApiTestAuth.LoginAdminAsync(_client);

        var create = await ApiTestAuth.PostAsync(_client, token, "/api/v1/backups");
        if (create.StatusCode == HttpStatusCode.Conflict)
            return;

        create.EnsureSuccessStatusCode();
        var created = await create.Content.ReadFromJsonAsync<JsonElement>();
        var jobId = created.GetProperty("id").GetInt32();

        var download = await ApiTestAuth.GetAsync(_client, token, $"/api/v1/backups/{jobId}/download");
        download.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await download.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("message").GetString().Should()
            .Be("Backup is not yet completed or has no file.");
    }

    [Fact]
    public async Task RestoreBackup_WhenSourceMissing_Returns404()
    {
        var token = await ApiTestAuth.LoginAdminAsync(_client);
        var response = await ApiTestAuth.PostAsync(_client, token, "/api/v1/backups/999999/restore");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static async Task AssertConflictMessageAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("message").GetString().Should().Be(RunningJobWithWaitMessage);
    }
}
