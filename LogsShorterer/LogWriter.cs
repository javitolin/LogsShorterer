using LogsShorterer.Entities;
using LogsShorterer.Writer;
using Newtonsoft.Json;
using System.Reflection;

namespace LogsShorterer
{
    public class LogWriter : ILogWriter
    {
        private readonly IWriter _writer;
        private readonly string[] _requiredKeys;

        public LogWriter(IWriter writer, string[] requiredKeys)
        {
            _writer = writer;
            _requiredKeys = requiredKeys;
        }

        public SplitResult PrintLogEntity(LogEntity logEntity, int maxLength)
        {
            var serialized = JsonConvert.SerializeObject(logEntity);
            if (serialized.Length <= maxLength)
            {
                _writer.Write(serialized);
                return SplitResult.SHORTER_THAN_MAX;
            }

            var propertiesValuesDict = new Dictionary<string, object>();
            var minimalLog = new Dictionary<string, object>();

            foreach (PropertyInfo propertyInfo in logEntity.GetType().GetProperties())
            {
                propertiesValuesDict[propertyInfo.Name] = propertyInfo.GetValue(logEntity)!;

                if (_requiredKeys.Contains(propertyInfo.Name))
                {
                    minimalLog.Add(propertyInfo.Name, propertyInfo.GetValue(logEntity)!);
                }
            }

            int minimalLogLength = JsonConvert.SerializeObject(minimalLog).Length;
            if (minimalLogLength > maxLength)
            {
                return SplitResult.CANT_BE_PRINTED; // There's nothing we can do
            }

            // Ordered asc
            var extraKeysOrderedDictoinary = propertiesValuesDict
                .Where(kv => !_requiredKeys.Contains(kv.Key))
                .OrderBy(kv => JsonConvert.SerializeObject(kv.Value).Length)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            int partNumber = 0;
            var totalNumberOfParts = SplitAndWriteLogMessage(maxLength, partNumber, extraKeysOrderedDictoinary, minimalLog);
            WriteFinalLog(minimalLog, totalNumberOfParts);

            return SplitResult.SPLITTED;
        }

        private int SplitAndWriteLogMessage(int maxLength, int partNumber, Dictionary<string, object> extraPropertiesOrdered, Dictionary<string, object> minimalMessage)
        {
            while (extraPropertiesOrdered.Count > 0)
            {
                var currentDictionaryToWrite = minimalMessage.ToDictionary(entry => entry.Key, entry => entry.Value);
                currentDictionaryToWrite["Part"] = partNumber.ToString();

                var currentLogToWrite = JsonConvert.SerializeObject(currentDictionaryToWrite);
                int currentLengthOfLog = currentLogToWrite.Length;

                foreach (var keyValue in extraPropertiesOrdered)
                {
                    int availableLength = maxLength - (currentLengthOfLog + keyValue.Key.Length + 6);

                    if (currentLengthOfLog >= maxLength || extraPropertiesOrdered.Count == 0 || availableLength <= 0)
                    {
                        // Need to log what we have and start from the beginning
                        partNumber++;
                        _writer.Write(currentLogToWrite);
                        break;
                    }

                    if (keyValue.Value.GetType() == typeof(string))
                    {
                        // If key holds a string value, we can split it into multiple logs
                        var propertyValue = keyValue.Value!.ToString()!;
                        int lengthToInsert = Math.Min(availableLength, propertyValue.Length);
                        currentDictionaryToWrite.Add(keyValue.Key, propertyValue[..lengthToInsert]);

                        if (availableLength >= propertyValue.Length)
                        {
                            // This means we managed to insert the whole value to the current log dictionary
                            extraPropertiesOrdered.Remove(keyValue.Key);
                        }
                        else
                        {
                            extraPropertiesOrdered[keyValue.Key] = propertyValue[lengthToInsert..];
                        }
                    }
                    else
                    {
                        // Otherwise, add it to the output dictionary as-is
                        currentDictionaryToWrite.Add(keyValue.Key, keyValue.Value);
                        extraPropertiesOrdered.Remove(keyValue.Key);
                    }

                    currentLogToWrite = JsonConvert.SerializeObject(currentDictionaryToWrite);
                    currentLengthOfLog = currentLogToWrite.Length;
                }

            }

            return partNumber;
        }

        private void WriteFinalLog(Dictionary<string, object> minimalLog, int totalNumberOfParts)
        {
            Dictionary<string, object> shorterLogEntity = minimalLog;
            shorterLogEntity["Part"] = totalNumberOfParts.ToString();
            shorterLogEntity["TotalNumberOfParts"] = totalNumberOfParts.ToString();
            _writer.Write(JsonConvert.SerializeObject(shorterLogEntity));
        }
    }
}
