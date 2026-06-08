# Technical Reflection Report — TechMove GLMS Part 3

**Student Number:** ST10197860  
**Module:** PROG7311  
**Date:** 2026-06-08

---

## Section 1 — DevOps and Automated Testing

Modern software delivery relies on the principle of continuous integration and continuous delivery (CI/CD), a practice in which every code change is automatically built, tested, and validated before it reaches production [1]. Within this pipeline, automated integration testing occupies a uniquely critical role: it validates not individual units of logic in isolation, but the behaviour of the entire system as a connected whole [2].

Unit tests, such as the thirteen tests present in TechMove.GLMS.Tests, are invaluable for verifying the correctness of discrete components — the `CurrencyCalculationService`, `FileValidationService`, and `WorkflowValidationService` are each exercised independently, with no external dependencies. However, as noted by Humble and Farley [1], unit tests cannot detect failures that arise from the interaction between layers: a misconfigured HTTP pipeline, a broken JWT validation chain, or a controller that returns the wrong status code under certain conditions. These are precisely the failures that reach users when integration testing is absent.

The integration test suite in `TechMove.GLMS.Tests/IntegrationTests/ApiIntegrationTests.cs` targets the live TechMove GLMS API at `http://localhost:5001` and validates behaviour that spans the full request pipeline. `GetContracts_ReturnsHttpOK()` confirms that the API is reachable and that the EF Core–to–SQL Server pipeline returns a successful response. `Login_WithValidCredentials_ReturnsToken()` verifies that the JWT authentication endpoint issues a token correctly [3], while `Login_WithInvalidCredentials_Returns401()` confirms that the API rejects bad credentials with the appropriate HTTP 401 status code. `CreateClient_ThenReadBack_ReturnsSameData()` verifies data integrity by creating a record and reading it back — a pattern recommended by Microsoft for integration testing of ASP.NET Core applications [4]. `CreateContract_WithoutAuth_Returns401()` validates that the `[Authorize]` attribute on POST endpoints is properly enforced by the JWT bearer middleware — a security guarantee that no unit test could provide.

In a CI/CD pipeline, these tests execute automatically on every pull request, ensuring that regressions are caught before a merge [1]. The cost of a failed integration test in a pipeline is a blocked pull request; the cost of the same failure reaching production is user-facing downtime or a security incident. Automated integration testing is therefore not optional in a professional delivery context: it is a safety net that makes frequent, confident deployment possible [2].

---

## Section 2 — Containerisation with Docker

One of the most persistent challenges in software delivery is environment inconsistency — the phenomenon known as the "works on my machine" problem [5]. A developer's local machine may have a different operating system patch level, a different version of the .NET runtime, a different SQL Server collation, or environment variables that satisfy a hidden dependency. When that software is deployed to a test server or production host, any of these differences can cause subtle or catastrophic failures.

Docker addresses this problem by packaging an application together with its entire runtime environment — the OS libraries, the .NET runtime, the compiled binaries, and the configuration — into a portable image [5]. That image is identical regardless of whether it runs on a developer's laptop, a CI agent, or a cloud virtual machine. As described in the Docker documentation [5], the image itself is the artefact that moves through the pipeline, so there is no surface area for environmental drift.

For the TechMove GLMS system, Docker Compose [6] orchestrates three containers that together constitute the complete application stack.

**sql-server-db** runs the official Microsoft SQL Server 2022 image. A named volume (`sqldata`) persists the database across container restarts, and a `healthcheck` probes the server using `sqlcmd` on a ten-second interval. This healthcheck enforces that the API container does not attempt to run EF Core migrations until SQL Server is genuinely ready to accept connections — eliminating a common race-condition failure in containerised applications [6].

**glms-backend-api** is built from `Dockerfile.api` using a multi-stage build [5] — the SDK image compiles and publishes the application, then only the slim ASP.NET runtime image is used for the final layer, keeping the production image lean. The container is configured via environment variables (`ConnectionStrings__DefaultConnection`, `Jwt__Key`) that follow ASP.NET Core's configuration provider conventions [4], allowing the same image to be deployed against different databases or with different secrets without rebuilding. The `depends_on: condition: service_healthy` clause ensures that EF Core's `MigrateAsync()` call in `Program.cs` only executes once SQL Server is ready [4].

**glms-frontend-web** is built from `Dockerfile.web` and configured with `ApiBaseUrl=http://glms-backend-api:5001`. This URL uses Docker's internal DNS to resolve the service name `glms-backend-api` to the correct container's IP address on the shared `glms-network` bridge network [6]. Containers communicate using stable, human-readable names rather than ephemeral IP addresses, meaning the frontend configuration does not change across environments.

The result is a fully reproducible deployment: a single `docker-compose up --build` command provisions the database, migrates the schema, starts the API, and starts the web frontend, in the correct dependency order, on any machine with Docker installed [5].

---

## References

[1] J. Humble and D. Farley, *Continuous Delivery: Reliable Software Releases through Build, Test, and Deployment Automation*. Boston, MA: Addison-Wesley, 2010.

[2] M. Fowler, "Integration Testing," *MartinFowler.com*, 2018. [Online]. Available: https://martinfowler.com/bliki/IntegrationTest.html [Accessed: 08 Jun. 2026].

[3] M. Jones, J. Bradley, and N. Sakimura, "JSON Web Token (JWT)," *RFC 7519*, Internet Engineering Task Force (IETF), May 2015. [Online]. Available: https://datatracker.ietf.org/doc/html/rfc7519 [Accessed: 08 Jun. 2026].

[4] Microsoft, "Integration tests in ASP.NET Core," *Microsoft Learn*, 2024. [Online]. Available: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests [Accessed: 08 Jun. 2026].

[5] Docker Inc., "Docker overview," *Docker Documentation*, 2024. [Online]. Available: https://docs.docker.com/get-started/docker-overview/ [Accessed: 08 Jun. 2026].

[6] Docker Inc., "Docker Compose overview," *Docker Documentation*, 2024. [Online]. Available: https://docs.docker.com/compose/ [Accessed: 08 Jun. 2026].
