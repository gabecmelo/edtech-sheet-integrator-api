using EdTech.SheetIntegrator.Domain.Common;

namespace EdTech.SheetIntegrator.Domain.UnitTests.Common;

public class EntityTests
{
    private sealed class FakeEntity : Entity<Guid>
    {
        public FakeEntity(Guid id)
            : base(id)
        {
        }
    }

    private sealed class OtherFakeEntity : Entity<Guid>
    {
        public OtherFakeEntity(Guid id)
            : base(id)
        {
        }
    }

    [Fact]
    public void Entities_Of_Same_Type_With_Same_Id_Are_Equal()
    {
        var id = Guid.NewGuid();
        var a = new FakeEntity(id);
        var b = new FakeEntity(id);

        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Entities_Of_Same_Type_With_Different_Ids_Are_Not_Equal()
    {
        var a = new FakeEntity(Guid.NewGuid());
        var b = new FakeEntity(Guid.NewGuid());

        a.Equals(b).Should().BeFalse();
        (a == b).Should().BeFalse();
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Entities_Of_Different_Types_With_Same_Id_Are_Not_Equal()
    {
        var id = Guid.NewGuid();
        Entity<Guid> a = new FakeEntity(id);
        Entity<Guid> b = new OtherFakeEntity(id);

        a.Equals(b).Should().BeFalse();
        (a == b).Should().BeFalse();
    }

    [Fact]
    public void Entity_Is_Not_Equal_To_Null()
    {
        var a = new FakeEntity(Guid.NewGuid());

        a.Equals(null).Should().BeFalse();
        (a == null).Should().BeFalse();
        (null == a).Should().BeFalse();
        (a != null).Should().BeTrue();
    }

    [Fact]
    public void Two_Null_Entity_References_Are_Equal_Via_Operator()
    {
        FakeEntity? a = null;
        FakeEntity? b = null;

        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
    }

    [Fact]
    public void Entity_Is_Reference_Equal_To_Itself()
    {
        var a = new FakeEntity(Guid.NewGuid());

        a.Equals(a).Should().BeTrue();
    }
}
