namespace Big_Beautiful_Bot_Redux
{
    public class Config
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
        public string ProgFolder { get; set; }
        public string LorielleFolder { get; set; }
        public string PurinFolder { get; set; }
        public int MinWeight { get; set; }
        public decimal WeightLossRate { get; set; }
        public decimal HungerRate { get; set; }
        public int WeightAppetiteRatio { get; set; }
        public int OverfeedLimit { get; set; }
        public int TickInterval { get; set; }
        public string GeneralSizesFolder { get; set; }
        public string DefaultImageSource { get; set; }
        public string ThreeDimensionalFatsFolder { get; set; }
    }
}