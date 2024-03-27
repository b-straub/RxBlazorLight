
using RxBlazorLightCore;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RxBlazorLightCoreTests")]

namespace RxBlazorLightCoreTestBase
{
    public record CRUDTest(string Item, Guid Id);

    public enum TestEnum
    {
        ONE,
        TWO,
        THREE
    }

    public partial class ServiceFixture : RxBLService
    {
        public IState<int> IntState { get; }

        public int IntStateResult { get; internal set; }
        public IStateCommand IntCommand { get; }
        public IStateCommandAsync IntCommandAsync { get; }

        public List<CRUDTest> CRUDList { get; } = [];
        public IStateCommandAsync CRUDListCommand { get; }

        public Dictionary<Guid, CRUDTest> CRUDDict { get; } = [];
        public IStateCommandAsync CRUDDictCommand { get; }

        public IStateGroup<TestEnum> EnumStateGroup { get; }
        public IStateGroupAsync<TestEnum> EnumStateGroupAsync { get; }

        public ServiceFixture()
        {
            IntState = this.CreateState(-1);

            IntCommand = this.CreateStateCommand();
            IntCommandAsync = this.CreateStateCommandAsync();

            CRUDListCommand = this.CreateStateCommandAsync();
            CRUDDictCommand = this.CreateStateCommandAsync();
            EnumStateGroup = this.CreateStateGroup([TestEnum.ONE, TestEnum.TWO, TestEnum.THREE], TestEnum.ONE, i => i == 1);
            EnumStateGroupAsync = this.CreateStateGroupAsync([TestEnum.ONE, TestEnum.TWO, TestEnum.THREE], TestEnum.ONE, i => i == 1);
        }
    }
}
