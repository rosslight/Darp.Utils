namespace Darp.Utils.TestRail.Tests;

using Models;
using VerifyTests.Http;
using static VerifyHelper;

public sealed class TestRailServiceTests
{
    [Fact]
    public Task Run() => VerifyChecks.Run();

    [Fact]
    public async Task GetCaseAsync()
    {
        const string caseResponse = """
            {
                "id": 1,
                "title": "Print document history and attributes",
                "section_id": 1,
                "template_id": 1,
                "type_id": 2,
                "priority_id": 2,
                "milestone_id": null,
                "refs": null,
                "created_by": 1,
                "created_on": 1646317844,
                "updated_by": 1,
                "updated_on": 1646317844,
                "estimate": null,
                "estimate_forecast": "8m 40s",
                "suite_id": 1,
                "display_order": 1,
                "is_deleted": 0,
                "custom_automation_type": 0,
                "custom_preconds": null,
                "custom_steps": null,
                "custom_expected": null,
                "custom_steps_separated": null,
                "custom_mission": null,
                "custom_goals": null
            }
            """;
        MockHttpClient client = CreateService(caseResponse, out ITestRailService service);
        GetCaseResponse response = await service.GetCaseAsync((CaseId)1);
        await VerifyCalls(client, response);
    }
}
