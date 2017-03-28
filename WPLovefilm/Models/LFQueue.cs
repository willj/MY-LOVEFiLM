namespace WPLovefilm.Models
{
    public enum LFQueueTypes
    {
        RentedTitles,
        RentalQueue
    }

    public class LFQueueBase
    {
        public LFQueueTypes Type { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public string AllocatedDiscsImage { get; set; }
    }

    public class LFQueue : LFQueueBase
    {
        public string Url { get; set; }
        public bool Default { get; set; }
        public int Allocation { get; set; }

        //Hide inherited Prop
        new public string AllocatedDiscsImage
        {
            get
            {
                string imageUrl = string.Empty;

                switch (Allocation)
                {
                    case 1:
                        imageUrl = "/Images/Allocations/1disc.png";
                        break;
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        imageUrl = "/Images/Allocations/" + Allocation + "discs.png";
                    break;
                }

                return imageUrl;
            }
        }

    }

}
