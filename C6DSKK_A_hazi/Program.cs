using c6dskk_a_hazi;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Linq;

internal class Program
{
    // Kollekció a CSV beolvasott adatok tárolására
    public static List<Athlete> athletes = new List<Athlete>(); // Az athletes listában tároljuk az összes beolvasott versenyzőt
    public static string outputDirectory = ""; // A fájlok kimeneti könyvtára

    private static void Main(string[] args)
    {
        // 6. feladat: Utolsó argumentum az output útvonal
        if (args.Length != 2) // Ellenőrzi, hogy két argumentumot adtunk-e meg (CSV fájl elérési útvonal és az output könyvtár)
        {
            Console.WriteLine("Hiba: Egy CSV fájl elérési útvonalát és egy output könyvtárat várok!");
            return; // Hibás bemenet esetén kilép a program
        }

        string csvFilePath = args[0]; // Az első argumentum a CSV fájl elérési útvonala
        outputDirectory = args[1]; // A második argumentum az output könyvtár

        // Ellenőrizzük, hogy a megadott fájl létezik-e
        if (!File.Exists(csvFilePath)) // Megnézi, hogy a megadott CSV fájl létezik-e
        {
            Console.WriteLine("Hiba: A megadott fájl nem található.");
            return; // Ha nem található a fájl, a program kilép
        }

        // CSV fájl beolvasása a CsvHelper segítségével
        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                HeaderValidated = null,
                Delimiter = ",",
            };

            using (var reader = new StreamReader(csvFilePath))
            using (var csv = new CsvReader(reader, config))
            {
                // Beállítjuk, hogy az "NA" értékeket null-ként kezelje az Age mezőnél is
                csv.Context.TypeConverterOptionsCache.GetOptions<int?>().NullValues.Add("NA");
                csv.Context.TypeConverterOptionsCache.GetOptions<double?>().NullValues.Add("NA");

                athletes = csv.GetRecords<Athlete>().ToList();
            }

