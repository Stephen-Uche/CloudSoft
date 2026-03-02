
# 11. Implement Unit Tests and Organize Solution

## Goal
Organize the solution with proper project structure and implement unit tests for the service layer.

---

## Learning Objectives
By the end of this exercise, you will:

- Understand .NET solution and project organization
- Learn how to create and structure test projects
- Write unit tests using xUnit
- Use dependency injection in unit tests
- Work with test assertions and test patterns (AAA)

---

# Step-by-Step Instructions

## Step 1: Create the Solution Structure

### 1. Delete the old solution file
From your project root, remove the existing `.sln` file (if you have one):

```bash
rm *.sln
```

### 2. Move the Web project into `/src`

Goal structure: keep only these in root:

* `.gitignore`
* `ReadMe.md`

Move everything else into `src/`.

Example (macOS/Linux):

```bash
mkdir -p src
# Move everything EXCEPT .gitignore, ReadMe.md, and src itself
shopt -s extglob
mv !(.gitignore|ReadMe.md|src) src/
```

> If `shopt` fails, just move the project folder manually in Finder or VS Code.

### 3. Create a new solution file

From the project root:

```bash
dotnet new sln
```

### 4. Add the main project to the solution

```bash
dotnet sln add src/CloudSoft.csproj
```

### 5. Run the web project and verify it still works

```bash
dotnet run --project src/CloudSoft.csproj
```

✅ **Expected Result**

* The web app runs successfully
* Solution now has a clean structure:

  * Root: `CloudSoft.sln`
  * Source code: `src/`

---

## Step 2: Create the Test Project

### 1. Create `tests/` folder and a new xUnit test project

From the solution root:

```bash
mkdir tests
cd tests
dotnet new xunit -n Services.UnitTests
```

### 2. Add reference to the main project

Still inside `tests/`:

```bash
dotnet add Services.UnitTests reference ../src/CloudSoft.csproj
```

### 3. Add the test project to the solution

Go back to the solution root:

```bash
cd ..
dotnet sln add tests/Services.UnitTests/Services.UnitTests.csproj
```

### 4. Add required packages (in the test project folder)

```bash
cd tests/Services.UnitTests
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package coverlet.collector
```

### 5. Run tests (should run the default sample test)

Back to solution root:

```bash
cd ../..
dotnet test
```

✅ **Expected Result**

* Test project builds
* Example tests run successfully

---

## Step 3: Create the Newsletter Service Tests

### 1. Create the test file

Create this file:

`tests/Services.UnitTests/NewsletterServiceTests.cs`

### 2. Add the unit tests (AAA pattern)

```csharp
using CloudSoft.Models;
using CloudSoft.Services;

namespace CloudSoft.Services.UnitTests;

public class NewsletterServiceTests
{
    private readonly INewsletterService _sut;

    public NewsletterServiceTests()
    {
        _sut = new NewsletterService();
    }

    [Fact]
    public async Task SignUpForNewsletterAsync_WithValidSubscriber_ReturnsSuccess()
    {
        // Arrange
        var subscriber = new Subscriber { Name = "Test User", Email = "user@example.com" };

        // Act
        var result = await _sut.SignUpForNewsletterAsync(subscriber);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains("Welcome to our newsletter", result.Message);
    }

    [Fact]
    public async Task SignUpForNewsletterAsync_WithDuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var subscriber1 = new Subscriber { Name = "Test User 1", Email = "duplicate@example.com" };
        var subscriber2 = new Subscriber { Name = "Test User 2", Email = "duplicate@example.com" };
        await _sut.SignUpForNewsletterAsync(subscriber1);

        // Act
        var result = await _sut.SignUpForNewsletterAsync(subscriber2);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("already subscribed", result.Message);
    }

    [Fact]
    public async Task OptOutFromNewsletterAsync_WithExistingEmail_ReturnsSuccess()
    {
        // Arrange
        var subscriber = new Subscriber { Name = "Test User", Email = "optoutuser@example.com" };
        await _sut.SignUpForNewsletterAsync(subscriber);

        // Act
        var result = await _sut.OptOutFromNewsletterAsync("optoutuser@example.com");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains("successfully removed", result.Message);
    }

    [Fact]
    public async Task OptOutFromNewsletterAsync_WithNonexistentEmail_ReturnsFailure()
    {
        // Act
        var result = await _sut.OptOutFromNewsletterAsync("nonexistent@example.com");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("couldn't find your subscription", result.Message);
    }

    [Fact]
    public async Task GetActiveSubscribersAsync_ReturnsAllSubscribers()
    {
        // Arrange
        var subscriber1 = new Subscriber { Name = "Test User 1", Email = "test1@example.com" };
        var subscriber2 = new Subscriber { Name = "Test User 2", Email = "test2@example.com" };
        await _sut.SignUpForNewsletterAsync(subscriber1);
        await _sut.SignUpForNewsletterAsync(subscriber2);

        // Act
        var subscribers = await _sut.GetActiveSubscribersAsync();

        // Assert
        Assert.True(subscribers.Count() >= 2); // At least 2 subscribers. Other tests add subscribers.
        Assert.Contains(subscribers, s => s.Email == "test1@example.com");
        Assert.Contains(subscribers, s => s.Email == "test2@example.com");
    }
}
```

✅ **Expected Result**

* Tests compile
* Each test checks one behavior
* AAA structure is clear:

  * Arrange (setup)
  * Act (execute)
  * Assert (verify)

---

## Step 4: Run the Tests

### Run all tests

```bash
dotnet test
```

### Run tests with coverage

```bash
dotnet test /p:CollectCoverage=true
```

✅ **Expected Result**

* All tests pass
* Coverage output appears in the test output (and/or produces coverage artifacts depending on your setup)

---

# Final Checks

## Step 1: Verify Solution Structure

Open in VS Code and confirm:

```text
CloudSoft.sln
src/
  CloudSoft.csproj
  ...
tests/
  Services.UnitTests/
    Services.UnitTests.csproj
    NewsletterServiceTests.cs
```

✅ **Expected Result**

* Clean structure
* Correct solution references

---

## Step 2: Run All Tests

```bash
dotnet test
```

✅ **Expected Result**

* All tests pass

---

## Step 3: Run Tests with Coverage in VS Code

1. Open **Test Explorer** (beaker icon / E-kolv)
2. Click **Run Tests with Coverage** (beaker with checkmark)
3. Review coverage highlights + percent per class

✅ **Expected Result**

* Coverage shown in Test Explorer
* Tested/untested lines are highlighted

---

# Exercise Done 🎉

You’ve successfully organized your project and implemented unit tests!

