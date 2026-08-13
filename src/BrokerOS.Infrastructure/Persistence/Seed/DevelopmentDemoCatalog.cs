using BrokerOS.Domain.Enums;

namespace BrokerOS.Infrastructure.Persistence.Seed;

internal sealed record DemoUserSpec(string Email, string FullName, UserRole Role);

internal sealed record DemoInsurerSpec(string Name, string Code, string Email, string Phone, string Website);

internal sealed record DemoClientSpec(
    string CompanyName,
    string Industry,
    ClientType ClientType,
    string City,
    string State,
    string PostalCode,
    string AddressLine1);

internal enum DemoRenewalBucket
{
    Overdue,
    DueToday,
    DueWithin7Days,
    DueWithin30Days,
    DueWithin60Days,
    Completed,
    Lost,
    Later
}

internal static class DevelopmentDemoCatalog
{
    public const int ClientCount = 50;
    public const int PolicyCount = 100;

    public static readonly DemoUserSpec[] Users =
    [
        new("admin@apexbrokers.in", "Apex Admin", UserRole.BrokerAdmin),
        new("manager@apexbrokers.in", "Apex Manager", UserRole.BrokerManager),
        new("employee@apexbrokers.in", "Apex Employee", UserRole.BrokerEmployee),
        new("employee2@apexbrokers.in", "Meera Kulkarni", UserRole.BrokerEmployee),
        new("employee3@apexbrokers.in", "Vikram Nair", UserRole.BrokerEmployee)
    ];

    public static readonly DemoInsurerSpec[] Insurers =
    [
        new("ICICI Lombard", "ICICIL", "brokers@icicil.apexdemo.in", "+91 22 4000 1001", "https://www.icicilombard.com"),
        new("HDFC ERGO", "HDFCE", "brokers@hdfce.apexdemo.in", "+91 22 4000 1002", "https://www.hdfcergo.com"),
        new("Bajaj Allianz General", "BAJAJA", "brokers@bajaja.apexdemo.in", "+91 20 4000 1003", "https://www.bajajallianz.com"),
        new("Tata AIG General", "TATAIG", "brokers@tataig.apexdemo.in", "+91 22 4000 1004", "https://www.tataaig.com"),
        new("New India Assurance", "NIACL", "brokers@niacl.apexdemo.in", "+91 22 4000 1005", "https://www.newindia.co.in"),
        new("United India Insurance", "UIIC", "brokers@uiic.apexdemo.in", "+91 44 4000 1006", "https://www.uiic.co.in"),
        new("Oriental Insurance", "OICL", "brokers@oicl.apexdemo.in", "+91 11 4000 1007", "https://www.orientalinsurance.org.in"),
        new("National Insurance", "NICL", "brokers@nicl.apexdemo.in", "+91 33 4000 1008", "https://nationalinsurance.nic.co.in"),
        new("SBI General", "SBIG", "brokers@sbig.apexdemo.in", "+91 22 4000 1009", "https://www.sbigeneral.in"),
        new("Reliance General", "RGI", "brokers@rgi.apexdemo.in", "+91 22 4000 1010", "https://www.reliancegeneral.co.in")
    ];

