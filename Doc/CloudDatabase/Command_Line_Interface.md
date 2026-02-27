# Provisioning Cosmos DB via Azure CLI

## Goal

Provision an **Azure Cosmos DB** account with the **MongoDB API**, create a database and collection, and retrieve the connection string using the **Azure CLI**. Build a reusable shell script that automates the full provisioning workflow.

---

## What You'll Learn

* How to provision a Cosmos DB account with MongoDB API using `az cosmosdb` commands
* How to create databases and collections from the command line
* How to query connection strings and keys programmatically
* How to build a reusable provisioning script for repeatable deployments
* Key advantages of CLI-based provisioning over portal-based approaches

---

## Prerequisites

Before starting, ensure you have:

* ✓ Active Azure subscription with resource creation permissions
* ✓ Azure CLI installed and authenticated with `az login`
* ✓ A terminal such as `bash`, `zsh`, or Azure Cloud Shell
* ✓ Familiarity with basic shell scripting concepts

---

## Exercise Steps

### Overview

1. Set Up Variables and Resource Group
2. Create the Cosmos DB Account
3. Create the Database and Collection
4. Retrieve the Connection String
5. Build the Complete Provisioning Script
6. Test and Verify

---

## Step 1: Set Up Variables and Resource Group

Define reusable shell variables and create a resource group to hold your Cosmos DB resources. Using variables at the top of your workflow makes commands reusable and reduces errors caused by typos.

The resource group acts as a logical container for related Azure resources, making it easier to manage lifecycle, access control, and cost tracking together.

### Instructions

1. Open your terminal and ensure you are logged in to Azure:

```bash
az login
```

2. Define the following shell variables. Replace `yourname` with your actual name:

```bash
RESOURCE_GROUP="CloudDatabasesRG"
LOCATION="northeurope"
ACCOUNT_NAME="cosmosdb-yourname-bcd"
DATABASE_NAME="bookmarks_db"
COLLECTION_NAME="bookmarks"
```

3. Create the resource group:

```bash
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION
```

4. Verify the resource group was created:

```bash
az group show \
  --name $RESOURCE_GROUP \
  --query "{name:name, location:location}" \
  --output table
```

### Concept Deep Dive

Shell variables keep configuration values in one place, making scripts easier to maintain and reuse across environments.

The `--location` parameter determines the Azure region where the resource group metadata is stored. Choose a region close to you for lower latency during development.

The resource group itself is free; you only pay for the resources inside it.

### Common Mistakes

* Using uppercase letters or spaces in `ACCOUNT_NAME`. Cosmos DB account names must use lowercase letters, numbers, and hyphens only, and be 3 to 44 characters long.
* Forgetting to run `az login` first, which results in authentication errors on every subsequent command.
* Mixing variable names. In `bash`, `$RESOURCE_GROUP` and `$resource_group` are different.

✓ **Quick check:**
The `az group show` command displays your resource group name and location in a table.

---

## Step 2: Create the Cosmos DB Account

Create the Cosmos DB account with MongoDB API compatibility and serverless capacity mode.

This is the foundational resource. All databases and collections live within this account. Account creation takes several minutes because Azure is provisioning the underlying infrastructure.

### Instructions

1. Run the following command to create the Cosmos DB account:

```bash
az cosmosdb create \
  --name $ACCOUNT_NAME \
  --resource-group $RESOURCE_GROUP \
  --kind MongoDB \
  --server-version "4.2" \
  --capabilities EnableServerless \
  --locations regionName=$LOCATION failoverPriority=0 isZoneRedundant=false
```

2. Wait for the command to complete. This typically takes 3 to 8 minutes.

3. Verify the account was created successfully:

```bash
az cosmosdb show \
  --name $ACCOUNT_NAME \
  --resource-group $RESOURCE_GROUP \
  --query "{name:name, kind:kind, location:location}" \
  --output table
```

### Concept Deep Dive

Each flag in the create command has a specific role:

