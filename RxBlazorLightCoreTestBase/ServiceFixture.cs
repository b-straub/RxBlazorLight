
using RxBlazorLightCore;

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
        public IState<int> ObservableIntState { get; }
        public IState<int> IntState { get; }
        public IStateAsync<int> IntStateAsync { get; }

        public IStateAsync<IList<CRUDTest>> CRUDListState { get; }
        public IStateAsync<IDictionary<Guid, CRUDTest>> CRUDDictState { get; }

        public IStateGroup<TestEnum> EnumStateGroup { get; }
        public IStateGroupAsync<TestEnum> EnumStateGroupLR { get; }

        public IState ServiceState { get; }
        public IStateAsync ServiceStateLR { get; }

        public string Test { get; private set; } = string.Empty;

        public ServiceFixture() 
        {
            ObservableIntState = this.CreateState(0);
            IntState = this.CreateState(-1);
            IntStateAsync = this.CreateStateAsync(10);

            CRUDListState = this.CreateStateAsync<IList<CRUDTest>>([]);
            CRUDDictState = this.CreateStateAsync<IDictionary<Guid, CRUDTest>>(new Dictionary<Guid, CRUDTest>());
            EnumStateGroup = this.CreateStateGroup([TestEnum.ONE, TestEnum.TWO, TestEnum.THREE], TestEnum.ONE, i => i == 1);
            EnumStateGroupLR = this.CreateStateGroupAsync([TestEnum.ONE, TestEnum.TWO, TestEnum.THREE], TestEnum.ONE, i => i == 1);

            ServiceState = this.CreateState();
            ServiceStateLR = this.CreateStateAsync();
        }

        public void ClearTest()
        {
            Test = string.Empty;
        }
    }
}
