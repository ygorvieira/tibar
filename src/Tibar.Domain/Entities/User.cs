namespace Tibar.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;

    private User() { }

    public User(string name, string email)
    {
        Name = name;
        Email = email;
    }

    public void UpdateName(string name)
    {
        Name = name;
        MarkAsUpdated();
    }

    public void UpdateEmail(string email)
    {
        Email = email;
        MarkAsUpdated();
    }
}
