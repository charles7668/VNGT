namespace GameManager.Modules.ToolInfo
{
    public interface IToolInfo
    {
        /// <summary>
        /// The name of the tool.
        /// </summary>
        string ToolName { get; }

        /// <summary>
        /// The version of the tool.
        /// </summary>
        string ToolVersion { get; }

        /// <summary>
        /// Launches the tool.
        /// </summary>
        /// <returns></returns>
        Task LaunchToolAsync();
    }
}