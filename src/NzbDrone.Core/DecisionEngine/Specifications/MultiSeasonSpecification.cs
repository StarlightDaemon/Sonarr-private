using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class MultiSeasonSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public MultiSeasonSpecification(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
        {
            if (subject.ParsedEpisodeInfo.IsMultiSeason)
            {
                if (_configService.AllowMultiSeasonPacks)
                {
                    // The release spans seasons but the user has opted in. Let the quality and
                    // upgrade specifications (UpgradeDisk, History, FullSeason) do the real evaluation.
                    _logger.Debug("Multi-season release {0} allowed by config", subject.Release.Title);
                    return DownloadSpecDecision.Accept();
                }

                _logger.Debug("Multi-season release {0} rejected. Not supported", subject.Release.Title);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.MultiSeason, "Multi-season releases are not supported");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
