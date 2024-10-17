using System;

namespace c6dskk_a_hazi
{
    internal class Athlete
    {
        public string Name { get; set; }
        public string Sex { get; set; }
        public int? Age { get; set; } // Nullable int típus az "NA" értékek kezeléséhez
        public double? Height { get; set; } // Nullable double típus
        public double? Weight { get; set; } // Nullable double típus
        public string Team { get; set; }
        public string NOC { get; set; }
    }
}
