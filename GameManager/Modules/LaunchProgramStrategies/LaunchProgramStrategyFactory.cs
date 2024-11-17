using GameManager.DB.Models;
using GameManager.Models;

namespace GameManager.Modules.LaunchProgramStrategies
{
    public class LaunchProgramStrategyFactory
    {
        public static IStrategy Create(GameInfo gameInfo, Action<int>? tryLaunchVNGTTranslator = null)
        {
            if (gameInfo.LaunchOption?.RunWithSandboxie is true)
            {
                return new LaunchWithSandboxie(gameInfo, tryLaunchVNGTTranslator);
            }

            if (gameInfo.LaunchOption?.LaunchWithLocaleEmulator is not "None")
            {
                return new LaunchWithLocaleEmulator(gameInfo, tryLaunchVNGTTranslator);
            }

            return new DirectLaunch(gameInfo, tryLaunchVNGTTranslator);
        }
    }
}