# Urgent Help Needed: Order Processing System Not Working

Hi Support team,

We've been trying to get our order processing system working with the Dapr pub/sub API, but we're running into issues. The messages aren't getting through to our processing service, and we can't figure out why. We've spent the last two days trying to debug this. Things have become worse after we moved the Redis host configuration to a local secret store. Now the apps won't even start :(

## Our Setup

We have two microservices built with .NET:

1. An Order Publisher service that accepts orders through a REST API
2. An Order Processor service that should receive and process these orders

The code is in our repository with this structure:

```text
📁 src/
├── Publisher/      # REST API to submit orders
└── Subscriber/     # Order processing service
```

We're using Dapr's pub/sub building block with Redis (the default one that comes with Dapr). Our component configurations are in the `components` directory.

## How to Run Our Services

First service (Publisher):

```bash
dapr run --app-id publisher --app-port 6500 --dapr-http-port 3500 --resources-path ../../components/ -- dotnet run
```

The second (subscriber) service runs on port 6501 and should be run with app-id subscriber.

## What was Happening when the apps were running

1. When we submit an order through the API (http://localhost:6500/order), the Publisher service accepts it and returns a success message. We've been testing this by running the following:

```bash
curl -X POST http://localhost:6500/order -H "Content-Type: application/json" -d '{
    "items": [
        {
            "productId": "PROD-1",
            "quantity": 2
        }
    ]
}'
```

2. The order details look correct in the Publisher's logs
3. But the Subscriber service never receives or processes the order

## What We've Tried

1. The services fail to initialize with Dapr but we don't know why!
2. Before, we could see in the logs that the Publisher service was receiving the orders
3. We've checked that Redis is running (using the default Dapr Redis)
4. We're now using the local secret store for Redis configuration

Could you please help us figure out what's wrong? This is blocking our development team from moving forward.

## Additional Context

- We're using .NET 8.0
- Dapr version: 1.14.4
- All components are deployed locally for development

Thank you for your help!

---
# Dapr PubSub Issue Resolution

## Problem Summary
The customer's order processing system using Dapr pub/sub was failing because of two critical issues:
1. **Naming mismatch**: The publisher and subscriber were configured to use different pubsub component names
2. **Secret configuration error**: A typo in the secrets file prevented proper authentication

## Detailed Analysis

### Environment Setup (Apple Silicon Mac)
```bash
# Install Dapr CLI
brew install dapr/tap/dapr-cli

# Install .NET 8
brew install dotnet@8

# Initialize Dapr
dapr init

```

### Investigation Steps 

1. Initial run of publisher revealed a secret configuration error:

```log
ERRO[0000] Error getting secret: secret redis-password not found
```

2. After fixing the secret, both services started successfully but no messages were delivered

3. Log analysis revealed a critical mismatch:

```log
# Publisher component registered as:
INFO[0000] Component loaded: orderpubsub (pubsub.redis/v1)

# But subscriber was listening on:
INFO[0002] app is subscribed to the following topics: [[order]] through pubsub=order-pubsub
```

## Root Causes Identified 
1. Component Name Mismatch:
    - Publisher configuration defined `orderpubsub` (no hyphen)
    - Subscriber code expected `order-pubsub` (with hyphen)
    - This subtle difference prevented message delivery despite both services being operational

2. Secret Configuration Typo:
    - `redis-passworld` instead of `redis-password` in secrets.json
    - This prevented loading the Redis password correctly

## Solution Applied 
1. Fix Secret Typo
```json
{
    "redis-password": ""
}
```

2. Standardize Component Names
    - Updated pubsub.yaml to use `order-pubsub`
    - Modified `OrderController.cs` constant:
```csharp
// Changed from
private const string PUBSUB_NAME = "orderpubsub";
// To
private const string PUBSUB_NAME = "order-pubsub";
```
3. Restart both services with correct component paths:

```bash
# Publisher
dapr run --app-id publisher --app-port 6500 --dapr-http-port 3500 --resources-path ./components/ -- dotnet run --project ./src/Publisher 

# Subscriber
dapr run --app-id subscriber --app-port 6501 --dapr-http-port 3501 --resources-path ./components/ -- dotnet run --project ./src/Subscriber 
```

## Confirmation 
After applying these fixes, the publisher successfully sent messages and the subscriber processed them:


```log
# Publisher log
== APP == info: OrderController[0]
== APP ==       Publishing order: e1d0538c-b874-4f4f-9464-2f3f8b1bfd97

# Subscriber log
== APP == info: OrderProcessor[0]
== APP ==       Processing order: 88264fb3-6b45-4f4d-8779-22724ad97c14
== APP == info: OrderProcessor[0]
== APP ==       Order processed successfully: 88264fb3-6b45-4f4d-8779-22724ad97c14
```

## Recommendations 

To prevent similar issues in the future 
1. Use Environment Variables for component names across services
2. Implement Configuration Management to ensure consistency
3. For kubernetes deployments 
    - Use K8s Secrets instead of file-based secrets
    - Define components as K8s resources
    - Use ConfigMaps for environment variables

This approach ensures component names remain synchronized and eliminates the file path and naming issues encountered in local development