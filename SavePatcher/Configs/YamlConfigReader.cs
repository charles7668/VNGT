using SavePatcher.Models;
using YamlDotNet.Serialization;

namespace SavePatcher.Configs
{
    public class YamlConfigReader<TResult> : IConfigReader<TResult>
    {
        /// <inheritdoc />
        public Result<TResult> Read(string content)
        {
            var deserializer = new DeserializerBuilder()
                .Build();
            TResult? result;
            try
            {
                result = deserializer.Deserialize<TResult>(content);
            }
            catch (Exception e)
            {
                return Result<TResult>.Failure(e.Message);
            }

            return Result<TResult>.Ok(result);
        }
    }
}