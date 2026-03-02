
# Provisioning Cosmos DB via Azure Portal

## 🎯 Goal

Create an **Azure Cosmos DB** account using the **MongoDB API** through the **Azure Portal**, set up a **database and collection**, explore **Data Explorer**, and retrieve the **connection string** for application use.

---

## 📘 What You’ll Learn

* How to create a Cosmos DB account with **MongoDB API compatibility**
* How to choose **Serverless capacity mode** to minimize costs
* How to create **databases and collections** in Cosmos DB
* How to navigate and use **Data Explorer** to manage data
* How to locate and copy the **connection string** for application use
* Key differences between **Cosmos DB** and **native MongoDB**

---

## ✅ Prerequisites

Before starting, ensure you have:

* ✓ Active **Azure subscription** with resource creation permissions
* ✓ Familiarity with the **Azure Portal** (or completed *Section 1: Server Foundation*)
* ✓ Basic understanding of **databases** and **JSON document structure**

---

## 🧪 Exercise Steps

### Overview

1. Create a Cosmos DB Account
2. Create a Database
3. Create a Collection
4. Insert a Test Document
5. Retrieve the Connection String
6. Explore Data Explorer Features

---

## 🟦 Step 1: Create a Cosmos DB Account

Set up an Azure Cosmos DB account configured with the **MongoDB API**.

Cosmos DB is Microsoft’s **globally distributed, multi-model NoSQL database service**. By selecting the MongoDB API, you get a cloud-hosted database that speaks the **MongoDB wire protocol**, meaning existing MongoDB drivers and tools work with minimal changes.

Choosing **Serverless capacity mode** ensures you only pay for what you use — ideal for learning and development.

### 🛠️ Instructions

