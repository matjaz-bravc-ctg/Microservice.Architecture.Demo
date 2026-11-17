![](res/Containers.jpg)
# Microservice Architecture in ASP.NET Core

This demo shows a working sample of microservices architecture using ASP.NET Core 10 (.NET 10). It covers how to create microservices, how to create API gateways using [Ocelot](https://github.com/ThreeMammals/Ocelot), how to use [MassTransit](http://masstransit-project.com) as distributed application framework, [RabbitMQ](https://www.rabbitmq.com) as the message broker, how to unit test microservices using [xUnit](https://xunit.net) and how to deploy microservices using [Docker](https://www.docker.com/resources/what-container) containers on Linux distributions.

## Introduction

A microservices architecture consists of a collection of small, independent, and loosely coupled services. Each service is self-contained, implements a single business capability, is responsible for persisting its own data, is a separate codebase, and can be deployed independently. Microservices do not know about each other and the only coupling between them is the standalone contract project that both references.

API gateways are entry points for clients. Instead of calling services directly, clients call the API gateway, which forwards the call to the appropriate services.

## Advantages

* Services are easy and self-deployable
* Services are easy to change and test
* Services can be deployed in multiple servers to enhance performance
* Failure in one service does not impact other services
* It's easier to manage bug fixes and feature releases
* Developers can better understand the functionality of a service

## Why MassTransit?
[MassTransit](http://masstransit-project.com) is a free, open source, lightweight distributed application framework for creating distributed applications using the .NET framework. MassTransit provides an extensive set of features on top existing message transports, resulting in a developer friendly way to asynchronously connect services using message-based conversation patterns. Message-based communication is a reliable and scalable way to implement a service oriented architecture.
If you want to know more please visit [MassTransit](http://masstransit-project.com).

## Why RabbitMQ?
[RabbitMQ](https://www.rabbitmq.com) describes itself as the most widely deployed open-source message broker. It is easy to implement and supports a wide variety of technologies like Docker, .Net or Go. It also offers plugins for monitoring, managing or authentication. I've chose RabbitMQ because it is well known, quickly implemented and especially can be easily run using Docker.

## Why Ocelot?
There are so many API Gateway frameworks from Open Source to Commercial, but you have to buy license or subscription like NGNIX, Kong, Tyk ...
Microsoft and Amazon also provide API Gateway services on the cloud: [Azure API Management](https://azure.microsoft.com/en-us/services/api-management/) and [AWS API Gateway](https://aws.amazon.com/api-gateway/).
[Ocelot](https://github.com/ThreeMammals/Ocelot) is an open-source API Gateway built on ASP.Net Core. Although, it is lightweight but it also provides fully basic functions that an API Gateway must-have (Routing, API Composition, Caching, Logging, Integration with Identity Provider, Rate limiting).

## Architecture
* **Exchange.Rates.Gateway**: Gateway to all APIs
* **Exchange.Rates.CoinCap.OpenApi**: REST for retrieving pricing and market activity for crypto currencies
* **Exchange.Rates.CoinCap.Polling.Api**: Service to retrieve crypto currencies info. CoinCap API 2.0 has been discontinued, so it now uses the public [CoinGecko API v3](https://docs.coingecko.com/reference/coins-markets) (no API key needed) and maps the result to the original CoinCap response shape.
* **Exchange.Rates.Ecb.OpenApi**: REST for European Central Bank (ECB) Foreign exchange rates
* **Exchange.Rates.Ecb.Polling.Api**: Service to retrieve ECB exchange rates. It uses the [Frankfurter API](https://frankfurter.dev/), which publishes the European Central Bank reference rates (no API key needed).

Each service is hosted in it's own Docker container (take a look into docker-compose project). Every service exposes a **/health** endpoint; the OpenApi and polling services also report the state of their RabbitMQ connection there.

![](res/Diagram.png)

## Implementation
For a sake of simplicity I didn't implement any load balancing, service discovery or identity functionality. I've used a [Docker Desktop](https://www.docker.com/products/docker-desktop) for Windows, so I didn't pollute my development environment with many things concurrently installed. I also assumed you know how to work [using Docker](https://docs.docker.com/docker-for-windows/) workflow.

Solution folder structure looks like this:  
![](res/VS_Solution.jpg)

## Certificate
Creating a Certificate to use ASP.NET Core with **HTTPS** in Docker you can create a certificate with command: **dotnet dev-certs https -ep [Path]-p [Password]**. I've created the certificate with SecretPassword as its password.
After creating the certificate, you only have to share it with your container.  
**NOTE:** You must set a password. Otherwise, Kestrel won’t be able to use the certificate!

## Docker-Compose Files
**Docker-compose.yml** file with setup for all the containers looks like this:
```yml
services:
    rabbitmq:
        container_name: rabbitmq
        hostname: rabbitmq
        image: masstransit/rabbitmq:latest
        healthcheck:
            # Wait for the broker to create its Erlang cookie; running the CLI earlier would create it as root and break the broker
            test: ["CMD-SHELL", "test -f /var/lib/rabbitmq/.erlang.cookie && rabbitmq-diagnostics -q ping"]
            interval: 5s
            timeout: 5s
            retries: 30
        networks:
            - common_network

    exchange.rates.coincap.openapi:
        container_name: exchange.rates.coincap.openapi
        image: exchange.rates.coincap.openapi:latest
        build:
            context: .
            dockerfile: Exchange.Rates.CoinCap.OpenApi/Dockerfile
        restart: on-failure
        depends_on:
            rabbitmq:
                condition: service_healthy
        networks:
            - common_network

    exchange.rates.coincap.polling.api:
        container_name: exchange.rates.coincap.polling.api
        image: exchange.rates.coincap.polling.api:latest
        build:
            context: .
            dockerfile: Exchange.Rates.CoinCap.Polling.Api/Dockerfile
        depends_on:
            rabbitmq:
                condition: service_healthy
        networks:
            - common_network

    exchange.rates.ecb.openapi:
        container_name: exchange.rates.ecb.openapi
        image: exchange.rates.ecb.openapi:latest
        build:
            context: .
            dockerfile: Exchange.Rates.Ecb.OpenApi/Dockerfile
        depends_on:
            rabbitmq:
                condition: service_healthy
        networks:
            - common_network

    exchange.rates.ecb.polling.api:
        container_name: exchange.rates.ecb.polling.api
        image: exchange.rates.ecb.polling.api:latest
        build:
            context: .
            dockerfile: Exchange.Rates.Ecb.Polling.Api/Dockerfile
        depends_on:
            rabbitmq:
                condition: service_healthy
        networks:
            - common_network

    exchange.rates.gateway:
        container_name: exchange.rates.gateway
        image: exchange.rates.gateway:latest
        build:
            context: .
            dockerfile: Exchange.Rates.Gateway/Dockerfile
        restart: on-failure
        networks:
            - common_network
        depends_on:
            - exchange.rates.ecb.openapi
            - exchange.rates.coincap.openapi

networks:
    common_network:
        driver: bridge
```
and **Docker-compose.override.yml** file:
```yml
services:
    rabbitmq:
        environment:
            - RABBITMQ_DEFAULT_USER=guest
            - RABBITMQ_DEFAULT_PASS=guest
        ports:
            - 5672:5672
            - 15672:15672

    exchange.rates.coincap.openapi:
        environment:
            - ASPNETCORE_ENVIRONMENT=Docker
            - ASPNETCORE_URLS=https://+:8081;http://+:8080
            - Kestrel__Certificates__Default__Path=/app/Infrastructure/Certificate/cert-aspnetcore.pfx
            - Kestrel__Certificates__Default__Password=SecretPassword
        ports:
            - 4000:8080
            - 4001:8081

    exchange.rates.coincap.polling.api:
        environment:
            - ASPNETCORE_ENVIRONMENT=Docker
            - ASPNETCORE_URLS=https://+:8081;http://+:8080
            - Kestrel__Certificates__Default__Path=/app/Infrastructure/Certificate/cert-aspnetcore.pfx
            - Kestrel__Certificates__Default__Password=SecretPassword
        ports:
            - 7000:8080
            - 7001:8081

    exchange.rates.ecb.openapi:
        environment:
            - ASPNETCORE_ENVIRONMENT=Docker
            - ASPNETCORE_URLS=https://+:8081;http://+:8080
            - Kestrel__Certificates__Default__Path=/app/Infrastructure/Certificate/cert-aspnetcore.pfx
            - Kestrel__Certificates__Default__Password=SecretPassword
        ports:
            - 5000:8080
            - 5001:8081

    exchange.rates.ecb.polling.api:
        environment:
            - ASPNETCORE_ENVIRONMENT=Docker
            - ASPNETCORE_URLS=https://+:8081;http://+:8080
            - Kestrel__Certificates__Default__Path=/app/Infrastructure/Certificate/cert-aspnetcore.pfx
            - Kestrel__Certificates__Default__Password=SecretPassword
        ports:
            - 6000:8080
            - 6001:8081

    exchange.rates.gateway:
        environment:
            - ASPNETCORE_ENVIRONMENT=Docker
            - ASPNETCORE_URLS=https://+:8081;http://+:8080
            - Kestrel__Certificates__Default__Path=/app/Infrastructure/Certificate/cert-aspnetcore.pfx
            - Kestrel__Certificates__Default__Password=SecretPassword
        ports:
            - 8000:8080
            - 8001:8081
```
**NOTE**: When starting multiple containers with a compose file, a **common_network** is created in **which all containers are using**. Containers can reach each other with the container name.

The services wait until RabbitMQ reports healthy, and they listen on ports 8080 (HTTP) and 8081 (HTTPS) inside the containers because the .NET images run as a non-root user.

## Setup the Containers
To execute compose file, open Powershell, and navigate to the compose file in the root folder. Then execute the following command: **docker-compose up -d --build --remove-orphans** (or **podman compose up -d --build** with Podman). The -d parameter executes the command detached. This means that the containers run in the background and don’t block your Powershell window.
To check all running Containers use **docker ps**.

## Runnig in Docker
![](res/DockerRunning.jpg)

## Swagger UI for [Exchange.Rates.CoinCap.OpenApi](https://localhost:4001/swagger/index.html)
![](res/CoinCapOpenApi.jpg)

## Swagger UI for [Exchange.Rates.Ecb.OpenApi](https://localhost:5001/swagger/index.html)
![](res/EcbOpenApi.jpg)

## Call Gateway
Let’s try to access **CoinCap API** through API Gateway:
**[https://localhost:8001/exchangeratescoincap/assetinfo?id=ethereum](https://localhost:8001/exchangeratescoincap/assetinfo?id=ethereum)**
![](res/Gateway.jpg)
And access **Ecb API** through API Gateway:
**[https://localhost:8001/exchangeratesecb/eurbaserates?symbols=USD,CHF,CZK](https://localhost:8001/exchangeratesecb/eurbaserates?symbols=USD,CHF,CZK)**
![](res/Gateway_Ecb.jpg)

## Gateway: QoS and Caching
Both gateway routes in **ocelot.json** use Ocelot's quality of service (Polly) and response caching ([CacheManager](https://ocelot.readthedocs.io/en/latest/features/caching.html), in-memory dictionary handle):
```json
"QoSOptions": { "ExceptionsAllowedBeforeBreaking": 3, "DurationOfBreak": 10000, "TimeoutValue": 20000 },
"CacheOptions": { "TtlSeconds": 15, "Region": "coincap-assets" }
```
Crypto prices are cached for 15 seconds and ECB rates (published once a day) for 60 seconds. Call the same gateway URL twice and the second answer comes from the cache: the **timestamp** of a crypto asset stays the same until the TTL expires, while calling the CoinCap OpenApi service directly (port 4000) returns a new one every time.

## Testing
The test project covers both services end to end without RabbitMQ or the external APIs:
* the OpenApi services are hosted with WebApplicationFactory, RabbitMQ is replaced with the MassTransit in-memory test harness, and the real polling consumers answer using fake API clients;
* the polling services are hosted with stubbed HTTP responses, to check their configuration and health endpoints;
* the CoinGecko and Frankfurter clients are unit tested against recorded responses.

Tests use [xUnit v3](https://xunit.net) on Microsoft.Testing.Platform (selected in **global.json**). Run them with:
```
dotnet test --solution Exchange.Rates.Microservices.Demo.sln
```

## Conclusion
From today’s DevOps position it is necessary to change applications fast and often. Additionally, microservices should run inside a container and Docker is the defacto standard container.

Enjoy!

- [Visual Studio](https://www.visualstudio.com/vs/community) 2026 or greater
- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/resources/what-container) or [Podman](https://podman.io)

## Tags & Technologies
- [.NET 10](https://github.com/dotnet/core/blob/main/release-notes/10.0)
- [Docker](https://www.docker.com/resources/what-container)  
- [Ocelot](https://github.com/ThreeMammals/Ocelot)  
- [MassTransit](http://masstransit-project.com)  
- [RabbitMQ](https://www.rabbitmq.com)  
- [OpenAPI](https://swagger.io/specification/)
- [Serilog](https://serilog.net/)
- [xUnit](https://xunit.net)

## Licence
Licenced under [MIT](http://opensource.org/licenses/mit-license.php).
Contact me on [LinkedIn](https://si.linkedin.com/in/matjazbravc).
