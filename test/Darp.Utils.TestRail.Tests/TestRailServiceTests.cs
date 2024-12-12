namespace Darp.Utils.TestRail.Tests;

using System.Text.Json;
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

    [Fact]
    public async Task GetCasesAsync()
    {
        const string caseResponse = """
            {
                "offset": 0,
                "limit": 250,
                "size": 1,
                "_links": {
                    "next": "/api/v2/get_cases/1&limit=250&offset=250",
                    "prev": null
                },
                "cases": [
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
                        "is_deleted": 0
                    }
                ]
            }
            """;
        MockHttpClient client = CreateService(caseResponse, out ITestRailService service);
        GetCaseResponse[] response = await service.GetCases((ProjectId)1).ToArrayAsync();
        await VerifyCalls(client, response);
    }

    [Fact]
    public async Task UpdateCaseAsync()
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
                "is_deleted": 0
            }
            """;
        MockHttpClient client = CreateService(caseResponse, out ITestRailService service);
        GetCaseResponse response = await service.UpdateCaseAsync(
            new UpdateCaseRequest
            {
                CaseId = (CaseId)2,
                Title = "New test title",
                Estimate = TimeSpan.FromSeconds(62),
            }
        );
        await VerifyCalls(client, response);
    }

    [Fact]
    public async Task GetMilestoneAsync()
    {
        const string caseResponse = """
            {
                "completed_on": 1389968184,
                "description": "...",
                "due_on": 1391968184,
                "id": 1,
                "is_completed": true,
                "name": "Release 1.5",
                "project_id": 1,
                "refs": "RF-1, RF-2",
                "url": "http:///testrail/index.php?/milestones/view/1"
            }
            """;
        MockHttpClient client = CreateService(caseResponse, out ITestRailService service);
        GetMilestone response = await service.GetMilestoneAsync((MilestoneId)1);
        await VerifyCalls(client, response);
    }

    [Fact]
    public async Task GetProjectAsync()
    {
        const string caseResponse = """
            {
                "id": 1,
                "announcement": "Welcome to project X",
                "completed_on": 1389968184,
                "default_role_id": 3,
                "default_role": "Tester",
                "is_completed": false,
                "name": "Project X",
                "show_announcement": true,
                "suite_mode": 1,
                "url": "https://instance.testrail.io/index.php?/projects/overview/1",
                "users": [
                    {
                        "id": 3,
                        "global_role_id": null,
                        "global_role": null,
                        "project_role_id": null,
                        "project_role": null
                    }
                ],
                "groups": []
            }
            """;
        MockHttpClient client = CreateService(caseResponse, out ITestRailService service);
        GetProject response = await service.GetProjectAsync((ProjectId)1);
        await VerifyCalls(client, response);
    }

    [Fact]
    public async Task GetProjects()
    {
        const string projectsResponse = """
            {
                "offset": 0,
                "limit": 250,
                "size": 1,
                "_links": {
                    "next": null,
                    "prev": null
                },
                "projects": [
                    {
                        "id": 1,
                        "announcement": "Welcome to project X",
                        "completed_on": 1389968184,
                        "default_role_id": 3,
                        "default_role": "Tester",
                        "is_completed": false,
                        "name": "Project X",
                        "show_announcement": true,
                        "suite_mode": 1,
                        "url": "https://instance.testrail.io/index.php?/projects/overview/1"
                    }
                ]
            }
            """;
        MockHttpClient client = CreateService(projectsResponse, out ITestRailService service);
        GetProject[] response = await service.GetProjects().ToArrayAsync();
        await VerifyCalls(client, response);
    }

    [Fact]
    public async Task AddResultAsync()
    {
        MockHttpClient client = CreateService<AddResultRequest, GetResultResponse>(
            request => new GetResultResponse
            {
                Id = (ResultId)7,
                AssignedtoId = request.AssignedtoId,
                Comment = request.Comment,
                Defects = request.Defects,
                Elapsed = request.Elapsed,
                StatusId = request.StatusId,
                Version = request.Version,
                Properties = request.Properties,
            },
            out ITestRailService service
        );
        GetResultResponse response = await service.AddResultAsync(
            (TestId)1,
            new AddResultRequest
            {
                Version = "V1",
                StatusId = StatusId.Passed,
                Elapsed = TimeSpan.FromSeconds(4430),
                Comment = "Some comment",
                Properties = { ["custom_prop"] = JsonSerializer.SerializeToElement("asdasd") },
            }
        );
        await VerifyCalls(client, response);
    }

    [Fact]
    public async Task GetRunAsync()
    {
        const string caseResponse = """
            {
                "assignedto_id": 6,
                "blocked_count": 0,
                "completed_on": null,
                "config": "Firefox, Ubuntu 12",
                "config_ids": [
                    2,
                    6
                ],
                "created_by": 1,
                "created_on": 1393845644,
                "refs": "SAN-1",
                "custom_status1_count": 0,
                "custom_status2_count": 0,
                "custom_status3_count": 0,
                "custom_status4_count": 0,
                "custom_status5_count": 0,
                "custom_status6_count": 0,
                "custom_status7_count": 0,
                "description": null,
                "failed_count": 2,
                "id": 81,
                "include_all": false,
                "is_completed": false,
                "milestone_id": 7,
                "name": "File Formats",
                "passed_count": 2,
                "plan_id": 80,
                "project_id": 1,
                "retest_count": 1,
                "suite_id": 4,
                "untested_count": 3,
                "updated_on": null,
                "url": "http://{server}/testrail/index.php?/runs/view/81"
            }
            """;
        MockHttpClient client = CreateService(caseResponse, out ITestRailService service);
        GetRun response = await service.GetRunAsync((RunId)81);
        await VerifyCalls(client, response);
    }

    [Fact]
    public async Task GetRuns()
    {
        const string caseResponse = """
            {
                "offset": 0,
                "limit": 250,
                "size": 1,
                "_links": {
                    "next": "/api/v2/get_runs/1&limit=250&offset=250",
                    "prev": null
                },
                "runs": [
                    {
                        "assignedto_id": 6,
                        "blocked_count": 0,
                        "completed_on": null,
                        "config": "Firefox, Ubuntu 12",
                        "config_ids": [
                            2,
                            6
                        ],
                        "created_by": 1,
                        "created_on": 1393845644,
                        "refs": "SAN-1",
                        "description": null,
                        "failed_count": 2,
                        "id": 81,
                        "include_all": false,
                        "is_completed": false,
                        "milestone_id": 7,
                        "name": "File Formats",
                        "passed_count": 2,
                        "plan_id": 80,
                        "project_id": 1,
                        "retest_count": 1,
                        "suite_id": 4,
                        "untested_count": 3,
                        "updated_on": null,
                        "url": "http://{server}/testrail/index.php?/runs/view/81"
                    }
                ]
            }
            """;
        MockHttpClient client = CreateService(caseResponse, out ITestRailService service);
        GetRun[] response = await service.GetRuns((ProjectId)1).ToArrayAsync();
        await VerifyCalls(client, response);
    }

    [Fact]
    public async Task CloseRunAsync()
    {
        const string caseResponse = """
            {
                "assignedto_id": 6,
                "blocked_count": 0,
                "completed_on": null,
                "config": "Firefox, Ubuntu 12",
                "config_ids": [
                    2,
                    6
                ],
                "created_by": 1,
                "created_on": 1393845644,
                "refs": "SAN-1",
                "description": null,
                "failed_count": 2,
                "id": 81,
                "include_all": false,
                "is_completed": false,
                "milestone_id": 7,
                "name": "File Formats",
                "passed_count": 2,
                "plan_id": 80,
                "project_id": 1,
                "retest_count": 1,
                "suite_id": 4,
                "untested_count": 3,
                "updated_on": null,
                "url": "http://{server}/testrail/index.php?/runs/view/81"
            }
            """;
        MockHttpClient client = CreateService(caseResponse, out ITestRailService service);
        GetRun response = await service.CloseRunAsync((RunId)81);
        await VerifyCalls(client, response);
    }

    [Fact]
    public async Task GetSectionAsync()
    {
        const string caseResponse = """
            {
                "depth": 0,
                "description": null,
                "display_order": 1,
                "id": 1,
                "name": "Prerequisites",
                "parent_id": null,
                "suite_id": 1
            }
            """;
        MockHttpClient client = CreateService(caseResponse, out ITestRailService service);
        GetSection response = await service.GetSectionAsync((SectionId)1);
        await VerifyCalls(client, response);
    }

    [Fact]
    public async Task GetSections()
    {
        const string caseResponse = """
            {
                "offset": 0,
                "limit": 250,
                "size": 3,
                "_links": {
                    "next": null,
                    "prev": null
                },
                "sections": [
                    {
                        "depth": 0,
                        "display_order": 1,
                        "id": 1,
                        "name": "Prerequisites",
                        "parent_id": null,
                        "suite_id": 1
                    },
                    {
                        "depth": 0,
                        "display_order": 2,
                        "id": 2,
                        "name": "Documentation & Help",
                        "parent_id": null,
                        "suite_id": 1
                    },
                    {
                        "depth": 1,
                        "display_order": 3,
                        "id": 3,
                        "name": "Licensing & Terms",
                        "parent_id": 2,
                        "suite_id": 1
                    }
                ]
            }
            """;
        MockHttpClient client = CreateService(caseResponse, out ITestRailService service);
        GetSection[] response = await service.GetSections((ProjectId)1).ToArrayAsync();
        await VerifyCalls(client, response);
    }

    [Fact]
    public async Task GetSuiteAsync()
    {
        const string suiteResponse = """
            {
                "description": "..",
                "id": 1,
                "name": "Setup & Installation",
                "project_id": 1,
                "url": "http:///testrail/index.php?/suites/view/1"
            }
            """;
        MockHttpClient client = CreateService(suiteResponse, out ITestRailService service);
        GetSuite response = await service.GetSuiteAsync((SuiteId)1);
        await VerifyCalls(client, response);
    }

    [Fact]
    public async Task GetSuites()
    {
        const string suitesResponse = """
            [
                {
                    "id": 1,
                    "name": "Setup & Installation"
                },
                {
                    "id": 2,
                    "name": "Document Editing"
                }
            ]
            """;
        MockHttpClient client = CreateService(suitesResponse, out ITestRailService service);
        GetSuite[] response = await service.GetSuites((ProjectId)3).ToArrayAsync();
        await VerifyCalls(client, response);
    }

    [Fact]
    public async Task GetTestAsync()
    {
        const string suiteResponse = """
            {
                "assignedto_id": 1,
                "case_id": 1,
                "custom_expected": "..",
                "custom_preconds": "..",
                "custom_steps_separated": [
                    {
                        "content": "Step 1",
                        "expected": "Expected Result 1"
                    },
                    {
                        "content": "Step 2",
                        "expected": "Expected Result 2"
                    }
                ],
                "estimate": "1m 5s",
                "estimate_forecast": null,
                "id": 100,
                "priority_id": 2,
                "run_id": 1,
                "status_id": 5,
                "title": "Verify line spacing on multi-page document",
                "type_id": 4
            }
            """;
        MockHttpClient client = CreateService(suiteResponse, out ITestRailService service);
        GetTestResponse response = await service.GetTestAsync((TestId)100);
        await VerifyCalls(client, response);
    }

    [Fact]
    public async Task GetTests()
    {
        const string testsResponse = """
            {
                "offset": 0,
                "limit": 250,
                "size": 236,
                "_links": {
                    "next": null,
                    "prev": null
                },
                "tests": [
                    {
                        "id": 1,
                        "title": "Test conditional formatting with basic value range"
                    },
                    {
                        "id": 2,
                        "title": "Verify line spacing on multi-page document"
                    }
                ]
            }
            """;
        MockHttpClient client = CreateService(testsResponse, out ITestRailService service);
        GetTestResponse[] response = await service.GetTests((RunId)3).ToArrayAsync();
        await VerifyCalls(client, response);
    }

    [Fact]
    public async Task GetUserByEmailAsync()
    {
        const string userResponse = """
            {
                "id": 1,
                "email": "john.doe@gurock.io",
                "email_notifications": true,
                "is_active": true,
                "is_admin": false,
                "name": "John Doe",
                "role_id": 3,
                "role": "Tester",
                "group_ids": [1, 2, 3],
                "mfa_required": false
            }
            """;
        MockHttpClient client = CreateService(userResponse, out ITestRailService service);
        GetUser response = await service.GetUserByEmailAsync("john.doe@gurock.io");
        await VerifyCalls(client, response);
    }
}