    public static readonly DemoClientSpec[] Clients =
    [
        new("Sharma Logistics Pvt Ltd", "Logistics", ClientType.Corporate, "Mumbai", "Maharashtra", "400069", "12 Andheri East"),
        new("ABC Infrastructure Pvt Ltd", "Infrastructure", ClientType.Corporate, "Ahmedabad", "Gujarat", "380001", "44 SG Highway"),
        new("Patel Engineering Ltd", "Engineering", ClientType.Corporate, "Vadodara", "Gujarat", "390001", "8 Race Course Road"),
        new("Shree Logistics", "Logistics", ClientType.SME, "Surat", "Gujarat", "395003", "21 Ring Road"),
        new("Ahmedabad Manufacturing Co", "Manufacturing", ClientType.Corporate, "Ahmedabad", "Gujarat", "380015", "16 Odhav GIDC"),
        new("Gujarat Textile Industries", "Textiles", ClientType.Corporate, "Ahmedabad", "Gujarat", "380002", "5 Relief Road"),
        new("Western Construction Pvt Ltd", "Construction", ClientType.Corporate, "Pune", "Maharashtra", "411001", "9 Bund Garden"),
        new("Deccan Steel Works Pvt Ltd", "Manufacturing", ClientType.Corporate, "Pune", "Maharashtra", "411019", "72 Pimpri MIDC"),
        new("Malabar Spices Trading Co", "Trading", ClientType.SME, "Kochi", "Kerala", "682001", "3 Willingdon Island"),
        new("Sunrise Packers & Movers", "Logistics", ClientType.SME, "Bengaluru", "Karnataka", "560001", "18 MG Road"),
        new("Narmada Agro Industries", "Agriculture", ClientType.SME, "Bharuch", "Gujarat", "392001", "6 NH-48"),
        new("Coastal Marine Services Pvt Ltd", "Shipping", ClientType.Corporate, "Mumbai", "Maharashtra", "400038", "11 Ballard Estate"),
        new("Rajkot Auto Components Ltd", "Automotive", ClientType.Corporate, "Rajkot", "Gujarat", "360002", "4 Aji GIDC"),
        new("Himalaya Cold Storage", "Warehousing", ClientType.SME, "Chandigarh", "Chandigarh", "160001", "22 Industrial Area"),
        new("Kaveri Irrigation Systems", "Agriculture", ClientType.SME, "Coimbatore", "Tamil Nadu", "641001", "15 Avinashi Road"),
        new("Orient Pharma Distributors", "Pharmaceuticals", ClientType.SME, "Hyderabad", "Telangana", "500001", "8 Banjara Hills"),
        new("Pune Precision Tools Pvt Ltd", "Manufacturing", ClientType.Corporate, "Pune", "Maharashtra", "411018", "33 Chakan MIDC"),
        new("Chennai Port Handling Co", "Shipping", ClientType.Corporate, "Chennai", "Tamil Nadu", "600001", "2 Rajaji Salai"),
        new("Jaipur Handicrafts Exports", "Exports", ClientType.SME, "Jaipur", "Rajasthan", "302001", "7 MI Road"),
        new("Godavari Rice Mills", "Agro processing", ClientType.SME, "Rajahmundry", "Andhra Pradesh", "533101", "12 Danavaipeta"),
        new("Bengal Chemicals Traders", "Chemicals", ClientType.SME, "Kolkata", "West Bengal", "700001", "19 BBD Bagh"),
        new("Udaipur Marble Industries", "Mining", ClientType.SME, "Udaipur", "Rajasthan", "313001", "5 Sukher Industrial Area"),
        new("Coimbatore Knitwear Park", "Textiles", ClientType.Corporate, "Coimbatore", "Tamil Nadu", "641004", "27 Tiruppur Road"),
        new("Indore Food Processors Pvt Ltd", "Food processing", ClientType.Corporate, "Indore", "Madhya Pradesh", "452001", "14 Sanwer Road"),
        new("Lucknow Packaging Solutions", "Packaging", ClientType.SME, "Lucknow", "Uttar Pradesh", "226001", "9 Gomti Nagar"),
        new("Vizag Ship Repair Services", "Shipping", ClientType.Corporate, "Visakhapatnam", "Andhra Pradesh", "530001", "4 Harbour Park"),
        new("Nashik Vineyard Logistics", "Logistics", ClientType.SME, "Nashik", "Maharashtra", "422001", "16 College Road"),
        new("Bhopal Heavy Fabricators", "Engineering", ClientType.SME, "Bhopal", "Madhya Pradesh", "462001", "8 Govindpura"),
        new("Surat Diamond Tools Pvt Ltd", "Manufacturing", ClientType.Corporate, "Surat", "Gujarat", "395007", "11 Varachha"),
        new("Kochi Seafood Exporters", "Exports", ClientType.SME, "Kochi", "Kerala", "682005", "6 Aroor"),
        new("Hubli Transport Corporation", "Logistics", ClientType.SME, "Hubballi", "Karnataka", "580020", "3 Gokul Road"),
        new("Kanpur Leather Works", "Manufacturing", ClientType.SME, "Kanpur", "Uttar Pradesh", "208001", "17 Jajmau"),
        new("Nagpur Orange Traders", "Trading", ClientType.SME, "Nagpur", "Maharashtra", "440001", "5 Sitabuldi"),
        new("Thrissur Rubber Industries", "Manufacturing", ClientType.SME, "Thrissur", "Kerala", "680001", "10 Kanjani Road"),
        new("Jodhpur Solar Installations", "Energy", ClientType.SME, "Jodhpur", "Rajasthan", "342001", "8 Pal Road"),
        new("Guntur Tobacco Warehousing", "Warehousing", ClientType.SME, "Guntur", "Andhra Pradesh", "522001", "2 Brodipet"),
        new("Mysore Silk Weavers Co", "Textiles", ClientType.SME, "Mysuru", "Karnataka", "570001", "13 Sayyaji Rao Road"),
        new("Aurangabad Auto Ancillary", "Automotive", ClientType.Corporate, "Chhatrapati Sambhajinagar", "Maharashtra", "431001", "21 Waluj MIDC"),
        new("Ranchi Mining Supplies", "Mining", ClientType.SME, "Ranchi", "Jharkhand", "834001", "6 Main Road"),
        new("Guwahati Tea Estates Pvt Ltd", "Agriculture", ClientType.Corporate, "Guwahati", "Assam", "781001", "4 GS Road"),
        new("Panipat Textile Finishers", "Textiles", ClientType.SME, "Panipat", "Haryana", "132103", "9 GT Road"),
        new("Vapi Chemical Processors", "Chemicals", ClientType.Corporate, "Vapi", "Gujarat", "396195", "15 GIDC Phase 2"),
        new("Belgaum Foundry Works", "Engineering", ClientType.SME, "Belagavi", "Karnataka", "590001", "7 Udyambag"),
        new("Agra Footwear Components", "Manufacturing", ClientType.SME, "Agra", "Uttar Pradesh", "282001", "12 Sikandra"),
        new("Madurai Temple City Hotels", "Hospitality", ClientType.SME, "Madurai", "Tamil Nadu", "625001", "3 West Veli Street"),
        new("Silvassa Plastics Pvt Ltd", "Manufacturing", ClientType.Corporate, "Silvassa", "Dadra and Nagar Haveli", "396230", "8 Piparia"),
        new("Haridwar Ayurvedic Formulations", "Pharmaceuticals", ClientType.SME, "Haridwar", "Uttarakhand", "249401", "5 Industrial Area"),
        new("Bhavnagar Salt Works", "Chemicals", ClientType.SME, "Bhavnagar", "Gujarat", "364001", "11 Ghogha Road"),
        new("Tiruppur Garment Exporters", "Exports", ClientType.Corporate, "Tiruppur", "Tamil Nadu", "641601", "16 Kangeyam Road"),
        new("Palakkad Timber Traders", "Trading", ClientType.SME, "Palakkad", "Kerala", "678001", "4 College Road")
    ];

