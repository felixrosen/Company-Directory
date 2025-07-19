using CompanyDirectory.API.Infrastructure.Orleans.Types;
using CompanyDirectory.API.Operations.Employees.Services.Grains.EmployeeGrain.Types;

namespace CompanyDirectory.API.Operations.Employees.Services.Grains.EmployeeGrain;

[GenerateSerializer]
[Alias("EmployeeGrainState")]
public class EmployeeGrainState
{
    private EmployeeGrainState() { }

    [Id(0)]
    public required string Id { get; set; }
    [Id(1)]
    public required string FirstName { get; set; }
    [Id(2)]
    public required string LastName { get; set; }
    [Id(3)]
    public required string Department { get; set; }
    [Id(4)]
    public required decimal Salary { get; set; }
    [Id(5)]
    public string? CompanyId { get; set; }

    public static EmployeeGrainState? Apply(EmployeeGrainState? state, List<IEvent> events)
    {
        foreach (var e in events)
        {
            state = Apply(state, e);
        }

        return state;
    }

    public static EmployeeGrainState Apply(EmployeeGrainState? state, IEvent e)
    {
        return e switch
        {
            EmployeeCreated @event => On(state, @event),
            EmployeeNameUpdated @event => On(state, @event),
            EmployeeSalaryUpdated @event => On(state, @event),
            EmployeeDepartmentUpdated @event => On(state, @event),

            _ => throw new Exception("Not handled event"),
        };
    }

    private static EmployeeGrainState On(EmployeeGrainState? state, EmployeeCreated e)
    {
        if (state is not null)
            return state;

        state = new EmployeeGrainState
        {
            Id = e.Id,

            FirstName = e.FirstName,
            LastName = e.LastName,
            Department = e.Department,
            Salary = e.Salary,

            CompanyId = e.CompanyId
        };

        return state;
    }

    private static EmployeeGrainState On(EmployeeGrainState? state, EmployeeNameUpdated e)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state), "State cannot be null");

        state.FirstName = e.FirstName;
        state.LastName = e.LastName;
        return state;
    }

    private static EmployeeGrainState On(EmployeeGrainState? state, EmployeeSalaryUpdated e)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state), "State cannot be null");

        state.Salary = e.Salary;
        return state;
    }

    private static EmployeeGrainState On(EmployeeGrainState? state, EmployeeDepartmentUpdated e)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state), "State cannot be null");

        state.Department = e.Department;
        return state;
    }
}