# RedisKeyPurger

A simple console application designed for batch removal of Redis keys based on a specified pattern. It reads keys in batches, purges them, and logs the process on the console.

### Configuration
The configuration for this application is managed through the appsettings.json file. Below is a guide on how to set up.

```json
{
  "KeyPattern": "mykey*",
  "ConnectionStrings": {
    "Redis": "localhost:6379,password=******,abortConnect=false"
  },
  "KeyPurgeOptions": {
    "BatchReadSize": 3000,
    "KeyInsertLogInterval": 100,
    "RemovalThreshold": 10000000,
    "ReadBatchDelay": "0:00:00:00.0100000",
    "BatchPurgeSize": 3000,
    "PurgeBatchDelay": "0:00:00:03.0000000"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Configuration Fields

- **KeyPattern:** The Redis key pattern to search for and remove (e.g., mykey* to match all keys starting with "mykey").
- **Redis:** Connection string for the Redis instance.

### KeyPurgeOptions
These options allow you to control how the Redis key purging is done:

- **BatchReadSize:** The number of keys to read in each batch from Redis. Default: 1000.
- **KeyInsertLogInterval:** Number of key insertions after which a log entry will be created. Default: 100.
- **RemovalThreshold:** Number of keys to read before starting the removal process. Default: 10M (10 million keys).
- **ReadBatchDelay:** Time delay between reading batches of keys from Redis. Default: 0.
- **BatchPurgeSize:** Number of keys to remove in each batch during the purging process. Default: 1000.
- **PurgeBatchDelay:** Time delay between purging batches of keys from Redis. Default: 0.

## Running the Application
Update the appsettings.json file with your Redis configuration and desired options.
Build the project and run the console app, and it will begin reading and purging keys based on the provided settings.
