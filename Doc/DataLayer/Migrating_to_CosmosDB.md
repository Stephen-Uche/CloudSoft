
# Migrating to Cosmos DB

## Goal

Migrate the **Newsletter** application’s data layer from a local Docker-hosted MongoDB instance to **Azure Cosmos DB**.
Leverage the **repository pattern abstraction** to make this migration primarily a **configuration change** — updating the connection string and adjusting compatibility settings — **without modifying application code**.

---

## What You’ll Learn

* How repository pattern abstraction enables painless database migration
* How to configure Cosmos DB connection strings in ASP.NET Core
* How to handle Cosmos DB MongoDB API compatibility differences
* How to use feature flags to toggle between database backends
* How to verify data operations against a cloud-hosted database

---

## Prerequisites

Before starting, ensure you have:

✓ Completed the Newsletter app with repository pattern and MongoDB repository implementation
✓ Completed the **Cloud Databases** section (a provisioned Cosmos DB account with MongoDB API)
✓ The Cosmos DB connection string available
✓ The Newsletter application running locally with Docker MongoDB

---

## Exercise Steps – Overview

1. Prepare the Cosmos DB Database
2. Update the Connection String Configuration
3. Handle Cosmos DB Compatibility Settings
4. Configure the Feature Flag
5. Test the Migration

---

## Step 1: Prepare the Cosmos DB Database

Ensure your Cosmos DB account has a database and collection ready to receive the Newsletter application’s subscriber data. The collection needs a **shard key** that aligns with the application’s query patterns.

1. Open the Azure Portal

   ```
   https://portal.azure.com
   ```

2. Navigate to your **Cosmos DB account**

3. Open **Data Explorer** from the left menu

4. Create a new database named:

   ```
   cloudsoft
   ```

5. Create a new collection with the following settings:

   | Setting       | Value       |
   | ------------- | ----------- |
   | Collection id | subscribers |
   | Shard key     | /email      |

6. Verify the empty collection appears in the Data Explorer tree

### ℹ Concept Deep Dive

The shard key `/email` is chosen because:

* Email addresses have **high cardinality** (unique per subscriber)
* The application frequently queries by email (duplicate checks, unsubscribe)

This ensures **single-partition queries** for the most common operations.

The database and collection names **must match** the application configuration:

```json
"DatabaseName": "cloudsoft",
"SubscribersCollectionName": "subscribers"
```

Cosmos DB resource names are **case-sensitive**.

### ⚠ Common Mistakes

* Using a shard key that doesn’t exist in the document schema
* Case mismatch between `/email` and `/Email`
* Forgetting to create the collection before running the app

✓ **Quick check:**
Data Explorer shows `cloudsoft → subscribers` (empty collection)

---

## Step 2: Update the Connection String Configuration

Replace the local MongoDB connection string with the **Cosmos DB MongoDB API** connection string.

### Update `appsettings.json`

```json
{
  "MongoDb": {
    "ConnectionString": "mongodb://your-account:your-key@your-account.mongo.cosmos.azure.com:10255/?ssl=true&retrywrites=false&maxIdleTimeMS=120000",
    "DatabaseName": "cloudsoft",
    "SubscribersCollectionName": "subscribers"
  }
}
```

### 🔐 Recommended: Use User Secrets

```bash
dotnet user-secrets set "MongoDb:ConnectionString" "mongodb://your-account:your-key@your-account.mongo.cosmos.azure.com:10255/?ssl=true&retrywrites=false&maxIdleTimeMS=120000"
```

### ℹ Concept Deep Dive

* Cosmos DB implements the **MongoDB wire protocol**
* The same `MongoDB.Driver` works for:

  * Docker MongoDB
  * Azure Cosmos DB (MongoDB API)
* The migration succeeds because **infrastructure details are externalized into configuration**

User Secrets prevent accidental commits of sensitive credentials and are automatically loaded in the **Development** environment.

### ⚠ Common Mistakes

* Omitting `retrywrites=false` (causes write failures)
* Committing the Cosmos DB connection string to Git
* Leaving Docker MongoDB stopped while still using the old connection string

✓ **Quick check:**
Connection string contains `.mongo.cosmos.azure.com:10255`

---

## Step 3: Handle Cosmos DB Compatibility Settings

No code changes are required — only the **connection string parameters** matter.

### Verify `Program.cs`

