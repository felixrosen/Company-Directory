using CompanyDirectory.API.Infrastructure.Orleans.Types;
using CompanyDirectory.API.Operations.Companies.Services.Grains.CompanyGrain.Types;

namespace CompanyDirectory.API.Operations.Companies.Services.Grains.CompanyGrain;

public enum CompanyGrainStateType
{
    Created,
    Updated,
    Deleted,
}

public class GrainStateStore<T> where T : class
{
    public required string Id { get; set; }
    public required string Etag { get; set; }
    public required T Doc { get; set; }
}

[GenerateSerializer]
[Alias("CompanyGrainState")]
public class CompanyGrainState
{
    private CompanyGrainState() { }

    [Id(0)]
    public required string Id { get; set; }

    [Id(1)]
    public required CompanyInfo Info { get; set; }
    [Id(2)]
    public required CompanyMetrics Metrics { get; set; }

    [Id(3)]
    public required CompanyAddress Addresses { get; set; }

    [Id(4)]
    public required DateTime UpdateDateTime { get; set; }

    [Id(5)]
    public required CompanyGrainStateType StateType { get; set; } = CompanyGrainStateType.Created;

    public static CompanyGrainState? Apply(CompanyGrainState? state, List<IEvent> events)
    {
        foreach (var e in events)
        {
            state = Apply(state, e);
        }

        return state;
    }

    public static CompanyGrainState Apply(CompanyGrainState? state, IEvent e)
    {
        return e switch
        {
            CompanyCreated c => On(state, c),
            CompanyInfoUpdated i => On(state, i),
            CompanyAddressUpdated a => On(state, a),
            CompanyMetricsUpdated m => On(state, m),
            _ => throw new Exception("Not handled event"),
        };
    }

    private static CompanyGrainState On(CompanyGrainState? state, CompanyCreated e)
    {
        if (state is not null)
            return state;

        state = new CompanyGrainState
        {
            Id = e.Id,

            Info = new CompanyInfo
            {
                CompanyName = e.CompanyName,
                CeoName = e.CeoName,
                FoundedDate = e.FoundedDate,
                Industry = e.Industry,
            },

            Metrics = new CompanyMetrics
            {
                Revenue = e.Revenue,
                EmployeeCount = e.EmployeeCount,
            },

            Addresses = new CompanyAddress
            {
                HqAddress = e.HqAddress,
                HqCity = e.HqCity,
                HqCountry = e.HqCountry,
                HqState = e.HqState,
            },

            UpdateDateTime = DateTime.UtcNow,
            StateType = CompanyGrainStateType.Created,
        };

        return state;
    }

    private static CompanyGrainState On(CompanyGrainState? state, CompanyMetricsUpdated m)
    {
        ArgumentNullException.ThrowIfNull(state);

        state.Metrics = new CompanyMetrics
        {
            EmployeeCount = m.EmployeeCount,
            Revenue = m.Revenue,
        };

        state.UpdateDateTime = DateTime.UtcNow;
        state.StateType = CompanyGrainStateType.Updated;

        return state;
    }

    private static CompanyGrainState On(CompanyGrainState? state, CompanyAddressUpdated a)
    {
        ArgumentNullException.ThrowIfNull(state);

        state.Addresses = new CompanyAddress
        {
            HqAddress = a.HqAddress,
            HqCity = a.HqCity,
            HqCountry = a.HqCountry,
            HqState = a.HqState,
        };

        state.UpdateDateTime = DateTime.UtcNow;
        state.StateType = CompanyGrainStateType.Updated;

        return state;
    }

    private static CompanyGrainState On(CompanyGrainState? state, CompanyInfoUpdated i)
    {
        ArgumentNullException.ThrowIfNull(state);

        state.Info = new CompanyInfo
        {
            CeoName = i.CeoName,
            CompanyName = i.CompanyName,
            FoundedDate = i.FoundedDate,
            Industry = i.Industry,
        };

        state.UpdateDateTime = DateTime.UtcNow;
        state.StateType = CompanyGrainStateType.Updated;

        return state;
    }
}

[GenerateSerializer]
[Alias("CompanyAddress")]
public class CompanyAddress
{
    [Id(0)]
    public required string HqAddress { get; set; }
    [Id(1)]
    public required string HqCity { get; set; }
    [Id(2)]
    public required string HqState { get; set; }
    [Id(3)]
    public required string HqCountry { get; set; }

    public bool HasChanged(string hqAddress, string hqCity, string hqState, string hqCountry)
    {
        return HqAddress != hqAddress ||
               HqCity != hqCity ||
               HqState != hqState ||
               HqCountry != hqCountry;
    }
}

[GenerateSerializer]
[Alias("CompanyInfo")]
public class CompanyInfo
{
    [Id(0)]
    public required string CompanyName { get; set; }
    [Id(1)]
    public required string CeoName { get; set; }
    [Id(2)]
    public required string Industry { get; set; }
    [Id(3)]
    public required DateTime FoundedDate { get; set; }

    public bool HasChanged(string companyName, string ceoName, string industry, DateTime foundedDate)
    {
        return CompanyName != companyName ||
               CeoName != ceoName ||
               Industry != industry ||
               FoundedDate != foundedDate;
    }
}

[GenerateSerializer]
[Alias("CompanyMetrics")]
public class CompanyMetrics
{
    [Id(0)]
    public required decimal Revenue { get; set; }

    [Id(1)]
    public required int EmployeeCount { get; set; }

    public bool HasChanged(decimal revenue, int employeeCount)
    {
        return Revenue != revenue || EmployeeCount != employeeCount;
    }
}
