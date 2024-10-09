using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace RedisKeyPurger
{
    public class KeyPurgeOptions
    {
        public KeyPurgeOptions() { }

        [JsonConstructor]
        public KeyPurgeOptions(
            int batchReadSize,
            int keyInsertLogInterval,
            int removalThreshold,
            string readBatchDelay,
            int batchPurgeSize,
            string purgeBatchDelay
            )
        {
            BatchReadSize = batchReadSize;
            KeyInsertLogInterval = keyInsertLogInterval;
            RemovalThreshold = removalThreshold;

            if (!TimeSpan.TryParseExact(readBatchDelay, format: "G", CultureInfo.InvariantCulture,
                    out var readBatchDelayTimeSpan))
            {
                throw new FormatException($"{nameof(ReadBatchDelay)} property format is not correct, e.g. \"0:00:00:03.0000000\"");
            }

            ReadBatchDelay = readBatchDelayTimeSpan;

            BatchPurgeSize = batchPurgeSize;

            if (!TimeSpan.TryParseExact(purgeBatchDelay, format: "G", CultureInfo.InvariantCulture,
                    out var purgeBatchDelayTimeSpan))
            {
                throw new FormatException($"{nameof(PurgeBatchDelay)} property format is not correct, e.g. \"0:00:00:03.0000000\"");
            }

            PurgeBatchDelay = purgeBatchDelayTimeSpan;
        }

        /// <summary>
        /// The number of keys to read in each batch from Redis. Default is '1K'
        /// </summary>
        public int BatchReadSize { get; set; } = 1000;

        /// <summary>
        /// The number of key insertions after which a log entry will be created. Default is '100'
        /// </summary>
        public int KeyInsertLogInterval { get; set; } = 100;

        /// <summary>
        /// The threshold for starting the removal process after reading a certain number of keys. Default is '10M'.
        /// </summary>
        public int RemovalThreshold { get; set; } = 10000000;

        /// <summary>
        /// Delay between reading batches of keys from Redis. Default is '0'
        /// </summary>
        public TimeSpan ReadBatchDelay { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// The number of keys to purge in each batch. Default is '1K'
        /// </summary>
        public int BatchPurgeSize { get; set; } = 1000;

        /// <summary>
        /// Delay between purging batches of keys from Redis. Default is '0'
        /// </summary>
        public TimeSpan PurgeBatchDelay { get; set; } = TimeSpan.Zero;
    }
}
