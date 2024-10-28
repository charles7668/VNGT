using GameManager.Models;

namespace GameManager.Builders
{
    public class ProgramArgBuilder
    {
        private readonly ProgramArg _programArg = new();

        public ProgramArgBuilder WithDebugMode()
        {
            _programArg.IsDebugMode = true;
            return this;
        }

        public ProgramArg Build()
        {
            return _programArg;
        }
    }
}