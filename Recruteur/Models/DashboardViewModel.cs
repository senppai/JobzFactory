using System;
using System.Collections.Generic;

namespace Recruteur.Models
{
    public class DashboardViewModel
    {
        public string RecruiterName { get; set; }
        public int TotalOffers { get; set; }
        public int PublishedOffers { get; set; }
        public int DraftOffers { get; set; }
        public int TotalApplications { get; set; }
        public IList<DashboardOfferItem> RecentOffers { get; set; } = new List<DashboardOfferItem>();
        public IList<DashboardApplicationItem> RecentApplications { get; set; } = new List<DashboardApplicationItem>();
    }

    public class DashboardOfferItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string City { get; set; }
        public bool IsPublished { get; set; }
        public int ApplicationCount { get; set; }
    }

    public class DashboardApplicationItem
    {
        public int Id { get; set; }
        public string CandidateName { get; set; }
        public string OfferTitle { get; set; }
        public int OfferId { get; set; }
        public DateTime AppliedAt { get; set; }
    }
}