* `--kind MongoDB` selects the MongoDB API and enables MongoDB wire protocol compatibility.
* `--capabilities EnableServerless` sets the capacity mode to serverless, which charges only for consumed Request Units (RUs).
* `--server-version "4.2"` sets the MongoDB wire protocol version supported by the account.
* `--locations` configures the primary Azure region.

The `--query` and `--output table` flags use JMESPath expressions to extract specific fields from the JSON response and display them in a readable table.

### Common Mistakes

* Forgetting `--kind MongoDB`, which creates a NoSQL API account instead. The API type cannot be changed later.
* Omitting `--capabilities EnableServerless`, which defaults to provisioned throughput and can incur continuous charges.
* Reusing an account name that is already taken globally.

✓ **Quick check:**
The `az cosmosdb show` command displays a table where `kind` shows `MongoDB`.

---

## Step 3: Create the Database and Collection

Create the database and collection inside your Cosmos DB account.

These commands follow the Cosmos DB hierarchy:

```text
Account -> Database -> Collection -> Documents
```

The database is a logical grouping, while the collection stores your actual data.

### Instructions

1. Create the MongoDB database:

```bash
az cosmosdb mongodb database create \
  --account-name $ACCOUNT_NAME \
  --resource-group $RESOURCE_GROUP \
  --name $DATABASE_NAME
```

2. Create the collection with a shard key:

```bash
az cosmosdb mongodb collection create \
  --account-name $ACCOUNT_NAME \
  --resource-group $RESOURCE_GROUP \
  --database-name $DATABASE_NAME \
  --name $COLLECTION_NAME \
  --shard "category"
```

3. Verify the database was created:

```bash
az cosmosdb mongodb database list \
  --account-name $ACCOUNT_NAME \
  --resource-group $RESOURCE_GROUP \
  --output table
```

4. Verify the collection was created:

```bash
az cosmosdb mongodb collection list \
  --account-name $ACCOUNT_NAME \
  --resource-group $RESOURCE_GROUP \
  --database-name $DATABASE_NAME \
  --output table
```

### Concept Deep Dive

The `az cosmosdb mongodb` subcommand group provides MongoDB-specific operations. The command structure mirrors the data model:

* `az cosmosdb mongodb database` for database operations
* `az cosmosdb mongodb collection` for collection operations

The `--shard` parameter sets the partition key path and determines how Cosmos DB distributes documents across partitions.

Unlike the Azure Portal, CLI provisioning can be scripted, version-controlled, and integrated into CI/CD pipelines.

### Common Mistakes

* Using a leading `/` in the CLI shard key. In the CLI command, use `category`, not `/category`.
* Running the collection create command before the database exists.
* Mistyping `--account-name` as `--name` in the wrong place.

✓ **Quick check:**
Both list commands display your database and collection in table format.

---

## Step 4: Retrieve the Connection String

Retrieve the connection string that applications will use to connect to your Cosmos DB account.

The connection string uses the standard MongoDB URI format, so existing MongoDB drivers can connect to Cosmos DB without code changes.

### Instructions

1. Retrieve the primary connection string:

```bash
az cosmosdb keys list \
  --name $ACCOUNT_NAME \
  --resource-group $RESOURCE_GROUP \
  --type connection-strings \
  --query "connectionStrings[0].connectionString" \
  --output tsv
```

2. Store the connection string in an environment variable:

```bash
export COSMOS_CONNECTION_STRING=$(az cosmosdb keys list \
  --name $ACCOUNT_NAME \
  --resource-group $RESOURCE_GROUP \
  --type connection-strings \
  --query "connectionStrings[0].connectionString" \
  --output tsv)
```

3. Verify the variable was set without printing the full secret:

```bash
echo "Connection string starts with: ${COSMOS_CONNECTION_STRING:0:30}..."
```

