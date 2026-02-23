```md
# Implement the Repository Pattern (Newsletter Subscriptions)

## Goal
Implement a **thread-safe in-memory database** using the **Repository Pattern** for the newsletter subscription system.

---

## Learning Objectives
By the end of this exercise, you will:

- Understand the **Repository Pattern** and its benefits
- Implement a thread-safe in-memory database using **ConcurrentDictionary**
- Create an **abstraction layer** between data access and business logic
- Register and use repositories with **Dependency Injection (DI)**
- Apply proper **async/await** patterns with repositories

---

## Project Structure (Suggested)
```

CloudSoft/
│
├── Models/
│   └── Subscriber.cs
│
├── Repositories/
│   ├── ISubscriberRepository.cs
│   └── InMemorySubscriberRepository.cs
│
├── Services/
│   ├── INewsletterService.cs
│   └── NewsletterService.cs
│
└── Program.cs

````

---

## Step 1: Create the Repository Interface

Create a new file:

**File:** `Repositories/ISubscriberRepository.cs`

```csharp
using CloudSoft.Models;

namespace CloudSoft.Repositories;

public interface ISubscriberRepository
{
    Task<IEnumerable<Subscriber>> GetAllAsync();
    Task<Subscriber?> GetByEmailAsync(string email);
    Task<bool> AddAsync(Subscriber subscriber);
    Task<bool> UpdateAsync(Subscriber subscriber);
    Task<bool> DeleteAsync(string email);
    Task<bool> ExistsAsync(string email);
}
````

### Information

The repository interface:

* Defines a **contract** for data access operations
* Uses **async/await** for future database compatibility
* Supports **SOLID** by depending on abstractions (DIP)
* Returns `Task<bool>` for operations that indicate **success/failure**
* Uses nullable reference types (`Subscriber?`) for operations that might return `null`

---

## Step 2: Implement the Thread-Safe Repository

Create a new file:

**File:** `Repositories/InMemorySubscriberRepository.cs`

```csharp
using CloudSoft.Models;
using System.Collections.Concurrent;

namespace CloudSoft.Repositories;

public class InMemorySubscriberRepository : ISubscriberRepository
{
    // Using ConcurrentDictionary for thread safety
    private readonly ConcurrentDictionary<string, Subscriber> _subscribers =
        new(StringComparer.OrdinalIgnoreCase);

    public Task<IEnumerable<Subscriber>> GetAllAsync()
    {
        return Task.FromResult(_subscribers.Values.AsEnumerable());
    }

    public Task<Subscriber?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return Task.FromResult<Subscriber?>(null);
        }

        _subscribers.TryGetValue(email, out var subscriber);
        return Task.FromResult(subscriber);
    }

    public Task<bool> AddAsync(Subscriber subscriber)
    {
        if (subscriber == null || string.IsNullOrEmpty(subscriber.Email))
        {
            return Task.FromResult(false);
        }

        // TryAdd returns true if the key was added, false if it already exists
        return Task.FromResult(_subscribers.TryAdd(subscriber.Email, subscriber));
    }

    public Task<bool> UpdateAsync(Subscriber subscriber)
    {
        if (subscriber == null || string.IsNullOrEmpty(subscriber.Email))
        {
            return Task.FromResult(false);
        }

        // Only update if it exists
        if (!_subscribers.ContainsKey(subscriber.Email))
        {
            return Task.FromResult(false);
        }

        // AddOrUpdate ensures thread safety
        _subscribers.AddOrUpdate(
            subscriber.Email,
            subscriber,
            (key, oldValue) => subscriber
        );

        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return Task.FromResult(false);
        }

        // TryRemove returns true if the item was removed
        return Task.FromResult(_subscribers.TryRemove(email, out _));
    }

    public Task<bool> ExistsAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(_subscribers.ContainsKey(email));
    }
}
```

### Information

This implementation:

* Uses `ConcurrentDictionary` for **thread-safe operations**
* Uses `Task.FromResult(...)` to simulate async operations
  *(in real apps, these would be actual async DB calls)*
* Uses **email as the key** for fast lookups
* Performs **null/empty checks** before operations
* Uses **case-insensitive comparison** for emails (`StringComparer.OrdinalIgnoreCase`)

---

## Step 3: Update the Newsletter Service

Update your service to depend on the repository abstraction.

**File:** `Services/NewsletterService.cs`

```csharp
using CloudSoft.Models;
using CloudSoft.Repositories;