```csharp
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    var mongoDbOptions = builder.Configuration
        .GetSection(MongoDbOptions.SectionName)
        .Get<MongoDbOptions>();

    return new MongoClient(mongoDbOptions?.ConnectionString);
});
```

### Required Parameters

```text
ssl=true
retrywrites=false
maxIdleTimeMS=120000
```

### ℹ Concept Deep Dive

* MongoDB enables **retryable writes** by default
* Cosmos DB MongoDB API does **not support retryable writes**
* `retrywrites=false` disables this unsupported feature
* `MongoClient(connectionString)` automatically parses all parameters

Because your architecture already isolates infrastructure concerns, the migration remains **configuration-only**.

### ⚠ Common Mistakes

* Setting `ssl=false`
* Forgetting `retrywrites=false`
* Assuming Cosmos DB behaves exactly like native MongoDB

✓ **Quick check:**
Application starts without MongoDB connection errors

---

## Step 4: Configure the Feature Flag

Ensure the MongoDB repository is active via feature flags.

### Update `appsettings.json`

```json
{
  "FeatureFlags": {
    "UseMongoDb": true
  }
}
```

### Verify Repository Selection Logic

```csharp
bool useMongoDb = builder.Configuration.GetValue<bool>("FeatureFlags:UseMongoDb");

if (useMongoDb)
{
    builder.Services.Configure<MongoDbOptions>(
        builder.Configuration.GetSection(MongoDbOptions.SectionName));

    builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
    {
        var mongoDbOptions = builder.Configuration
            .GetSection(MongoDbOptions.SectionName)
            .Get<MongoDbOptions>();

        return new MongoClient(mongoDbOptions?.ConnectionString);
    });

    builder.Services.AddSingleton<IMongoCollection<Subscriber>>(serviceProvider =>
    {
        var mongoDbOptions = builder.Configuration
            .GetSection(MongoDbOptions.SectionName)
            .Get<MongoDbOptions>();

        var client = serviceProvider.GetRequiredService<IMongoClient>();
        var database = client.GetDatabase(mongoDbOptions?.DatabaseName);

        return database.GetCollection<Subscriber>(
            mongoDbOptions?.SubscribersCollectionName);
    });

    builder.Services.AddSingleton<ISubscriberRepository, MongoDbSubscriberRepository>();
    Console.WriteLine("Using MongoDB repository");
}
else
{
    builder.Services.AddSingleton<ISubscriberRepository, InMemorySubscriberRepository>();
    Console.WriteLine("Using in-memory repository");
}
```

### ℹ Concept Deep Dive

The feature flag controls **which repository is registered**, not how it works.
Because the MongoDB repository is compatible with both backends, switching databases requires **no code changes**.

✓ **Quick check:**
Console output shows:

```
Using MongoDB repository
```

---

## Step 5: Test the Migration

### Start the Application

```bash
dotnet run
```

### Test Workflows

#### Subscribe

* Enter email + name
* Submit form
* Subscriber appears in list

#### List

* Navigate to subscribers page
* Verify data is displayed

#### Unsubscribe

* Remove a subscriber
* Verify removal in UI

### Verify in Azure Portal

1. Open **Data Explorer**
2. Navigate to:

   ```
   cloudsoft → subscribers → Documents
   ```
3. Confirm documents appear with correct fields
4. Verify unsubscribed entries are removed (or flagged)

✓ **Success Indicators**

* Data visible in both app and Data Explorer
* No runtime or console errors
* CRUD operations work as expected

---

## Final Verification Checklist

☐ Application connects to Cosmos DB
☐ CRUD operations work correctly
☐ Data visible in Data Explorer
☐ No repository or controller code changed
☐ Feature flag toggles correctly

---

## Summary

You’ve successfully migrated the Newsletter application from **Docker MongoDB** to **Azure Cosmos DB**:

✓ Migration was configuration-only
✓ Repository pattern proved its value
✓ Same MongoDB.Driver works locally and in the cloud
✓ Feature flags preserve flexible development workflows

**Key takeaway:**
By programming against interfaces and following the **Dependency Inversion Principle**, a major infrastructure change became a routine configuration update.

---

## Going Deeper (Optional)

* Use `appsettings.Development.json` vs `appsettings.Production.json`
* Monitor Request Unit (RU) usage in Cosmos DB Metrics
* Add database health checks with `IHealthCheck`
* Explore global distribution with multi-region replicas

---

🎉 **Done!**
Excellent work — this exercise clearly demonstrates why architectural patterns matter in real-world systems.