> Treat this like a password. Never commit it to source control.
>
> For application configuration, use the secure setup flow in `Doc/DataLayer/Migrating_to_CosmosDB.md` under **Step 2: Update the Connection String Configuration**. That guide shows how to use `User Secrets` for Development and environment variables for Production.

### Concept Deep Dive

The `--type connection-strings` flag retrieves the full connection URI instead of raw access keys.

The `--query` parameter uses a JMESPath expression to extract the first connection string from the response array.

Using `--output tsv` produces raw text without JSON quotes, which is required when storing the value in a shell variable.

### Common Mistakes

* Using `--type keys` instead of `--type connection-strings`, which returns raw access keys rather than a MongoDB URI.
* Forgetting `--output tsv`, which leaves JSON quotes around the value.
* Assuming the environment variable persists across terminal sessions.
* Echoing the full connection string in shared terminals or logs.

✓ **Quick check:**
The echoed prefix starts with `mongodb://` and contains `.mongo.cosmos.azure.com:10255`.

---

## Step 5: Build the Complete Provisioning Script

Combine the individual commands into a reusable script.

Scripting the full provisioning flow means you can recreate the same infrastructure with a single command, which is useful for new environments, disaster recovery, and onboarding.

### Instructions

1. Create a file named `provision-cosmosdb.sh`

2. Add the following script:

```bash
#!/bin/bash
set -euo pipefail

# ============================================
# Cosmos DB Provisioning Script
# Provisions a Cosmos DB account with MongoDB API
# ============================================

# Configuration - change these for your environment
RESOURCE_GROUP="CloudDatabasesRG"
LOCATION="northeurope"
ACCOUNT_NAME="cosmosdb-yourname-bcd"
DATABASE_NAME="bookmarks_db"
COLLECTION_NAME="bookmarks"

echo "=== Cosmos DB Provisioning Script ==="
echo ""

# Step 1: Resource Group
echo "Creating resource group '$RESOURCE_GROUP' in '$LOCATION'..."
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION \
  --output none

# Step 2: Cosmos DB Account
echo "Creating Cosmos DB account '$ACCOUNT_NAME' (this may take several minutes)..."
az cosmosdb create \
  --name $ACCOUNT_NAME \
  --resource-group $RESOURCE_GROUP \
  --kind MongoDB \
  --server-version "4.2" \
  --capabilities EnableServerless \
  --locations regionName=$LOCATION failoverPriority=0 isZoneRedundant=false \
  --output none

# Step 3: Database
echo "Creating database '$DATABASE_NAME'..."
az cosmosdb mongodb database create \
  --account-name $ACCOUNT_NAME \
  --resource-group $RESOURCE_GROUP \
  --name $DATABASE_NAME \
  --output none

# Step 4: Collection
echo "Creating collection '$COLLECTION_NAME' with shard key 'category'..."
az cosmosdb mongodb collection create \
  --account-name $ACCOUNT_NAME \
  --resource-group $RESOURCE_GROUP \
  --database-name $DATABASE_NAME \
  --name $COLLECTION_NAME \
  --shard "category" \
  --output none

# Step 5: Retrieve Connection String
echo "Retrieving connection string..."
CONNECTION_STRING=$(az cosmosdb keys list \
  --name $ACCOUNT_NAME \
  --resource-group $RESOURCE_GROUP \
  --type connection-strings \
  --query "connectionStrings[0].connectionString" \
  --output tsv)

echo ""
echo "=== Provisioning Complete ==="
echo "Account:    $ACCOUNT_NAME"
echo "Database:   $DATABASE_NAME"
echo "Collection: $COLLECTION_NAME"
echo ""
echo "Connection String:"
echo "$CONNECTION_STRING"
```

3. Make the script executable:

```bash
chmod +x provision-cosmosdb.sh
```

4. Run the script:

```bash
./provision-cosmosdb.sh
```

### Concept Deep Dive

The `set -euo pipefail` line enables strict error handling:

* `-e` exits immediately if a command fails
* `-u` treats unset variables as errors
* `-o pipefail` ensures pipeline failures are not hidden

