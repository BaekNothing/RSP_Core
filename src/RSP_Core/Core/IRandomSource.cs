namespace RSP_Core.Core
{
    /// <summary>
    /// Random number generator interface for dependency injection
    /// </summary>
    public interface IRandomSource
    {
        int Next(int min, int max);
    }

    /// <summary>
    /// Default implementation using System.Random
    /// </summary>
    public class DefaultRandomSource : IRandomSource
    {
        private readonly System.Random random;

        public DefaultRandomSource()
        {
            random = new System.Random();
        }

        public DefaultRandomSource(int seed)
        {
            random = new System.Random(seed);
        }

        public int Next(int min, int max)
        {
            return random.Next(min, max);
        }
    }
}