    public static readonly string[] ContactFirstNames =
    [
        "Ananya", "Devansh", "Isha", "Kabir", "Meera", "Neil", "Pooja", "Ravi", "Sana", "Tarun",
        "Diya", "Arjun", "Nisha", "Harsh", "Kavya", "Rohan", "Aditi", "Kunal", "Riya", "Varun"
    ];

    public static readonly string[] ContactLastNames =
    [
        "Shah", "Iyer", "Kulkarni", "Nair", "Desai", "Reddy", "Banerjee", "Joshi", "Menon", "Kapoor"
    ];

    public static readonly string[] ContactDesignations =
    [
        "Director", "Chief Financial Officer", "Admin Manager", "Plant Head", "Partner", "Operations Head"
    ];

    public static readonly PolicyType[] PolicyTypes =
    [
        PolicyType.Property,
        PolicyType.Marine,
        PolicyType.Engineering,
        PolicyType.Liability,
        PolicyType.Motor,
        PolicyType.Health,
        PolicyType.EmployeeBenefits
    ];

    public static DemoRenewalBucket BucketFor(int policyIndex) => policyIndex switch
    {
        >= 0 and <= 7 => DemoRenewalBucket.Overdue,
        >= 8 and <= 11 => DemoRenewalBucket.DueToday,
        >= 12 and <= 21 => DemoRenewalBucket.DueWithin7Days,
        >= 22 and <= 39 => DemoRenewalBucket.DueWithin30Days,
        >= 40 and <= 54 => DemoRenewalBucket.DueWithin60Days,
        >= 55 and <= 66 => DemoRenewalBucket.Completed,
        >= 67 and <= 72 => DemoRenewalBucket.Lost,
        _ => DemoRenewalBucket.Later
    };