namespace CloudSoft.Services;

public class NewsletterService : INewsletterService
{
    private readonly ISubscriberRepository _subscriberRepository;

    public NewsletterService(ISubscriberRepository subscriberRepository)
    {
        _subscriberRepository = subscriberRepository;
    }

    public async Task<OperationResult> SignUpForNewsletterAsync(Subscriber subscriber)
    {
        // Validate input
        if (subscriber == null || string.IsNullOrWhiteSpace(subscriber.Email))
        {
            return OperationResult.Failure("Invalid subscriber information.");
        }

        // Prevent duplicates
        if (await _subscriberRepository.ExistsAsync(subscriber.Email))
        {
            return OperationResult.Failure("You are already subscribed to our newsletter.");
        }

        // Add to repository
        var success = await _subscriberRepository.AddAsync(subscriber);

        if (!success)
        {
            return OperationResult.Failure("Failed to add your subscription. Please try again.");
        }

        return OperationResult.Success(
            $"Welcome to our newsletter, {subscriber.Name}! You'll receive updates soon."
        );
    }

    public async Task<OperationResult> OptOutFromNewsletterAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return OperationResult.Failure("Invalid email address.");
        }

        var subscriber = await _subscriberRepository.GetByEmailAsync(email);

        if (subscriber == null)
        {
            return OperationResult.Failure("We couldn't find your subscription in our system.");
        }

        var success = await _subscriberRepository.DeleteAsync(email);

        if (!success)
        {
            return OperationResult.Failure("Failed to remove your subscription. Please try again.");
        }

        return OperationResult.Success(
            "You have been successfully removed from our newsletter. We're sorry to see you go!"
        );
    }

    public async Task<IEnumerable<Subscriber>> GetActiveSubscribersAsync()
    {
        var subscribers = await _subscriberRepository.GetAllAsync();
        return subscribers.ToList();
    }
}
```

### Information

The updated service:

* Depends on the abstraction (`ISubscriberRepository`) ✅
* Uses **constructor injection** for testability ✅
* Delegates all **data access** to the repository ✅
* Keeps business logic focused on **rules + messages** ✅

---

## Step 4: Register the Repository in the DI Container

Open `Program.cs` and register your repository + service.

**File:** `Program.cs`

```csharp
using CloudSoft.Repositories;
using CloudSoft.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Register repository (in-memory DB)
builder.Services.AddSingleton<ISubscriberRepository, InMemorySubscriberRepository>();

// Register service (business logic)
builder.Services.AddScoped<INewsletterService, NewsletterService>();

var app = builder.Build();

// ...rest of the application setup...
app.Run();
```

### Information

Why `Singleton` for the repository?

* It represents the **in-memory database**
* Ensures **all requests share the same store**
* Data persists **between requests** while the app is running

Why `Scoped` for the service?

* One instance per request is typical for business logic services
* Keeps request-level behavior predictable

---

## Final Tests

### Testing the Repository Implementation

1. Run the application
2. Go to **Subscribe** page and add a subscriber
3. Check **Subscribers** page to verify it was added
4. Try subscribing with the **same email** (should block duplicates)
5. Test **unsubscribe**

✅ Expected Result:

* Subscription form works
* Subscribers persist while the app runs
* Duplicate emails are blocked
* Unsubscribe removes the subscriber

---

## Testing Thread Safety

1. Open the application in multiple browser tabs/windows
2. Add multiple subscribers quickly / simultaneously
3. Confirm everything is saved correctly

✅ Expected Result:

* All subscribers added without corruption
* No exceptions during concurrent access
* App handles multiple requests safely

---

## Exercise Done!

You’ve successfully implemented the **Repository Pattern** with a **thread-safe in-memory database** for newsletter subscriptions.

```
```
