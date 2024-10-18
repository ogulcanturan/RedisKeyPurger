using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RedisKeyPurger
{
    public class Service
    {
        private readonly ILogger _logger;
        private readonly Task<ConnectionMultiplexer> _connectionTask;
        private readonly KeyPurgeOptions _options;

        public Service(ILogger<Service> logger, Task<ConnectionMultiplexer> connectionTask, KeyPurgeOptions options)
        {
            _logger = logger;
            _connectionTask = connectionTask;
            _options = options;
        }

        public async Task DeleteAsync(string keyPattern)
        {
            if (keyPattern == null)
            {
                throw new ArgumentNullException(nameof(keyPattern), "Key pattern can't be empty!");
            }

            var removedCount = 0L;

            const string filePath = nameof(DeleteAsync);

            var connection = await _connectionTask;

            while (true)
            {
                List<string> keys = null;

                try
                {
                    _logger.LogInformation("Retrieving uncompleted keys from file...");

                    keys = (await File.ReadAllLinesAsync(filePath))?.ToList();
                }
                catch (FileNotFoundException)
                {
                    _logger.LogInformation("Uncompleted keys not found.");
                }

                if (keys == null || string.Join("", keys).Length == 0)
                {
                    _logger.LogInformation("Retrieving keys from redis...");

                    keys = await GetSafeRedisKeysAsync(keyPattern, filePath);
                }

                _logger.LogInformation("Retrieving completed. Key count: {keyCount}", keys.Count);

                var redisKeys = keys.Select(k => new RedisKey(k)).ToArray();

                var iteration = redisKeys.Length / _options.BatchPurgeSize;

                var totalIteration = iteration + 1;

                _logger.LogInformation("Purging started. Total iteration: {totalIteration}, Batch purge size: {batchPurgeSize}", totalIteration, _options.BatchPurgeSize);

                for (var i = 0; i <= iteration; i++)
                {
                    var currentIteration = i + 1;
                    var remainingIteration = totalIteration - currentIteration;

                    _logger.LogInformation("Current iteration: {currentIteration} - Remaining iteration: {remainingIteration}", currentIteration, remainingIteration);

                    var removedKeys = redisKeys.Skip(_options.BatchPurgeSize * i).Take(_options.BatchPurgeSize).ToArray();
                    var result = 0L;
                    try
                    {
                        result = await connection.GetDatabase().KeyDeleteAsync(removedKeys);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Exception occurred during purge!");
                        
                        if (i == 0)
                        {
                            throw;
                        }

                        var uncompletedKeys = keys.Skip(i * _options.BatchPurgeSize).ToArray();

                        FileRemoveIfExists(filePath);

                        await File.WriteAllLinesAsync(filePath, uncompletedKeys);

                        throw;
                    }

                    await Task.Delay(_options.PurgeBatchDelay);

                    removedCount += result;
                }

                _logger.LogInformation("Purged count: {removedCount}", removedCount);

                if (keys.Count == 0)
                {
                    break;
                }

                FileRemoveIfExists(filePath);

                _logger.LogInformation("Preparing for the next loop...");
            }

            _logger.LogInformation("No further results found. Application is stopping...");
        }

        private async Task<List<string>> GetSafeRedisKeysAsync(string redisKey, string filePath)
        {
            var keys = await GetRedisKeys(redisKey);

            if (keys.Count > 0)
            {
                await File.WriteAllLinesAsync(filePath, keys);
            }

            return keys;
        }

        public async Task<List<string>> GetRedisKeys(string keyPattern)
        {
            var redisKeys = new List<string>();

            var count = 1;

            var connection = await _connectionTask;

            try
            {
                await foreach (var key in connection.GetServer(connection.GetEndPoints().First()).KeysAsync(pattern: keyPattern, pageSize: _options.BatchReadSize))
                {
                    redisKeys.Add(key);

                    if (count % _options.KeyInsertLogInterval == 0)
                    {
                        _logger.LogInformation("{keyInsertLogInterval} Keys added - Count: {count}", _options.KeyInsertLogInterval, redisKeys.Count);
                    }

                    if (redisKeys.Count == _options.RemovalThreshold)
                    {
                        _logger.LogInformation("Removal threshold of {removalThreshold} keys reached.", _options.RemovalThreshold);
                        break;
                    }

                    await Task.Delay(_options.ReadBatchDelay);

                    count++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured during the read");
            }

            return redisKeys;
        }

        private static void FileRemoveIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}