    public static DateOnly ExpiryFor(int policyIndex, DateOnly today) => BucketFor(policyIndex) switch
    {
        DemoRenewalBucket.Overdue => today.AddDays(-(1 + policyIndex * 3)),
        DemoRenewalBucket.DueToday => today,
        DemoRenewalBucket.DueWithin7Days => today.AddDays(7 - (policyIndex - 12) % 7),
        DemoRenewalBucket.DueWithin30Days => today.AddDays(8 + (int)Math.Round((policyIndex - 22) * 22d / 17d)),
        DemoRenewalBucket.DueWithin60Days => today.AddDays(31 + (policyIndex - 40) * 2),
        DemoRenewalBucket.Completed => today.AddDays(-(25 + (policyIndex - 55) * 4)),
        DemoRenewalBucket.Lost => today.AddDays(4 + (policyIndex - 67) * 3),
        _ => today.AddDays(75 + (policyIndex - 73) * 8)
    };

    public static int ClientIndexForPolicy(int policyIndex) =>
        policyIndex is 0 or 8 or 12 or 22 or 40 or 55 ? 0 : 1 + policyIndex % 49;

    public static string PolicyNumber(int policyIndex) => $"POL-D{(policyIndex + 1).ToString("D3")}";

    public static string ClientCode(int clientIndex) => $"CLI-{(clientIndex + 1).ToString("D3")}";

    public static (decimal Premium, decimal SumInsured, decimal CommissionPercentage) MoneyFor(
        PolicyType policyType,
        int salt)
    {
        var jitter = 1m + salt % 9 * 0.07m;
        var (premium, sumInsured, commission) = policyType switch
        {
            PolicyType.Property => (320000m, 12500000m, 10.00m),
            PolicyType.Marine => (275000m, 9800000m, 12.50m),
            PolicyType.Engineering => (210000m, 7500000m, 11.50m),
            PolicyType.Liability => (165000m, 15000000m, 15.00m),
            PolicyType.Motor => (145000m, 4200000m, 10.00m),
            PolicyType.Health => (98000m, 1800000m, 8.00m),
            PolicyType.EmployeeBenefits => (240000m, 6500000m, 12.50m),
            _ => (120000m, 3000000m, 10.00m)
        };

        return (RoundMoney(premium * jitter), RoundMoney(sumInsured * jitter), commission);
    }

    public static string CompanyEmail(string companyName, int clientIndex)
    {
        var slug = new string(companyName.ToLowerInvariant().Where(character => char.IsLetterOrDigit(character)).ToArray());
        if (slug.Length > 24)
        {
            slug = slug[..24];
        }

        return $"accounts.{clientIndex + 1:D2}@{slug}.apexdemo.in";
    }

    public static string DemoPhone(int index) => $"+91 90000 {10000 + index:D5}";

    private static decimal RoundMoney(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
