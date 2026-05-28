using Server.Models;
using Server.Services;
using Shared.Models;

namespace Tests;

public class MatchmakingServiceTests
{
    private MatchmakingService CreateService() => new();

    private Person CreatePerson(string username, string city, int years,
        Gender gender, List<Gender> interestedIn) => new()
        {
            Username = username,
            City = city,
            Years = years,
            Gender = gender,
            InterestedIn = interestedIn,
            PhoneNumber = "0600000000",
            ConnectionId = Guid.NewGuid().ToString()
        };

    // AddPerson

    [Fact]
    public void AddPerson_NewUser_ReturnsTrue()
    {
        var service = CreateService();
        var person = CreatePerson("Ana", "Novi Sad", 25, Gender.Female, [Gender.Male]);

        var result = service.AddPerson(person);

        Assert.True(result);
    }

    [Fact]
    public void AddPerson_DuplicateUsername_ReturnsFalse()
    {
        var service = CreateService();
        var person = CreatePerson("Ana", "Novi Sad", 25, Gender.Female, [Gender.Male]);

        service.AddPerson(person);
        var result = service.AddPerson(person);

        Assert.False(result);
    }

    [Fact]
    public void AddPerson_MultipleUsers_AllAdded()
    {
        var service = CreateService();
        service.AddPerson(CreatePerson("Ana", "Novi Sad", 25, Gender.Female, [Gender.Male]));
        service.AddPerson(CreatePerson("Boban", "Novi Sad", 26, Gender.Male, [Gender.Female]));

        Assert.Equal(2, service.GetAll().Count);
    }

    // RemovePerson

    [Fact]
    public void RemovePerson_ExistingUser_Removed()
    {
        var service = CreateService();
        service.AddPerson(CreatePerson("Ana", "Novi Sad", 25, Gender.Female, [Gender.Male]));

        service.RemovePerson("Ana");

        Assert.Empty(service.GetAll());
    }

    [Fact]
    public void RemovePerson_NonExistingUser_NoError()
    {
        var service = CreateService();

        var exception = Record.Exception(() => service.RemovePerson("nonexistent"));

        Assert.Null(exception);
    }

    // GetByUsername

    [Fact]
    public void GetByUsername_ExistingUser_ReturnsPerson()
    {
        var service = CreateService();
        service.AddPerson(CreatePerson("Ana", "Novi Sad", 25, Gender.Female, [Gender.Male]));

        var result = service.GetByUsername("Ana");

        Assert.NotNull(result);
        Assert.Equal("Ana", result.Username);
    }

    [Fact]
    public void GetByUsername_NonExistingUser_ReturnsNull()
    {
        var service = CreateService();

        var result = service.GetByUsername("nonexistent");

        Assert.Null(result);
    }

    // GetByConnectionId

    [Fact]
    public void GetByConnectionId_ExistingConnection_ReturnsPerson()
    {
        var service = CreateService();
        var person = CreatePerson("Ana", "Novi Sad", 25, Gender.Female, [Gender.Male]);
        service.AddPerson(person);

        var result = service.GetByConnectionId(person.ConnectionId);

        Assert.NotNull(result);
        Assert.Equal("Ana", result.Username);
    }

    // FindBestMatch

    [Fact]
    public void FindBestMatch_OnlyOnePerson_ReturnsNull()
    {
        var service = CreateService();
        var ana = CreatePerson("Ana", "Novi Sad", 25, Gender.Female, [Gender.Male]);
        service.AddPerson(ana);

        var result = service.FindBestMatch(ana);

        Assert.Null(result);
    }

    [Fact]
    public void FindBestMatch_NoCompatibleGender_ReturnsNull()
    {
        var service = CreateService();
        var ana = CreatePerson("Ana", "Novi Sad", 25, Gender.Female, [Gender.Female]);
        var boban = CreatePerson("Boban", "Novi Sad", 26, Gender.Male, [Gender.Female]);
        service.AddPerson(ana);
        service.AddPerson(boban);

        var result = service.FindBestMatch(ana);

        Assert.Null(result);
    }

    [Fact]
    public void FindBestMatch_BlockedUser_NotReturned()
    {
        var service = CreateService();
        var ana = CreatePerson("Ana", "Novi Sad", 25, Gender.Female, [Gender.Male]);
        var boban = CreatePerson("Boban", "Novi Sad", 26, Gender.Male, [Gender.Female]);
        ana.BlockedUsers.Add("Boban");
        service.AddPerson(ana);
        service.AddPerson(boban);

        var result = service.FindBestMatch(ana);

        Assert.Null(result);
    }

    [Fact]
    public void FindBestMatch_SenderBlockedByCandiate_NotReturned()
    {
        var service = CreateService();
        var ana = CreatePerson("Ana", "Novi Sad", 25, Gender.Female, [Gender.Male]);
        var boban = CreatePerson("Boban", "Novi Sad", 26, Gender.Male, [Gender.Female]);
        boban.BlockedUsers.Add("Ana");
        service.AddPerson(ana);
        service.AddPerson(boban);

        var result = service.FindBestMatch(ana);

        Assert.Null(result);
    }

    [Fact]
    public void FindBestMatch_CompatiblePair_ReturnsMatch()
    {
        var service = CreateService();
        var ana = CreatePerson("Ana", "Novi Sad", 25, Gender.Female, [Gender.Male]);
        var boban = CreatePerson("Boban", "Novi Sad", 26, Gender.Male, [Gender.Female]);
        service.AddPerson(ana);
        service.AddPerson(boban);

        var result = service.FindBestMatch(ana);

        Assert.NotNull(result);
        Assert.Equal("Boban", result.Username);
    }

    [Fact]
    public void FindBestMatch_NeverReturnsSelf()
    {
        var service = CreateService();
        var ana = CreatePerson("Ana", "Novi Sad", 25, Gender.Female, [Gender.Female]);
        var dragana = CreatePerson("Dragana", "Novi Sad", 24, Gender.Female, [Gender.Female]);
        service.AddPerson(ana);
        service.AddPerson(dragana);

        for (int i = 0; i < 20; i++)
        {
            var result = service.FindBestMatch(ana);
            Assert.NotEqual("Ana", result?.Username);
        }
    }

    // ToDto

    [Fact]
    public void ToDto_DoesNotExposeConnectionId()
    {
        var person = CreatePerson("Ana", "Novi Sad", 25, Gender.Female, [Gender.Male]);

        var dto = person.ToDto();

        Assert.Equal("Ana", dto.Username);
        Assert.Equal("Novi Sad", dto.City);
        Assert.Equal(25, dto.Years);
    }
}