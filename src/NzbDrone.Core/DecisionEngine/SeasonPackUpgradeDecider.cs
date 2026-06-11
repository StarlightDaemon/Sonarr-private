using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.DecisionEngine
{
    public static class SeasonPackUpgradeDecider
    {
        /// <summary>
        /// Shared season pack upgrade arithmetic for the All/Threshold/Any modes. Used by
        /// UpgradeDiskSpecification (disk path) and HistorySpecification (RSS grab path) so both
        /// specs apply identical criteria, differing only in the rejection reason they report.
        /// </summary>
        /// <param name="upgradableCount">Episodes in the pack that are missing or upgradable.</param>
        /// <param name="totalCount">All episodes mapped to the pack.</param>
        /// <param name="mode">The configured SeasonPackUpgrade mode.</param>
        /// <param name="threshold">The configured threshold percentage (used by Threshold mode).</param>
        /// <param name="rejectionReason">Reason to report when the pack does not meet the criteria.</param>
        public static DownloadSpecDecision Decide(int upgradableCount, int totalCount, SeasonPackUpgradeType mode, double threshold, DownloadRejectionReason rejectionReason)
        {
            if (totalCount == 0)
            {
                // Should not happen, but good to guard against it.
                return DownloadSpecDecision.Accept();
            }

            var upgradablePercentage = (double)upgradableCount / totalCount * 100;

            if (mode == SeasonPackUpgradeType.Any)
            {
                if (upgradableCount > 0)
                {
                    return DownloadSpecDecision.Accept();
                }
            }
            else
            {
                var effectiveThreshold = mode == SeasonPackUpgradeType.All
                    ? 100.0
                    : threshold;

                if (upgradablePercentage >= effectiveThreshold)
                {
                    return DownloadSpecDecision.Accept();
                }
            }

            return DownloadSpecDecision.Reject(rejectionReason, $"Season pack does not meet the upgrade criteria. Upgradable: {upgradableCount}/{totalCount} ({upgradablePercentage:0.##}%), Mode: {mode}, Threshold: {threshold}%");
        }
    }
}