1. Navigate to the Azure Portal
   👉 [https://portal.azure.com](https://portal.azure.com)

2. Search for **Azure Cosmos DB**

3. Select **Azure Cosmos DB** from the results

4. Click **+ Create**

5. Select **Azure Cosmos DB for MongoDB**

6. Choose **Request unit (RU) database account**

7. Configure the **Basics** tab:

   * **Subscription**: Your subscription
   * **Resource Group**: Create new or use existing

     * Example: `CloudDatabasesRG`
   * **Account Name**: Globally unique

     * Example: `cosmosdb-yourname-bcd`
   * **Region**: `North Europe` (or nearest region)
   * **Capacity mode**: **Serverless**

8. Click **Review + Create**

9. Click **Create**

10. Wait for deployment to complete (≈ 2–5 minutes)

---

### ℹ️ Concept Deep Dive

* Cosmos DB supports multiple APIs:

  * MongoDB
  * Cassandra
  * Gremlin (Graph)
  * Table
  * Native NoSQL API
* The **MongoDB API compatibility layer** allows you to:

  * Use standard MongoDB drivers
  * Use MongoDB query syntax
  * Avoid rewriting application code

#### Serverless Capacity Mode

* You pay only for **consumed Request Units (RU)**
* No minimum charge when idle
* Ideal for:

  * Learning
  * Development
  * Intermittent workloads

> 💡 A simple point-read of a 1 KB document ≈ **1 RU**

---

### ⚠ Common Mistakes

* ❌ Account name must be **globally unique**
* ❌ Choosing **Provisioned throughput** causes continuous charges
* ❌ Selecting a region far away increases latency
* ❌ Forgetting the resource group makes cleanup harder

**✓ Quick Check**

* API: **MongoDB**
* Capacity mode: **Serverless**

---

## 🟦 Step 2: Create a Database

Create a database to logically group your collections.

### 🛠️ Instructions

1. Open your **Cosmos DB account**

2. Click **Data Explorer**

3. Click **New Database**

4. Enter:

   ```text
   Database id: bookmarks_db
   ```

5. Click **OK**

---

### ℹ️ Concept Deep Dive

Cosmos DB hierarchy (MongoDB API):

```text
Account → Database → Collection → Document
```

* Databases are **organizational units**
* Scaling happens at the **collection** or **account** level

**✓ Quick Check**
`bookmarks_db` appears in the Data Explorer tree.

---

## 🟦 Step 3: Create a Collection

Create a collection to store your documents.

### 🛠️ Instructions

1. Expand **bookmarks_db**

2. Click the **⋯** menu

3. Select **New Collection**

4. Configure:

   ```text
   Collection id: bookmarks
   Shard key: /category
   ```

5. Click **OK**

---

### ℹ️ Concept Deep Dive

The **shard key (partition key)** determines how data is distributed.

Good shard keys:

* High cardinality
* Even data distribution
* Frequently used in queries

Example values for `/category`:

* `cloud`
* `development`
* `tools`
* `learning`

> ⚠ Partition keys **cannot be changed** after creation.

---

### ⚠ Common Mistakes

* ❌ Low-cardinality shard keys create hot partitions
* ❌ Missing shard key field causes null-key partitioning
* ❌ Shard key paths are **case-sensitive**

**✓ Quick Check**
`bookmarks` collection shows shard key `/category`.

---

## 🟦 Step 4: Insert a Test Document

### 🛠️ Instructions

1. Expand:

   ```text
   bookmarks_db → bookmarks → Documents
   ```
2. Click **New Document**
3. Replace JSON with:

```json
{
  "title": "Azure Cosmos DB Documentation",
  "url": "https://learn.microsoft.com/en-us/azure/cosmos-db/",
  "category": "cloud",
  "tags": ["azure", "database", "nosql"],
  "createdAt": "2026-02-22T10:00:00Z"
}
```

4. Click **Save**

Insert a second document:

```json
{
  "title": "MongoDB Driver for .NET",
  "url": "https://www.mongodb.com/docs/drivers/csharp/current/",
  "category": "development",
  "tags": ["mongodb", "dotnet", "driver"],
  "createdAt": "2026-02-22T10:05:00Z"
}
```

---

### ℹ️ Concept Deep Dive

Cosmos DB automatically adds system properties:

* `_id` – document identifier
* `_etag` – optimistic concurrency
* `_ts` – last modified timestamp
* `_rid`, `_self` – internal metadata

> 🧠 No schema required — documents can differ in structure.

---

### ⚠ Common Mistakes

* ❌ Missing shard key field
* ❌ Duplicate `_id` values
* ❌ Documents larger than **2 MB**

**✓ Quick Check**
Both documents appear with system properties.

---

## 🟦 Step 5: Retrieve the Connection String

### 🛠️ Instructions

1. Open the **Cosmos DB account overview**
2. Click **Connection strings**
3. Copy **PRIMARY CONNECTION STRING**

Example format:

```text
mongodb://<account>:<key>@<account>.mongo.cosmos.azure.com:10255/?ssl=true&retrywrites=false
```

> 🔐 Treat this like a password — never commit it to source control.
>
> Next step: use this connection string in the migration guide’s secure setup flow (`User Secrets` for Development and environment variables for Production) in `Doc/DataLayer/Migrating_to_CosmosDB.md` under **Step 2: Update the Connection String Configuration**.

---

### ℹ️ Concept Deep Dive

Key differences from native MongoDB:

* **Port 10255** (not 27017)
* **TLS/SSL required**
* `retrywrites=false` is mandatory
* Uses standard MongoDB URI format

---

## 🟦 Step 6: Explore Data Explorer Features

### 🛠️ Try These Queries

Filter by category:

```json
{ "category": "cloud" }
```

Filter by tag:

```json
{ "tags": "azure" }
```

Mongo Shell example:

```javascript
use bookmarks_db
db.bookmarks.find({ "category": "cloud" })
```

---

### ℹ️ Concept Deep Dive

* Queries using the shard key are **efficient**
* Queries without it perform **cross-partition scans**
* Data Explorer supports:

  * Visual browsing
  * MongoDB shell (subset)

---

## 🧯 Common Issues & Fixes

| Issue               | Solution                      |
| ------------------- | ----------------------------- |
| Account name exists | Add random suffix             |
| Long deployment     | Check **Deployments**         |
| No Data Explorer    | Ensure you’re in the resource |
| No query results    | Check case sensitivity        |
| Wrong API           | Must recreate account         |

---

## 📌 Summary

You have successfully:

* ✓ Provisioned Cosmos DB with MongoDB API
* ✓ Used **serverless capacity**
* ✓ Created a database and collection
* ✓ Inserted and queried documents
* ✓ Retrieved the MongoDB connection string

**Key takeaway:**
Cosmos DB with MongoDB API combines **managed cloud infrastructure** with **MongoDB ecosystem compatibility** — existing drivers work without code changes.

---

## 🚀 Going Deeper (Optional)

* Explore **Metrics** for RU consumption
* Try **global replication**
* Study **consistency levels**
* Review **Firewall & VNet** settings

---
