namespace Darp.Utils.TestRail;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Models;

/// <summary> Extensions of the testRail service </summary>
public static partial class TestRailService
{
    /// <summary> Returns an existing test case </summary>
    /// <param name="testRailService"> The TestRail service to execute the request </param>
    /// <param name="caseId"> The ID of the test case </param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <returns> A task providing the <see cref="GetCaseResponse"/> on completion </returns>
    /// <seealso href="https://support.testrail.com/hc/en-us/articles/7077292642580-Cases#getcase"/>
    public static async Task<GetCaseResponse> GetCaseAsync(
        this ITestRailService testRailService,
        CaseId caseId,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(testRailService);
        return await testRailService
            .GetAsync($"/get_case/{(int)caseId}", SourceGenerationContext.TestRail.GetCaseResponse, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary> Returns a list of test cases for a project or specific test suite (if the project has multiple suites enabled) </summary>
    /// <param name="testRailService"> The TestRail service to execute the request </param>
    /// <param name="projectId"> The ID of the project </param>
    /// <param name="suiteId"> The ID of the test suite (optional if the project is operating in single suite mode) </param>
    /// <param name="chunkSize"> Number of cases to be requested in each batch </param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <returns> A task providing the <see cref="GetCaseResponse"/> on completion </returns>
    /// <seealso href="https://support.testrail.com/hc/en-us/articles/7077292642580-Cases#getcases"/>
    public static async IAsyncEnumerable<GetCaseResponse> GetCases(
        this ITestRailService testRailService,
        ProjectId projectId,
        SuiteId suiteId = SuiteId.None,
        int chunkSize = 250,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(testRailService);
        var offset = 0;
        while (true)
        {
            var requestUri = $"/get_cases/{(int)projectId}&limit={chunkSize}&offset={offset}";
            if (suiteId is not SuiteId.None)
            {
                requestUri += $"&suite_id={(int)suiteId}";
            }
            GetCases results = await testRailService
                .GetAsync(requestUri, SourceGenerationContext.TestRail.GetCases, cancellationToken)
                .ConfigureAwait(false);
            foreach (GetCaseResponse getCase in results.Cases)
            {
                yield return getCase;
            }
            if (results.Size < chunkSize)
            {
                yield break;
            }
            offset += chunkSize;
        }
    }

    /// <summary> Returns an existing test </summary>
    /// <param name="testRailService"> The TestRail service to execute the request </param>
    /// <param name="testId"> The ID of the test </param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <returns> A task providing the <see cref="GetTestResponse"/> on completion </returns>
    /// <seealso href="https://support.testrail.com/hc/en-us/articles/7077990441108-Tests#gettest"/>
    public static async Task<GetTestResponse> GetTestAsync(
        this ITestRailService testRailService,
        TestId testId,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(testRailService);
        return await testRailService
            .GetAsync($"/get_test/{(int)testId}", SourceGenerationContext.TestRail.GetTestResponse, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary> Returns a list of tests for a test run </summary>
    /// <param name="testRailService"> The TestRail service to execute the request </param>
    /// <param name="runId"> The ID of the test run </param>
    /// <param name="chunkSize"> Number of tests to be requested in each batch </param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <returns> An enumerable yielding <see cref="GetTestResponse"/> </returns>
    /// <seealso href="https://support.testrail.com/hc/en-us/articles/7077990441108-Tests#gettests"/>
    public static async IAsyncEnumerable<GetTestResponse> GetTests(
        this ITestRailService testRailService,
        RunId runId,
        int chunkSize = 250,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(testRailService);
        var offset = 0;
        while (true)
        {
            var requestUri = $"/get_tests/{(int)runId}&limit={chunkSize}&offset={offset}";
            GetTests results = await testRailService
                .GetAsync(requestUri, SourceGenerationContext.TestRail.GetTests, cancellationToken)
                .ConfigureAwait(false);
            foreach (GetTestResponse getRun in results.Tests)
            {
                yield return getRun;
            }
            if (results.Size < chunkSize)
            {
                yield break;
            }
            offset += chunkSize;
        }
    }

    /// <summary>
    /// Add a new result of a test:
    /// <a href="https://support.testrail.com/hc/en-us/articles/7077819312404-Results#addresult">TestRail Documentation</a>
    /// </summary>
    /// <param name="testRailService">The test rail service to call</param>
    /// <param name="testId">The testId to add the result to</param>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">The cancellationToken to cancel the operation</param>
    /// <returns>An error or the result</returns>
    public static async Task<GetResults> AddResultAsync(
        this ITestRailService testRailService,
        TestId testId,
        AddResultRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(testRailService);
        return await testRailService
            .PostAsync(
                $"/add_result/{(int)testId}",
                request,
                SourceGenerationContext.TestRail.AddResultRequest,
                SourceGenerationContext.TestRail.GetResults,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Add a new result of a test:
    /// <a href="https://support.testrail.com/hc/en-us/articles/7077978310292-Users#getuserbyemail">TestRail Documentation</a>
    /// </summary>
    /// <param name="testRailService">The test rail service to call</param>
    /// <param name="userEmail">The testId to add the result to</param>
    /// <param name="cancellationToken">The cancellationToken to cancel the operation</param>
    /// <returns>An error or the result</returns>
    public static async Task<GetUser> GetUserByEmailAsync(
        this ITestRailService testRailService,
        string userEmail,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(testRailService);
        return await testRailService
            .GetAsync(
                $"/get_user_by_email&email={userEmail}",
                SourceGenerationContext.TestRail.GetUser,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="testRailService"> The TestRail service to execute the request </param>
    /// <param name="milestoneId"></param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <returns></returns>
    public static async Task<GetMilestone> GetMilestone(
        this ITestRailService testRailService,
        MilestoneId milestoneId,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(testRailService);
        return await testRailService
            .GetAsync(
                $"/get_milestone/{(int)milestoneId}",
                SourceGenerationContext.TestRail.GetMilestone,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="testRailService"> The TestRail service to execute the request </param>
    /// <param name="runId"></param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <returns></returns>
    public static async Task<GetRun> GetRunAsync(
        this ITestRailService testRailService,
        RunId runId,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(testRailService);
        return await testRailService
            .GetAsync($"/get_run/{(int)runId}", SourceGenerationContext.TestRail.GetRun, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="testRailService"> The TestRail service to execute the request </param>
    /// <param name="runId"></param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <returns></returns>
    public static async Task<GetRun> CloseRunAsync(
        this ITestRailService testRailService,
        RunId runId,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(testRailService);
        return await testRailService
            .PostAsync($"/close_run/{(int)runId}", SourceGenerationContext.TestRail.GetRun, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="testRailService"> The TestRail service to execute the request </param>
    /// <param name="projectId"></param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <returns></returns>
    public static async Task<GetProject> GetProjectAsync(
        this ITestRailService testRailService,
        ProjectId projectId,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(testRailService);
        return await testRailService
            .GetAsync($"/get_project/{(int)projectId}", SourceGenerationContext.TestRail.GetProject, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="testRailService"> The TestRail service to execute the request </param>
    /// <param name="projectsFilter"></param>
    /// <param name="chunkSize"></param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <returns></returns>
    public static async IAsyncEnumerable<GetProject> GetProjects(
        this ITestRailService testRailService,
        ProjectsFilter projectsFilter = ProjectsFilter.All,
        int chunkSize = 250,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(testRailService);
        var offset = 0;
        while (true)
        {
            var requestUri = $"/get_projects&limit={chunkSize}&offset={offset}";
            if (projectsFilter is not ProjectsFilter.All)
            {
                requestUri += $"&is_completed={(int)projectsFilter}";
            }

            GetProjects results = await testRailService
                .GetAsync(requestUri, SourceGenerationContext.TestRail.GetProjects, cancellationToken)
                .ConfigureAwait(false);
            foreach (GetProject getProject in results.Projects)
            {
                yield return getProject;
            }
            if (results.Size < chunkSize)
            {
                yield break;
            }
            offset += chunkSize;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="testRailService"> The TestRail service to execute the request </param>
    /// <param name="projectId"></param>
    /// <param name="suiteId"></param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <returns></returns>
    public static async Task<GetSection> GetSectionAsync(
        this ITestRailService testRailService,
        ProjectId projectId,
        SuiteId suiteId = SuiteId.None,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(testRailService);
        var requestUri = $"/get_section/{(int)projectId}";
        if (suiteId is not SuiteId.None)
        {
            requestUri += $"&suite_id={(int)suiteId}";
        }
        return await testRailService
            .GetAsync(requestUri, SourceGenerationContext.TestRail.GetSection, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="testRailService"> The TestRail service to execute the request </param>
    /// <param name="projectId"></param>
    /// <param name="suiteId"></param>
    /// <param name="chunkSize"></param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <returns></returns>
    public static async IAsyncEnumerable<GetSection> GetSections(
        this ITestRailService testRailService,
        ProjectId projectId,
        SuiteId suiteId = SuiteId.None,
        int chunkSize = 250,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(testRailService);
        var offset = 0;
        while (true)
        {
            var requestUri = $"/get_sections/{(int)projectId}&limit={chunkSize}&offset={offset}";
            if (suiteId is not SuiteId.None)
            {
                requestUri += $"&suite_id={(int)suiteId}";
            }
            GetSections results = await testRailService
                .GetAsync(requestUri, SourceGenerationContext.TestRail.GetSections, cancellationToken)
                .ConfigureAwait(false);
            foreach (GetSection getCase in results.Sections)
            {
                yield return getCase;
            }
            if (results.Size < chunkSize)
            {
                yield break;
            }
            offset += chunkSize;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="testRailService"> The TestRail service to execute the request </param>
    /// <param name="suiteId"></param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <returns></returns>
    public static async Task<GetSuite> GetSuiteAsync(
        this ITestRailService testRailService,
        SuiteId suiteId,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(testRailService);
        var requestUri = $"/get_suite/{(int)suiteId}";
        return await testRailService
            .GetAsync(requestUri, SourceGenerationContext.TestRail.GetSuite, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="testRailService"> The TestRail service to execute the request </param>
    /// <param name="projectId"></param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <returns></returns>
    public static async IAsyncEnumerable<GetSuite> GetSuites(
        this ITestRailService testRailService,
        ProjectId projectId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(testRailService);
        IEnumerable<GetSuite> enumerable = await testRailService
            .GetAsync(
                $"/get_suites/{(int)projectId}",
                SourceGenerationContext.TestRail.IEnumerableGetSuite,
                cancellationToken
            )
            .ConfigureAwait(false);
        foreach (GetSuite getSection in enumerable)
        {
            yield return getSection;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="testRailService"> The TestRail service to execute the request </param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    public static async Task UpdateCase(
        this ITestRailService testRailService,
        UpdateCaseRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(testRailService);
        ArgumentNullException.ThrowIfNull(request);
        await testRailService
            .PostAsync(
                $"/update_case/{(int)request.CaseId}",
                request,
                SourceGenerationContext.TestRail.UpdateCaseRequest,
                SourceGenerationContext.TestRail.GetCaseResponse,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="testRailService"> The TestRail service to execute the request </param>
    /// <param name="projectId"></param>
    /// <param name="runsFilter"></param>
    /// <param name="chunkSize"></param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <returns></returns>
    public static async IAsyncEnumerable<GetRun> GetRuns(
        this ITestRailService testRailService,
        ProjectId projectId,
        RunsFilter runsFilter,
        int chunkSize = 250,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(testRailService);
        var offset = 0;
        while (true)
        {
            var requestUri = $"/get_runs/{(int)projectId}&limit={chunkSize}&offset={offset}";
            if (runsFilter is not RunsFilter.All)
            {
                requestUri += $"&is_completed={(int)runsFilter}";
            }
            GetRuns results = await testRailService
                .GetAsync(requestUri, SourceGenerationContext.TestRail.GetRuns, cancellationToken)
                .ConfigureAwait(false);
            foreach (GetRun getRun in results.Runs)
            {
                yield return getRun;
            }
            if (results.Size < chunkSize)
            {
                yield break;
            }
            offset += chunkSize;
        }
    }

    /// <summary> Execute a <c>GET</c> request to the TestRail instance </summary>
    /// <param name="testRailService"> The TestRail service to execute the request </param>
    /// <param name="path"> The relative path in the URL </param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <typeparam name="TResponse"> The type of the response </typeparam>
    /// <returns> A task which contains the response when completed </returns>
    [RequiresUnreferencedCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved."
    )]
    [RequiresDynamicCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications."
    )]
    public static Task<TResponse> GetAsync<TResponse>(
        this ITestRailService testRailService,
        string path,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(testRailService);
        return testRailService.GetAsync(
            path,
            SourceGenerationContext.CreateDefaultJsonTypeInfo<TResponse>(),
            cancellationToken
        );
    }

    /// <summary> Execute a <c>GET</c> request to the TestRail instance </summary>
    /// <param name="testRailService"> The TestRail service to execute the request </param>
    /// <param name="path"> The relative path in the URL </param>
    /// <param name="body"> The request body which will be serialized as json </param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <typeparam name="TRequest"> The type of the request </typeparam>
    /// <typeparam name="TResponse"> The type of the response </typeparam>
    /// <returns> A task which contains the response when completed </returns>
    [RequiresUnreferencedCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved."
    )]
    [RequiresDynamicCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications."
    )]
    public static Task<TResponse> GetAsync<TRequest, TResponse>(
        this ITestRailService testRailService,
        string path,
        TRequest body,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(testRailService);
        return testRailService.GetAsync(
            path,
            body,
            SourceGenerationContext.CreateDefaultJsonTypeInfo<TRequest>(),
            SourceGenerationContext.CreateDefaultJsonTypeInfo<TResponse>(),
            cancellationToken
        );
    }

    /// <summary> Execute a <c>POST</c> request to the TestRail instance </summary>
    /// <param name="testRailService"> The TestRail service to execute the request </param>
    /// <param name="path"> The relative path in the URL </param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <typeparam name="TResponse"> The type of the response </typeparam>
    /// <returns> A task which contains the response when completed </returns>
    [RequiresUnreferencedCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved."
    )]
    [RequiresDynamicCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications."
    )]
    public static Task<TResponse> PostAsync<TResponse>(
        this ITestRailService testRailService,
        string path,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(testRailService);
        return testRailService.PostAsync(
            path,
            SourceGenerationContext.CreateDefaultJsonTypeInfo<TResponse>(),
            cancellationToken
        );
    }

    /// <summary> Execute a <c>POST</c> request to the TestRail instance </summary>
    /// <param name="testRailService"> The TestRail service to execute the request </param>
    /// <param name="path"> The relative path in the URL </param>
    /// <param name="body"> The request body which will be serialized as json </param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <typeparam name="TRequest"> The type of the request </typeparam>
    /// <typeparam name="TResponse"> The type of the response </typeparam>
    /// <returns> A task which contains the response when completed </returns>
    [RequiresUnreferencedCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved."
    )]
    [RequiresDynamicCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications."
    )]
    public static Task<TResponse> PostAsync<TRequest, TResponse>(
        this ITestRailService testRailService,
        string path,
        TRequest body,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(testRailService);
        return testRailService.PostAsync(
            path,
            body,
            SourceGenerationContext.CreateDefaultJsonTypeInfo<TRequest>(),
            SourceGenerationContext.CreateDefaultJsonTypeInfo<TResponse>(),
            cancellationToken
        );
    }
}