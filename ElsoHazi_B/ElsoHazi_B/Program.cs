using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using ElsoHazi_B;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

internal class Program
{
    public static List<Match> matches = new List<Match>();

    public static int city(string varos)
    {
        var count = Program.matches.Where(a => !string.IsNullOrEmpty(a.city) && a.city.Equals(varos)).Count();
        Console.WriteLine("{0} városban osszesen {1} mérkőzést játszottak",varos,count);
        return count;
    }

    public static IEnumerable<Match> year(string orszag, int kezdet, int veg)
    {
        try
        {
            var results = Program.matches.Where(a => !string.IsNullOrEmpty(a.country) && a.country.Equals(orszag) &&
                                                !string.IsNullOrEmpty(a.year) && int.Parse(a.year) >= kezdet && int.Parse(a.year) <= veg)
                .Select(a => a);

            Console.WriteLine($"{orszag} országban {kezdet} és {veg} közötti mérkózések:");
            foreach(Match item in results)
            {
                Console.WriteLine($"{item.home_team} vs {item.away_team} in {item.year}");
            }

            return results;
        } catch (Exception ex) {
            Console.WriteLine($"{ex.Message}");
            return Enumerable.Empty<Match>();
        }
    }

    public static double goal(string csapat, string isHazai)
    {
        try
        {
            bool hazai = isHazai == "true" ? true : false;

            if (hazai)
            {
                double result = Program.matches.Where(a => !string.IsNullOrEmpty(a.home_team) && a.home_team.Equals(csapat) &&
                     !string.IsNullOrEmpty(a.home_score) && !string.IsNullOrEmpty(a.away_score))
                    .Select(a => int.Parse(a.home_score) + int.Parse(a.away_score))
                    .Average();
                Console.WriteLine($"A {csapat} hazai szereplésekor az átalgos gólszám: {result}");
                return result;
            }
            else
            {
                double result = Program.matches.Where(a => !string.IsNullOrEmpty(a.away_team) && a.away_team.Equals(csapat) &&
                     !string.IsNullOrEmpty(a.home_score) && !string.IsNullOrEmpty(a.away_score))
                    .Select(a => int.Parse(a.home_score) + int.Parse(a.away_score))
                    .Average();
                Console.WriteLine($"A {csapat} vendég szereplésekor az átalgos gólszám: {result}");
                return result;
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
            return 0;
        }
    }


    private static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Hiba: Egy csv file bolvasását várom!");
            return;
        }

        string csvFilePath = args[0];
        string directoryPath = Path.GetDirectoryName(csvFilePath);

        if (!File.Exists(csvFilePath))
        {
            Console.WriteLine("Hiba: A megadott fájl az adott útvonalon nem létezik.");
            return;
        }

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
                csv.Context.TypeConverterOptionsCache.GetOptions<string?>().NullValues.Add("NA");
                matches = csv.GetRecords<Match>().ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hiba a CSV fájl beolvasása során: {ex.Message}");
            return;
        }

        while (true)
        {
            Console.Write("> ");
            string[] input = Console.ReadLine()?.Split(' ') ?? Array.Empty<string>();

            if (input[0].Equals("stop"))
            {
                Console.Write("A program leáll");
                return;
            }

            StreamWriter writer;
            CsvWriter csv2;

            switch (input[0])
            {
                case "city":
                    if(input.Length == 2)
                    {
                        var result = city(input[1]);

                        string filePath = Path.Combine(directoryPath, $"{input[0]}-{input[1]}.csv");
                        using (writer = new StreamWriter(filePath))
                        using (csv2 = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            HasHeaderRecord = false
                        }))
                        {
                            csv2.WriteRecord(result);
                        }
                    }
                    break;
                case "year":
                    if(input.Length == 4)
                    {
                        var results = year(input[1], int.Parse(input[2]), int.Parse(input[3]));

                        string filePath = Path.Combine(directoryPath, $"{input[0]}-{input[1]}-{input[2]}-{input[3]}.csv");
                        using (writer = new StreamWriter(filePath))
                        using (csv2 = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            HasHeaderRecord = false
                        }))
                        {                       
                            csv2.WriteRecords(results);
                        }
                    }
                    break;
                case "goal":
                    if(input.Length == 3)
                    {
                        var result = goal(input[1], input[2]);

                        string filePath = Path.Combine(directoryPath, $"{input[0]}-{input[1]}-{input[2]}.csv");
                        using (writer = new StreamWriter(filePath))
                        using (csv2 = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            HasHeaderRecord = false
                        }))
                        {
                            csv2.WriteRecord(result);
                        }
                    }
                    break;
            }
        }
    }
}