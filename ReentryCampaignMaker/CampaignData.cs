using System.Collections.Generic;

namespace Reentry.Campaign
{
    public class CampaignMissionData
    {
        public enum CampaignMissionProgram { Mercury, Gemini, Apollo };
        public CampaignMissionProgram Program { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string CampaignProgressInfo { get; set; }
        public int missionIndex = 0;
    }

    public class CampaignSection
    {
        public enum CampaignSectionType { Header, Text, Image };
        public CampaignSectionType Type { get; set; } = CampaignSectionType.Text;
        public string Data { get; set; }
        public int Value { get; set; }
    }

    public class CampaignData
    {
        public string fullPathDescriptionFile;
        public string campaignPath;

        public string Title { get; set; }
        public string TileBackgroundImage { get; set; }
        public string PageBackgroundImage { get; set; }
        public List<CampaignSection> Sections { get; set; } = new List<CampaignSection>();
        public List<CampaignMissionData> Missions { get; set; } = new List<CampaignMissionData>();
        public string CompletionMessage { get; set; }
    }
}