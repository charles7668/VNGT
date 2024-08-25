using GameManager.DB.Models;

namespace GameManager.Models.LaunchProgramStrategies
{
    public class LaunchProgramStrategyFactory
    {
        public static IStrategy Create(GameInfo gameInfo, Action<int>? tryLaunchVNGTTranslator = null)
        {
            if (gameInfo.LaunchOption?.LaunchWithLocaleEmulator is not "None")
            {
                return new LaunchWithLocaleEmulator(gameInfo, tryLaunchVNGTTranslator);
            }

            return new DirectLaunch(gameInfo, tryLaunchVNGTTranslator);
        }
    }
}