The `--output none` flag suppresses Azure CLI JSON output so the script prints only the progress messages you define.

This script is idempotent for the resource group, but not fully idempotent for the Cosmos DB account. Re-running it with the same account name may fail if the account already exists.

### Common Mistakes

* Forgetting `chmod +x`, which causes `Permission denied`.
* Running the script before logging in with `az login`.
* Printing the connection string to stdout and then sharing terminal output or CI logs.

✓ **Quick check:**
The script runs without errors and prints the account name, database, collection, and connection string at the end.

---

## Step 6: Test and Verify

Confirm that all resources were provisioned correctly by checking them from both the CLI and the Azure Portal.

### Instructions

1. Verify the Cosmos DB account exists and has the expected configuration:

```bash
az cosmosdb show \
  --name $ACCOUNT_NAME \
  --resource-group $RESOURCE_GROUP \
  --query "{name:name, kind:kind, capabilities:capabilities[0].name}" \
  --output table
```

2. Verify the database exists:

```bash
az cosmosdb mongodb database list \
  --account-name $ACCOUNT_NAME \
  --resource-group $RESOURCE_GROUP \
  --query "[].name" \
  --output tsv
```

3. Open the Azure Portal at `https://portal.azure.com` and confirm:

* The account shows `API: MongoDB`
* The account uses `Serverless` capacity
* The `bookmarks_db` database appears in Data Explorer
* The `bookmarks` collection exists with shard key `/category`

4. Clean up resources when you are finished experimenting:

```bash
az group delete --name $RESOURCE_GROUP --yes --no-wait
```

### Concept Deep Dive

The `--no-wait` flag starts the deletion process and returns immediately. The resource group and its contents are still deleted asynchronously in the background.

Verifying CLI-provisioned resources in the Azure Portal is good practice because it confirms the automation produced the expected result.

### Common Mistakes

* Running `az group delete` without `--yes`, which causes the CLI to wait for interactive confirmation.
* Deleting a resource group that contains other resources you meant to keep.
* Assuming `--no-wait` means the deletion completed instantly.

✓ **Quick check:**
The portal shows your Cosmos DB account with MongoDB API and Serverless capacity, and the database and collection are visible in Data Explorer.

---

## Common Issues

If you encounter problems:

* `"The subscription is not registered to use namespace 'Microsoft.DocumentDB'"`: run `az provider register --namespace Microsoft.DocumentDB` and wait a few minutes before retrying.
* `"Account name already exists"`: Cosmos DB account names are globally unique. Change `ACCOUNT_NAME` to include a random suffix or your initials.
* `"AuthorizationFailed"`: ensure your Azure account has at least `Contributor` on the subscription or resource group.
* Command takes a long time: `az cosmosdb create` can take up to 10 minutes. If it exceeds that, check the deployment status in the Azure Portal under the resource group's **Deployments** section.
* Wrong Azure subscription: verify with `az account show` and switch with `az account set --subscription "Your Subscription Name"`.

---

## Summary

You've provisioned Azure Cosmos DB infrastructure entirely from the command line:

* ✓ Created a Cosmos DB account with MongoDB API and Serverless capacity
* ✓ Created a database and collection with a partition key
* ✓ Retrieved the connection string programmatically
* ✓ Packaged the workflow into a reusable provisioning script

**Key takeaway:**
CLI-based provisioning turns manual portal actions into repeatable, scriptable commands that can be version-controlled, shared across teams, and integrated into CI/CD pipelines.

---

## Going Deeper

Want to extend this exercise?

* Add command-line arguments to the script with positional parameters or `getopts`
* Explore `az cosmosdb mongodb user` commands for database users and roles
* Add existence checks to make the script more idempotent
* Investigate `az cosmosdb keys regenerate` for key rotation

---

## Done

You can now provision and tear down Azure Cosmos DB infrastructure with Azure CLI automation, which sets up the next step toward infrastructure as code with Bicep templates.