            Console.WriteLine($"Sikeresen beolvastuk {athletes.Count} versenyzőt."); // Visszajelzés a konzolon, hogy hány versenyzőt olvasott be
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hiba a CSV fájl beolvasása során: {ex.Message}"); // Hibaüzenet, ha a fájl beolvasása közben probléma adódik
            return; // Kilépés hiba esetén
        }

        // Felhasználói parancsok kezelése
        while (true)
        {
            Console.Write("> "); // Parancssor megjelenítése
            string? input = Console.ReadLine(); // Felhasználói bemenet beolvasása

            if (input != null && input.Equals("stop", StringComparison.OrdinalIgnoreCase)) // Ha a bemenet "stop", a program kilép
            {
                Console.WriteLine("A program leáll.");
                break;
            }

            if (!string.IsNullOrEmpty(input)) // Ellenőrzi, hogy a bemenet nem üres-e
            {
                string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries); // A parancsot és paramétereket szóközzel választja szét

                if (parts.Length == 2 && parts[0].Equals("team", StringComparison.OrdinalIgnoreCase)) // "team CSAPATNÉV" formátum feldolgozása
                {
                    string teamName = parts[1].Trim(); // A csapat nevét tárolja
                    CountAthletesByTeam(teamName); // Meghívja a metódust, amely megszámolja az adott csapatban lévő versenyzőket
                }
                else if (parts.Length == 4 && parts[0].Equals("count", StringComparison.OrdinalIgnoreCase)) // "count TULAJDONSÁG ÉRTÉK1 ÉRTÉK2" formátum feldolgozása
                {
                    string property = parts[1].Trim(); // A tulajdonságot tárolja (Age, Height, Weight)
                    int.TryParse(parts[2], out int value1); // Az első értéket int-re konvertálja
                    int.TryParse(parts[3], out int value2); // A második értéket int-re konvertálja
                    CountAthletesByProperty(property, value1, value2); // Meghívja a metódust, amely megszámolja a megadott tulajdonság alapján a versenyzőket
                }
                else if (parts.Length == 3 && parts[0].Equals("average", StringComparison.OrdinalIgnoreCase)) // "average CSAPATNÉV TULAJDONSÁG" formátum feldolgozása
                {
                    string teamName = parts[1].Trim(); // A csapat nevét tárolja
                    string property = parts[2].Trim(); // A tulajdonságot tárolja (Age, Height, Weight)
                    CalculateAverageByTeamAndProperty(teamName, property); // Meghívja a metódust, amely kiszámítja az adott csapat versenyzőinek átlagát az adott tulajdonság alapján
                }
                else
                {
                    Console.WriteLine("Ismeretlen parancs."); // Hibás bemenet esetén kiírja, hogy ismeretlen a parancs
                }
            }
        }
    }

    // 3. feladat: Metódus a csapat versenyzőinek megszámolására és CSV kiírása
    private static void CountAthletesByTeam(string teamName)
    {
        var query = from athlete in athletes
                    where athlete.Team.Equals(teamName, StringComparison.OrdinalIgnoreCase)
                    select athlete; // LINQ segítségével megszűri azokat a versenyzőket, akik az adott csapatban vannak

        int count = query.Count(); // Megszámolja, hányan tartoznak az adott csapathoz

        Console.WriteLine($"Az {teamName} csapatban összesen {count} versenyző indult."); // Kiírja a versenyzők számát

        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory); // Létrehozza az output könyvtárat, ha nem létezik
        }

        // CSV írása
        string filePath = Path.Combine(outputDirectory, $"team-{teamName}.csv"); // Létrehozza a fájl elérési útvonalát az output könyvtárban
        WriteToCsv(filePath, new[] { new { Team = teamName, Count = count } }); // Kiírja az eredményt egy CSV fájlba
    }

    // 4. feladat: Metódus az adott tulajdonság szerinti versenyzők kilistázására és CSV kiírása
    private static void CountAthletesByProperty(string property, int minValue, int maxValue)
    {
        IEnumerable<Athlete> filteredAthletes = Enumerable.Empty<Athlete>(); // Üres lista előkészítése

        switch (property.ToLower()) // A tulajdonság alapján dönt, hogy Age, Height vagy Weight alapján szűr
        {
            case "age":
                filteredAthletes = from athlete in athletes
                                   where athlete.Age >= minValue && athlete.Age <= maxValue
                                   select athlete; // Megszűri azokat a versenyzőket, akiknek az életkora a megadott tartományba esik
                break;
            case "height":
                filteredAthletes = from athlete in athletes
                                   where athlete.Height >= minValue && athlete.Height <= maxValue
                                   select athlete; // Megszűri azokat a versenyzőket, akiknek a magassága a megadott tartományba esik
                break;
            case "weight":
                filteredAthletes = from athlete in athletes
                                   where athlete.Weight >= minValue && athlete.Weight <= maxValue
                                   select athlete; // Megszűri azokat a versenyzőket, akiknek a súlya a megadott tartományba esik
                break;
            default:
                Console.WriteLine("Ismeretlen tulajdonság. Kérjük, használja az 'age', 'height' vagy 'weight' tulajdonságot.");
                return; // Hibás tulajdonság esetén visszatér


        }
        if (filteredAthletes.Any()) // Ellenőrzi, hogy van-e olyan versenyző, aki megfelel a feltételeknek
        {
            Console.WriteLine($"Versenyzők, akiknek a {property} értéke {minValue} és {maxValue} között van:"); // Kiírja, hogy mely versenyzők felelnek meg

            foreach (var athlete in filteredAthletes) // Végigmegy a megfelelő versenyzőkön
            {
                Console.WriteLine($"{athlete.Name}, {property}: {GetAthleteProperty(athlete, property)}"); // Kiírja a versenyző nevét és a megfelelő tulajdonságát
            }

            // CSV írása
            string filePath = Path.Combine(outputDirectory, $"count-{property}-{minValue}-{maxValue}.csv"); // Létrehozza a fájl elérési útvonalát az output könyvtárban
            WriteToCsv(filePath, filteredAthletes.Select(a => new { a.Name, PropertyValue = GetAthleteProperty(a, property) })); // Kiírja a szűrt versenyzők adatait egy CSV fájlba
        }
        else
        {
            Console.WriteLine($"Nincs olyan versenyző, akinek a {property} értéke {minValue} és {maxValue} között van."); // Ha nincs találat, kiírja, hogy nincs megfelelő versenyző
        }
    }

    // 5. feladat: Metódus a csapat és tulajdonság szerinti átlag számításához és CSV kiírása
    private static void CalculateAverageByTeamAndProperty(string teamName, string property)
    {
        IEnumerable<Athlete> teamAthletes = athletes.Where(a => a.Team.Equals(teamName, StringComparison.OrdinalIgnoreCase)); // Megkeresi az adott csapat versenyzőit

        if (!teamAthletes.Any()) // Ellenőrzi, hogy van-e versenyző az adott csapatban
        {
            Console.WriteLine($"Nincs versenyző az {teamName} csapatban."); // Ha nincs, kiírja, hogy nincs találat
            return;
        }

        // Átlag számítása az adott tulajdonság alapján
        double average = property.ToLower() switch
        {
            "age" => teamAthletes.Average(a => a.Age ?? 0), // Átlagos életkor
            "height" => teamAthletes.Average(a => a.Height ?? 0), // Átlagos magasság (ha a magasság null, akkor 0-ként kezeljük)
            "weight" => teamAthletes.Average(a => a.Weight ?? 0), // Átlagos súly (ha a súly null, akkor 0-ként kezeljük)
            _ => 0 // Ismeretlen tulajdonság esetén 0 visszaadása
        };

        if (average == 0 && (property.ToLower() != "age")) // Ellenőrzi, hogy ismeretlen tulajdonságot adtak-e meg
        {
            Console.WriteLine($"Ismeretlen tulajdonság: {property}. Használja az 'age', 'height' vagy 'weight' tulajdonságokat."); // Hibás tulajdonság esetén hibaüzenet
        }
        else
        {
            Console.WriteLine($"Az {teamName} csapat átlagos {property} értéke: {average:F2}."); // Kiírja az átlagot, formázott tizedesjegyekkel

            // CSV írása
            string filePath = Path.Combine(outputDirectory, $"average-{teamName}-{property}.csv"); // Létrehozza a fájl elérési útvonalát az output könyvtárban
            WriteToCsv(filePath, new[] { new { Team = teamName, Property = property, Average = average } }); // Kiírja az átlagot egy CSV fájlba
        }
    }

    // Segédfüggvény az adott tulajdonság visszaadására
    private static double GetAthleteProperty(Athlete athlete, string property)
    {
        // Visszaadja az adott versenyző megfelelő tulajdonságának értékét
        return property.ToLower() switch
        {
            "age" => athlete.Age ?? 0, // Életkor visszaadása (nem nullable, így nincs szükség további ellenőrzésre)
            "height" => athlete.Height ?? 0, // Magasság visszaadása (ha null, akkor 0-ként kezeljük)
            "weight" => athlete.Weight ?? 0, // Súly visszaadása (ha null, akkor 0-ként kezeljük)
            _ => 0 // Hibás tulajdonság esetén 0 visszaadása
        };
    }


    // 6. feladat: Eredmények kiírása CSV fájlba
    private static void WriteToCsv<T>(string filePath, IEnumerable<T> records)
    {
        using (var writer = new StreamWriter(filePath)) // Létrehozza a fájlba író StreamWriter-t
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture)) // Létrehozza a CsvWriter-t, amely a StreamWriter-t használja
        {
            csv.WriteRecords(records); // A rekordok írása a CSV fájlba
        }
        Console.WriteLine($"Eredmény kiírva: {filePath}"); // Visszajelzés konzolon, hogy a fájl sikeresen létrejött
    }
}
