# Microsoft.Extensions.Caching.Dapr

### Introduction

This package serves as an implementation of `Microsoft.Extension.Caching.Abstractions`, providing caching functionality through Dapr's state store component.

### Warning

It is advised to consider this package solely as a transitional solution for the following reasons:

1. Dapr's state management building block does not implement sliding expiration time. The sliding expiration time implemented by this package is achieved by **adding fields to the cached values**. Thus, you need to ensure that all read and write operations on the cache are handled through this library.
2. Dapr only supports caching data in JSON format and does not support byte arrays. Therefore, this package utilizes **Base64 decoding and encoding** when reading and writing cache data, resulting in unnecessary performance overhead.

### Recommendation

It is recommended to use this library only for feasibility testing of integrating your project with Dapr and not advisable for use in production environments. If you intend to use Dapr for caching operations in a production environment, it is recommended not to use `Microsoft.Extensions.Caching.Abstractions`.

### Running Tests

Before running, make sure you have initialized Dapr and your state store component is named `statestore`.

Run the following command in the `test` directory:

```
dapr run --app-id myapp --dapr-http-port 3500 dotnet test
```

### License

This project is licensed under the terms of the MIT license.

------

*Note: In this document, "Dapr" refers to "Distributed Application Runtime," an open-source project supported by Microsoft.*

