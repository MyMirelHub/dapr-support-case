# Urgent Help Needed: Order Processing System Not Working

Hi Support team,

We've been trying to get our order processing system working with Dapr pub/sub, but we're running into issues. The messages aren't getting through to our processing service, and we can't figure out why. We've spent the last two days trying to debug this.

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
cd src/Publisher
dapr run --app-id publisher --app-port 5000 --dapr-http-port 3500 --resources-path ../../components/ -- dotnet run
```

Second service (Subscriber):

```bash
cd src/Subscriber
dapr run --app-id subscriber --app-port 5001 --dapr-http-port 3501 --resources-path ../../components/ -- dotnet run
```

## What's Happening

1. When we submit an order through the API (http://localhost:5000/order), the Publisher service accepts it and returns a success message
2. The order details look correct in the Publisher's logs
3. But the Subscriber service never receives or processes the order
4. We've verified both services are running and healthy

## Example Order Request

This is how we test it:

```bash
curl -X POST http://localhost:5000/order -H "Content-Type: application/json" -d '{
    "items": [
        {
            "productId": "PROD-1",
            "quantity": 2
        }
    ]
}'
```

## What We've Tried

1. We can see in the logs that the Publisher service is receiving the orders
2. The services fail to initialize but we don't know why!
3. We've checked that Redis is running (using the default Dapr Redis)
4. We're using the local secret store for Redis configuration
5. The subscriber has the proper Topic attribute on the endpoint

Could you please help us figure out what's wrong? This is blocking our development team from moving forward.

## Additional Context

- We're using .NET 8.0
- Dapr version: 1.14.4
- All components are deployed locally for development
- The API swagger is available at http://localhost:5000/swagger

Thank you for your help!