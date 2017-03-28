using System.Collections.Generic;

namespace WPLovefilm.Models
{
    public class LFTitle
    {
        public string Id { get; set; }
        public string WebUrl { get; set; }
        public string TitleName { get; set; }
        public string Studio { get; set; }                          //optional
        public string ReleaseDate { get; set; }
        public float Rating { get; set; }                           //optional
        public int NumberOfRatings { get; set; }                    //optional
        public int ProductionYear { get; set; }                     //optional
        public int RunTime { get; set; }                            //optional
        public string Certificate { get; set; }
        public string SmallImage { get; set; }
        public string MediumImage { get; set; }
        public bool CanRent { get; set; }
        public string Synopsis { get; set; }
        public string Format { get; set; }
        public string Category { get; set; }
        public List<string> GenreList { get; set; }
        public List<string> ActorList { get; set; }
        public List<string> DirectorList { get; set; }
        public string Developer { get; set; }                       //Games only, optional
        public string Players { get; set; }                         //Games only, optional
        public bool IsVideo
        {
            get
            {
                return (this.Category == "video") ? true : false;
            }
        }
        public bool IsGame
        {
            get
            {
                return (this.Category == "games") ? true : false;
            }
        }

        public string Genres
        {
            get
            {
                return string.Join(", ", GenreList.ToArray());
            }
        }

        public string Actors
        {
            get
            {
                return string.Join(", ", ActorList.ToArray());
            }
        }

        public string Directors
        {
            get
            {
                return string.Join(", ", DirectorList.ToArray());
            }
        }

        public string CertImage
        {
            get
            {
                string imageUri = string.Empty;

                switch (this.Certificate)
                {
                    case "u":
                    case "U":
                        imageUri = "/Images/bbfc_u_50x.png";
                    break;
                    case "pg":
                    case "PG":
                        imageUri = "/Images/bbfc_pg_50x.png";
                    break;
                    case "12":
                        imageUri = "/Images/bbfc_12_50x.png";
                    break;
                    case "15":
                        imageUri = "/Images/bbfc_15_50x.png";
                    break;
                    case "18":
                        imageUri = "/Images/bbfc_18_50x.png";
                    break;
                }

                return imageUri;
            }
        }

        public string RatingImage
        {
            get
            {
                string imageUri = string.Empty;

                if (this.Rating == 0)
                {
                    imageUri = "/Images/Ratings/Stars0.0.jpg";
                }
                else if (this.Rating == 0.5)
                {
                    imageUri = "/Images/Ratings/Stars0.5.jpg";
                }
                else if (this.Rating == 1)
                {
                    imageUri = "/Images/Ratings/Stars1.0.jpg";
                }
                else if (this.Rating == 1.5)
                {
                    imageUri = "/Images/Ratings/Stars1.5.jpg";
                }
                else if (this.Rating == 2)
                {
                    imageUri = "/Images/Ratings/Stars2.0.jpg";
                }
                else if (this.Rating == 2.5)
                {
                    imageUri = "/Images/Ratings/Stars2.5.jpg";
                }
                else if (this.Rating == 3)
                {
                    imageUri = "/Images/Ratings/Stars3.0.jpg";
                }
                else if (this.Rating == 3.5)
                {
                    imageUri = "/Images/Ratings/Stars3.5.jpg";
                }
                else if (this.Rating == 4)
                {
                    imageUri = "/Images/Ratings/Stars4.0.jpg";
                }
                else if (this.Rating == 4.5)
                {
                    imageUri = "/Images/Ratings/Stars4.5.jpg";
                }
                else if (this.Rating == 5)
                {
                    imageUri = "/Images/Ratings/Stars5.0.jpg";
                }

                return imageUri;
            }
        }

        public PNTrailer Trailer { get; set; }
    }

    public class LFAtHomeTitle : LFTitle
    {
        public string ShipDate { get; set; }
    }

    public class LFQueueTitle : LFTitle
    {
        public string InQueueId { get; set; }
        public string QueueId { get; set; }
        public int Priority { get; set; }
        public string Availability { get; set; }
        public string EstimatedWait { get; set; }
        public int NumberOfDiscs { get; set; }

        public string DiscNumString
        {
            get
            {
                if (NumberOfDiscs > 1)
                {
                    return NumberOfDiscs + " Discs";
                }
                else
                {
                    return "";
                }
            }
        }

        public string QueueStatus
        {
            get
            {
                if (Availability == "unavailable")
                {
                    return "Reserved";
                }
                else if (Availability == "quarantined")
                {
                    //return "Upgrade Required";
                    return "Upgrade Package";
                }
                else if (EstimatedWait != "normal" && !string.IsNullOrEmpty(EstimatedWait))
                {
                    return EstimatedWait + " wait";
                }

                return "";
            }
        }

        public string PriorityImage
        {
            get
            {
                string p = "";
                switch (Priority)
                {
                    case 1:
                        p = "/Images/Priority/High.png";
                        break;
                    case 2:
                        p = "/Images/Priority/Medium.png";
                        break;
                    case 3:
                        p = "/Images/Priority/Low.png";
                        break;
                }
                return p;
            }
        }

    }
}
