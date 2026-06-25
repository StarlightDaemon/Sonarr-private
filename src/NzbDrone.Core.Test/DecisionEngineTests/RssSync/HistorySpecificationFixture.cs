using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.History;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.CustomFormats;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests.RssSync
{
    [TestFixture]
    public class HistorySpecificationFixture : CoreTest<HistorySpecification>
    {
        private const int FIRST_EPISODE_ID = 1;
        private const int SECOND_EPISODE_ID = 2;

        private HistorySpecification _upgradeHistory;

        private RemoteEpisode _parseResultMulti;
        private RemoteEpisode _parseResultSingle;
        private Tuple<QualityModel, Language> _upgradableQuality;
        private Tuple<QualityModel, Language> _notupgradableQuality;
        private Series _fakeSeries;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();
            _upgradeHistory = Mocker.Resolve<HistorySpecification>();

            CustomFormatsTestHelpers.GivenCustomFormats();

            var singleEpisodeList = new List<Episode> { new Episode { Id = FIRST_EPISODE_ID, SeasonNumber = 12, EpisodeNumber = 3 } };
            var doubleEpisodeList = new List<Episode>
            {
                                                            new Episode { Id = FIRST_EPISODE_ID, SeasonNumber = 12, EpisodeNumber = 3 },
                                                            new Episode { Id = SECOND_EPISODE_ID, SeasonNumber = 12, EpisodeNumber = 4 },
                                                            new Episode { Id = 3, SeasonNumber = 12, EpisodeNumber = 5 }
            };

            _fakeSeries = Builder<Series>.CreateNew()
                .With(c => c.QualityProfile = new QualityProfile
                {
                    UpgradeAllowed = true,
                    Cutoff = Quality.Bluray1080p.Id,
                    FormatItems = CustomFormatsTestHelpers.GetSampleFormatItems("None"),
                    MinFormatScore = 0,
                    Items = Qualities.QualityFixture.GetDefaultQualities()
                })
                .Build();

            _parseResultMulti = new RemoteEpisode
            {
                Series = _fakeSeries,
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)), Languages = new List<Language> { Language.English } },
                Episodes = doubleEpisodeList,
                CustomFormats = new List<CustomFormat>()
            };

            _parseResultSingle = new RemoteEpisode
            {
                Series = _fakeSeries,
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)), Languages = new List<Language> { Language.English } },
                Episodes = singleEpisodeList,
                CustomFormats = new List<CustomFormat>()
            };

            _upgradableQuality = new Tuple<QualityModel, Language>(new QualityModel(Quality.SDTV, new Revision(version: 1)), Language.English);

            _notupgradableQuality = new Tuple<QualityModel, Language>(new QualityModel(Quality.HDTV1080p, new Revision(version: 2)), Language.English);

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableCompletedDownloadHandling)
                  .Returns(true);

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(x => x.ParseCustomFormat(It.IsAny<EpisodeHistory>(), It.IsAny<Series>()))
                  .Returns(new List<CustomFormat>());
        }

        private void GivenMostRecentForEpisode(int episodeId, string downloadId, Tuple<QualityModel, Language> quality, DateTime date, EpisodeHistoryEventType eventType)
        {
            Mocker.GetMock<IHistoryService>().Setup(s => s.MostRecentForEpisode(episodeId))
                  .Returns(new EpisodeHistory { DownloadId = downloadId, Quality = quality.Item1, Date = date, EventType = eventType, Languages = new List<Language> { quality.Item2 } });
        }

        private void GivenCdhDisabled()
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableCompletedDownloadHandling)
                  .Returns(false);
        }

        [Test]
        public void should_return_true_if_it_is_a_search()
        {
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new ReleaseDecisionInformation(false, new SeasonSearchCriteria())).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_latest_history_item_is_null()
        {
            Mocker.GetMock<IHistoryService>().Setup(s => s.MostRecentForEpisode(It.IsAny<int>())).Returns((EpisodeHistory)null);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_latest_history_item_is_not_grabbed()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.DownloadFailed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

// [Test]
//        public void should_return_true_if_latest_history_has_a_download_id_and_cdh_is_enabled()
//        {
//            GivenMostRecentForEpisode(FIRST_EPISODE_ID, "test", _notupgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
//            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
//        }

        [Test]
        public void should_return_true_if_latest_history_item_is_older_than_twelve_hours()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow.AddHours(-13), EpisodeHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_only_episode_is_upgradable()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_both_episodes_are_upgradable()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);
            GivenMostRecentForEpisode(SECOND_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_not_be_upgradable_if_both_episodes_are_not_upgradable()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);
            GivenMostRecentForEpisode(SECOND_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_first_episodes_is_upgradable()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_second_episodes_is_upgradable()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);
            GivenMostRecentForEpisode(SECOND_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_episode_is_of_same_quality_as_existing()
        {
            _fakeSeries.QualityProfile = new QualityProfile { Cutoff = Quality.Bluray1080p.Id, Items = Qualities.QualityFixture.GetDefaultQualities() };
            _parseResultSingle.ParsedEpisodeInfo.Quality = new QualityModel(Quality.WEBDL1080p, new Revision(version: 1));
            _upgradableQuality = new Tuple<QualityModel, Language>(new QualityModel(Quality.WEBDL1080p, new Revision(version: 1)), Language.English);

            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_cutoff_already_met()
        {
            _fakeSeries.QualityProfile = new QualityProfile { Cutoff = Quality.WEBDL1080p.Id, Items = Qualities.QualityFixture.GetDefaultQualities() };
            _parseResultSingle.ParsedEpisodeInfo.Quality = new QualityModel(Quality.WEBDL1080p, new Revision(version: 1));
            _upgradableQuality = new Tuple<QualityModel, Language>(new QualityModel(Quality.WEBDL1080p, new Revision(version: 1)), Language.Spanish);

            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_latest_history_item_is_only_one_hour_old()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow.AddHours(-1), EpisodeHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_latest_history_has_a_download_id_and_cdh_is_disabled()
        {
            GivenCdhDisabled();
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, "test", _upgradableQuality, DateTime.UtcNow.AddDays(-100), EpisodeHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_cutoff_already_met_and_cdh_is_disabled()
        {
            GivenCdhDisabled();
            _fakeSeries.QualityProfile = new QualityProfile { Cutoff = Quality.WEBDL1080p.Id, Items = Qualities.QualityFixture.GetDefaultQualities() };
            _parseResultSingle.ParsedEpisodeInfo.Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1));
            _upgradableQuality = new Tuple<QualityModel, Language>(new QualityModel(Quality.WEBDL1080p, new Revision(version: 1)), Language.Spanish);

            GivenMostRecentForEpisode(FIRST_EPISODE_ID, "test", _upgradableQuality, DateTime.UtcNow.AddDays(-100), EpisodeHistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_only_episode_is_not_upgradable_and_cdh_is_disabled()
        {
            GivenCdhDisabled();
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, "test", _notupgradableQuality, DateTime.UtcNow.AddDays(-100), EpisodeHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }

        private void GivenSeasonPack(SeasonPackUpgradeType mode, double threshold = 100.0)
        {
            _parseResultMulti.ParsedEpisodeInfo.FullSeason = true;

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.SeasonPackUpgrade)
                  .Returns(mode);

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.SeasonPackUpgradeThreshold)
                  .Returns(threshold);
        }

        [Test]
        public void should_accept_season_pack_when_no_episode_has_blocking_history()
        {
            GivenSeasonPack(SeasonPackUpgradeType.All);

            Mocker.GetMock<IHistoryService>().Setup(s => s.MostRecentForEpisode(It.IsAny<int>())).Returns((EpisodeHistory)null);

            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_season_pack_with_history_not_upgrade_reason_when_mode_is_all_and_one_episode_is_blocked()
        {
            GivenSeasonPack(SeasonPackUpgradeType.All);

            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);

            var decision = _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new());

            decision.Accepted.Should().BeFalse();
            decision.Reason.Should().Be(DownloadRejectionReason.HistoryNotUpgrade);
        }

        [Test]
        public void should_accept_season_pack_when_mode_is_any_and_one_episode_is_blocked()
        {
            GivenSeasonPack(SeasonPackUpgradeType.Any);

            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_season_pack_when_mode_is_any_and_all_episodes_are_blocked()
        {
            GivenSeasonPack(SeasonPackUpgradeType.Any);

            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);
            GivenMostRecentForEpisode(SECOND_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);
            GivenMostRecentForEpisode(3, string.Empty, _notupgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);

            var decision = _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new());

            decision.Accepted.Should().BeFalse();
            decision.Reason.Should().Be(DownloadRejectionReason.HistoryNotUpgrade);
        }

        [Test]
        public void should_accept_season_pack_when_mode_is_threshold_and_upgradable_percentage_meets_threshold()
        {
            GivenSeasonPack(SeasonPackUpgradeType.Threshold, 50.0);

            // 1 of 3 episodes blocked -> 66.67% upgradable >= 50%
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_season_pack_when_mode_is_threshold_and_upgradable_percentage_is_below_threshold()
        {
            GivenSeasonPack(SeasonPackUpgradeType.Threshold, 80.0);

            // 1 of 3 episodes blocked -> 66.67% upgradable < 80%
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);

            var decision = _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new());

            decision.Accepted.Should().BeFalse();
            decision.Reason.Should().Be(DownloadRejectionReason.HistoryNotUpgrade);
        }

        [Test]
        public void should_apply_season_pack_upgrade_mode_for_multi_season_pack()
        {
            _parseResultMulti.ParsedEpisodeInfo.IsMultiSeason = true;

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.SeasonPackUpgrade)
                  .Returns(SeasonPackUpgradeType.Any);

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.SeasonPackUpgradeThreshold)
                  .Returns(100.0);

            // One of three episodes has a blocking grab event; mode is Any so
            // at least one upgradable episode is sufficient — should Accept.
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_apply_season_pack_upgrade_mode_for_complete_series_pack()
        {
            _parseResultMulti.ParsedEpisodeInfo.IsCompleteSeries = true;

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.SeasonPackUpgrade)
                  .Returns(SeasonPackUpgradeType.Any);

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.SeasonPackUpgradeThreshold)
                  .Returns(100.0);

            // One of three episodes has a blocking grab event; mode is Any so
            // at least one upgradable episode is sufficient — should Accept.
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, EpisodeHistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }
    }
}
